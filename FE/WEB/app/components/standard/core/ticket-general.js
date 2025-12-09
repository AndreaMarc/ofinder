import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { ticketUtlity } from 'poc-nuovo-fwk/utility/utils-ticket';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { v4 } from 'ember-uuid';
import { task } from 'ember-concurrency';

export default class StandardCoreTicketGeneralComponent extends Component {
  @service('siteSetup') stp;
  @service translation;
  @service permissions;
  @service siteLayout;
  @service session;
  @service dialogs;
  @service router;
  @service store;
  @service fetch;

  @tracked ticketInfo = {}; // tracket object che contiene la risposta di ticketUtlity (parametri di funzionamento globali e custom dei ticket)

  @tracked error = '';
  @tracked ticketTenantEnabled = false; // indica se per questo tenant i ticket sono attivi (licenza in corso o illimitata)
  @tracked ticketUnableMessage = '';
  @tracked serviceAvailable = 'waiting';
  @tracked currentLang = this.translation.currentLang;
  @tracked activeTab = 'ticket-management';
  @tracked activeTabName = htmlSafe('<i class="fa fa-tasks mr-2"></i> TICKET');
  // VOCI DI MENU'
  // Deve contenere un numero di oggetti che sia multiplo di 3,
  // aggiungere oggetti vuoti se necessario.
  tabList = [
    {
      id: 'ticket-management',
      label: 'Ticket',
      activeLicense: true,
      activeCustom: true,
      icon: 'fa fa-tasks',
      alarm: false,
    },
    {
      id: 'ticket-areas',
      label: 'Reparti',
      activeLicense: true,
      activeCustom: true,
      icon: 'fa fa-th-large',
      alarm: false,
    },
    {
      id: 'ticket-operators',
      label: 'Operatori',
      activeLicense: true,
      activeCustom: true,
      icon: 'fa fa-users',
      alarm: false,
    },
    {
      id: 'ticket-scopes',
      label: 'Ambiti',
      activeLicense: true,
      activeCustom: true,
      icon: 'fa fa-list',
      alarm: false,
    },
    {
      id: 'ticket-tag',
      label: 'Tags',
      activeLicense: true,
      activeCustom: true,
      icon: 'fa fa-tags',
      alarm: false,
    },
    {
      id: 'ticket-sla',
      label: 'S.L.A.',
      activeLicense: true,
      activeCustom: true,
      icon: 'fa fa-hourglass-half',
      alarm: false,
    },
    {
      id: 'ticket-roles',
      label: 'Ruoli',
      activeLicense: true,
      activeCustom: true,
      icon: 'fa fa-graduation-cap',
      alarm: false,
    },
    {
      id: 'ticket-tracker',
      label: 'Workflow',
      activeLicense: true,
      activeCustom: true,
      icon: 'fa fa-recycle',
      alarm: false,
    },
    {
      id: 'ticket-setup',
      label: 'Impostazioni',
      activeLicense: true,
      activeCustom: true,
      icon: 'fa fa-cogs',
      alarm: false,
    },
    {},
  ];
  @tracked tabListTraked = new TrackedObject(this.tabList);
  @tracked lastRefresh = Date.now(); // condiviso con sotto-componenti tramite did-update.
  @tracked authorization = {};
  @tracked ticketMode = 'all'; // indica se tutte le aziende del sito possono gestire Ticket o se occorrono licenze
  @tracked unlimitedService = true;
  @tracked expirationDate = null;
  @tracked hourFormat = JSON.stringify({
    year: 'numeric',
    month: 'long',
    day: '2-digit',
  });
  currentTenant = 0;

  constructor(...attributes) {
    super(...attributes);
    this.currentTenant = this.session.get('data.tenantId');
    this.start.perform();
  }

