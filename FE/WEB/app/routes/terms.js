import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';

export default class TermsRoute extends Route {
  @service translation;
  @service store;

  routerTermsAvailable = false;

  async model(params) {
    let { legal_code } = params;
    return { legalCode: legal_code };
  }
}
