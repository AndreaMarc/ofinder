import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';

export default class StandardCoreSendCommunicationsComponent extends Component {
  @service session;
  @service jsonApi;
  @service dialogs;
  @service store;

  @tracked available = 'waiting';
  @tracked users = [];
  @tracked newRecord = null; // usato in creazione nuovo record

  constructor(...attributes) {
    super(...attributes);
    this.newRecord = this.initializeRecord();
    this.newRecord.sendEmail = false;
    this.start.perform();
  }

  start = task({ drop: true }, async () => {
    try {
      let search = {
        filter: [
          {
            function: 'any',
            column: 'state',
            value: `'accepted','ownerCreated','selfCreated'`,
          },
          {
            function: 'equals',
            column: 'tenantId',
            value: this.session.get('data.tenantId'),
          },
        ],
        sort: `user.userProfile.lastName,user.userProfile.firstName,user.email`,
        include: 'user.userProfile', //
      };
      let query = this.jsonApi.queryBuilder(search);
      let userTenants = await this.store.query('user-tenant', query);
      let arr = [];
      userTenants.forEach((element) => {
        arr.push({
          id: element.user.get('id'),
          value: `${element.user.get(
            'userProfile.lastName'
          )} ${element.user.get('userProfile.firstName')} - ${element.user.get(
            'email'
          )}`,
        });
      });
      this.users = arr;

      this.available = 'available';
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
  });

  get usersText() {
    return JSON.stringify(this.users);
  }

  @action
  changeValue(field, event) {
    let val = '';
    try {
      val = event.target.value.trim();
    } catch (e) {
      val = event.trim();
    }

    this.newRecord[field] = val;
  }

  @action
  changeNotificationType(event) {
    this.newRecord['sendPushNotification'] = true;
    this.newRecord['sendEmail'] = true;

    let val = event.target.value;
    if (val === '1') {
      this.newRecord['sendEmail'] = false;
    } else if (val === '2') {
      this.newRecord['sendPushNotification'] = false;
    }
  }

  @action
  changeSilent(event) {
    this.newRecord['onlyData'] = false;

    let val = event.target.value;
    if (val !== '') {
      this.newRecord['onlyData'] = true;
    }
  }

  @action
  changeWebSocket(event) {
    this.newRecord['sendWebSocket'] = true;

    let val = event.target.value;
    if (val === '') {
      this.newRecord['sendWebSocket'] = false;
      this.newRecord['forceWebSocketApp'] = false;
    }
  }

  @action
  changeWebSocketApp(event) {
    this.newRecord['forceWebSocketApp'] = true;

    let val = event.target.value;
    if (val === '') {
      this.newRecord['forceWebSocketApp'] = false;
    }
  }

  @action
  changeUser(id) {
    this.newRecord['userId'] = id;
  }

  @action
  send() {
    let self = this;

    if (this.newRecord.userId === '') {
      this.dialogs.toast(
        'Scegli il destinatario!',
        'warning',
        'bottom-right',
        4
      );
      return;
    }

    if (this.newRecord.title === '') {
      this.dialogs.toast('Inserisci il Titolo!', 'warning', 'bottom-right', 4);
      return;
    }

    if (this.newRecord.body === '') {
      this.dialogs.toast(
        'Inserisci il Messaggio!',
        'warning',
        'bottom-right',
        4
      );
      return;
    }

    this.dialogs.confirm(
      '<h6>Invio della comunicazione</h6>',
      `<p>Confermi l'invio?</p>`,
      () => {
        self.sendCommunications.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  sendCommunications = task({ drop: true }, async () => {
    let self = this;
    try {
      // Creare un nuovo record
      let selectedUser = this.store.peekRecord('user', this.newRecord.userId);

      let notification = this.store.createRecord('notification', {
        id: v4(),
        userId: this.newRecord.userId,
        email: selectedUser.email, // Assicurati che questo campo sia correttamente popolato
        title: this.newRecord.title,
        body: this.newRecord.body,
        data: this.newRecord.data,
        onlyData: this.newRecord.onlyData,
        pushType: this.newRecord.type,
        //messageId: this.newRecord.messageId,
        dateSent: new Date(), // Imposta la data di invio
        //dateRead: this.newRecord.dateRead,
        //read: this.newRecord.read,
        //erased: this.newRecord.erased,
        //templateCode: this.newRecord.templateCode,
        sendPushNotification: this.newRecord.sendPushNotification,
        sendEmail: this.newRecord.sendEmail,
        // recipientsIdList: this.newRecord.recipientsIdList, // Se necessario
      });

      // Salvare il record
      await notification.save();

      // Gestire il successo dell'operazione
      self.dialogs.toast(
        'Comunicazione inviata con successo!',
        'success',
        'bottom-right',
        4
      );
    } catch (error) {
      console.error(error);
      self.dialogs.toast(
        `Errore durante l'invio della comunicazione.`,
        'error',
        'bottom-right',
        4
      );
    }
  });

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord('notification'); // Crea una nuova istanza del modello (ad esempio, 'user')
    // Crea un semplice oggetto JavaScript con tutte le proprietà del modello
    let modelData = {};
    modelInstance.eachAttribute((key) => {
      modelData[key] = modelInstance.get(key);
    });

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }
}
