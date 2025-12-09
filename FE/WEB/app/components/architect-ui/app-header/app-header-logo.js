import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import customMenu from 'poc-nuovo-fwk/_customs/customMenu';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

export default class AppHeaderLogoComponent extends Component {
  @service permissions;
  @service siteLayout;
  @service session;
  @service header;
  @service store;

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
  start() {
    let cm = customMenu && customMenu.customMenu ? customMenu.customMenu : null;

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

  get showMobileAdvice() {
    return this.header.totalAdvise > 0;
  }
}
