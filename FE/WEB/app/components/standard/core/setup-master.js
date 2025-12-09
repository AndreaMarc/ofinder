import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';

export default class StandardCoreSetupMasterComponent extends Component {
  @service store;

  @tracked recordWeb = null;
  @tracked recordApp = null;
  @tracked serviceAvailable = 'waiting';

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    this.recordWeb = await this.getRecord('web');
    this.recordApp = await this.getRecord('app');

    if (!this.recordApp || !this.recordWeb) {
      this.serviceAvailable = 'unavailable';
    } else {
      this.serviceAvailable = 'available';
    }
  }

  async getRecord(environment) {
    let self = this;
    return new Promise((resolve) => {
      // recupero i record di setup
      try {
        self.store
          .queryRecord('setup', {
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
