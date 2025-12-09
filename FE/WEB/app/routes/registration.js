import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';

export default class RegistrationRoute extends Route {
  @service session;
  @service('siteSetup') stp;
  @service router;

  siteSetup = {};

  async beforeModel(transition) {
    await this.session.setup();
    this.session.prohibitAuthentication('');

    this.siteSetup = this.stp.siteSetup;

    if (!this.siteSetup || !this.siteSetup.publicRegistration) {
      console.warn('Registrazione non consentita!');
      transition.abort();
    }
  }
}
