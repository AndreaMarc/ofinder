/* eslint-disable no-undef */
/**
RICORDATI DI INSTALLARE IL PLUGIN:
cordova plugin add community-cordova-plugin-nfc
*/
import Service from '@ember/service';

export default class CordovaNfcService extends Service {
  writeInProgress = false; // Variabile per evitare scritture multiple in parallelo
  writeListenerActive = false;

  /**
   * DECODIFICA I MESSAGGI CONTENUTI NEL TAG
   * @param {nfc} tag : tag estratto dal plugin cordova
   * @returns {array} decodedMessages : array di stringhe, contenenti i messaggi estratti
   */
  decodeNdefMessages(tag) {
    if (tag && typeof tag.tag !== 'undefined') {
      tag = tag.tag;
    }

    const ndefMessages = tag.ndefMessage; // Assumo che ci siano più messaggi NDEF
    let decodedMessages = [];

    // Itera su tutti i messaggi NDEF presenti
    if (ndefMessages) {
      ndefMessages.forEach((ndefRecord) => {
        const payload = ndefRecord.payload;

        // Ignora il primo byte, che è il language code length
        const languageCodeLength = payload[0];

        // Decodifica il resto del payload in una stringa
        const message = String.fromCharCode.apply(
          null,
          payload.slice(languageCodeLength + 1)
        );

        decodedMessages.push(message); // Aggiungi il messaggio decodificato all'array
      });
    }

    return decodedMessages; // Restituisce un array di messaggi decodificati
  }

  // VERFICO SE IL DEVICE HA L'HARDWARE NFC
  async nfcEnabled() {
    if (typeof window.cordova === 'undefined') {
      throw new Error('Only for Cordova applications!');
    }

    return new Promise((resolve, reject) => {
      nfc.enabled(resolve, reject);
    });
  }

  // Rimuove tutti i listener NFC (NdefListener, TagDiscoveredListener, MimeTypeListener)
  async removeAllListeners() {
    if (typeof window.cordova === 'undefined') {
      throw new Error('Only for Cordova applications!');
    }

    const promises = [];

    if (this.isAndroid()) {
      promises.push(this.removeNdefListener());
      promises.push(
        new Promise((resolve, reject) => {
          nfc.removeTagDiscoveredListener(null, resolve, reject);
        })
      );
      promises.push(
        new Promise((resolve, reject) => {
          nfc.removeMimeTypeListener('text/plain', null, resolve, reject);
        })
      );
    } else if (this.isIOS()) {
      promises.push(nfc.cancelScan());
    }

    await Promise.all(promises);
    console.log('Tutti i listener NFC rimossi correttamente');
  }

  // AVVIA L'ASCOLTO DI UN TAG NFC
  // su Android, la callback viene chiamata ogni volta che viene letto un tag
  // su iOS, la lettura è unitaria
  async addNdefListener(callback) {
    if (typeof window.cordova === 'undefined') {
      throw new Error('Only for Cordova applications!');
    }

    if (this.isAndroid()) {
      return new Promise((resolve, reject) => {
        nfc.addNdefListener(callback, resolve, reject);
      });
    } else if (this.isIOS()) {
      const tag = await nfc.scanNdef();
      callback(tag);
      console.log('Tag NFC rilevato su iOS');
    }
  }

  // RIMUOVE IL LISTENER NFC
  async removeNdefListener() {
    if (typeof window.cordova === 'undefined') {
      throw new Error('Only for Cordova applications!');
    }

    if (this.isAndroid()) {
      return new Promise((resolve, reject) => {
        nfc.removeNdefListener(null, resolve, reject);
      });
    } else if (this.isIOS()) {
      await nfc.cancelScan();
      console.log('Listener NFC rimosso correttamente su iOS');
    }
  }

  // SCRITTURA DIRETTA DEL TAG NFC, SENZA GESTIONE DEL LISTENER
  async _writeTag(ndefMessage, onlyRead = false) {
    if (!ndefMessage || !Array.isArray(ndefMessage)) {
      throw new Error('Messaggio NDEF non valido');
    }

    if (this.isAndroid()) {
      await new Promise((resolve, reject) => {
        nfc.write(ndefMessage, resolve, reject);
      });
      console.log('Scrittura avvenuta su Android!');

      if (onlyRead) {
        await new Promise((resolve, reject) => {
          nfc.makeReadOnly(resolve, reject);
        });
        console.log('Tag reso read-only con successo');
      }

      return true;
    } else if (this.isIOS()) {
      await new Promise((resolve, reject) => {
        nfc.share(ndefMessage, resolve, reject);
      });
      console.log('Dati condivisi via NFC su iOS');
      return true;
    }

    throw new Error('Piattaforma non supportata');
  }

