import Route from '@ember/routing/route';
import { action } from '@ember/object';
import { inject as service } from '@ember/service';

export default class ConfirmRegistrationRoute extends Route {
  @service session;

  async beforeModel() {
    await this.session.setup();
    this.session.prohibitAuthentication('');
  }

  model(_, transition) {
    return {
      otp: transition.to.queryParams.otp,
      email: transition.to.queryParams.email,
    };
  }

  @action
  setupController(controller, model) {
    super.setupController(controller, model);
    controller.loadData();
  }
}
