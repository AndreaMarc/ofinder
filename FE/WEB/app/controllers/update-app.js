import Controller from '@ember/controller';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import ENV from 'poc-nuovo-fwk/config/environment';

export default class UpdateAppController extends Controller {
  @service('siteSetup') stp;
  @service router;

  @tracked bgColor = 'bg-secondary';
  @tracked show = false;
  @tracked information = '';

  //*************
  inDebug = false;
  //*************

  constructor(...attributes) {
    super(...attributes);
    this.loadData();
  }

  loadData() {
    let setup = this.stp.siteSetup;
    this.bgColor = setup.headerBackground;

    if (
      // eslint-disable-next-line no-undef
      (this.isApp && device && device.platform !== 'browser') ||
      this.inDebug
    ) {
      this.show = true;

      let wantedVersion = this.inDebug ? '0.0.0' : setup.minAppVersion;
      if (
        (typeof BuildInfo !== 'undefined' &&
          // eslint-disable-next-line no-undef
          BuildInfo &&
          wantedVersion &&
          wantedVersion.toString() !== '' &&
          wantedVersion.toString() !== '0.0.0') ||
        this.inDebug
      ) {
        wantedVersion = wantedVersion.split('.'); // array
        let currentAppVersion = this.appVersion.split('.'); // array

        if (parseInt(wantedVersion[2]) > parseInt(currentAppVersion[2])) {
          // Aggiornamento obbligatorio
          this.information = `É necessario aggiornare l'app alla nuova versione`;
        } else if (
          parseInt(wantedVersion[1]) > parseInt(currentAppVersion[1])
        ) {
          // Aggiornamento obbligatorio
          this.information = `É necessario aggiornare l'app alla nuova versione`;
        } else if (
          parseInt(wantedVersion[0]) > parseInt(currentAppVersion[0])
        ) {
          // aggiornamento facoltativo
          this.information = `É disponibile una nuova versione dell'app`;
        } else {
          this.information = `Verifica nello Store se sono presenti aggiornamenti dell'app!`;
        }
      } else {
        this.show = false;
      }
    }
  }

  @action
  goToHome() {
    console.log('to home');
    this.router.transitionTo('authenticated');
  }

  @action
  goToStore() {
    try {
      // eslint-disable-next-line no-undef
      var appPackage = BuildInfo.packageName;
      // eslint-disable-next-line no-undef
      var platform = device.platform.toLowerCase();

      // Costruisco l'URL dello Store
      let storeUrl = '';
      if (platform === 'android') {
        storeUrl = 'market://details?id=' + appPackage;
      } else if (platform === 'ios') {
        // Inserisci qui il tuo Apple App Store ID
        if (ENV.cordovaIosAppId && ENV.cordovaIosAppId !== '') {
          storeUrl = 'https://apps.apple.com/app/id' + ENV.cordovaIosAppId;
        }
      }

      // Apro il Play Store
      console.log('Store link: ' + storeUrl);
      // eslint-disable-next-line no-undef
      if (cordova && cordova.InAppBrowser && storeUrl !== '') {
        console.log('Apro lo store...');
        // eslint-disable-next-line no-undef
        cordova.InAppBrowser.open(storeUrl, '_self', {
          location: 'yes',
        });
      }
    } catch (e) {
      console.error(e);
    }
  }

  get appVersion() {
    try {
      if (this.isApp) {
        return typeof BuildInfo !== 'undefined' &&
          // eslint-disable-next-line no-undef
          BuildInfo &&
          // eslint-disable-next-line no-undef
          typeof BuildInfo.version !== 'undefined'
          ? // eslint-disable-next-line no-undef
            BuildInfo.version
          : '0.0.0';
      } else return '0.0.0';
    } catch (e) {
      console.error('Error in appVersion() (file authenticated.js)', e);
      return '0.0.0';
    }
  }

  get isApp() {
    return typeof window.cordova !== 'undefined';
  }
}
