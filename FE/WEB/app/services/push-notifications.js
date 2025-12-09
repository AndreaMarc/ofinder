/* eslint-disable no-undef */
/**
 * SERVIZIO PER LA GESTIONE DELLE NOTIFICHE PUSH DELL'APP
 * Attivato da application.js se il setup del sito prevede le notifiche push.
 *
 * NOTA:
 * nel servizio push-callback.js valorizzare le callback di ciascuna notifica prevista
 */
import Service from '@ember/service';

export default class PushNotificationsService extends Service {
  // NOTA: i servizi vengono valorizzati dall'inializzatore
  pushCallback;
  translation;
  getAdvices;
  session;
  header;
  router;
  store;
  device;

  interval_token = null;

  constructor() {
    super();
    this.siteSetup = {};
  }

  // eslint-disable-next-line prettier/prettier
  start(session, store, pushCallback, translation, header, getAdvices, device, router) {
    this.pushCallback = pushCallback;
    this.translation = translation;
    this.session = session;
    this.header = header;
    this.store = store;
    this.getAdvices = getAdvices;
    this.device = device;
    this.router = router;

    if (typeof device === 'undefined') return false;

    this._verify_permission('pushNotificationsService')
      .then(() => {
        // disattivo l'autogenerazione del token (quando la riattiverò otterrò un nuovo token)
        return this._auto_init_token(false, 'pushNotificationsService');
      })
      .then(() => {
        setTimeout(() => {
          return this._token_listener(this._new_token);
        }, 100);
      })
      .then(() => {
        return this._notify_listener();
      })
      .then(() => {
        if (this.session.isAuthenticated) {
          // loggato, riattivo generazione automatica del token
          this._auto_init_token(true, 'pushNotificationsService');
        }
      })
      .catch((e) => {
        console.error('Errore di inizializzazione PUSH: ' + e);
      });
  }

  // verifica se l'utente ha concesso i permessi e in caso contrario li richiede
  _verify_permission(from) {
    return new Promise((resolve, reject) => {
      if (from && from !== '') {
        console.log(
          '*** Verify Notification Permission *** called by: ' + from
        );

        window.FirebasePlugin.hasPermission(function (data) {
          console.log('Notification permission: ' + data.toString());
          if (!data) {
            window.FirebasePlugin.grantPermission(function (hasPermission) {
              if (hasPermission) {
                console.log('permesso accordato');
                resolve();
              } else {
                console.log('permesso negato');
                reject();
              }
            });
          } else {
            console.log('Notifiche push già ammesse per questa app!');
            resolve();
          }
        });
      } else {
        reject();
      }
    });
  }

  // setta l'auto-refresh del token
  _auto_init_token(enable, from) {
    return new Promise((resolve, reject) => {
      if (enable) {
        // Abilito la rigenerazione automatica del token

        console.log('*** ABILITA AUTO-INIT-TOKEN *** called by ' + from);
        window.FirebasePlugin.isAutoInitEnabled(function (en) {
          if (!en) {
            window.FirebasePlugin.setAutoInitEnabled(
              true,
              function () {
                resolve();
              },
              function () {
                reject();
              }
            );
          } else {
            resolve();
          }
        });
      } else {
        // Disabilito la rigenerazione automatica del token
        console.log('*** DISATTIVA AUTO-INIT-TOKEN *** called by ' + from);

        window.FirebasePlugin.isAutoInitEnabled(function (en) {
          if (en) {
            window.FirebasePlugin.setAutoInitEnabled(
              false,
              function () {
                resolve();
              },
              function () {
                reject();
              }
            );
          } else {
            resolve();
          }
        });
      }
    });
  }

  // attivo un listner di "assegnazione/cambio token"
  _token_listener(cb) {
    return new Promise((resolve, reject) => {
      window.FirebasePlugin.onTokenRefresh(cb, function (error) {
        console.warn(error);
        reject(error);
      });
      resolve();
    });
  }

  _getCurrentToken() {
    return new Promise((resolve, reject) => {
      window.FirebasePlugin.getToken(
        function (fcmToken) {
          resolve(fcmToken);
        },
        function (error) {
          reject(error);
        }
      );
    });
  }

  // cb da eseguire alla ricezione di un nuovo token
  _new_token = (fcmToken) => {
    let self = this;
    console.log(
      '*** Ricezione del token (REFRESH) ***: ' + fcmToken.slice(-10)
    );

    try {
      clearInterval(this.interval_token);
      this.interval_token = null;
      // eslint-disable-next-line no-empty
    } catch (e) {}

    if (fcmToken && fcmToken.length > 0) {
      if (this.session.isAuthenticated) {
        // é loggato: aggiorno il db
        this.interval_token = setInterval(function () {
          self._update_db_token();
        }, 90000); // 1,5 minuti
        this._update_db_token();
      } else {
        // non loggato: creo setInterval che verifica temporaneamente se è loggato.
        this.interval_token = setInterval(function () {
          self._update_db_token();
        }, 90000); // 1,5 minuti
      }
    }
  };