  // AVVIA LA MODALITÀ DI SCRITTURA NFC — SOLO ANDROID
  async startWriteMode(ndefMessage, onlyRead = false) {
    await this.removeNdefListener();
    this.writeInProgress = false;

    return new Promise((resolve, reject) => {
      const listener = (tag) => {
        if (this.writeInProgress) {
          console.warn('Scrittura già in corso, evento NFC ignorato');
          return;
        }

        this.writeInProgress = true;

        this._writeTag(ndefMessage, onlyRead)
          .then(() => {
            //console.log('Scrittura completata, risolvo la promise');
            this.removeNdefListener().then(() => {
              this.writeInProgress = false;
              resolve(true);
            });
          })
          .catch((err) => {
            console.error('Errore durante la scrittura NFC:', err);
            this.removeNdefListener().then(() => {
              this.writeInProgress = false;
              reject(err);
            });
          });
      };

      nfc.addNdefListener(
        listener,
        () => {
          //console.log('Listener NFC attivo, in attesa del tag...');
          // NON risolvo qui!
        },
        (err) => {
          //console.error('Errore nell’aggiungere il listener:', err);
          reject(err);
        }
      );
    });
  }

  _restoreWriteListener() {
    nfc.addNdefListener(
      this._nfcWriteListener,
      () => {
        console.log('Listener NFC ripristinato per scritture future');
      },
      (err) => {
        console.error('Errore nel ripristino del listener NFC:', err);
      }
    );
  }

  // SCRITTURA DEL TAG
  /**
   *
   * @param {array} message : messaggio da scrivere. Es: ndefMessage = [ ndef.textRecord(new String(new Date())) ];
   * @param {bool} onlyRead : indica se effettuare scrittura non cancellabile. Solo su Android, ignorato su Apple
   * @returns {promise} : risolve in caso di successo. In caso d'errore rigetta con questi codici errore:
   *    1: Only for Cordova applications
   *    2: scrittura fallita
   *    3: errore nel rendere il tag non riscrivibile
   *    4: ReadOnly available only on iOS applications
   *    5: unsupported platform
   */
  async writeNdef(ndefMessage, onlyRead) {
    if (this.writeListenerActive) {
      return;
    }
    this.writeListenerActive = true;

    if (typeof window.cordova === 'undefined') {
      const error = new Error('Only for Cordova applications');
      error.code = 1;
      throw error;
    }

    if (this.isAndroid()) {
      return new Promise((resolve, reject) => {
        nfc.addNdefListener(
          async () => {
            if (this.writeInProgress) {
              console.warn('Scrittura già in corso, tag ignorato');
              return;
            }

            this.writeInProgress = true;

            try {
              await this._writeTag(ndefMessage, onlyRead);
              resolve(true);
            } catch (e) {
              reject(e);
            } finally {
              this.writeInProgress = false;
              await this.removeNdefListener();
            }
          },
          () => {
            console.log('In attesa di rilevare un tag NFC su Android...');
          },
          (e) => {
            const error = new Error(e);
            error.code = 2;
            reject(error);
          }
        );
      });
    } else if (this.isIOS()) {
      try {
        await nfc.scanNdef();
        await this._writeTag(ndefMessage, onlyRead);
        this.writeInProgress = false;
        return true;
      } catch (e) {
        const error = new Error(e);
        error.code = 2;
        throw error;
      }
    } else {
      const error = new Error('Unsupported platform');
      error.code = 5;
      throw error;
    }
  }

  // metodo per fermare la scrittura
  async stopWriteListener() {
    await this.removeNdefListener();
    this.writeListenerActive = false;
    this.writeInProgress = false;
    console.log('Listener scrittura NFC fermato');
  }

  // Helper methods to check the platform
  isAndroid() {
    return window.device && window.device.platform.toLowerCase() === 'android';
  }

  isIOS() {
    return window.device && window.device.platform.toLowerCase() === 'ios';
  }
}
