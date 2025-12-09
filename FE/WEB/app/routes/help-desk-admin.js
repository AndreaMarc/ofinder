import Route from '@ember/routing/route';
import { service } from '@ember/service';

export default class HelpDeskAdminRoute extends Route {
  @service session;

  async beforeModel(transition) {
    await this.session.setup();
    this.session.requireAuthentication(transition, 'login'); // solo utenti loggati
  }

  queryParams = {
    dest: {
      refreshModel: true,
    },
  };

  model(_, transition) {
    return {
      dest: transition.to.queryParams.dest,
    };
  }
}
