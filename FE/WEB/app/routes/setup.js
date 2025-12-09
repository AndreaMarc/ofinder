import Route from '@ember/routing/route';
import { service } from '@ember/service';
//import RSVP from 'rsvp';

export default class SetupRoute extends Route {
  @service session;
  @service store;

  async beforeModel(transition) {
    await this.session.setup();
    this.session.requireAuthentication(transition, 'login'); // solo utenti loggati
  }

  /*async model() {
  }*/

  // scarica i record di setup
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
        resolve(null);
      }
    });
  }
}
