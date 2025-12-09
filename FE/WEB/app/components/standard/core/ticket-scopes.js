import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';
import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins';
import { ticketUtlity } from 'poc-nuovo-fwk/utility/utils-ticket';

export default class StandardCoreTicketScopesComponent extends Component {
  @service('siteSetup') stp;
  @service session;
  @service dialogs;
  @service store;

  @tracked available = 'waiting';
  @tracked areas = [];
  @tracked pertinences = [];
  @tracked pertinencesString = '';
  @tracked operatorsForArea = 0;
  @tracked newRecord = null;
  @tracked editRecord = null;

  @tracked selectedArea = null;
  @tracked selectedPertinence = '';
  @tracked availableAreas = '[]';
  @tracked canChoiseUnlogged = 0;
  @tracked inEditing = false;
  recordInEdit = null;
  areaToAssign = '';
  auth = {};

  tenantOperators = [];
  currentTenant = null;

  constructor(...args) {
    super(...args);
    if (
      this.args.updateSubComponent &&
      typeof this.args.updateSubComponent === 'function'
    ) {
      this.updateSubComponent = this.args.updateSubComponent;
    }

    if (this.args.auth) {
      this.auth = this.args.auth;
    }

    this.currentTenant = this.session.get('data.tenantId');
    this.preload.perform();
  }

  // estrae elenco record
  preload = task({ drop: true }, async () => {
    try {
      this.newRecord = this.initializeRecord('ticket-pertinence');
      this.editRecord = this.initializeRecord('ticket-pertinence');

      let pertinences = await this.store.query('ticket-pertinence', {
        sort: 'name',
        filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
        include: 'ticketPertinenceMappings.area',
      });

      // Carica tutte le ticketPertinenceMappings per ogni ticketPertinence
      await Promise.all(
        pertinences.map(async (pertinence) => {
          await pertinence.get('ticketPertinenceMappings');
        })
      );

      this.pertinences = pertinences;

      this.pertinencesString = JSON.stringify(
        this.pertinences.map((x) => {
          return { id: x.id, value: x.name };
        })
      );

      let areas = await this.store.query('area', {
        sort: 'name',
        filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
        include: 'ticketOperator',
      });
      // Crea un array di promesse che risolvono a true se l'area ha operatori, altrimenti false
      let hasOperatorsPromises = areas.map(async (area) => {
        let operators = await area.get('ticketOperator');
        return operators && operators.length > 0;
      });
      // Attendere la risoluzione di tutte le promesse
      let hasOperators = await Promise.all(hasOperatorsPromises);
      // Filtra le aree basandoti sugli indici di quelle che hanno operatori
      this.areas = areas.filter((_, index) => hasOperators[index]);

      // ricavo impostazioni dei ticket
      let ticketInfo = await ticketUtlity(
        this.store,
        this.stp,
        v4,
        this.currentTenant
      );
      this.canChoiseUnlogged = ticketInfo.canChoiseUnlogged;

      this.available = 'available';
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
  });

  //#region LISTA AMBITI
  // abilita la modifica dell'ambito
  @action
  editPertinence(record) {
    this.recordInEdit = record;

    this.inEditing = true;
    this.editRecord.id = record.id;
    this.editRecord.name = record.name;
    this.editRecord.note = record.note;
    this.editRecord.availableFor = record.availableFor || 1;
  }

  // annulla la modifica dell'ambito
  @action
  uneditPertinence() {
    this.inEditing = false;
    this.editRecord = this.initializeRecord('ticket-pertinence');
  }

  // salva modifiche all'ambito
  @action
  saveEdit() {
    let self = this;
    this.dialogs.confirm(
      '<h6>Salvataggio delle modifiche</h6>',
      `<p>Confermi il salvataggio?</p>`,
      () => {
        self.saveEditConfirmed.perform();
      },
      null,
      ['Sì', 'No']
    );
  }
  saveEditConfirmed = task({ drop: true }, async () => {
    try {
      this.recordInEdit.name = this.editRecord.name;
      this.recordInEdit.note = this.editRecord.note;
      this.recordInEdit.availableFor = this.editRecord.availableFor;

      await this.recordInEdit.save();
      if (this.updateSubComponent) {
        this.updateSubComponent();
      }

      this.uneditPertinence();
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        3
      );
    }
  });

  // aggiorna i valori dell'ambito
  @action
  updatePertinence(event, field) {
    let val = typeof event.target !== 'undefined' ? event.target.value : event;
    if (field === 'availableFor') val = parseInt(val);

    this.editRecord[field] = val;
  }

