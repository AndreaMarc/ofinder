import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { inject as service } from '@ember/service';

export default class StandardCoreUsersMasterComponent extends Component {
  @service jsonApi;
  @service session;
  @service store;

  @tracked userTenantsPending = [];
  @tracked confirmedUsers = []; // elenco utenze confermate
  @tracked seeAllTenantsUsers = false;
  @tracked triggerRefresh = 0;

  constructor(...attributes) {
    super(...attributes);
  }

  @action
  triggerRefreshFn() {
    this.triggerRefresh = new Date().getTime();
  }

  @action
  updateSeeAllTenantsUsers(canSee) {
    this.seeAllTenantsUsers = canSee;
    this.triggerRefreshFn();
  }
}
