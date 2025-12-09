/**
 * Definisce il comportamento custom dei ticket,
 * specifico e personalizzabile da ciascuna azienda.
 *
 * L'oggetto ticketSetup dell'entità ticket-setup-custom è così configurato:
 * {
 *  canChoiseUnlogged: int, (0 = false, 1 = inserendo la sola email, 2 = inserendo email e p.iva)
 *
 *  canManageProjects: bool,
 *  canUseTag: bool,
 *  canUseToDo: bool,
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

export default class StandardCoreTicketCustomSetupComponent extends Component {
  @service('siteSetup') stp;
  @service dialogs;
  @service session;
  @service store;

  @tracked currentTenant = null;
  @tracked serviceAvailable = 'waiting';
  auth = null;
  @tracked tickedInfo = null;

  @tracked customSetup = null;
  @tracked savedChanges = true;
  @tracked license = null;
  updateSubComponent = null;

  constructor(...attributes) {
    super(...attributes);
    this.currentTenant = this.session.get('data.tenantId');
    this.start();
  }

  @action
  async start() {
    try {
      let auth = {}; // è l'oggetto restituito dall'utility utility/utils-ticket.js
      if (typeof this.args.auth !== 'undefined') {
        auth = this.args.auth;
      }
      this.tickedInfo = new TrackedObject(auth);

      if (
        this.args.updateSubComponent &&
        typeof this.args.updateSubComponent === 'function'
      ) {
        this.updateSubComponent = this.args.updateSubComponent;
      }

      // recupero il record di ticket-custom-setup
      this.customSetup = await this.getTicketCustomSetup();

      this.serviceAvailable = 'available';
    } catch (e) {
      console.error(e);
      this.serviceAvailable = 'unavailable';
    }
  }

  @action
  changeValue(field, event) {
    let val = event.target.value;

    let ticketSetup = { ...this.customSetup.ticketSetup };

    // adatto il valore
    if (['canChoiseUnlogged'].includes(field)) {
      val = parseInt(val);
    } else {
      val = !!val;
    }
    ticketSetup[field] = val;
    this.tickedInfo.custom[field] = val;

    this.customSetup.ticketSetup = ticketSetup;

    if (this.customSetup.hasDirtyAttributes) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }
  }

  save = task({ drop: true }, async () => {
    try {
      // salvo i record
      await this.customSetup.save();

      this.dialogs.toast('Operazione riuscita', 'success', 'bottom-right', 3);
      this.savedChanges = true;

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

  async getTicketCustomSetup() {
    let record = await this.store.queryRecord('ticket-custom-setup', {
      filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
    });

    if (record) {
      return record;
    } else {
      throw new Error();
    }
  }

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord('ticket-custom-setup'); // Crea una nuova istanza del modello
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
