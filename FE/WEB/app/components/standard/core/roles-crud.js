/**
 * Gestisce i ruoli associati a un Tenant
 *
 * @param {string} currentTenant guid del tenant corrente
 * @param {function} updateRole funzione da chiamare per comunicare agli altri sotto-componenti che i ruoli sono cambiati
 */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !

export default class StandardCoreRolesCrudComponent extends Component {
  @service dialogs;
  @service store;

  modelName = 'role';
  @tracked serviceAvailable = 'waiting';
  @tracked records = [];
  //@tracked tenants = [];
  @tracked editingRecordId = null;
  @tracked currentTenant = null;
  @tracked filterText = '';

  newValue = {}; // usato nella modifica del record
  @tracked newRecord = null; // usato in creazione nuovo record
  updateRole = null;

  constructor(...attributes) {
    super(...attributes);
    this.updateRole = this.args.updateRole ? this.args.updateRole : null;
    this.newRecord = this.initializeRecord();
    this.start();
  }

  @action
  async start() {
    this.records = [];
    this.serviceAvailable = 'waiting';
    let currentTenant = this.args.currentTenant ? this.args.currentTenant : '';
    if (currentTenant === '') return false;
    this.currentTenant = await this.store.peekRecord('tenant', currentTenant);
    this.findRecords.perform(); // Avvia la task.
  }

  findRecords = task({ drop: true }, async () => {
    try {
      this.records = await this.store.query(this.modelName, {
        filter: `equals(tenantId,'${parseInt(this.currentTenant.id)}')`,
        sort: `level`, //`-needful,name`,
      });
      this.serviceAvailable = 'available';
    } catch (e) {
      this.serviceAvailable = 'unavailable';
    }
  });

  get filteredRecords() {
    if (this.filterText) {
      return this.records.filter((record) =>
        record.name.toLowerCase().includes(this.filterText.toLowerCase())
      );
    } else {
      return this.records;
    }
  }

  get recordsLength() {
    return this.records.length;
  }

  // abilito modifica di un record
  @action
  editRecord(record) {
    this.editingRecordId = record.id;
    //this.newValue = { ...record };
    record.eachAttribute((key) => {
      this.newValue[key] = record.get(key);
    });
  }
  // annullo modifica di un record
  @action
  undoRecord() {
    this.store.findRecord(this.modelName, this.editingRecordId, {
      reload: true,
    });
    this.editingRecordId = null;
    this.newValue = {};
  }

  // cattura il valore inserito dall'utente per la creazione/modifica di un record
  @action
  storeNewValue(field, isUpdate, event) {
    if (isUpdate) {
      this.newValue[field] = event.target.value;
    } else {
      this.newRecord[field] = event.target.value;
    }
  }

  // chiede conferma per salvataggio modifiche a un record
  @action
  saveVoice(record) {
    let self = this;
    this.dialogs.confirm(
      '<h6>Modifica record</h6>',
      `<p>Azione irreversibile. Confermi la modifica?</p>`,
      () => {
        self.saveVoiceConfirmed.perform(record);
        //self.saveVoiceConfirmed(record);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  saveVoiceConfirmed = task({ drop: true }, async (record) => {
    Object.keys(this.newValue).forEach((key) => {
      record[key] = this.newValue[key];
    });

    try {
      await record.save();
      this.editingRecordId = null;
      this.newValue = {};
      this.findRecords.perform();
      if (this.updateRole) this.updateRole();
    } catch (e) {
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        5
      );
    }
  });

  // crea un nuovo record
  saveNewVoice = task({ drop: true }, async () => {
    // verifica coerenza dei dati
    if (!this.currentTenant.id) {
      this.dialogs.toast(
        'Si è verificato un errore. Ricaricare la pagina',
        'error',
        'bottom-right',
        5
      );
      return false;
    }
    if (!this.newRecord || !this.newRecord.name || this.newRecord.name === '') {
      this.dialogs.toast(
        'Il nome del ruolo è obbligatorio!',
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    try {
      let newR = this.store.createRecord(this.modelName, {
        id: v4(),
        name: this.newRecord.name,
        tenantId: this.currentTenant.id,
        needful: false,
      });

      await newR.save();
      this.findRecords.perform();

      // resetto l'oggetto newRecord per svuotare i campi input collegati
      this.newRecord = this.initializeRecord();
      if (this.updateRole) this.updateRole();
    } catch (e) {
      this.dialogs.toast(
        'Si è verificato un errore, riprovare!',
        'error',
        'bottom-right',
        4
      );
    }
  });

  @action
  delVoice(record) {
    this.dialogs.confirm(
      '<h6>Cancellazione record</h6>',
      `<p class="text-danger">Azione irreversibile. Confermi la cancellazione?</p>`,
      () => {
        record.deleteRecord();
        record.save();
        if (this.updateRole) this.updateRole();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  @action
  insertFilter(event) {
    this.filterText = event.target.value;
  }

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord(this.modelName); // Crea una nuova istanza del modello (ad esempio, 'user')
    // Crea un semplice oggetto JavaScript con tutte le proprietà del modello
    let modelData = {};
    modelInstance.eachAttribute((key) => {
      modelData[key] = modelInstance.get(key);
    });

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }
  // reset dell'oggetto di appoggio utilizzato in creazione nuove record
  @action
  resetRecord() {
    this.newRecord = this.initializeRecord();
  }
}
