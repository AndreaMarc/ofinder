import Controller from '@ember/controller';
import { inject as service } from '@ember/service';
//import { tracked } from '@glimmer/tracking';
export default class ApplicationController extends Controller {
  @service statusService;
  @service siteLayout;
  @service session;

  constructor(...attributes) {
    super(...attributes);
  }

  // definisce lo stile grafico della pagina, sulla base di quanto impostato in _custom/layoutStyle.js
  get applicationStyle() {
    let layoutStyle = this.siteLayout;
    let styleClass = '';
    if (layoutStyle.fixedHeader) {
      styleClass += 'fixed-header ';
    }
    if (layoutStyle.fixedFooter) {
      styleClass += 'fixed-footer ';
    }
    if (layoutStyle.fixedSidebar) {
      styleClass += 'fixed-sidebar ';
    }

    if (layoutStyle.bodyTabsLine) {
      styleClass += 'body-tabs-line ';
    } else {
      styleClass += 'body-tabs-shadow ';
    }
    if (layoutStyle.appThemeWhite) {
      styleClass += 'app-theme-white ';
    }
    return styleClass;
  }
}
