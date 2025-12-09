import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';
import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins';

export default class StandardCoreTicketRolesComponent extends Component {
  @service dialogs;
  @service session;
  @service store;

  @tracked available = 'waiting';
  updateSubComponent = null;
  currentTenant = null;
  auth = {};
  @tracked ticketRoles = [];
  @tracked newName = '';
  @tracked editingGuid = '';
  @tracked editRecord = null;

  constructor(...args) {
    super(...args);
    if (
      this.args.updateSubComponent &&
      typeof this.args.updateSubComponent === 'function'
    ) {
      this.updateSubComponent = this.args.updateSubComponent;
    }

    if (this.args.auth) {
      this.auth = this.args.auth; // oggetto come restituito da utility/utils-ticket.js
    }

    this.currentTenant = this.session.get('data.tenantId');

    this.editRecord = this.initializeRecord();
    this.start.perform();
  }

  start = task({ drop: true }, async () => {
    try {
      let tr = await this.store.query('role', {
        sort: 'name',
        filter: `and(equals(tenantId,'${this.currentTenant}'),equals(typology,'ticket'))`,
        include: 'userRoles',
      });
      this.ticketRoles = tr.filter((item) => item.typology === 'ticket');

      this.available = 'available';
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
  });

  @action
  setNewName(event) {
    this.newName = event.target.value;
  }

  // memorizzo nuovo record
  save = task({ drop: true }, async () => {
    try {
      this.newName = this.newName.trim();

      if (this.newName === '') {
        this.dialogs.toast(
          'Il nome è obbligatorio!',
          'warning',
          'bottom-right',
          3
        );
        return false;
      }

      let newR = this.store.createRecord('role', {
        id: v4(),
        name: this.newName,
        needful: true,
        tenantId: this.currentTenant,
        typology: 'ticket',
      });
      await newR.save();

      this.newName = '';

      if (this.updateSubComponent) {
        this.updateSubComponent();
      }
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        3
      );
    }
  });

  @action
  modifyRecord(record) {
    this.editingGuid = record.id;
    this.editRecord.id = record.id;
    this.editRecord.name = record.name;
    this.editRecord.needful = record.needful;
    this.editRecord.tenantId = record.tenantId;
    this.editRecord.typology = record.typology;
  }

  @action
  restoreRecord(record) {
    if (record.id !== this.editingGuid) return false;

    this.editingGuid = '';
    this.editRecord = this.initializeRecord();
  }

  @action
  updateName(event) {
    this.editRecord.name = event.target.value;
  }

  @action
  async saveUpdatedRecord() {
    try {
      this.editRecord.name = this.editRecord.name.trim();
      this.editRecord.note = this.editRecord.note.trim();

      if (this.editRecord.name === '') {
        this.dialogs.toast(
          'Il nome è obbligatorio!',
          'warning',
          'bottom-right',
          3
        );
        return false;
      }

      let oldR = await this.store.peekRecord('area', this.editingGuid);
      oldR.name = this.editRecord.name;
      oldR.note = this.editRecord.note;
      await oldR.save();

      this.editingGuid = '';
      this.editRecord = this.initializeRecord();

      if (this.updateSubComponent) {
        this.updateSubComponent();
      }
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        3
      );
    }
  }

  @action
  deleteArea(record) {
    let self = this;
    this.dialogs.confirm(
      `<h6 class="text-danger">Eliminazione Ruolo "<em>${record.name}</em>"</h6>`,
      `<p class="text-danger">${
        record.userRoles > 0
          ? record.userRoles +
            ' operatori sono assegnati a questo reparto.<br /><br />'
          : ''
      }Confermi l'eliminazione?</p>`,
      () => {
        self.deleteAreaConfirmed(record);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  async deleteAreaConfirmed(record) {
    try {
      await record.destroyRecord();
      if (this.updateSubComponent) {
        this.updateSubComponent();
      }
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        3
      );
    }
  }

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord('area'); // Crea una nuova istanza del modello (ad esempio, 'user')
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
