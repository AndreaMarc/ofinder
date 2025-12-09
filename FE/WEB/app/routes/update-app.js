import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';

export default class UpdateAppRoute extends Route {
  @service session;

  async beforeModel() {
    await this.session.setup();
  }

  // eslint-disable-next-line no-unused-vars
  model(_, transition) {
    return {};
  }

  @action
  setupController(controller, model) {
    super.setupController(controller, model);
    //controller.loadData();
  }
}
