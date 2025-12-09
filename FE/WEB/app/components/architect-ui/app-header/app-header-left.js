import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
//import customMenu from 'poc-nuovo-fwk/_customs/customMenu';

export default class AppHeaderLeftComponent extends Component {
  @service('header') headerService;
  @service addonConfig;
  @service siteLayout;

  @tracked showSearch = false;

  @tracked showCustomMenu = false;
  @tracked hasPerms = false;
  @tracked customIcon = '';
  @tracked customLabel = '';
  @tracked customSubLabel = '';
  @tracked styleHeaderClass = '';
  @tracked popupBg = 'bg-premium-dark';

  constructor(...attributes) {
    super(...attributes);

    if (this.siteLayout.headerLight === 'white') {
      this.styleHeaderClass = 'text-light ';
    } else if (this.siteLayout.headerLight === 'black') {
      this.styleHeaderClass = 'text-dark ';
    }
  }

  @action
  async start() {
    this.showSearch = this.headerService.search;

    let cm = this.addonConfig.config
      ? this.addonConfig.config.customMenu
      : null;

    if (cm && cm.active) {
      if (
        !cm.permissions ||
        !Array.isArray(cm.permissions) ||
        cm.permissions.length === 0
      ) {
        this.hasPerms = true;
      } else {
        let can = true;
        cm.permissions.forEach((element) => {
          if (!this.permissions.hasPermissions([element])) {
            can = false;
          }
        });
        this.hasPerms = can;
      }

      if (cm.colorClass !== '') {
        this.popupBg = cm.colorClass;
      } else this.popupBg = this.siteLayout.headerBackground;

      this.customIcon = cm.icon;
      this.customLabel = cm.label;
      this.customSubLabel = cm.subLabel;
      this.showCustomMenu = true;
    }
  }
}
