import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import { task } from 'ember-concurrency';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';
import { TrackedObject } from 'tracked-built-ins';

export default class StandardCoreHelpDeskComponent extends Component {
  @service translation;
  @service jsUtility;
  @service dialogs;
  @service session;
  @service store;
  @service fetch;

  @tracked unlogged = true;
  @tracked error = '';
  @tracked newRecord = null; // usato in creazione nuovo record
  @tracked myTickets = [];
  @tracked filterByUser = true;
  @tracked currentLang = this.translation.currentLang;
  currentTenant = null;

  constructor(...attributes) {
    super(...attributes);

    this.newRecord = this.initializeRecord();

    if (this.session.isAuthenticated) {
      this.unlogged = false;
      this.findTenantbyId.perform();
      this.findMyTask.perform();
    }
  }

  findTenantbyId = task({ drop: true }, async () => {
    try {
      //this.currentTenant = await this.store.findRecord('tenant', tenantId);

      await this.fetch
        .call('customers/mine', 'GET', null, {}, true, this.session)
        .then((res) => {
          if (res && res.data) {
            this.newRecord.vat = res.data.tb009Piva ? res.data.tb009Piva : '';
            this.newRecord.email = res.data.tb009Email
              ? res.data.tb009Email
              : '';
            this.newRecord.phone = res.data.tb009Telefono
              ? res.data.tb009Telefono
              : '';
            this.newRecord.lastName = this.session.get('data.lastName');
            this.newRecord.firstName = this.session.get('data.firstName');
          }
        })
        .catch((e) => {
          console.error(e);
          this.unlogged = true;
        });
    } catch (e) {
      console.error(e);
      this.error = htmlSafe(
        `<span class="text-danger">Si è verificato un errore. Riprovare in un secondo momento.</span>`
      );
    }
  });

  @action
  changeValue(field, event) {
    let val = event.target ? event.target.value : event;
    this.newRecord[field] = val;
  }

  @action
  send(event) {
    event.preventDefault();
    if (
      this.newRecord.vat === '' ||
      this.newRecord.email === '' ||
      this.newRecord.phone === '' ||
      this.newRecord.message === '' ||
      this.newRecord.lastName === '' ||
      this.newRecord.firstName === '' ||
      (this.newRecord.organizationToBeConfirmed === '' && this.unlogged)
    ) {
      this.dialogs.toast(
        `Tutti i campi sono obbligatori!`,
        'warning',
        'bottom-right',
        4
      );
      return false;
    }

    let regex = this.jsUtility.regex('email');
    if (!regex.test(this.newRecord.email)) {
      this.dialogs.toast(
        `Inserire un'e-mail corretta!`,
        'warning',
        'bottom-right',
        4
      );
      return false;
    }

    if (this.newRecord.vat.length !== 11) {
      this.dialogs.toast(
        `La Partita IVA inserita non contiene 11 caratteri!`,
        'warning',
        'bottom-right',
        4
      );
      return false;
    }

    let self = this;
    this.dialogs.confirm(
      '<h4>Invio segnalazione</h4>',
      `<p>Confermi l'invio?</p>`,
      () => {
        self.save();
      },
      null,
      ['Sì', 'No']
    );
  }

  @action
  async save() {
    this.newRecord.message = this.newRecord.message.trim();

    try {
      this.newRecord.id = v4();
      this.newRecord.tenantId = this.session.isAuthenticated
        ? parseInt(this.session.get('data.tenantId'))
        : 0;
      this.newRecord.userId = this.session.isAuthenticated
        ? this.session.get('data.id')
        : '';
      let rec = this.store.createRecord('ticket', this.newRecord);
      await rec.save();

      if (this.session.isAuthenticated) {
        this.newRecord.message = '';
      } else {
        this.newRecord = this.initializeRecord();
      }

      this.findMyTask.perform();
      // eslint-disable-next-line no-undef
      Swal.fire({
        icon: 'success',
        title: 'Operazione riuscita',
        text: `Ti contatteremo nel più breve tempo possibile`,
      });

      this.sendNotifications(this.newRecord.id, this.newRecord.userId, true);
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        4
      );
    }
  }

  // per inviare notifiche push ed email ai destinatari
  /**
   * @param {string} ticketId : guid del record ticket
   * @param {string} toUserId : guid dell'utente destinatario della notifica. Se è una notifica collettiva, sarà ''.
   * @param {bool} toTicketManagement : true se la notifica va inviata agli utenti che gestiscono il Servizio di Assistenza Clienti, false altrimenti.
   */
  async sendNotifications(ticketId, toUserId, toTicketManagement) {
    await this.fetch.call(
      'tickets/helpDeskNotifications',
      'POST',
      {
        ticketId: ticketId,
        userId: toUserId,
        toTicketManagement,
      },
      {},
      true,
      this.session
    );
  }

  findMyTask = task({ drop: true }, async () => {
    try {
      let filter = '';
      if (this.filterByUser) {
        filter = `and(equals(userId,'${this.session.get(
          'data.id'
        )}'),equals(tenantId,'${this.session.get(
          'data.tenantId'
        )}'),not(equals(status,'trashed')))`;
      } else {
        filter = `and(equals(tenantId,'${this.session.get(
          'data.tenantId'
        )}'),not(equals(status,'trashed')))`;
      }
      this.myTickets = await this.store.query('ticket', {
        filter: filter,
        sort: `-creationDate`,
        include: `user,user.userProfile`,
      });
    } catch (e) {
      console.error(e);
    }
  });

  @action
  changeFilter(status, event) {
    event.preventDefault;
    this.filterByUser = !!status;
    this.findMyTask.perform();
  }

  getStatus(status) {
    let res = '';

    if (typeof status === 'undefined') {
      res = `<div class="badge badge-secondary align-top">IN ATTESA</div>`;
    } else {
      switch (status) {
        case 'anonymous':
        case 'unprocessed':
          res = `<div class="badge badge-secondary align-top">IN ATTESA</div>`;
          break;
        case 'running':
          res = `<div class="badge badge-warning align-top">IN CORSO</div>`;
          break;
        case 'complete':
          res = `<div class="badge badge-success align-top">COMPLETATO</div>`;
          break;
        default:
          res = `<div class="badge badge-secondary align-top">IN ATTESA</div>`;
      }
    }
    return htmlSafe(res);
  }

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord('ticket'); // Crea una nuova istanza del modello (ad esempio, 'user')
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
