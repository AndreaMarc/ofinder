/**
 * Gestisce l'array di entità disponibili in setup/entitiesList.
 * Ciascuna entità è definita dal seguente oggetto:
 * {
      id: // guid,
      entity: // entità, come definita in route,js,
      title: // versione "parlante" dell'entità',
      key: // chiave di traduzione del titolo (facoltativa),
      description: // descrizione dell'entità,
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
//import config from 'poc-nuovo-fwk/config/environment';

export default class StandardCoreSetupEntitiesComponent extends Component {
  @service('siteSetup') stp;
  @service session;
  @service jsUtility;
  @service fetch;
  @service dialogs;
  @service store;

  recordWeb = null;
  recordApp = null;
  @tracked serviceAvailable = 'waiting';
  @tracked savedChanges = true;
  @tracked entitiesList = [];
  @tracked editingRecord = null;
  @tracked entitiesOnDb = [];

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
      let el = this.recordWeb.entitiesList ? this.recordWeb.entitiesList : [];

      // ordinamento alfabetico
      el.sort((a, b) => a.entity.localeCompare(b.entity));
      this.entitiesList = el;

      await this.findEntity.perform();
    }
  }

  // estrae l'elenco delle entità
  findEntity = task({ drop: true }, async () => {
    this.entitiesOnDb = [];
    let self = this;

    this.fetch
      .call('entity/getAll', 'GET', null, {}, true, this.session)
      .then((res) => {
        if (res.data && res.data.length > 0) {
          let x = [];
          res.data.forEach((element) => {
            let ent = element.split('.').at(-1);

            let existing = this.entitiesList.filter((item) => {
              return item.entity === ent;
            });

            if (existing.length === 0) {
              x.push(ent);
            }
          });
          self.entitiesOnDb = x;
        }
      })
      .catch((e) => {
        console.error(e);
        self.serviceAvailable = false;
      });
  });

  get recordsLength() {
    return this.entitiesList.length;
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
    this.entitiesList = this.entitiesList.map((obj) =>
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

    this.entitiesList = this.entitiesList.map((obj) =>
      obj.id === record.id ? this.newValue : obj
    );

    await this.saveOnServer(this.entitiesList);
    this.editingRecord = null;
    this.newValue = {};
  });

  // aggiorno i record di setup sul server
  async saveOnServer(entitiesList) {
    try {
      let recordWeb = await this.store.queryRecord('setup', {
        filter: `equals(environment,'web')`,
      });
      let recordApp = await this.store.queryRecord('setup', {
        filter: `equals(environment,'app')`,
      });
      recordWeb.entitiesList = entitiesList;
      recordApp.entitiesList = entitiesList;
      recordWeb.save();
      recordApp.save();

      this.stp.setSetup('entitiesList', this.entitiesList);
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
      !this.newRecord.entity ||
      this.newRecord.entity === '' ||
      !this.newRecord.title ||
      this.newRecord.title === '' ||
      !this.newRecord.description ||
      this.newRecord.description === ''
    ) {
      this.dialogs.toast(
        'Entità, Titolo e Descrizione sono obbligatori!',
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    let regex = this.jsUtility.regex('lndd'); // lettere, numeri, punti, underscore
    if (
      !regex.test(this.newRecord.entity) ||
      (!regex.test(this.newRecord.key) && this.newRecord.key !== '') ||
      (!regex.test(this.newRecord.keyDescription) &&
        this.newRecord.keyDescription !== '')
    ) {
      this.dialogs.toast(
        'Nel campo Entità e nelle chiavi sono ammessi solo lettere, numeri, underscore e punti',
        'error',
        'bottom-right',
        6
      );
      return false;
    }

    let toAdd = {
      id: v4(),
      entity: this.newRecord.entity,
      title: this.newRecord.title,
      key: this.newRecord.key ? this.newRecord.key : '',
      description: this.newRecord.description,
      keyDescription: this.newRecord.keyDescription
        ? this.newRecord.keyDescription
        : '',
    };

    let el = this.entitiesList;
    el.push(toAdd);
    this.entitiesList = el;

    await this.saveOnServer(this.entitiesList);
    this.editingRecord = null;
    this.newValue = {};
    this.newRecord = this.initializeRecord();
    this.entitiesOnDb = this.entitiesOnDb.filter(
      (item) => item !== toAdd.entity
    );
  });

  @action
  delVoice(record) {
    this.dialogs.confirm(
      '<h6>Cancellazione record</h6>',
      `<p class="text-danger">Azione irreversibile. Confermi la cancellazione?</p>`,
      async () => {
        let el = this.entitiesList;
        el = el.filter((obj) => {
          return obj.id !== record.id;
        });
        this.entitiesList = el;

        await this.saveOnServer(this.entitiesList);
        this.editingRecord = null;
        this.newValue = {};
        this.newRecord = this.initializeRecord();
        await this.findEntity.perform();
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