  @action
  deletePertinence(record) {
    let self = this;
    this.dialogs.confirm(
      '<h6 class="text-danger">Cancellazione Ambito</h6>',
      `<p class="text-danger">Vuoi davvero CANCELLARE l'ambito selezionato?</p>`,
      () => {
        self.deletePertinenceConfirmed.perform(record);
      },
      null,
      ['Sì', 'No']
    );
  }
  deletePertinenceConfirmed = task({ drop: true }, async (record) => {
    try {
      await record.destroyRecord();
      if (this.updateSubComponent) {
        this.updateSubComponent();
      }
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        3
      );
    }
  });

  @action
  deleteAssociation(mappingId) {
    let self = this;
    this.dialogs.confirm(
      '<h6 class="text-danger">Cancellazione Associazione</h6>',
      `<p class="text-danger">Stai rimuovendo il Reparto di pertinenza dall'Ambito. Confermi?</p>`,
      () => {
        self.deleteAssociationConfirmed.perform(mappingId);
      },
      null,
      ['Sì', 'No']
    );
  }
  deleteAssociationConfirmed = task({ drop: true }, async (mappingId) => {
    try {
      let recordAssociation = await this.store.findRecord(
        'ticket-pertinence-mapping',
        mappingId
      );

      if (recordAssociation) {
        await recordAssociation.destroyRecord();
        if (this.updateSubComponent) {
          this.updateSubComponent();
        }
      } else {
        throw new Error('No mapping record found');
      }
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        3
      );
    }
  });
  //#endregion

  //#region CREAZIONE AMBITO
  @action
  setName(event) {
    this.newRecord.name = event.target.value;
  }

  @action
  setUnlogged(event) {
    this.newRecord.availableFor = parseInt(event.target.value);
  }

  @action
  addPertinence() {
    if (this.newRecord.name.trim() === '') {
      this.dialogs.toast(
        `Digitare il nome dell'Ambito!`,
        'warning',
        'bottom-right',
        3
      );
    }
    let self = this;
    this.dialogs.confirm(
      '<h6>Creazione Ambito</h6>',
      `<p>Confermi?</p>`,
      () => {
        self.addPertinenceConfirmed.perform();
      },
      null,
      ['Sì', 'No']
    );
  }
  addPertinenceConfirmed = task({ drop: true }, async () => {
    try {
      let nr = this.store.createRecord('ticket-pertinence', {
        id: v4(),
        tenantDestinationId: this.currentTenant,
        name: this.newRecord.name.trim(),
      });
      await nr.save();
      if (this.updateSubComponent) {
        this.updateSubComponent();
      }
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        3
      );
    }
  });
  //#endregion

  //#region ASSOCIAZIONE PERTINENZA
  @action
  async selectPertinence(value) {
    this.areaToAssign = '';
    this.availableAreas = '[]';
    this.selectedPertinence = value;

    if (this.selectedPertinence !== '') {
      let av = await this.getFreeAreas(this.selectedPertinence);
      this.availableAreas = JSON.stringify(av);

      if (av.length === 0) {
        this.dialogs.toast(
          `Hai già associato tutte le Aree a questo ambito!`,
          'warning',
          'bottom-right',
          5
        );
      }
    }
  }

  @action
  async getFreeAreas(id) {
    let sr = this.pertinences.find((item) => item.id === id);
    if (!sr) {
      console.error('Pertinence not found');
      return [];
    }

    let mappings = await sr.get('ticketPertinenceMappings');

    // mi assicuro che mappings sia un array e che ogni mapping abbia una area caricata
    let includedAreasPromises = mappings.map(
      async (mapping) => await mapping.get('area')
    );
    let includedAreas = await Promise.all(includedAreasPromises);

    // Filtro eventuali valori undefined o null
    includedAreas = includedAreas.filter(Boolean);
    let includedIds = includedAreas.map((area) => area.id);

    let free = this.areas.filter((item) => !includedIds.includes(item.id));
    free = free.map((x) => {
      return { id: x.id, value: x.name };
    });
    return free;
  }

  @action
  async selectAreaToAssign(value) {
    this.areaToAssign = value;
  }

  @action
  saveAssociation() {
    if (this.selectedPertinence === '') {
      this.dialogs.toast(`Seleziona l'Ambito!`, 'warning', 'bottom-right', 3);
      return false;
    }
    if (this.areaToAssign === '') {
      this.dialogs.toast(`Seleziona l'Area!`, 'warning', 'bottom-right', 3);
      return false;
    }

    let self = this;
    this.dialogs.confirm(
      '<h6>Conferma Associazione</h6>',
      `<p>Stai associando una nuova Area all'Ambito selezionato.<br />Confermi?</p>`,
      () => {
        self.saveAssociationConfirmed.perform();
      },
      null,
      ['Sì', 'No']
    );
  }
  saveAssociationConfirmed = task({ drop: true }, async () => {
    try {
      let nr = this.store.createRecord('ticket-pertinence-mapping', {
        id: v4(),
        ticketPertinenceId: this.selectedPertinence,
        areaId: this.areaToAssign,
      });
      await nr.save();
      this.availableAreas = '[]';
      if (this.updateSubComponent) {
        this.updateSubComponent();
      }
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        3
      );
    }
  });
  //#endregion

  initializeRecord(modelName) {
    let modelInstance = this.store.createRecord(modelName); // Crea una nuova istanza del modello (ad esempio, 'user')
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
