import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';
import RSVP from 'rsvp';
//import config from 'poc-nuovo-fwk/config/environment';

export default class RolesPermissionsRoute extends Route {
  @service session;
  @service store;

  async beforeModel(transition) {
    await this.session.setup();
    this.session.requireAuthentication(transition, 'login');
  }

  async model() {
    return await RSVP.hash({
      tenants: this.store.findAll('tenant'),
    });
  }
}
