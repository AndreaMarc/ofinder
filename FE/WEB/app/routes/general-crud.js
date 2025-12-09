import Route from '@ember/routing/route';
import { service } from '@ember/service';

export default class GeneralCrudRoute extends Route {
  @service session;

  queryParams = {
    entity: {
      refreshModel: true,
    },
  };

  async beforeModel(transition) {
    await this.session.setup();
    this.session.requireAuthentication(transition, 'login'); // solo utenti loggati
  }

  model(params) {
    return params.entity || null;
  }
}
