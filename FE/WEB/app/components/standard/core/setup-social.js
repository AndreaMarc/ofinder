import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
//import ENV from 'poc-nuovo-fwk/config/environment';
import { task } from 'ember-concurrency';

export default class StandardCoreSetupSocialComponent extends Component {
  @service siteSetup;
  @service dialogs;
  @service jsUtility;
  @service store;

  @tracked recordApp;
  @tracked recordWeb;
  @tracked serviceAvailable = 'waiting';
  @tracked savedChanges = true;

  // proprietà di Google
  @tracked googleEnabledApp = false;
  @tracked googleEnabledWeb = false;
  @tracked facebookEnabledApp = false;
  @tracked facebookEnabledWeb = false;
  @tracked googleIdSite = '';
  @tracked googleAppName = '';
  @tracked redirectUriLogin = '';
  @tracked redirectAfterGoogleLogin = '';
  @tracked redirectAfterGoogleRegister = '';
  @tracked redirectAfterGoogleError = '';
  @tracked googleAPIKey = '';
  @tracked googleSecret = '';
  @tracked googleAPPId = '';
  @tracked googleScopes = '';

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    // recupero i record di setup
    this.recordWeb = this.args.recordWeb;
    this.recordApp = this.args.recordApp;

    if (typeof this.recordApp.thirdPartsAccesses === 'undefined')
      this.recordApp.thirdPartsAccesses = {};
    if (typeof this.recordWeb.thirdPartsAccesses === 'undefined')
      this.recordWeb.thirdPartsAccesses = {};

    this.googleEnabledApp =
      typeof this.recordApp.thirdPartsAccesses.googleEnabled !== 'undefined'
        ? !!this.recordApp.thirdPartsAccesses.googleEnabled
        : false;
    this.googleEnabledWeb =
      typeof this.recordWeb.thirdPartsAccesses.googleEnabled !== 'undefined'
        ? !!this.recordWeb.thirdPartsAccesses.googleEnabled
        : false;
    this.facebookEnabledApp =
      typeof this.recordApp.thirdPartsAccesses.facebookEnabled !== 'undefined'
        ? !!this.recordApp.thirdPartsAccesses.facebookEnabled
        : false;
    this.facebookEnabledWeb =
      typeof this.recordWeb.thirdPartsAccesses.facebookEnabled !== 'undefined'
        ? !!this.recordWeb.thirdPartsAccesses.facebookEnabled
        : false;

    if (typeof this.recordApp.googleCredentials === 'undefined')
      this.recordApp.googleCredentials = {};
    if (typeof this.recordWeb.googleCredentials === 'undefined')
      this.recordWeb.googleCredentials = {};

    this.googleIdSite =
      typeof this.recordWeb.googleCredentials.googleIdSite !== 'undefined'
        ? this.recordWeb.googleCredentials.googleIdSite
        : '';

    this.googleAppName =
      typeof this.recordWeb.googleCredentials.googleAppName !== 'undefined'
        ? this.recordWeb.googleCredentials.googleAppName
        : '';

    this.redirectUriLogin =
      typeof this.recordWeb.googleCredentials.redirectUriLogin !== 'undefined'
        ? this.recordWeb.googleCredentials.redirectUriLogin
        : '';

    this.redirectAfterGoogleLogin =
      typeof this.recordWeb.googleCredentials.redirectAfterGoogleLogin !==
      'undefined'
        ? this.recordWeb.googleCredentials.redirectAfterGoogleLogin
        : '';

    this.redirectAfterGoogleRegister =
      typeof this.recordWeb.googleCredentials.redirectAfterGoogleRegister !==
      'undefined'
        ? this.recordWeb.googleCredentials.redirectAfterGoogleRegister
        : '';

    this.redirectAfterGoogleError =
      typeof this.recordWeb.googleCredentials.redirectAfterGoogleError !==
      'undefined'
        ? this.recordWeb.googleCredentials.redirectAfterGoogleError
        : '';

    this.googleAPIKey =
      typeof this.recordWeb.googleCredentials.googleAPIKey !== 'undefined'
        ? this.recordWeb.googleCredentials.googleAPIKey
        : '';

    this.googleSecret =
      typeof this.recordWeb.googleCredentials.googleSecret !== 'undefined'
        ? this.recordWeb.googleCredentials.googleSecret
        : '';

    this.googleAPPId =
      typeof this.recordWeb.googleCredentials.googleAPPId !== 'undefined'
        ? this.recordWeb.googleCredentials.googleAPPId
        : '';

    this.googleScopes =
      typeof this.recordWeb.googleCredentials.googleScopes !== 'undefined'
        ? this.recordWeb.googleCredentials.googleScopes
        : '';

    this.serviceAvailable = this.args.serviceAvailable;
  }

  @action
  changeValue(field, event) {
    let val = event.target.value;
    if (
      [
        'googleEnabledApp',
        'googleEnabledWeb',
        'facebookEnabledApp',
        'facebookEnabledWeb',
      ].includes(field)
    ) {
      val = !!val;
    } else {
      val = val.trim();
    }

    this[field] = val;

    // aggiorno il campo thirdPartsAccesses (per App e Web)
    let thirdPartsAccesses = {
      googleEnabled: this.googleEnabledWeb,
      facebookEnabled: this.facebookEnabledWeb,
    };
    this.recordWeb.thirdPartsAccesses = thirdPartsAccesses;
    thirdPartsAccesses = {
      googleEnabled: this.googleEnabledApp,
      facebookEnabled: this.facebookEnabledApp,
    };
    this.recordApp.thirdPartsAccesses = thirdPartsAccesses;

    // aggiorno il campo googleCredentials (per App e Web)
    let googleCredentials = {
      googleIdSite: this.googleIdSite,
      redirectUriLogin: this.redirectUriLogin,
      redirectAfterGoogleLogin: this.redirectAfterGoogleLogin,
      redirectAfterGoogleRegister: this.redirectAfterGoogleRegister,
      redirectAfterGoogleError: this.redirectAfterGoogleError,
      googleAPIKey: this.googleAPIKey,
      googleSecret: this.googleSecret,
      googleAPPId: this.googleAPPId,
      googleScopes: this.googleScopes,
    };

    this.recordWeb.googleCredentials = googleCredentials;
    this.recordApp.googleCredentials = googleCredentials;

    if (
      this.recordApp.hasDirtyAttributes ||
      this.recordWeb.hasDirtyAttributes
    ) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }
  }

  save = task({ drop: true }, async () => {
    try {
      // salvo i record
      await this.recordApp.save();
      await this.recordWeb.save();

      // aggiorno lo stato del sito
      let origin =
        typeof window.cordova !== 'undefined' ? this.recordApp : this.recordWeb;

      this.siteSetup.setSetup('googleCredentials', origin.googleCredentials);

      this.dialogs.toast('Operazione riuscita', 'success', 'bottom-right', 3);
      this.savedChanges = true;
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        3
      );
    }
  });
}
