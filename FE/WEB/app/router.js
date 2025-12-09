import EmberRouter from '@ember/routing/router';
import config from 'poc-nuovo-fwk/config/environment';
import { inject as service } from '@ember/service';

export default class Router extends EmberRouter {
  @service session;
  @service router;
  location = config.locationType;
  rootURL = config.rootURL;

  constructor() {
    super(...arguments);
    this.router.on('routeDidChange', this.handleRouteChange);
  }

  async beforeModel() {
    await this.session.setup();
    /*return this.sessionAccount
      .loadCurrentUser()
      .catch(() => this.session.invalidate());*/
  }

  // IMPEDISCO NAVIGAZIONE SE TENANT DISATTIVATO (Redirect ad apposita pagina)
  //@action
  handleRouteChange = async (transition) => {
    try {
      if (this.session.isAuthenticated) {
        if (
          this.session.isAuthenticated &&
          !this.session.get('data.currentTenantActive') &&
          transition.to.name !== 'tenant-fallback'
        ) {
          this.transitionTo('tenant-fallback');
        }
      }
    } catch (e) {
      console.error('Router.js - error in handleRouteChange: ', e);
    }
  };
}

Router.map(function () {
  // NOT LOGGED USERS
  this.route('login');
  this.route('registration');
  this.route('welcome-slider');
  this.route('recovery-password');
  this.route('update-password');
  this.route('maintenance');
  this.route('terms', { path: '/terms/:legal_code' });
  this.route('confirm-registration');
  this.route('google-error');
  this.route('google-login');
  this.route('about-us');
  this.route('help-desk-unlog');

  // NOT LOGGED AND LOGGED USERS
  this.route('access-permissions', { queryParams: ['otp'] });

  // LOGGED USERS
  this.route('authenticated', { path: '/' }, function () {
    // all routes that require the session to be authenticated
  });
  this.route('setup');
  this.route('translate-structure');
  this.route('translations');
  this.route('roles-permissions');
  this.route('media');
  this.route('users');
  this.route('categories');
  this.route('templates');
  this.route('template-details', {
    path: '/template-details/:template_id',
  });
  this.route('change-tenant');
  this.route('user-profile');
  this.route('change-password');
  this.route('tenant-fallback');
  this.route('user-audit');
  this.route('devices');
  this.route('developer-guide');
  this.route('developer-summary');
  this.route('user-guide');
  this.route('legals');
  this.route('legals-details', { path: '/legals-details/:legal_id' });
  this.route('articles');
  this.route('general-crud'); // richiamabile con /general-crud oppure /general-crud?entity=xxx
  this.route('notifications');

  this.route('empty-logged-page');
  this.route('auth-error');
  this.route('callback');

  this.route('media-categories');

  this.route('home');
  this.route('change-tenant-info');
  this.route('company-profile');
  this.route('update-app');

  this.route('not-found', { path: '/*path' }); // 404 page

  this.route('help-desk');
  this.route('help-desk-admin', { queryParams: ['dest'] });
  this.route('help-desk-details', {
    path: '/help-desk-details/:ticket_id',
  });
  this.route('ticket-details');
  this.route('ticket-create');
  this.route('ticket-create-unlogged');

  this.route('ticket-license');

  this.route('erp-site');
});
