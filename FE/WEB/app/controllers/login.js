/* eslint-disable no-undef */
/* eslint-disable ember/no-jquery */
import Controller from '@ember/controller';
import { inject as service } from '@ember/service';
import ENV from 'poc-nuovo-fwk/config/environment';

export default class LoginController extends Controller {
  @service translation;
  @service session;
  @service('siteSetup') stp;
  leftSlider = false;
  rightSlider = false;
  loginAreaClass = 'col-lg-12';
  showRegistration = false;

  siteSetup = {};

  constructor(...attributes) {
    super(...attributes);

    this.siteSetup = this.stp.siteSetup;

    if (
      this.siteSetup &&
      this.siteSetup.sliderPosition &&
      this.siteSetup.sliderPosition !== ''
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
          return item.page === 'login' && item.active;
        });

        this.loginAreaClass = 'col-lg-12';
        if (filteredPics.length > 0) {
          if (this.siteSetup.sliderPosition === 'right') {
            this.rightSlider = true;
            this.loginAreaClass = 'col-lg-8';
          } else if (this.siteSetup.sliderPosition === 'left') {
            this.leftSlider = true;
            this.loginAreaClass = 'col-lg-8';
          }
        }
      }
    }

    if (this.siteSetup.publicRegistration) {
      this.showRegistration = true;
    }
  }

  get isApp() {
    return typeof window.cordova !== 'undefined';
  }

  get environment() {
    // Controlla se l'ambiente è "development"
    return ENV.environment;
  }

  get appVersion() {
    if (this.isApp) {
      return typeof BuildInfo !== 'undefined' &&
        BuildInfo &&
        typeof BuildInfo.version !== 'undefined'
        ? 'v.' + BuildInfo.version
        : '';
    } else return '';
  }
}
