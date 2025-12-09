/* eslint-disable no-undef */
import Controller from '@ember/controller';
import ENV from 'poc-nuovo-fwk/config/environment';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';

export default class AuthenticatedController extends Controller {
  @service('siteSetup') stp;
  @service permissions;
  @service siteLayout;
  @service session;
  @service dialogs;
  @service router;

  @tracked tenantName = '';
  @tracked tenantNumber = 0;

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  start() {
    if (
      this.canChangeTenant ||
      this.permissions.hasPermissions(['canSeeAllTenants'])
    ) {
      // componente visibile solo se in Setup ho abilitato l'opzione di Cambio Tenant (o se ho il permesso di cambiare Tenant)
      let tenants = this.session.get('data.associatedTenants');
      this.tenantNumber = tenants.length;

      if (tenants.length > 1) {
        // ricavo il nome del tenant corrente
        let currentTenant = tenants.filter((item) => {
          return item.tenantId === this.session.get('data.tenantId');
        });
        this.tenantName = `${currentTenant[0].name}`;
      }
    }
  }

  get canChangeTenant() {
    return this.stp.siteSetup.canChangeTenants;
  }

  get isApp() {
    return typeof window.cordova !== 'undefined';
  }

  get environment() {
    // Controlla se l'ambiente è "development"
    return ENV.environment;
  }

  get appVersion() {
    try {
      if (this.isApp) {
        return typeof BuildInfo !== 'undefined' &&
          BuildInfo &&
          typeof BuildInfo.version !== 'undefined'
          ? 'v.' + BuildInfo.version
          : '';
      } else return '';
    } catch (e) {
      console.error('Error in appVersion() (file authenticated.js)', e);
      return '';
    }
  }

  get getBgColor() {
    return htmlSafe(`${this.siteLayout.headerBackground}`);
  }

  get myName() {
    return this.session.get('data.firstName');
  }

  @action
  tryChangeTenant() {
    if (this.tenantNumber.length < 2) return false;
    if (this.router.currentRouteName === 'change-tenant') return false;

    let self = this;
    this.dialogs.confirm(
      'Cambio azienda',
      `Vuoi procedere con la selezione di un'altra azienda?`,
      self.goToChangeTenants,
      null,
      ['Conferma', 'Annulla']
    );
  }
  @action
  goToChangeTenants() {
    this.router.transitionTo('change-tenant');
  }
}
