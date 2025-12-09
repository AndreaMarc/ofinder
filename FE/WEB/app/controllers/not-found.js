import Controller from '@ember/controller';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';

export default class NotFoundController extends Controller {
  @service session;
  //@service siteLayout;

  @tracked isAuthenticated;

  constructor(...attributes) {
    super(...attributes);
    this.isAuthenticated = this.session.isAuthenticated;
  }
}
