import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';
import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins';
import { camelize } from '@ember/string';

export default class StandardCoreTicketTagsComponent extends Component {
  @service dialogs;
  @service session;
  @service store;

  @tracked tags = [];
  @tracked newName = '';
  @tracked newNote = '';
  @tracked currentTag = '--';
  @tracked editingGuid = '';
  @tracked editRecord = null;
  updateSubComponent = null;
  currentTenant = null;

  constructor(...args) {
    super(...args);
    if (
      this.args.updateSubComponent &&
      typeof this.args.updateSubComponent === 'function'
    ) {
      this.updateSubComponent = this.args.updateSubComponent;
    }

    this.currentTenant = this.session.get('data.tenantId');

    this.editRecord = this.initializeRecord();
    this.start.perform();
  }

  // estrae elenco record
  start = task({ drop: true }, async () => {
    this.tags = await this.store.query('ticket-tag', {
      sort: 'name',
      filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
    });
  });

  @action
  setNewNote(value) {
    this.newNote = value;
  }

  @action
  setNewName(event) {
    this.newName = event.target.value;
    if (this.newName.trim() === '') {
      this.currentTag = '--';
    } else {
      this.currentTag = camelize(this.newName.trim());
    }
  }

  // memorizzo nuovo record
  save = task({ drop: true }, async () => {
    try {
      this.newName = this.newName.trim();
      this.newNote = this.newNote.trim();

      if (this.newName === '') {
        this.dialogs.toast(
          'Il nome è obbligatorio!',
          'warning',
          'bottom-right',
          3
        );
        return false;
      }

      let newR = this.store.createRecord('ticket-tag', {
        id: v4(),
        name: this.newName,
        tag: camelize(this.newName.trim()),
        note: this.newNote,
        tenantDestinationId: this.currentTenant,
      });
      await newR.save();

      this.newName = '';
      this.newNote = '';
      this.currentTag = '--';

      //this.start.perform();
      this.updateSubComponent();
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
    this.editRecord.tag = camelize(this.editRecord.name.trim());
    this.editRecord.note = record.note;
    this.editRecord.tenantDestinationId = record.tenantDestinationId;
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
    this.editRecord.tag = camelize(this.editRecord.name.trim());
  }
  @action
  updateNote(value) {
    this.editRecord.note = value;
  }

  @action
  async saveUpdatedRecord() {
    try {
      this.editRecord.name = this.editRecord.name.trim();
      this.editRecord.tag = camelize(this.editRecord.name);
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

      let oldR = await this.store.peekRecord('ticket-tag', this.editingGuid);
      oldR.name = this.editRecord.name;
      oldR.tag = this.editRecord.tag;
      oldR.note = this.editRecord.note;
      await oldR.save();

      this.editingGuid = '';
      this.editRecord = this.initializeRecord();

      this.updateSubComponent();
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
      '<h6>Eliminazione Tag</h6>',
      `<p>Confermi?</p>`,
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
      this.updateSubComponent();
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
