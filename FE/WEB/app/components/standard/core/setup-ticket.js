/**
 * La voce ticketService del record di setup è così strutturata:
 * {
 *  active: bool,     // indica se il servizio di Ticket è attivo
 *  unloggedTicket: bool, // indica se gli utenti anonimi possono inviare ticket
 *  mode: string, (possibili valori: 'all', 'selected') // all = tutte le aziende usufruiscono del servizio ticket. "selected" = solo le aziende selezionate hanno il servizio ticket attivo
 *  message: string, // messaggio visualizzato quando una licenza è scaduta
 *  tenants: [
 *    {
 *      tenantId: string,
 *      tenantName: string,
 *      illimited: bool,
        expiration: date,
 *    }
 *  ]
 * }
 *
 *
 * Per le licenze di utilizzo, nella tabella ticket-license, abbiamo il seguente model:
 * {
 *  id,
 *  tenantDestinationId,
 *  activationDate,
 *  unlimitedService,
 *  expirationDate,
 *
 *  canChoiseUnlogged,
 *  canManageProjects,
 *  canUseTag,
 *  multiArea,
 *  canUseToDo,
 *  canUseLeader
 *  canRelateTicket,
 *  canUseRoles,
 *  canUseWorkflow,
 *  canUseScopes,
 *  canUseSla,
 *  canUseTracker,
 * }
 */

import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';
import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { ticketUtlity } from 'poc-nuovo-fwk/utility/utils-ticket';

export default class StandardCoreSetupTicketComponent extends Component {
  @service translation;
  @service siteSetup;
  @service dialogs;
  @service store;

  @tracked recordApp;
  @tracked recordWeb;
  @tracked tenants = '[]';
  @tracked license = [];
  originalTenants = [];
  @tracked serviceAvailable = 'waiting';
  @tracked tickedInstalled = true;
  @tracked ticketServiceWeb = {};
  @tracked ticketServiceApp = {};
  @tracked savedChanges = true;

  @tracked newRecord = null;

  @tracked editingTenantId = '';
  @tracked updateFunction = null;
  @tracked currentLang = this.translation.currentLang;
  @tracked hourFormat = JSON.stringify({
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  });

  // Colonne della licenza da mostrare
  @tracked fields = {
    unlimitedService: {
      type: 'checkbox',
      label: 'Illimitato',
      enableByAddons: true,
    },
    expirationDate: { type: 'date', label: 'Scadenza', enableByAddons: true },
    canChoiseUnlogged: {
      type: 'checkbox',
      label: 'Anonimi abilitati',
      enableByAddons: true,
    },
    canManageProjects: {
      type: 'checkbox',
      label: 'Progetti abilitati',
      enableByAddons: false, // DEVO VERIFICARE SE L'ADDON E' INSTALLATO
    },
    canUseTag: {
      type: 'checkbox',
      label: 'Tag abilitati',
      enableByAddons: true,
    },
    multiArea: {
      type: 'checkbox',
      label: 'Multi Area (Reparti)',
      enableByAddons: true,
    },
    canUseScopes: {
      type: 'checkbox',
      label: 'Ambiti abillitati',
      enableByAddons: true,
    },
    canUseTracker: {
      type: 'checkbox',
      label: 'Follower abillitati',
      enableByAddons: true,
    },
    canUseToDo: {
      type: 'checkbox',
      label: 'ToDo abilitati',
      enableByAddons: false, // DEVO VERIFICARE SE L'ADDON E' INSTALLATO
    },
    canUseLeader: {
      type: 'checkbox',
      label: 'Leader abilitati',
      enableByAddons: true,
    },
    canRelateTicket: {
      type: 'checkbox',
      label: 'Ticket relazionabili',
      enableByAddons: true,
    },
    canUseRoles: {
      type: 'checkbox',
      label: 'Ruoli custom',
      enableByAddons: true,
    },
    canUseSla: {
      type: 'checkbox',
      label: 'SLA abilitate',
      enableByAddons: true,
    },
    canUseWorkflow: {
      type: 'checkbox',
      label: 'Workflow abilitato',
      enableByAddons: false, // DEVO VERIFICARE SE L'ADDON E' INSTALLATO
    },
  };

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    this.newRecord = this.initializeRecord('ticket-license');

    // mi ricavo l'elenco degli addons installati
    let addons = await this.store.findAll('fwk-addon');

    let ticketExists =
      addons.filter((item) => item.addonsCode === 1).length > 0;

    if (!ticketExists) {
      this.tickedInstalled = false;
      return false;
    }

    let toDoExists = addons.filter((item) => item.addonsCode === 2).length > 0;
    let projectExists =
      addons.filter((item) => item.addonsCode === 3).length > 0;
    let workflowExists =
      addons.filter((item) => item.addonsCode === 3).length > 0;

    this.fields.canUseToDo.enableByAddons = toDoExists;
    this.fields.canManageProjects.enableByAddons = projectExists;
    this.fields.canUseWorkflow.enableByAddons = workflowExists;

