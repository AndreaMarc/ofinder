import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { inject as service } from '@ember/service';
//import { action } from '@ember/object';

// eslint-disable-next-line ember/no-empty-glimmer-component-classes
export default class ArchitectUIAppHeaderAppHeaderComponent extends Component {
  @service session;
  @service siteLayout;
  @tracked styleHeaderClass = '';

  constructor(...attributes) {
    super(...attributes);

    //this.styleHeaderClass = this.getStyleHeader;
  }

  // costruisce le classi di stile per l'header
  get getStyleHeader() {
    let layoutStyle = this.siteLayout;
    let styleHeader = '';
    if (this.session.isAuthenticated) {
      styleHeader += layoutStyle.headerShadow ? 'header-shadow ' : '';

      if (layoutStyle.headerLight === 'white') {
        styleHeader += 'header-text-light ';
      } else if (layoutStyle.headerLight === 'black') {
        styleHeader += 'header-text-dark ';
      }

      if (layoutStyle.headerBackground)
        styleHeader += layoutStyle.headerBackground + ' ';
    }

    return styleHeader;
  }
}
