import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import ENV from 'poc-nuovo-fwk/config/environment';
import { task } from 'ember-concurrency';

export default class StandardCoreSetupGeneralComponent extends Component {
  @service siteSetup;
  @service dialogs;
  @service jsUtility;
  @service store;

  @tracked recordApp;
  @tracked recordWeb;
  @tracked serviceAvailable = 'waiting';
  @tracked savedChanges = true;

  @tracked apiHostDev = '';
  @tracked apiHostTest = '';
  @tracked apiHostProd = '';

  constructor(...attributes) {
    super(...attributes);

    this.apiHostDev = ENV.apiHostDev;
    this.apiHostTest = ENV.apiHostTest;
    this.apiHostProd = ENV.apiHostProd;
    this.start();
  }

  @action
  async start() {
    // recupero i record di setup
    this.recordWeb = this.args.recordWeb;
    this.recordApp = this.args.recordApp;
    this.serviceAvailable = this.args.serviceAvailable;
  }

  @action
  changeValue(field, isWeb, event) {
    let val = event.target.value;
    if (
      ['maintenance', 'publicRegistration', 'canSearch', 'disableLog'].includes(
        field
      )
    ) {
      val = !!val;
    }

    if (isWeb === 'both') {
      this.recordWeb[field] = val;
      this.recordApp[field] = val;
    } else if (isWeb) {
      this.recordWeb[field] = val;
    } else {
      this.recordApp[field] = val;
    }

    if (
      this.recordApp.hasDirtyAttributes ||
      this.recordWeb.hasDirtyAttributes
    ) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }

    if (field === 'minAppVersion' && val !== '') {
      let regex = this.jsUtility.regex('appVersion');
      if (!regex.test(val)) {
        this.dialogs.toast(
          'Formato non corretto!<br />Inserire una versione del tipo<br />0.0.0',
          'error',
          'bottom-right',
          3
        );
      }
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

      this.siteSetup.setSetup('maintenance', origin.maintenance);
      this.siteSetup.setSetup('minAppVersion', origin.minAppVersion);
      this.siteSetup.setSetup('publicRegistration', origin.publicRegistration);
      this.siteSetup.setSetup('disableLog', origin.disableLog);
      this.siteSetup.setSetup('canSearch', origin.canSearch);

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
