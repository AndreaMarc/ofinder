import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { htmlSafe } from '@ember/template';
import { tracked } from '@glimmer/tracking';

export default class StandardCoreHeaderUserMenuComponent extends Component {
  @service siteLayout;
  @service session;
  @service store;

  @tracked userPic = '/assets/images/avatars/user-icon.png';
  @tracked sessionExpiration;
  @tracked byUrl = 'true';

  constructor(...attributes) {
    super(...attributes);

    this.start();
  }

  start() {
    this.lastName = this.session.get('data.lastName');
    this.firstName = this.session.get('data.firstName');
    this.associatedTenants = this.session.get('data.associatedTenants');
    this.tenantId = this.session.get('data.tenantId');
    this.authorizationExpiresIn = this.session.get(
      'data.authorizationExpiresIn'
    );
    this.sessionExpiration = this.session.get(
      'data.authorizationRefreshExpires'
    );

    let currentTenant = this.associatedTenants.filter((item) => {
      return item.tenantId === this.tenantId;
    });
    if (currentTenant[0] && currentTenant[0].name) {
      this.tenantName = currentTenant[0].name;
    } else {
      this.tenantName = 'Tenant non associato';
    }

    this.getProfilePic();
  }

  // recupero dal DB il base64 della foto profilo
  getProfilePic = async () => {
    if (
      this.session.get('data.profileImageId') &&
      this.session.get('data.profileImageId') !== ''
    ) {
      this.userPic = this.session.get('data.profileImageId');
      this.byUrl = '';
    } else {
      this.userPic = '/assets/images/avatars/user-icon.png';
      this.byUrl = 'true';
    }
  };

  get getTextColor() {
    let out = this.siteLayout.headerLight === 'white' ? 'text-white' : '';
    return htmlSafe(out);
  }
  get getBgColor() {
    let out = this.siteLayout.headerBackground;
    return htmlSafe(out);
  }

  get logoutStyle() {
    let out = '';
    if (this.siteLayout.headerLight === 'white') {
      out = `border: 1px solid #ffffff`;
    }
    return htmlSafe(out);
  }

  // TODO
  // Aggiungere recupero della foto profilo

  @action
  invalidateSession() {
    this.session.invalidateSessionGeneral();
  }
}
