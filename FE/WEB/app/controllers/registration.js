import Controller from '@ember/controller';
import { inject as service } from '@ember/service';

// TODO
// se this.siteSetup.publicRegistration === false,
// impedire la registrazione!

export default class RegistrationController extends Controller {
  @service session;
  @service('siteSetup') stp;

  leftSlider = false;
  rightSlider = false;
  loginAreaClass = 'col-lg-12';

  siteSetup = {};

  constructor(...attributes) {
    super(...attributes);

    this.siteSetup = this.stp.siteSetup;

    if (
      this.siteSetup &&
      this.siteSetup.sliderRegistrationPosition &&
      this.siteSetup.sliderRegistrationPosition !== ''
    ) {
      if (
        this.siteSetup.sliderPics &&
        this.siteSetup.sliderPics !== '' &&
        this.siteSetup.sliderPics !== '""'
      ) {
        let sliderPics =
          typeof this.siteSetup.sliderPics === 'string'
            ? JSON.parse(this.siteSetup.sliderPics)
            : this.siteSetup.sliderPics;
        let filteredPics = sliderPics.filter((item) => {
          return item.page === 'registration' && item.active;
        });

        this.loginAreaClass = 'col-lg-12';
        if (filteredPics.length > 0) {
          if (this.siteSetup.sliderRegistrationPosition === 'right') {
            this.rightSlider = true;
            this.loginAreaClass = 'col-lg-8';
          } else if (this.siteSetup.sliderRegistrationPosition === 'left') {
            this.leftSlider = true;
            this.loginAreaClass = 'col-lg-8';
          }
        }
      }
    }
  }
}
