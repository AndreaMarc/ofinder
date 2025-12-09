import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { task } from 'ember-concurrency';
import { v4 } from 'ember-uuid';
import { htmlSafe } from '@ember/template';

export default class StandardCoreExternalIntegrationsComponent extends Component {
  @service integrations;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  @tracked records = [];
  @tracked newRecord = null;
  @tracked totalRecords = 0;
  @tracked editingRecord = null;
  integrationForTest = '';
  @tracked
  integrationResult = '';

  constructor(...attributes) {
    super(...attributes);
    this.newRecord = this.initializeRecord();
    this.start.perform();
  }

  start = task({ drop: true }, async () => {
    try {
      let records = await this.store.query('integration', {
        sort: 'name',
      });

      this.records = records;
      this.totalRecords = records.meta.total;
    } catch (e) {
      console.error(e);
    }
  });

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord('integration'); // Crea una nuova istanza del modello
    // Crea un semplice oggetto JavaScript con tutte le proprietà del modello
    let modelData = {};
    modelInstance.eachAttribute((key) => {
      modelData[key] = modelInstance.get(key);
    });

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }

  @action
  changeNew(field, event) {
    let val = '';
    try {
      val = event.target.value;
    } catch (e) {
      val = event;
    }

    if (field === 'active') val = !!val;

    this.newRecord[field] = val;
  }

  @action
  changeNewActive(val) {
    this.changeNew('active', val);
  }

  saveNew = task({ drop: true }, async () => {
    if (this.newRecord.name === '' || this.newRecord.code === '') {
      this.dialogs.toast(
        'Nome e Hash sono obbligatori!',
        'error',
        'bottom-right',
        3
      );
      return false;
    }

    try {
      let record = this.store.createRecord('integration', {
        id: v4(),
        name: this.newRecord.name,
        code: this.newRecord.code,
        active: this.newRecord.active,
      });
      await record.save();
      this.newRecord = this.initializeRecord();
      this.start.perform();
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore, riprovare!',
        'error',
        'bottom-right',
        3
      );
    }
  });

  // modifica esistenti
  @action
  editRecord(record) {
    this.editingRecord = record;
  }

  @action
  async undoRecord(record) {
    await record.rollbackAttributes();
    this.editingRecord = null;
  }

  @action
  changeOld(field, event) {
    if (!this.editingRecord) return;
    let val = '';
    try {
      val = event.target.value;
    } catch (e) {
      val = event;
    }

    if (field === 'active') val = !!val;

    this.editingRecord[field] = val;
  }

  @action
  changeOldActive(val) {
    this.changeOld('active', val);
  }

  @action
  updateOld(record) {
    this.dialogs.confirm(
      '<h6>Conferma</h6>',
      `<p>Le modifiche avranno effetto immediato.<br />
        Un eventuale errore nei dati comprometterà l'integrazione con l'applicativo.<br />
        Confermi l'aggiornamento del record?</p>`,
      () => {
        this.updateOldConfirmed.perform(record);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  updateOldConfirmed = task({ drop: true }, async (record) => {
    try {
      await record.save();
      this.editingRecord = null;

      this.dialogs.toast('Operazione riuscita', 'success', 'bottom-right', 3);
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore, riprovare!',
        'error',
        'bottom-right',
        3
      );
    }
  });

  @action
  delOld(record) {
    this.dialogs.confirm(
      '<h6 class="text-danger">Conferma</h6>',
      `<p class="text-danger">Il record di integrazione sta per essere cancellato.<br />
        Azione irreversibile.<br />
        Confermi?</p>`,
      () => {
        this.delOldConfirmed.perform(record);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  delOldConfirmed = task({ drop: true }, async (record) => {
    try {
      await record.destroyRecord();
      this.editingRecord = null;

      this.dialogs.toast('Operazione riuscita', 'success', 'bottom-right', 3);
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore, riprovare!',
        'error',
        'bottom-right',
        3
      );
    }
  });

  @action
  saveIntegrationForTest(event) {
    this.integrationResult = '';
    this.integrationForTest = event.target.value;
  }

  testIntegration = task({ drop: true }, async () => {
    this.integrationResult = '';
    if (this.integrationForTest === '') {
      this.dialogs.toast(
        `Scegli l'integrazione!`,
        'warning',
        'bottom-right',
        3
      );
      return false;
    }

    let res = await this.integrations.connect.perform(
      this.integrationForTest,
      this.session.get('data.id'),
      this.fetch,
      this.dialogs,
      this.session,
      this.store
    );

    this.integrationResult = htmlSafe(res);
  });
}
