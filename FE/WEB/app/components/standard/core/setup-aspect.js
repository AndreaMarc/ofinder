/**
 * CREA IL COMPONENTE DI GESTIONE ASPETTO
 * Gestisce le configurazioni relative al tema Architect
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::Core::SetupAspect/>
 *
 */

/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import $ from 'jquery';
import { setupGetData } from 'poc-nuovo-fwk/utility/utils-startup';
import { task } from 'ember-concurrency';

export default class StandardCoreSetupAspectComponent extends Component {
  @service('siteLayout') serviceLayout;
  @service dialogs;
  @service store;
  recordApp;
  recordWeb;
  siteLayout;

  @tracked savedChanges = true;
  @tracked serviceAvailable = 'waiting';
  @tracked fixedHeader = null;
  @tracked fixedFooter = null;
  @tracked fixedSidebar = null;
  @tracked bodyTabsLine = null;
  @tracked appThemeWhite = null;
  @tracked headerShadow = null;
  @tracked headerLight = '';
  @tracked headerBackground = '';
  @tracked sidebarShadow = null;
  @tracked sidebarLight = '';
  @tracked sidebarBackground = '';

  constructor(...attributes) {
    super(...attributes);
    this.siteLayout = this.serviceLayout;
    this.start();
  }

  @action
  async start() {
    // recupero i record di setup
    this.recordWeb = this.args.recordWeb;
    this.recordApp = this.args.recordApp;
    this.serviceAvailable = this.args.serviceAvailable;

    if (this.serviceAvailable) {
      this.valueAdapter();
    }

    let self = this;
    $(document).on('click', '.switch-header-cs-class', function () {
      let classToSwitch = $(this).attr('data-class');
      let classTextToSwitch = $(this).attr('data-text-class');
      let containerElement = '.app-header';

      $('.switch-header-cs-class').removeClass('active');
      $(this).addClass('active');

      $(containerElement).attr('class', 'app-header');
      $(containerElement).addClass(
        `header-shadow ${classToSwitch} ${classTextToSwitch}`
      );

      let evt = { target: { value: classToSwitch } };
      self.changeValue('string', 'both', 'headerBackground', evt);
      /*self.headerLight =
        classTextToSwitch === 'header-text-light' ? 'white' : 'black';
      */
      let event = {
        target: {
          value: classTextToSwitch === 'header-text-light' ? 'white' : 'black',
        },
      };
      self.changeValue('string', 'both', 'headerLight', event);
      self.headerLight = event.target.value;
    });

    $(document).on('click', '.switch-sidebar-cs-class', function () {
      let classToSwitch = $(this).attr('data-class');
      let classTextToSwitch = $(this).attr('data-text-class');
      let containerElement = '.app-sidebar';

      $('.switch-sidebar-cs-class').removeClass('active');
      $(this).addClass('active');

      $(containerElement).attr('class', 'app-sidebar');
      $(containerElement).addClass(
        `sidebar-shadow ${classToSwitch} ${classTextToSwitch}`
      );

      let evt = { target: { value: classToSwitch } };
      self.changeValue('string', 'both', 'sidebarBackground', evt);
      /*self.sidebarLight =
        classTextToSwitch === 'sidebar-text-light' ? 'white' : 'black';
      */
      let event = {
        target: {
          value: classTextToSwitch === 'sidebar-text-light' ? 'white' : 'black',
        },
      };
      self.changeValue('string', 'both', 'sidebarLight', event);
      self.sidebarLight = event.target.value;
    });
  }

  valueAdapter() {
    this.fixedHeader = this.recordWeb.fixedHeader ? '1' : '';
    this.fixedFooter = this.recordWeb.fixedFooter ? '1' : '';
    this.fixedSidebar = this.recordWeb.fixedSidebar ? '1' : '';
    this.bodyTabsLine = this.recordWeb.bodyTabsLine ? '1' : '';
    this.appThemeWhite = this.recordWeb.appThemeWhite ? '1' : '';
    this.headerShadow = this.recordWeb.headerShadow ? '1' : '';
    this.sidebarShadow = this.recordWeb.sidebarShadow ? '1' : '';
    this.headerLight = this.recordWeb.headerLight;
    this.sidebarLight = this.recordWeb.sidebarLight;
    this.headerBackground = this.recordWeb.headerBackground;
    this.sidebarBackground = this.recordWeb.sidebarBackground;
  }

  @action
  changeValue(dataType, environment, paramName, event) {
    let value = event.target.value;
    if (dataType === 'boolean') {
      value = !!value;
    } else if (dataType === 'number') {
      value === parseInt(value);
    }

    this.serviceLayout.updateParam(paramName, value);
    if (environment === 'app') {
      this.recordApp[paramName] = value;
    } else if (environment === 'web') {
      this.recordWeb[paramName] = value;
    } else {
      this.recordApp[paramName] = value;
      this.recordWeb[paramName] = value;
    }

    if (
      this.recordApp.hasDirtyAttributes ||
      this.recordWeb.hasDirtyAttributes
    ) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }

    // jQuery per cambiamenti real-time del tema
    let containerElement = '.app-container';
    if (paramName === 'fixedHeader') {
      $(containerElement).removeClass('fixed-header');
      if (value) $(containerElement).addClass('fixed-header');
    }
    if (paramName === 'fixedSidebar') {
      $(containerElement).removeClass('fixed-sidebar');
      if (value) $(containerElement).addClass('fixed-sidebar');
    }
    if (paramName === 'fixedFooter') {
      $(containerElement).removeClass('fixed-footer');
      if (value) $(containerElement).addClass('fixed-footer');
    }
    if (paramName === 'bodyTabsLine') {
      $(containerElement).removeClass('body-tabs-line body-tabs-shadow');
      if (value) {
        $(containerElement).addClass('body-tabs-line');
      } else {
        $(containerElement).addClass('body-tabs-shadow');
      }
    }
    if (paramName === 'appThemeWhite') {
      $(containerElement).removeClass('app-theme-white');
      if (value) $(containerElement).addClass('app-theme-white');
    }
    containerElement = '.app-header';
    if (paramName === 'headerShadow') {
      $(containerElement).removeClass('header-shadow');
      if (value) $(containerElement).addClass('header-shadow');
    }
    containerElement = '.app-sidebar';
    if (paramName === 'sidebarShadow') {
      $(containerElement).removeClass('sidebar-shadow');
      if (value) $(containerElement).addClass('sidebar-shadow');
    }
  }

  save = task({ drop: true }, async () => {
    try {
      await this.recordWeb.save({ adapterOptions: { method: 'PATCH' } });
      await this.recordApp.save({ adapterOptions: { method: 'PATCH' } });
      this.savedChanges = true;

      this.dialogs.toast(
        'Salvataggio completato',
        'success',
        'bottom-right',
        3,
        null
      );

      // aggiorno il servizio siteLayout così da aggiornare i componenti che dipendono da esso
      let stp = await setupGetData(this.store);
      this.serviceLayout.updateLayoutStyle(stp);
    } catch (e) {
      this.dialogs.toast(
        'Errore nel salvataggio. Riprovare!',
        'error',
        'bottom-right',
        4,
        null
      );
    }
  });

  @action
  willDestroy() {
    super.willDestroy(...arguments);
    $(document).off('click', '.switch-header-cs-class');
    $(document).off('click', '.switch-sidebar-cs-class');
  }
}
