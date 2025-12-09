import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { htmlSafe } from '@ember/template';
import { getOwner } from '@ember/application';

export default class StandardCoreChangeTenantComponent extends Component {
  @service('broadcast-channel') channel;
  @service('siteSetup') stp;
  @service permissions;
  @service dialogs;
  @service session;
  @service store;

  @tracked serviceAvailable = 'waiting';
  @tracked currentTenantId = '';
  @tracked tenants = [];
  @tracked otherTenants = [];
  @tracked numberTenants = 0;
  @tracked notAvailableRaison = '';
  @tracked showRetry = false;
  @tracked filterTenant = '';
  @tracked filterMyTenant = '';

  userTenants = [];

  constructor(...attributes) {
    super(...attributes);

    if (this.session.isAuthenticated) {
      this.userTenants = this.session.get('data.associatedTenants');
      this.numberTenants = this.userTenants.length;

      if (this.permissions.hasPermissions(['canSeeAllTenants'])) {
        this.showMyTenant();
      } else {
        if (this.stp.siteSetup.canChangeTenants) {
          if (this.numberTenants === 0) {
            this.notAvailableRaison = htmlSafe(
              `Non ci sono aziende selezionabili.<br />Contatta l'amministratore dell'azienda per verificare eventuali problemi di Licenza.`
            );
            this.serviceAvailable = 'unavailable';
          } else if (this.numberTenants === 1) {
            this.notAvailableRaison = htmlSafe(
              `L'opzione 'cambio azienda' non è disponibile per il tuo profilo.`
            );
            this.serviceAvailable = 'unavailable';
          } else {
            this.showMyTenant();
          }
        } else {
          this.notAvailableRaison = htmlSafe(
            `L'opzione 'cambio azienda' non è disponibile per questo sito.`
          );
          this.serviceAvailable = 'unavailable';
        }
      }
    }
  }

  @action
  async showMyTenant() {
    try {
      this.showRetry = false;
      this.currentTenantId = this.session.get('data.tenantId').toString();

      let tenantIds = [];
      this.userTenants.forEach((element) => {
        tenantIds.push(`'${element.tenantId}'`);
      });

      await this.store
        .query('tenant', {
          filter: `any(id,${tenantIds.join()})`,
          sort: `name`,
        })
        .then((res) => {
          this.tenants = res;
          this.serviceAvailable = 'available';
        })
        .catch((e) => {
          console.error(e);
          this.notAvailableRaison = htmlSafe(
            `Il servizio non è al momento disponibile.`
          );
          this.showRetry = true;
          this.serviceAvailable = 'unavailable';
        });

      await this.store
        .query('tenant', {
          filter: `not(any(id,${tenantIds.join()}))`,
          sort: `name`,
        })
        .then((res) => {
          this.otherTenants = res;
        })
        .catch((e) => {
          console.error(e);
        });
    } catch (e) {
      console.error(e);
    }
  }

  get otherTenantLength() {
    return this.otherTenants.length;
  }

  get otherTenantFiltered() {
    if (this.filterTenant !== '') {
      return this.otherTenants.filter((item) => {
        return (
          (item.name &&
            item.name !== '' &&
            item.name
              .toLowerCase()
              .includes(this.filterTenant.toLowerCase())) ||
          (item.organization &&
            item.organization !== '' &&
            item.organization
              .toLowerCase()
              .includes(this.filterTenant.toLowerCase()))
        );
      });
    } else {
      return this.otherTenants;
    }
  }
  get myTenantFiltered() {
    if (this.filterMyTenant !== '') {
      return this.tenants.filter((item) => {
        return (
          item.name.toLowerCase().includes(this.filterMyTenant.toLowerCase()) ||
          item.organization
            .toLowerCase()
            .includes(this.filterMyTenant.toLowerCase())
        );
      });
    } else {
      return this.tenants;
    }
  }

  @action
  findTenant(event) {
    this.filterTenant = event.target.value.trim();
  }

  @action
  findMyTenant(event) {
    this.filterMyTenant = event.target.value.trim();
  }

  @action
  confirmChangeTenant(tenantId) {
    if (tenantId === this.currentTenantId) return false;
    this.tenantId = tenantId;
    this.dialogs.confirm(
      '<h6>Cambio azienda</h6>',
      `<p>Confermi il passaggio alla nuova azienda?</p>`,
      () => this.changeTenantConfirmed(tenantId),
      null,
      ['Conferma', 'Annulla']
    );
  }

  async changeTenantConfirmed(tenantId) {
    let owner = getOwner(this);
    let authenticator = owner.lookup('authenticator:jwt');
    try {
      await authenticator.jwtChangeTenant(tenantId);
      this.channel.postMessage('refresh-tenant');
      this.dialogs.toast('Attendere prego...', 'success', 'bottom-right', 2);
      setTimeout(() => {
        window.location.replace('/');
      }, 250);
    } catch (error) {
      console.error(`Change Tenant failed with status: `, error);
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        3
      );
    }
  }
}