    // recupero tutti i tenant attivi
    let tenants = await this.store.query('tenant', {
      filter: `equals(enabled,'true')`,
      sort: 'name',
    });

    this.originalTenants = tenants.map((item) => {
      return {
        id: item.id,
        value: `${item.name} ${item.vat ? '- ' + item.vat : ''}`,
      };
    });

    // recupero i record di setup
    this.recordWeb = this.args.recordWeb;
    this.recordApp = this.args.recordApp;

    this.ticketServiceWeb =
      typeof this.recordWeb.ticketService === 'undefined' ||
      this.recordWeb.ticketService === ''
        ? {}
        : this.recordWeb.ticketService;

    this.ticketServiceApp =
      typeof this.recordApp.ticketService === 'undefined' ||
      this.recordApp.ticketService === ''
        ? {}
        : this.recordApp.ticketService;

    this.serviceAvailable = this.args.serviceAvailable;

    if (
      this.args.updateFunction &&
      typeof this.args.updateFunction === 'function'
    ) {
      this.updateFunction = this.args.updateFunction;
    }

    // ricavo le licenze di utilizzo
    let license = await this.store.query('ticket-license', {
      include: 'tenantDestination',
      sort: 'tenantDestination.name',
    });

    // elimino dalla select di aggiunta licenza, le Aziende già aggiunte
    if (license && license.length > 0) {
      let t = this.originalTenants.filter(
        (tenant) =>
          !license.some(
            (lic) => lic.tenantDestinationId.toString() === tenant.id.toString()
          )
      );

      this.tenants = JSON.stringify(t);
    } else {
      this.tenants = JSON.stringify(this.originalTenants);
    }

