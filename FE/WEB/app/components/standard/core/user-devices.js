import Component from '@glimmer/component';
import { service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { task } from 'ember-concurrency';

export default class StandardCoreUserDevicesComponent extends Component {
  @service jsonApi;
  @service dialogs;
  @service session;
  @service store;
  @service fetch;

  @tracked available = 'waiting';
  @tracked myDevices = [];
  @tracked deviceLength = 0;

  constructor(...args) {
    super(...args);
    this.findDevices.perform();
  }

  findDevices = task({ drop: true }, async () => {
    try {
      this.myDevices = await this.store.query('user-device', {
        filter: `equals(userId,'${this.session.get('data.id')}')`,
        sort: '-lastAccess',
      });
      this.available = 'available';
      this.deviceLength = this.myDevices.length;
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
      this.deviceLength = 0;
    }
  });

  @action
  disconnectOtherDevice() {
    this.dialogs.confirm(
      '<h6>ATTENZIONE</h6>',
      `<p>Tutti gli altri dispositivi verranno disconnessi. Confermi?</p>`,
      () => {
        this.disconnectOtherDeviceConfirmed.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  disconnectOtherDeviceConfirmed = task({ drop: true }, async () => {
    let data = {
      userId: this.session.get('data.id'),
      deviceHash: this.session.getFingerprint(),
      isMine: true,
    };
    this.fetch
      .call(
        'account/disconnectAllDevices',
        'POST',
        data,
        {},
        true,
        this.session
      )
      .then(() => {
        // elimino record degli altri dispositivi
        let params = this.jsonApi.queryBuilder({
          filter: [
            {
              function: 'equals',
              column: 'userId',
              value: this.session.get('data.id'),
            },
            {
              negation: true,
              function: 'equals',
              column: 'deviceHash',
              value: this.session.getFingerprint(),
            },
          ],
        });
        return this.store.query('user-device', params);
      })
      .then((res) => {
        let deletionPromises = res
          .map((record) => {
            return record.destroyRecord();
          })
          .filter(Boolean);

        return Promise.all(deletionPromises);
      })
      .then(() => {
        this.findDevices.perform();
      })
      .catch((e) => {
        console.error(e);
        this.dialogs.toast(
          'Si è verificato un errore. Riprovare!',
          'error',
          'bottom-right',
          3,
          null
        );
      });
  });

  @action
  disconnectSingleDevice(device) {
    this.dialogs.confirm(
      '<h6>ATTENZIONE</h6>',
      `<p>Confermi la disconnessione del dispositivo selezionato?</p>`,
      () => {
        this.disconnectSingleDeviceConfirmed.perform(device);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  disconnectSingleDeviceConfirmed = task({ drop: true }, async (device) => {
    console.log('il device è: ', device);
    let data = {
      userId: this.session.get('data.id'),
      deviceHash: device.deviceHash,
      isMine: true,
    };
    await this.fetch
      .call('account/disconnectOneDevice', 'POST', data, {}, true, this.session)
      .then(() => {
        return device.destroyRecord();
      })
      .then(() => {
        this.dialogs.toast(
          'Operazione riuscita',
          'success',
          'bottom-right',
          2,
          null
        );
        this.findDevices.perform();
      })
      .catch((e) => {
        console.error(e);
        this.dialogs.toast(
          'Si è verificato un errore. Riprovare!',
          'error',
          'bottom-right',
          3,
          null
        );
      });
  });

  get myFingerprint() {
    return this.session.getFingerprint();
  }
}
