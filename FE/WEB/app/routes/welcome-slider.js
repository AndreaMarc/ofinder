import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';

export default class WelcomeSliderRoute extends Route {
  @service session;

  async beforeModel() {
    await this.session.setup();
    this.session.prohibitAuthentication('');
  }
}
