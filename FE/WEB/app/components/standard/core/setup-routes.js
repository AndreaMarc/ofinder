/**
 * Gestisce l'array di rotte disponibili in setup/routesList.
 * Ciascuna rotta è definita dal seguente oggetto:
 * {
      id: // guid,
      route: // rotta, come definita in route,js,
      title: // versione "parlante" della rotta,
      key: // chiave di traduzione del titolo (facoltativa),
      description: // descrizione della rotta,
      keyDescription: // chiave di traduzione della descrizione (facoltativa)
    }
 */

/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { task } from 'ember-concurrency';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { v4 } from 'ember-uuid';

export default class StandardCoreSetupRoutesComponent extends Component {
  @service('siteSetup') stp;
  @service jsUtility;
  @service dialogs;
  @service store;

  recordWeb = null;
  recordApp = null;
  @tracked serviceAvailable = 'waiting';
  @tracked savedChanges = true;
  @tracked routesList = [];
  @tracked editingRecord = null;

  newValue = {}; // usato nella modifica del record
  @tracked newRecord = null; // usato in creazione nuovo record

  // costruttore
  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    this.recordWeb = this.args.recordWeb;
    this.recordApp = this.args.recordApp;
    this.serviceAvailable = this.args.serviceAvailable;
    this.newRecord = this.initializeRecord();

    if (this.serviceAvailable === 'available') {
      // estraggo il dato da recordWeb (recordApp contiene sempre lo stesso valore)
      let rl = this.recordWeb.routesList ? this.recordWeb.routesList : [];

      rl.sort((a, b) => a.route.localeCompare(b.route));
      this.routesList = rl;
    }
  }

  get recordsLength() {
    return this.routesList.length;
  }

  // cattura il valore inserito dall'utente per la creazione/modifica di un record
  @action
  storeNewValue(field, isUpdate, event) {
    if (isUpdate) {
      this.newValue[field] = event.target.value;
    } else {
      this.newRecord[field] = event.target.value;
    }

    // verifica formato
    if (['route', 'key', 'keyDescription'].includes(field)) {
      let regex = this.jsUtility.regex('lndd'); // lettere, numeri, punti, underscore
      if (!regex.test(event.target.value)) {
        this.dialogs.toast(
          'Sono ammessi solo lettere, numeri, underscore e punti',
          'warning',
          'bottom-right',
          3
        );
      }
    }
  }

  // abilito modifica di un record
  @action
  editRecord(record) {
    this.editingRecord = record;
    Object.keys(record).forEach((key) => {
      this.newValue[key] = record[key];
    });
  }

  @action
  undoRecord(record) {
    this.routesList = this.routesList.map((obj) =>
      obj.id === record.id ? this.editingRecord : obj
    );

    this.editingRecord = null;
    this.newValue = {};
  }

  @action
  saveVoice(record) {
    let self = this;
    this.dialogs.confirm(
      '<h6>Modifica record</h6>',
      `<p>Azione irreversibile. Confermi la modifica?</p>`,
      () => {
        self.saveVoiceConfirmed.perform(record);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  saveVoiceConfirmed = task({ drop: true }, async (record) => {
    // verifica coerenza dei dati
    if (this.newValue.route === '' || this.newValue.title === '') {
      this.dialogs.toast(
        'Rotta e Titolo sono obbligatori!',
        'error',
        'bottom-right',
        5
      );
      return false;
    }

    Object.keys(this.newValue).forEach((key) => {
      if (typeof this.newValue[key] === 'string')
        this.newValue[key] = this.newValue[key].trim();
    });

    let regex = this.jsUtility.regex('lndd'); // lettere, numeri, punti, underscore
    if (
      !regex.test(this.newValue.route) ||
      (this.newValue.key !== '' && !regex.test(this.newValue.key))
    ) {
      this.dialogs.toast(
        'Sono ammessi solo lettere, numeri, underscore e punti',
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    this.routesList = this.routesList.map((obj) =>
      obj.id === record.id ? this.newValue : obj
    );

    await this.saveOnServer(this.routesList);
    this.editingRecord = null;
    this.newValue = {};
  });

  // aggiorno i record di setup sul server
  async saveOnServer(routesList) {
    try {
      let recordWeb = await this.store.queryRecord('setup', {
        filter: `equals(environment,'web')`,
      });
      let recordApp = await this.store.queryRecord('setup', {
        filter: `equals(environment,'app')`,
      });
      recordWeb.routesList = routesList;
      recordApp.routesList = routesList;
      recordWeb.save();
      recordApp.save();

      this.stp.setSetup('routesList', this.routesList);
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        5
      );
    }
  }

  // crea un nuovo record
  saveNewVoice = task({ drop: true }, async () => {
    if (
      !this.newRecord ||
      !this.newRecord.route ||
      this.newRecord.route === '' ||
      !this.newRecord.title ||
      this.newRecord.title === '' ||
      !this.newRecord.description ||
      this.newRecord.description === ''
    ) {
      this.dialogs.toast(
        'Rotta, Titolo e Descrizione sono obbligatori!',
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    let regex = this.jsUtility.regex('lndd'); // lettere, numeri, punti, underscore
    if (
      !regex.test(this.newRecord.route) ||
      (this.newRecord.key !== '' && !regex.test(this.newRecord.key)) ||
      (this.newRecord.keyDescription !== '' &&
        !regex.test(this.newRecord.keyDescription))
    ) {
      this.dialogs.toast(
        'Nel campo Rotta e nelle Chiavi sono ammessi solo lettere, numeri, underscore e punti',
        'error',
        'bottom-right',
        6
      );
      return false;
    }

    let toAdd = {
      id: v4(),
      route: this.newRecord.route,
      title: this.newRecord.title,
      description: this.newRecord.description,
      key: this.newRecord.key ? this.newRecord.key : '',
      keyDescription: this.newRecord.keyDescription
        ? this.newRecord.keyDescription
        : '',
    };

    let rl = this.routesList;
    rl.push(toAdd);
    this.routesList = rl;

    await this.saveOnServer(this.routesList);
    this.editingRecord = null;
    this.newValue = {};
    this.newRecord = this.initializeRecord();
  });

  @action
  delVoice(record) {
    this.dialogs.confirm(
      '<h6>Cancellazione record</h6>',
      `<p class="text-danger">Azione irreversibile. Confermi la cancellazione?</p>`,
      async () => {
        let rl = this.routesList;
        rl = rl.filter((obj) => {
          return obj.id !== record.id;
        });
        this.routesList = rl;

        await this.saveOnServer(this.routesList);
        this.editingRecord = null;
        this.newValue = {};
        this.newRecord = this.initializeRecord();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  initializeRecord() {
    let modelData = {
      id: '',
      rout: '',
      title: '',
      key: '',
      description: '',
      keyDescription: '',
    };

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }
}