  start = task({ drop: true }, async () => {
    try {
      this.serviceAvailable = 'waiting';

      // Ricavo tutti i parametri di funzionamento dei ticket (globali e custom)
      let ti = await ticketUtlity(this.store, this.stp, v4, this.currentTenant);
      this.ticketInfo = new TrackedObject(ti);

      if (!this.ticketInfo.settings.ticketActive) {
        // tickectActive indica se in setup il servizio ticket è attivo
        this.error = `Il servizio Ticket non è disponibile.`;
        this.serviceAvailable = 'unavailable';
        return false;
      }

      if (!this.ticketInfo.settings.ticketActive) {
        // tickectActive indica se in setup il servizio ticket è attivo
        this.error = `Il servizio Ticket non è attivo. Contattare gli amministratori di sistema.`;
        this.serviceAvailable = 'unavailable';
        return false;
      }

      this.ticketMode = this.ticketInfo.settings.ticketMode; // "all"/"selected" modalità di funzionamento globale dei ticket

      this.ticketUnableMessage = this.ticketInfo.settings.ticketUnableMessage; // messaggio per l'eventualità di "licenza scaduta"

      this.unlimitedService = this.ticketInfo.license.unlimitedService;
      this.expirationDate = this.ticketInfo.license.expirationDate;

      if (this.ticketMode === 'all') {
        this.ticketTenantEnabled = true;
      } else if (this.ticketMode === 'selected') {
        if (this.unlimitedService) {
          this.ticketTenantEnabled = true;
        } else {
          let exp = new Date(this.expirationDate);
          let now = Date.now();
          if (exp > now) {
            this.ticketTenantEnabled = true;
          }

          // coloro l'avviso di scadenza in base al tempo rimanente
          this.alertClass = '';
          if (exp.getTime() - now <= 604800000) {
            // 7 giorni
            this.alertClass = 'text-danger';
          } else if (exp.getTime() - now <= 2592000000) {
            // un mese
            this.alertClass = 'text-warning bg-secondary p-1 pl-2 pr-2';
          } else if (exp.getTime() - now <= 7776000000) {
            // 3 mesi
            this.alertClass = 'text-alternate';
          }
        }
      }

      // MOSTRO ALLARMI DI INCOMPLETEZZA NEL MENU' DEI TICKET

      // multi-area (reparti)
      let targetObject = this.tabList.find((obj) => obj.id === 'ticket-areas');
      if (targetObject) {
        if (ti.license.multiArea) {
          if (ti.custom.multiArea) {
            if (ti.options.areas === 0) {
              // non ci sono aree, notifico incompletezza
              targetObject.alarm = true;
            } else {
              targetObject.alarm = false;
            }
          } else {
            targetObject.activeCustom = false;
            targetObject.alarm = false;
          }
        } else {
          targetObject.activeLicense = false;
          targetObject.alarm = false;
        }
      }

      // operatori
      targetObject = this.tabList.find((obj) => obj.id === 'ticket-operators');
      if (targetObject) {
        if (ti.options.operators === 0) {
          targetObject.alarm = true; // non ci sono operatori, notifico incompletezza
        } else {
          targetObject.alarm = false;
        }
      }

      this.tabListTraked = new TrackedObject(this.tabList);
      this.lastRefresh = Date.now();
      this.serviceAvailable = 'available';

      // verifico se in query string è passato il parametro "dest" per specificare la Tab da visualizzare
      let currentUrl = window.location.href;
      let url = new URL(currentUrl);
      let params = new URLSearchParams(url.search);
      let dest = params.get('dest');
      let existsDest = this.tabList.filter((item) => item.id === dest);
      if (existsDest.length > 0) {
        let destValue = null;
        if (params.has('dest')) {
          destValue = dest;

          this.changeTab(destValue, existsDest[0].label, existsDest[0].icon);
        }
      } else console.log('dest inesistente!');
    } catch (e) {
      console.error(e);
      this.error = 'Si é verificato un errore di inizializzazione.';
      this.serviceAvailable = 'unavailable';
    }
  });

  // Azione per cambio Tab
  @action
  changeTab(tabId, tabLabel, tabIcon) {
    this.activeTab = tabId;
    this.activeTabName = htmlSafe(
      `<i class="fa ${tabIcon} mr-2"></i> ${
        tabLabel ? tabLabel.toUpperCase() : ''
      }`
    );

    let currentRoute = this.router.currentRouteName;

    this.router.transitionTo(currentRoute, {
      queryParams: {
        ['dest']: tabId,
      },
    });
  }

  @action
  changeTabUnabled(activeLicense, activeCustom, label) {
    let mess = '',
      mess2 = '',
      icon = '';

    if (this.permissions.hasPermissions(['isTicketManagement'])) {
      mess = `La Sua Licenza non prevede l'opzione "${label}".<br /><br />
        Esapenda la licenza per usufruire di tutte le opzioni disponibili.`;
      mess2 = `L'opzione "${label}" è prevista dalla tua Licenza ma non è stata attivata.<br/><br />
          <small>Premi il pulsante "<em>Risolvi</em>" per modificare le impostazioni di funzionmento.</small>`;
      icon = 'info';
    } else {
      mess = `L'opzione "${label}" al momento non è attiva.`;
      mess2 = mess;
      icon = 'warning';
    }
    // eslint-disable-next-line no-undef
    Swal.fire({
      title: activeLicense ? `Opzione non attivata!` : 'Licenza non attiva!',
      html: mess2,
      icon: icon,
      showCancelButton: true,
      showConfirmButton: true,
      cancelButtonText: `Ignora`,
      confirmButtonText: `RISOLVI`,
      customClass: {
        confirmButton: 'btn btn-outline-primary btn-hover-shine ml-2',
        cancelButton: 'btn btn-outline-secondary btn-hover-shine',
      },
      buttonsStyling: false,
      reverseButtons: true,
    }).then((result) => {
      if (result.isConfirmed) {
        console.log(result);
        if (!activeLicense) {
          this.router.transitionTo('ticket-license');
        } else {
          this.activeTab = 'ticket-setup';
          this.activeTabName = htmlSafe(
            `<i class="fa fa-cogs mr-2"></i> IMPOSTAZIONI`
          );
        }
      }
    });
  }

  // Azione per cambio tab che comporta una nuova inizializzazione e il refresh dei sotto-componenti
  @action
  async updateSubComponent() {
    await this.start.perform();
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

  get getDropdownBg() {
    return this.siteLayout.headerBackground;
  }
  get getBtnBg() {
    let bgColor = this.siteLayout.headerBackground;
    return bgColor.replace('bg-', 'btn-');
  }

  get getColorOnly() {
    let bgColor = this.siteLayout.headerBackground;
    return bgColor.replace('bg-', '');
  }

  get alarmExists() {
    return this.tabListTraked.filter((item) => item.alarm).length > 0;
  }
}
