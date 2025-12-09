import Controller from '@ember/controller';
import { service } from '@ember/service';
import { tracked } from '@glimmer/tracking';

export default class ChangePasswordController extends Controller {
  @service('siteSetup') stp;

  @tracked showInfo = false;
  @tracked expiration = '';
  @tracked previous = '';

  constructor(...attributes) {
    super(...attributes);
    if (this.stp.siteSetup.passwordExpirationPeriod) {
      if (this.stp.siteSetup.passwordExpirationPeriod > 30) {
        this.expiration = `La password ha una validità di ${parseInt(
          this.stp.siteSetup.passwordExpirationPeriod / 60
        )} mesi`;
      } else {
        this.expiration = `La password ha una validità di ${parseInt(
          this.stp.siteSetup.passwordExpirationPeriod
        )} giorni`;
      }
    }

    if (this.stp.siteSetup.previousPasswordsStored) {
      if (this.stp.siteSetup.previousPasswordsStored > 1) {
        this.previous = `La nuova password deve essere diversa dalle ${this.stp.siteSetup.previousPasswordsStored} precedenti`;
      } else {
        this.previous = `La nuova password deve essere diversa dalla precedente`;
      }
    }

    if (
      this.stp.siteSetup.passwordExpirationPeriod ||
      this.stp.siteSetup.previousPasswordsStored
    ) {
      this.showInfo = true;
    }
  }
}
