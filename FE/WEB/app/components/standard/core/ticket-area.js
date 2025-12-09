/**
 * @param {function} updateSubComponent comporta l'aggiornamento di tutti i componenti della pagina Ticket Management
 */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';
import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins';

export default class StandardCoreTicketAreaComponent extends Component {
  @service dialogs;
  @service session;
  @service store;

  @tracked available = 'waiting';
  @tracked areas = [];
  @tracked newName = '';
  @tracked newNote = '';
  @tracked editingGuid = '';
  @tracked editRecord = null;
  updateSubComponent = null;
  currentTenant = null;
  auth = {};

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

  // estrae elenco record
  start = task({ drop: true }, async () => {
    try {
      let areas = await this.store.query('area', {
        sort: 'name',
        filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
        include: 'ticketOperator',
      });
      let areasWithOperatorCount = await Promise.all(
        areas.map(async (area) => {
          let operators = await area.get('ticketOperator');
          let operatorCount = operators ? operators.length : 0;
          // Costruisci manualmente l'oggetto con i campi di cui hai bisogno
          return {
            id: area.id, // Assumendo che tu voglia l'id
            name: area.get('name'), // Accedi direttamente alla proprietà 'name'
            note: area.get('note'), // Accedi direttamente alla proprietà 'note'
            erasable: area.get('erasable'), // Accedi direttamente alla proprietà 'erasable'
            tenantDestinationId: area.get('tenantDestinationId'), // Accedi direttamente alla proprietà 'tenantDestinationId'
            operatorCount, // Aggiungi il conteggio degli operatori
            originalRecord: area,
          };
        })
      );
      this.areas = areasWithOperatorCount;

      this.available = 'available';
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
  });

  @action
  setNewNote(value) {
    this.newNote = value;
  }

  @action
  setNewName(event) {
    this.newName = event.target.value;
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

      let newR = this.store.createRecord('area', {
        id: v4(),
        name: this.newName,
        note: this.newNote,
        tenantDestinationId: this.currentTenant,
      });
      await newR.save();

      this.newName = '';
      this.newNote = '';

      //this.start.perform();
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
  }
  @action
  updateNote(value) {
    this.editRecord.note = value;
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
      `<h6 class="text-danger">Eliminazione Reparto "<em>${record.name}</em>"</h6>`,
      `<p class="text-danger">${
        record.operatorCount > 0
          ? record.operatorCount +
            ' operatori sono assegnati a questo reparto.<br /><br />'
          : ''
      }Confermi l'eliminazione?</p>`,
      () => {
        self.deleteAreaConfirmed(record.originalRecord);
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
