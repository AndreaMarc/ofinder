import Service, { inject as service } from '@ember/service';
import { registerDestructor } from '@ember/destroyable';
import { tracked } from '@glimmer/tracking';

export default class SessionAccountService extends Service {
  @service('permissions') permissionsService;
  @service siteSetup;
  @service session;
  @service router;
  @service store;

  @tracked account;

  async loadCurrentUser() {
    let accountId = this.session.get('data.tenantId');
    if (accountId) {
      //let account = await this.store.find('account', accountId);

      this.account = accountId; // account;
    }
  }

  // compone i permessi di rotta nella forma richiesta dal plugin ember-permissions
  async routePermissionsProcessing(routesList) {
    let routePermissions = {};

    routesList.forEach((item) => {
      routePermissions[item.route.toLowerCase()] = [
        `r-p-${item.route.toLowerCase()}`,
      ];
    });
    return routePermissions;
  }

  async extractTokenData(data) {
    //console.warn(data);
    return {
      id: data.id ? data.id : 0,
      firstName: data.firstName,
      lastName: data.lastName,
      fullName: data.fullName,
      profileImageId: data.profileImageId || '',
      tenantId: data.tenantId,
      associatedTenants: data.associatedTenants,
      currentTenantActive: data.currentTenantActive || true,
      permissions: data.claimsList.split(',') || [],

      access_token: data.authorizationBearer,
      refresh_token: data.authorizationRefreshBearer,
      authorizationExpiresIn: data.authorizationExpiresIn,
      authorizationRefreshExpiresIn: data.authorizationRefreshExpiresIn,
      termsAcceptanceDate: data.termsAcceptanceDate,
    };
  }

  // Imposta i Privilegi di Accesso
  async setPermissions(permissions) {
    let perms = [];
    if (this.session.isAuthenticated) {
      let dataPerms = this.session.get('data.permissions');
      if (typeof dataPerms !== 'undefined') {
        perms = dataPerms;
      } else if (permissions) {
        perms = permissions;
      }
      this.permissionsService.setPermissions(perms); // imposto i permessi dell'utente scaricati al login
    }

    // compone i permessi di rotta nella forma richiesta dal plugin ember-permissions
    let p = await this.routePermissionsProcessing(
      this.siteSetup.siteSetup.routesList
    );
    this.permissionsService.setRoutePermissions(p); // imposto i permessi di rotta
    this.permissionsService.addRouteAccessDeniedHandler(this.routeNotAllowed);

    registerDestructor(this, () => {
      this.permissionsService.removeRouteAccessDeniedHandler(
        this.routeNotAllowed
      );
    });

    //this.permissionsService.enableRouteValidation(transition);

    return;
  }

  routeNotAllowed = () => {
    console.error('Accesso non consentito alla pagina selezionata!');
    // eslint-disable-next-line no-undef
    Swal.fire(
      'Accesso negato!',
      'Non hai i permessi necessari per accedere alla pagina richiesta',
      'error'
    );
    this.router.transitionTo('authenticated');
  };
}
