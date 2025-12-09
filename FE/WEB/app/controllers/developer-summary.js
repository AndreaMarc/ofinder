/* eslint-disable no-undef */
import Controller from '@ember/controller';
import ENV from 'poc-nuovo-fwk/config/environment';

export default class DeveloperSummaryController extends Controller {
  constructor(...attributes) {
    super(...attributes);
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
