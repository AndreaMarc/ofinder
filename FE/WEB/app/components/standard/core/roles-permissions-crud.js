/**
 * @param {string} currentTenant guid del tenant corrente
 */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
//import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !

export default class StandardCoreRolesPermissionsCrudComponent extends Component {
  @service siteSetup;
  @service session;
  @service dialogs;
  @service store;

  modelName = 'role-claim';
  @tracked serviceAvailable = 'waiting';
  @tracked serviceUpperAvailable = 'waiting';
  @tracked records = [];
  @tracked tenants = [];
  @tracked roles = [];
  @tracked currentTenant = null;
  @tracked currentRole = '';
  @tracked filterText = '';
  //@tracked predefinedClaims = [];

  newValue = {}; // usato nella modifica del record
  @tracked newRecord = null; // usato in creazione nuovo record

  constructor(...attributes) {
    super(...attributes);
    this.newRecord = this.initializeRecord();
    this.start();
  }

  @action
  async start() {
    this.serviceAvailable = 'waiting';
    this.serviceUpperAvailable = 'waiting';
    this.roles = [];
    this.currentRole = '';

    let currentTenant = this.args.currentTenant ? this.args.currentTenant : '';
    if (currentTenant === '') return false;
    this.currentTenant = await this.store.peekRecord('tenant', currentTenant);
    this.findRoles.perform();
  }

  findRoles = task({ drop: true }, async () => {
    try {
      this.serviceAvailable = 'waiting';
      this.currentRole = '';
      this.roles = [];
      if (!this.currentTenant || !this.currentTenant.id) return;

      this.roles = await this.store.query('role', {
        filter: `equals(tenantId,'${parseInt(this.currentTenant.id)}')`,
        sort: `name`,
      });

      this.serviceUpperAvailable = 'available';
      this.serviceAvailable = 'available';
    } catch (e) {
      this.serviceAvailable = 'unavailable';
    }
  });

  // consente il cambio ruolo
  @action
  async updateRole(event) {
    this.currentRole = event.target.value;
    this.findRecords.perform();
  }

  findRecords = task({ drop: true }, async () => {
    try {
      this.serviceAvailable = 'waiting';
      if (!this.currentRole) return;

      this.records = await this.store.query(this.modelName, {
        filter: `and(equals(roleId,'${this.currentRole}'),equals(claimType,'custom'))`,
        sort: `claimValue`,
      });
      this.serviceAvailable = 'available';
      //this.predefinedClaims = this.siteSetup.siteSetup.defaultClaims;
      //console.warn(this.predefinedClaims);
    } catch (e) {
      console.error(e);
      this.serviceAvailable = 'unavailable';
    }
  });

  get predefinedClaims() {
    return this.siteSetup.siteSetup.defaultClaims;
  }

  get filteredRecords() {
    if (this.filterText) {
      return this.records.filter((record) =>
        record.claimValue.toLowerCase().includes(this.filterText.toLowerCase())
      );
    } else {
      return this.records;
    }
  }

  get freeClaims() {
    return this.predefinedClaims.filter(
      (item) => !this.records.some((itemB) => itemB.claimValue === item.name)
    );
  }

  get recordsLength() {
    return this.records.length;
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

  // crea un nuovo record
  saveNewVoice = task({ drop: true }, async () => {
    // verifica coerenza dei dati
    if (!this.currentTenant.id || !this.currentRole) {
      this.dialogs.toast(
        'Si è verificato un errore. Ricaricare la pagina',
        'error',
        'bottom-right',
        5
      );
      return false;
    }
    if (
      !this.newRecord ||
      !this.newRecord.claimValue ||
      this.newRecord.claimValue === ''
    ) {
      this.dialogs.toast(
        'Il nome del Permesso è obbligatorio!',
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    try {
      let newR = this.store.createRecord(this.modelName, {
        //id: v4(),
        claimValue: this.newRecord.claimValue,
        claimType: 'custom',
        roleId: this.currentRole,
      });

      await newR.save();
      this.findRecords.perform();

      // resetto l'oggetto newRecord per svuotare i campi input collegati
      this.newRecord = this.initializeRecord();
    } catch (e) {
      console.error(e);
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
