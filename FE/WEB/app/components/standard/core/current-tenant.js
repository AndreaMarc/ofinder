/**
 * Elemento per la scelta del Tenant
 * (visibile solo se in Setup/ è stata attivata l'opzione)
 * @param {bool} inHeader : se il componente è mostrato nell'header, impostare a true per applicare lo
 *                          stile appropriato. Stringa vuota per non applicarlo.
 * @param {string} withDividers : indica se mostrare divisori orizzontali sopra e sotto all'indicatore del
 *                                tenant corrente (utile per il menù utente). Stringa vuota per non mostrarli
 */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { htmlSafe } from '@ember/template';
//import $ from 'jquery';

export default class StandardCoreCurrentTenantComponent extends Component {
  @service('siteSetup') stp;
  @service permissions;
  @service siteLayout;
  @service session;
  @service dialogs;
  @service router;

  @tracked serviceAvailable = false;
  @tracked inHeader = false;
  @tracked withDividers = false;
  @tracked currentTenantName = '';
  @tracked numberTenants = 0;
  @tracked headerStyle = '';

  get canChangeTenant() {
    return this.stp.siteSetup.canChangeTenants;
  }

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  start() {
    if (
      this.canChangeTenant ||
      this.permissions.hasPermissions(['canSeeAllTenants'])
    ) {
      // componente visibile solo se in Setup ho abilitato l'opzione di Cambio Tenant (o se ho il permesso di cambiare Tenant)
      let tenants = this.session.get('data.associatedTenants');
      this.numberTenants = tenants.length;

      try {
        if (
          this.numberTenants > 1 ||
          this.permissions.hasPermissions(['canSeeAllTenants'])
        ) {
          // componente visibile solo se l'utente ha almeno 2 tenant! (o se ho il permesso di cambiare Tenant)
          this.serviceAvailable = true;

          if (this.args.inHeader) {
            this.inHeader = !!this.args.inHeader;
            this.headerStyle = htmlSafe('headerStyle');
          }

          if (this.args.withDividers) {
            this.withDividers = !!this.args.withDividers;
          }

          // ricavo il nome del tenant corrente
          let currentTenant = tenants.filter((item) => {
            return item.tenantId === this.session.get('data.tenantId');
          });
          if (currentTenant[0] && currentTenant[0].name) {
            this.currentTenantName = currentTenant[0].name;
          } else {
            this.currentTenantName = 'Tenant non associato';
          }
        }
      } catch (e) {
        console.error(e);
        this.serviceAvailable = false;
      }
    } else {
      this.serviceAvailable = false;
    }
  }

  get getTextColor() {
    let out = this.inHeader ? this.siteLayout.headerLight : '';
    return htmlSafe(out);
  }

  @action
  tryChangeTenant() {
    if (this.numberTenants.length < 2) return false;
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
