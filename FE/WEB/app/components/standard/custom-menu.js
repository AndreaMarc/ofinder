import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

export default class StandardCustomMenuComponent extends Component {
  @service addonConfig;
  @service session;
  @service router;

  constructor(...attributes) {
    super(...attributes);
  }

  get errorSite() {
    return true;
  }

  @action
  goToPage(page) {
    // Naviga alla pagina specificata
    this.router.transitionTo(page);
  }
}
