import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';

export default class StandardCoreTicketDetailsComponent extends Component {
  @service('siteSetup') stp;
  @service translation;
  @service siteLayout;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  @tracked ticketActive = false;
  @tracked ticketTenantEnabled = false;
  @tracked ticketUnableMessage = '';
  @tracked serviceAvailable = 'waiting';
  @tracked currentLang = this.translation.currentLang;
  @tracked lastRefresh = Date.now();
  @tracked authorization = {};
  currentTenant = 0;

  constructor(...attributes) {
    super(...attributes);
    this.currentTenant = this.session.get('data.tenantId');
    this.start();
  }

  @action
  async start() {
    try {
      // Verifico che, in setup, il servizio di Ticket sia attivo
      this.ticketActive =
        typeof this.stp.siteSetup.ticketService === 'object'
          ? this.stp.siteSetup.ticketService.active
          : false;

      // verifico inoltre se l'azienda corrente è abilitata alla gestione dei Ticket
      let ticketMode =
        typeof this.stp.siteSetup.ticketService === 'object' &&
        typeof this.stp.siteSetup.ticketService.mode !== 'undefined'
          ? this.stp.siteSetup.ticketService.mode
          : 'selected';

      // ricavo le licenze di utilizzo
      let license = await this.store.queryRecord('ticket-license', {
        include: 'tenantDestination',
        //sort: 'tenantDestination.name',
        filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
      });

      this.ticketUnableMessage =
        typeof this.stp.siteSetup.ticketService &&
        typeof this.stp.siteSetup.ticketService.message !== 'undefined' &&
        this.stp.siteSetup.ticketService.message !== ''
          ? this.stp.siteSetup.ticketService.message
          : 'Il servizio di Assistenza Clienti è momentaneamente disattivato. Riprovare in un secondo momento. (cod. 2)';

      if (ticketMode === 'all') {
        this.ticketTenantEnabled = true;
      } else if (ticketMode === 'selected') {
        if (license) {
          let unlimitedService = license.unlimitedService;
          let expirationDate = license.expirationDate;

          if (unlimitedService) {
            this.ticketTenantEnabled = true;
          } else {
            let exp = new Date(expirationDate);
            if (exp > Date.now()) {
              this.ticketTenantEnabled = true;
            }
          }
        }
      }

      // mi ricavo le impostazioni Custom del tenant corrente
      let record = await this.store.queryRecord('ticket-custom-setup', {
        filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
      });

      if (!record && !record.ticketSetup) {
        record = {
          ticketSetup: {
            canChoiseUnlogged: false,
            canManageProjects: false,
            canUseTag: false,
          },
        };
      }

      this.authorization = {
        canChoiseUnlogged: license.canChoiseUnlogged
          ? !!record.ticketSetup.canChoiseUnlogged
          : false,
        canManageProjects: license.canManageProjects
          ? !!record.ticketSetup.canManageProjects
          : false,
        canUseTag: license.canUseTag ? !!record.ticketSetup.canUseTag : false,
        canUseToDo: license.canUseToDo
          ? !!record.ticketSetup.canUseToDo
          : false,
        multiArea: license.multiArea,
        canUseTracker: license.canUseTracker,
      };

      this.lastRefresh = Date.now();
      this.serviceAvailable = 'available';
    } catch (e) {
      console.error(e);
      this.serviceAvailable = 'unavailable';
    }
  }

  @action
  async updateSubComponent() {
    await this.start();
    this.lastRefresh = Date.now();
  }

  get getBgColor() {
    let iconBg = '';
    if (this.args.iconBg && this.args.iconBg !== '') {
      iconBg = `${this.args.icon} icon-gradient ${this.args.iconBg}`;
    } else {
      iconBg = `${this.args.icon} icon-gradient ${this.siteLayout.headerBackground}`;
    }
    return htmlSafe(iconBg);
  }
}
