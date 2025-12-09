import Service from '@ember/service';
import defaultStyle from 'poc-nuovo-fwk/_customs/layoutStyle';
import { tracked } from '@glimmer/tracking';

export default class SiteLayoutService extends Service {
  //layoutStyle = {};

  // refactoring
  @tracked fixedHeader = '';
  @tracked fixedFooter = '';
  @tracked fixedSidebar = '';
  @tracked bodyTabsShadow = '';
  @tracked bodyTabsLine = '';
  @tracked appThemeWhite = '';
  @tracked headerShadow = '';
  @tracked sidebarShadow = '';
  @tracked headerLight = '';
  @tracked headerBackground = '';
  @tracked sidebarLight = '';
  @tracked sidebarBackground = '';

  constructor() {
    super();
    //this.layoutStyle = defaultStyle; // valore di default in _custom/layoutStyle.js
    Object.keys(defaultStyle).forEach((key) => {
      this[key] = defaultStyle[key];
    });
  }

  updateParam(paramName, value) {
    this[paramName] = value;
  }

  // chiamato all'avvio dell'applicazione, imposta i valori scaricati dal server
  updateLayoutStyle(newValues) {
    if (typeof newValues.fixedHeader !== 'undefined')
      this.fixedHeader = newValues.fixedHeader;
    if (typeof newValues.fixedFooter !== 'undefined')
      this.fixedFooter = newValues.fixedFooter;
    if (typeof newValues.fixedSidebar !== 'undefined')
      this.fixedSidebar = newValues.fixedSidebar;
    if (typeof newValues.bodyTabsShadow !== 'undefined')
      this.bodyTabsShadow = newValues.bodyTabsShadow;
    if (typeof newValues.bodyTabsLine !== 'undefined')
      this.bodyTabsLine = newValues.bodyTabsLine;
    if (typeof newValues.appThemeWhite !== 'undefined')
      this.appThemeWhite = newValues.appThemeWhite;
    if (typeof newValues.headerShadow !== 'undefined')
      this.headerShadow = newValues.headerShadow;
    if (typeof newValues.sidebarShadow !== 'undefined')
      this.sidebarShadow = newValues.sidebarShadow;
    if (typeof newValues.headerLight !== 'undefined')
      this.headerLight = newValues.headerLight;
    if (typeof newValues.headerBackground !== 'undefined')
      this.headerBackground = newValues.headerBackground;
    if (typeof newValues.sidebarLight !== 'undefined')
      this.sidebarLight = newValues.sidebarLight;
    if (typeof newValues.sidebarBackground !== 'undefined')
      this.sidebarBackground = newValues.sidebarBackground;
  }
}
