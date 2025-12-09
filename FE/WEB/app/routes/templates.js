import Route from '@ember/routing/route';
import { service } from '@ember/service';

export default class TemplatesRoute extends Route {
  @service permissions;
  @service session;

  async beforeModel(transition) {
    await this.session.setup();
    this.session.requireAuthentication(transition, 'login'); // solo utenti loggati
  }
}
