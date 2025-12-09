import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';

export default class GoogleLoginRoute extends Route {
  @service session;

  async beforeModel() {
    await this.session.setup();
    this.session.prohibitAuthentication('');
  }

  model(_, transition) {
    return {
      otp: transition.to.queryParams.otp,
    };
  }

  @action
  setupController(controller, model) {
    super.setupController(controller, model);
    controller.sendGoogleLoginData();
  }
}
