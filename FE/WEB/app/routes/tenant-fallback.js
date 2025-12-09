import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';

export default class TenantFallbackRoute extends Route {
  @service session;
  @service router;

  async beforeModel(transition) {
    await this.session.setup();
    this.session.requireAuthentication(transition, 'login'); // solo utenti loggati

    // accesso consentito solo se il tenant è bloccato
    if (
      this.session.isAuthenticated &&
      this.session.get('data.currentTenantActive')
    ) {
      transition.abort();
      this.router.transitionTo('/');
    }
  }
}
