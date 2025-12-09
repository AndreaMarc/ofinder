import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { setupGetData } from 'poc-nuovo-fwk/utility/utils-startup';
import { task } from 'ember-concurrency';
export default class StandardCoreSetupCommunicationsComponent extends Component {
  @service siteLayout;
  @service siteSetup;
  @service dialogs;
  @service header;
  @service store;

  @tracked recordApp;
  @tracked recordWeb;
  @tracked serviceAvailable = 'waiting';
  @tracked savedChanges = true;

  constructor(...attributes) {
    super(...attributes);
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
  changeValue(field, event) {
    let val = event.target.value;
    if (['internalNotifications', 'pushNotifications'].includes(field)) {
      val = !!val;
    }

    this.recordWeb[field] = val;
    this.recordApp[field] = val;

    if (field === 'internalNotifications' && !val) {
      this.recordWeb['pushNotifications'] = false;
      this.recordApp['pushNotifications'] = false;
    }

    if (
      this.recordWeb.hasDirtyAttributes ||
      this.recordApp.hasDirtyAttributes
    ) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }
  }

  save = task({ drop: true }, async () => {
    try {
      this.recordWeb.save();
      this.recordApp.save();

      let origin =
        typeof window.cordova !== 'undefined' ? this.recordApp : this.recordWeb;

      this.siteSetup.setSetup(
        'internalNotifications',
        origin.internalNotifications
      );
      this.siteSetup.setSetup('internalChat', origin.internalChat);
      this.siteSetup.setSetup('pushNotifications', origin.pushNotifications);

      this.dialogs.toast('Operazione riuscita', 'success', 'bottom-right', 3);
      this.savedChanges = true;

      // richiamo la configurazione del sito per aggiornare l'header
      let stp = await setupGetData(this.store);
      this.siteSetup.changeSetup(stp);
      this.siteLayout.updateLayoutStyle(stp);
      this.header.internalChat = stp.internalChat;
      this.header.internalNotifications = stp.internalNotifications;
    } catch (e) {
      console.error(e);
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
