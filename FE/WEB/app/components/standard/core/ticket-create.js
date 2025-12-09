/**
 * COMPONENRE PER LA CREAZIONE DEI TICKET
 * *
 * @param {string} @classUnlogged : classe di sfondo del componente, per utenti sloggati. Es: 'bg-transparent text-white'
 * @param {string} @classLogged : classe di sfondo del componente, per utenti loggati
 * @param {string} @classTab : classe dei pulsanti della Tab. Default 'btn-primary'.
 * @param {integer} @tenantId : id del Tenant a cui inviare il ticket
 */
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import { task } from 'ember-concurrency';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';
import { TrackedObject } from 'tracked-built-ins';
import { ticketUtlity } from 'poc-nuovo-fwk/utility/utils-ticket';

export default class StandardCoreTicketCreateComponent extends Component {
  @service('siteSetup') stp;
  @service translation;
  @service jsUtility;
  @service dialogs;
  @service session;
  @service store;
  @service fetch;

  @tracked serviceAvailable = 'waiting';
  @tracked tenantInfo = null;
  @tracked unlogged = true;
  @tracked error = '';
  @tracked classUnlogged = '';
  @tracked classLogged = '';
  @tracked newRecord = null; // usato in creazione nuovo record
  @tracked myTickets = [];
  @tracked customSetup = null;
  @tracked canChoiseUnlogged = 0; // indicherà l'opzione scelta dal Tenant in merito ai ticket per sloggati
  @tracked userTenants = []; // tutti i tenant a cui l'utente corrente è associato
  @tracked sel2Tenants = ''; // elenco dei tenant a cui l'utente è associato (per select2)
  ticketActive = false;
  tenantId = null;

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    try {
      this.serviceAvailable = 'waiting';

      // verifio che sia definito il Tenant di destinazione dei Ticket
      if (
        this.args.tenantId &&
        this.args.tenantId.toString().trim() !== '' &&
        !isNaN(this.args.tenantId)
      ) {
        this.tenantId = parseInt(this.args.tenantId);
      } else {
        this.error = `Errore di configurazione: non é stato specificato il Tenant a cui inviare i Ticket.`;
        this.serviceAvailable = 'unavailable';
        return false;
      }

      // ricavo info sul tenant di destinazione
      this.tenantInfo = await this.store.findRecord('tenant', this.tenantId);

      // ricavo impostazioni dei ticket
      let ticketInfo = await ticketUtlity(
        this.store,
        this.stp,
        v4,
        this.tenantId
      );

      this.ticketActive = ticketInfo.ticketActive;

      if (this.ticketActive) {
        // verifico se il tenant corrente può gestire ticket (licenza attiva non scaduta o licenza senza scadenza)
        let ticketTenantEnabled = ticketInfo.ticketTenantEnabled;

        if (!ticketTenantEnabled) {
          // licenza scaduta;
          this.error = ticketInfo.ticketUnableMessage; // messaggio per l'eventualità di "licenza scaduta"
          this.serviceAvailable = 'unavailable';
          return false;
        }

        this.classUnlogged =
          typeof this.args.classUnlogged !== 'undefined'
            ? this.args.classUnlogged
            : '';

        this.classLogged =
          typeof this.args.classLogged !== 'undefined'
            ? this.args.classLogged
            : '';

        this.classTab =
          typeof this.args.classTab !== 'undefined'
            ? this.args.classTab
            : 'btn-primary';

        if (this.session.isAuthenticated) {
          this.unlogged = false;
          this.newRecord = this.initializeRecord();

          // popolo record di nuovo ticket con informazioni sul tenant e su utente corrente
          await this.findTenantbyId.perform();

          // estrapolo eventuali altri ticket dell'utente corrente
          await this.findMyTask.perform();
        }

        this.canChoiseUnlogged = ticketInfo.canChoiseUnlogged;

        if (this.canChoiseUnlogged === 0 && this.unlogged) {
          this.error = `Solo gli utenti loggati possono inviare Ticket!`;
          this.serviceAvailable = 'unavailable';
          return false;
        }

        // TUTTO OK
        this.serviceAvailable = 'available';
      } else {
        this.error = `Il servizio Ticket non è attivo.`;
        this.serviceAvailable = 'unavailable';
      }
    } catch (e) {
      this.error = `Si é verificato un errore.`;
      this.serviceAvailable = 'unavailable';
      console.error(e);
    }
  }

  async getTicketCustomSetup() {
    try {
      let record = await this.store.queryRecord('ticket-custom-setup', {
        filter: `equals(tenantDestinationId,'${this.tenantId}')`,
      });

      if (record) {
        return record;
      } else {
        throw new Error();
      }
    } catch (e) {
      return this.createRecord();
    }
  }

  async createRecord() {
    try {
      let nr = await this.store.createRecord('ticket-custom-setup', {
        id: v4(),
        tenantDestinationId: this.tenantId,
        ticketSetup: {},
      });
      await nr.save();
      return nr;
    } catch (e) {
      console.error(e, 'Errore creazione del record di Ticket Custom Setup');
      return {};
    }
  }

  // popolo record di nuovo ticket con informazioni sul tenant e su utente corrente
  findTenantbyId = task({ drop: true }, async () => {
    try {
      // ricavo tutti i tenant a cui l'utente è assocato, compresi quelli dove
      // l'asspociazione fosse ancora pending
      this.userTenants = await this.store.query('user-tenant', {
        filter: `and(equals(userId,'${this.session.get(
          'data.id'
        )}'),not(equals(state,'denied')),not(equals(tenant.isRecovery,'true')))`,
        include: `tenant,user.userProfile`,
        sort: `tenant.name,tenant.organization`,
      });

      let user = await this.userTenants[0].user;
      let userProfile = await user.userProfile;
      if (user && userProfile) {
        this.newRecord.email = user.email;
        this.newRecord.phone = userProfile.mobilePhone
          ? userProfile.mobilePhone
          : '' + userProfile.fixedPhone
          ? ' ' + userProfile.fixedPhone
          : '';
        this.newRecord.lastName = userProfile.lastName;
        this.newRecord.firstName = userProfile.firstName;

        if (this.userTenants.length === 1) {
          // appartiene a un solo tenant, pre-popolo il valore 'tenantId' del nuovo ticket
          this.newRecord.tenantId = this.userTenants[0].tenantId;
        } else if (this.userTenants.length > 1) {
          // appartiene a più tenant, preparo array per select2
          let arr = [];
          this.userTenants.forEach((element) => {
            arr.push({
              id: element.tenantId,
              value: element.tenant.get('name'),
            });
            console.warn(arr);
          });
          this.sel2Tenants = JSON.stringify([]);
        }
      }
    } catch (e) {
      console.error(e);
      throw new Error('Error while finding tenant by ID');
    }
  });

  // estraggo dal db i miei task
  findMyTask = task({ drop: true }, async () => {
    try {
      let filter = '';
      if (this.filterByUser) {
        filter = `and(equals(userId,'${this.session.get(
          'data.id'
        )}'),equals(tenantId,'${this.session.get(
          'data.tenantId'
        )}'),not(equals(status,'trashed')))`;
      } else {
        filter = `and(equals(tenantId,'${this.session.get(
          'data.tenantId'
        )}'),not(equals(status,'trashed')))`;
      }
      this.myTickets = await this.store.query('ticket', {
        filter: filter,
        sort: `-creationDate`,
        include: `user,user.userProfile`,
      });
    } catch (e) {
      console.error(e);
    }
  });

  @action
  changeValue() {}

  // Verifico che, in setup, il servizio di Ticket sia attivo
  /*
  this.ticketActive =
  typeof this.stp.siteSetup.ticketService === 'object'
    ? this.stp.siteSetup.ticketService.active
    : false;
*/

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