    this.license = license;
  }

  // modifico i campi di setup dei ticket
  @action
  changeValue(field, isWeb, event) {
    let val = event.target.value;
    if (['active', 'unloggedTicket'].includes(field)) {
      val = !!val;
    }

    let newTicketServiceWeb = { ...this.ticketServiceWeb };
    let newTicketServiceApp = { ...this.ticketServiceApp };

    if (isWeb === 'both') {
      newTicketServiceWeb[field] = val;
      newTicketServiceApp[field] = val;
    } else if (isWeb) {
      newTicketServiceWeb[field] = val;
    } else {
      newTicketServiceApp[field] = val;
    }

    this.ticketServiceWeb = newTicketServiceWeb;
    this.ticketServiceApp = newTicketServiceApp;

    this.recordApp.ticketService = this.ticketServiceApp;
    this.recordWeb.ticketService = this.ticketServiceWeb;

    if (
      this.recordApp.hasDirtyAttributes ||
      this.recordWeb.hasDirtyAttributes
    ) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }
  }

  @action
  setMessage(message) {
    this.changeValue('message', 'both', { target: { value: message } });
  }

  // memorizzo i campi di setup dei ticket
  save = task({ drop: true }, async () => {
    try {
      // salvo i record
      await this.recordApp.save();
      await this.recordWeb.save();

      // aggiorno lo stato del sito
      let origin =
        typeof window.cordova !== 'undefined' ? this.recordApp : this.recordWeb;

      this.siteSetup.setSetup('ticketService', origin.ticketService);

      this.dialogs.toast('Operazione riuscita', 'success', 'bottom-right', 3);
      this.savedChanges = true;

      this.start();
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
  setNewRecordTenantId(val) {
    if (val !== '') {
      this.newRecord.tenantDestinationId = parseInt(val);
    } else {
      this.newRecord.tenantDestinationId = 0;
    }
  }

  @action
  changeNewRecordValues(field, event) {
    let val;

    if (
      [
        'unlimitedService',
        'canChoiseUnlogged',
        'canManageProjects',
        'canUseTag',
        'multiArea',
        'canUseToDo',
        'canUseLeader',
        'canRelateTicket',
        'canUseRoles',
        'canUseWorkflow',
        'canUseSla',
        'canUseScopes',
      ].includes(field)
    ) {
      val = !!event.target.checked;
    } else {
      val = event.target.value;
    }

    console.warn(field, event);
    // verifico compatibilità e dipendenze

    if (field === 'unlimitedService' && val) {
      this.newRecord.expirationDate = null;
    } else if (field === 'expirationDate' && val !== '') {
      this.newRecord.unlimitedService = false;

      let expiration = new Date(val);

      // imposto la scadenza alle 0:00 del giorno successivo, cioè alla fine del giorno corrente
      expiration.setHours(23, 59, 59, 99);

      var expiration_utc = Date.UTC(
        expiration.getUTCFullYear(),
        expiration.getUTCMonth(),
        expiration.getUTCDate(),
        expiration.getUTCHours(),
        expiration.getUTCMinutes(),
        expiration.getUTCSeconds()
      );

      val = new Date(expiration_utc).toISOString();
    }

    this.newRecord[field] = val;
  }

  // creo una nuova licenza
  @action
  newTenant() {
    let self = this;

    if (this.newRecord.tenantDestinationId === 0) {
      this.dialogs.toast(`Seleziona l'azienda!`, 'warning', 'bottom-right', 3);
      return false;
    }

    if (
      !this.newRecord.unlimitedService &&
      this.newRecord.activationDate === ''
    ) {
      this.dialogs.toast(
        `Selezionare la data di scadenza del servizio!`,
        'warning',
        'bottom-right',
        4
      );
      return false;
    }

    this.dialogs.confirm(
      '<h4>CREAZIONE LICENZA</h4>',
      `<p>Confermi i dati inseriti?</p>`,
      () => {
        self.addTenant.perform();
      },
      null,
      ['Sì', 'No']
    );
  }
  addTenant = task({ drop: true }, async () => {
    let nr = this.store.createRecord('ticket-license');
    nr.id = v4();
    Object.keys(this.newRecord).forEach((key) => {
      nr[key] = this.newRecord[key];
    });

    await nr.save();

    if (typeof this.updateFunction !== 'undefined') {
      this.updateFunction();
    }
  });

  // aggiorno i valori delle licenze
  @action
  updateTenant(field, record, event) {
    try {
      let val;
      if (
        [
          'unlimitedService',
          'canChoiseUnlogged',
          'canManageProjects',
          'canUseTag',
          'multiArea',
          'canUseToDo',
          'canUseLeader',
          'canRelateTicket',
          'canUseRoles',
          'canUseTracker',
          'canUseWorkflow',
          'canUseSla',
          'canUseScopes',
        ].includes(field)
      ) {
        val = !!event.target.checked;
      } else {
        val = event.target.value;
      }

      if (field === 'unlimitedService' && val) {
        record.expirationDate = null;
      } else if (field === 'expirationDate' && val !== '') {
        record.unlimitedService = false;
        let expiration = new Date(val);

        // imposto la scadenza alle 0:00 del giorno successivo, cioè alla fine del giorno corente
        expiration.setHours(23, 59, 59, 99);

        var expiration_utc = Date.UTC(
          expiration.getUTCFullYear(),
          expiration.getUTCMonth(),
          expiration.getUTCDate(),
          expiration.getUTCHours(),
          expiration.getUTCMinutes(),
          expiration.getUTCSeconds()
        );

        val = new Date(expiration_utc).toISOString();
      }

      record[field] = val;
    } catch (e) {
      console.error(e);
    }
  }

  // elimino una licenza
  @action
  deleteLicense(record) {
    let self = this;
    this.dialogs.confirm(
      '<h4 class="text-danger">Disattivazione Servizio Ticket</h4>',
      `<p class="text-danger">L'azienda non avrà più accesso al servizio Ticket.<br />Perderai ogni informazione relativa alla Licenza del Cliente.<br /><br />AZIONE IRREVERSIBILE.<br /><br />CONFERMI?</p>`,
      () => {
        self.deleteLicenseConfirmed.perform(record);
      },
      null,
      ['Sì', 'No']
    );
  }
  deleteLicenseConfirmed = task({ drop: true }, async (record) => {
    try {
      await record.destroyRecord();

      this.dialogs.toast(`Operazione riuscita!`, 'success', 'bottom-right', 3);
      this.start();
    } catch (e) {
      console.error(e);
    }
  });

  // abilito modifica licanza
  @action
  editRecord(recordId) {
    this.editingTenantId = recordId;
  }

  // annullo modifica licenza
  @action
  undoRecord(record) {
    record.rollbackAttributes();
    this.editingTenantId = '';
    this.start();
  }

  // memorizzo cambiamenti alla licenza
  @action
  saveLicense(record) {
    if (record.tenantDestinationId === 0) {
      this.dialogs.toast(
        `Si è verificato un errore!`,
        'error',
        'bottom-right',
        3
      );
      return false;
    }

    if (
      !record.unlimitedService &&
      (!record.expirationDate || record.expirationDate === '')
    ) {
      this.dialogs.toast(
        `Selezionare la data di scadenza del servizio!`,
        'warning',
        'bottom-right',
        4
      );
      return false;
    }

    this.dialogs.confirm(
      '<h4>MODIFICA DELLA LICENZA</h4>',
      `<p>ATTENZIONE:<br />Stai modificando i parametri della licenza.<br />Azione irreversibile.<br /><br />Confermi i dati inseriti?</p>`,
      () => {
        this.saveLicenseConfirmed.perform(record);
      },
      null,
      ['Sì', 'No']
    );
  }
  saveLicenseConfirmed = task({ drop: true }, async (record) => {
    try {
      await record.save();
      this.editingTenantId = '';
      this.start();

      this.dialogs.toast(`Operazione riuscita!`, 'success', 'bottom-right', 3);
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        4
      );
    }
  });

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord(entity) {
    let modelInstance = this.store.createRecord(entity); // Crea una nuova istanza del modello (ad esempio, 'user')
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
