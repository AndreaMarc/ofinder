import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
//import { action } from '@ember/object';
//import $ from 'jquery';

export default class AppFooterAppUnloggedFooterComponent extends Component {
  @service store;
  @service session;
  @service('siteSetup') stp;
  @tracked showRegistration = false;

  siteSetup = {};

  // costruttore
  constructor(...attributes) {
    super(...attributes);

    this.siteSetup = this.stp.siteSetup;

    if (this.siteSetup && this.siteSetup.publicRegistration) {
      this.showRegistration = true;
    }
  }
}
