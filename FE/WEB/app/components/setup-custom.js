import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { task } from 'ember-concurrency';

export default class SetupCustomComponent extends Component {
  @service siteSetup;
  @service dialogs;
  @service jsUtility;
  @service store;

  @tracked recordApp;
  @tracked recordWeb;
  @tracked serviceAvailable = 'waiting';
  @tracked recordCustomApp;
  @tracked recordCustomWeb;
  @tracked serviceCustomAvailable = 'waiting';
  @tracked savedChanges = true;

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    // recupero i record di setup
    try {
      this.serviceCustomAvailable = 'waiting';
      this.recordWeb = this.args.recordWeb;
      this.recordApp = this.args.recordApp;
      this.serviceMasterAvailable = this.args.serviceAvailable;
      this.recordCustomWeb = await this.getRecord('web');
      this.recordCustomApp = await this.getRecord('app');
      this.serviceCustomAvailable = 'available';
    } catch (e) {
      console.error(e);
      this.serviceAvailable = 'unavailable';
    }
  }

  get serviceTotalAvailable() {
    if (
      this.serviceAvailable === 'waiting' ||
      this.serviceCustomAvailable === 'waiting'
    ) {
      return 'waiting';
    } else if (
      this.serviceAvailable === 'unavailable' ||
      this.serviceCustomAvailable === 'unavailable'
    ) {
      return 'unavailable';
    } else if (
      this.serviceAvailable === 'available' &&
      this.serviceCustomAvailable === 'available'
    ) {
      return 'available';
    } else {
      return 'unavailable';
    }
  }

  get maintenanceApp() {
    return this.recordApp.maintenance;
  }
  get maintenanceWeb() {
    return this.recordWeb.maintenance;
  }

  @action
  changeValue(field, isWeb, event) {
    let val = event.target.value;
    if (['maintenanceAdmin'].includes(field)) {
      val = !!val;
    }

    if (isWeb) {
      this.recordCustomWeb[field] = val;
    } else {
      this.recordCustomApp[field] = val;
    }

    if (
      this.recordCustomApp.hasDirtyAttributes ||
      this.recordCustomWeb.hasDirtyAttributes
    ) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }
  }

  save = task({ drop: true }, async () => {
    try {
      // salvo i record
      await this.recordCustomApp.save();
      await this.recordCustomWeb.save();

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

  async getRecord(environment) {
    let self = this;
    return new Promise((resolve) => {
      // recupero i record di setup
      try {
        self.store
          .queryRecord('custom-setup', {
            filter: `equals(environment,'${environment}')`,
          })
          .then(function (record) {
            resolve(record);
          });
      } catch (e) {
        console.error(e);
        resolve(null);
      }
    });
  }
}