  // aggiorno il token nel DB
  _update_db_token = () => {
    let self = this;
    this._getCurrentToken()
      .then(async (token) => {
        if (
          token &&
          typeof token !== 'undefined' &&
          token !== 'null' &&
          token !== ''
        ) {
          try {
            if (!self.session.isAuthenticated) {
              clearInterval(self.interval_token);
              self.interval_token = null;
            } else {
              // anzitutto ricavo l'id del record user-device corrispondente all'utente e al fingerprint corrente
              let ud = await self.store.query('user-device', {
                filter: `and(equals(userId,'${self.session.get(
                  'data.id'
                )}'),equals(deviceHash,'${self.session.getFingerprint()}'))`,
              });

              // aggiorno i dati
              ud = ud[0];
              ud.pushToken = token;
              ud.platform = this.device.platform.toLowerCase();
              ud.deviceName = `Device model: ${this.device.model} - Device UUID: ${this.device.uuid} - Device platform: ${this.device.platform} - OS Version: ${this.device.version}`;
              ud.save();

              clearInterval(self.interval_token);
              console.log(
                '*****************************************************'
              );
              console.log(
                '***---*** TOKEN FCM aggiornato nel database ***---***'
              );
              console.log(
                '*****************************************************'
              );
            }
          } catch (e) {
            console.error(e);
            throw new Error(e);
          }
        }
      })
      .catch((e) => {
        console.error(
          'Token FCM non aggiornato (catch api call): ' + JSON.stringify(e)
        );
      });
  };

  // listener per nuova notifica
  _notify_listener() {
    let self = this;
    return new Promise((resolve) => {
      window.FirebasePlugin.onMessageReceived(
        function (data) {
          console.log('Notifica ricevuta: ' + JSON.stringify(data));
          console.log('Message type: ' + data.messageType); // notification|data
          //alert(JSON.stringify(data));

          if (data.messageType === 'notification') {
            // HO RICEVUTRO UNA NOTIFICA
            self.masterMessage(data);
          } else {
            // HO RICEVUTO UN DATA-MESSAGE
            self.masterNotification(data);
          }
        },
        function (err) {
          console.log(
            'Error notification onMessageReceived: ' + JSON.stringify(err)
          );
          return false;
        }
      );

      console.log('*************************************');
      console.log('AGGIUNTO LISTENER PER NOTIFICHE PUSH!');
      console.log('*************************************');
      resolve();
    });
  }

  // MASTER CALLBACK PER NOTIFICA DI TIPO DATI
  masterMessage(res) {
    this.masterNotification(res);
  }

  // MASTER CALLBACK PER NOTIFICA DI TIPO NOTIFICATIONS
  masterNotification(res) {
    if (typeof this.device === 'undefined') return false;
    let translation = this.translation.languageTranslation;
    //let my_media = this._getSound('new_message');

    // eslint-disable-next-line no-undef
    window.FirebasePlugin.clearAllNotifications(); // valutarne l'utilità!
    // eslint-disable-next-line no-undef
    window.FirebasePlugin.setBadgeNumber(0); // valutarne l'utilità!

    /*if (!res.title) {
      res.title = translation.service.pushCallback.defaultTile;
    }
    if (!res.body) {
      res.message = translation.service.pushCallback.defaultMessage;
    }
    */

    if (!res || !res.pushType) {
      console.error('PushType not defined!!!');
      return false;
    }

    console.warn('Notifica ricevuta:', res);
    try {
      // eslint-disable-next-line prettier/prettier
      this.pushCallback[res.pushType.trim()](res, translation, this.store, this.session, this.header, this.getAdvices, this.router);
    } catch (e) {
      console.error(
        `Non è stata definita la callback per il push-type: ${res.pushType.trim()}`,
        e
      );
    }
  }

  // utility per i file musicali
  _getMediaURL(s) {
    if (typeof this.device === 'undefined') return false;
    // eslint-disable-next-line no-undef
    if (this.device.platform.toLowerCase() === 'android')
      return '/android_asset/www/' + s;
    return s;
  }

  _getSound(file_name) {
    if (typeof this.device === 'undefined') return false;
    let media;
    // eslint-disable-next-line no-undef
    if (this.device.platform.toUpperCase() === 'IOS') {
      let fileUrl = this._getMediaURL(`assets/sounds/${file_name}.m4a`);
      // eslint-disable-next-line no-undef
      media = new Media(fileUrl, null, (err) => {
        console.warn('Audio Error: ' + err.code);
      });
    } else {
      let fileUrl = this._getMediaURL(`assets/sounds/${file_name}.aac`);
      // eslint-disable-next-line no-undef
      media = new Media(fileUrl, null, (err) => {
        console.warn('Audio Error: ' + err.code);
      });
    }

    return media;
  }
}
