/**
 * SERVIZIO DOWNLOAD
 * Contiene tutti i metodi necessari per il download dei file, sia in
 * ambiente Web che su Cordova (Android, iOS).
 *
 * METODI PUBBLICI:
 * @method download : per scaricare i file, sia in ambiente web che Cordova
 * @method base64ToBlob : converte un base64 in un blob
 * @method getMimeType : a partire da un'estensione, restituisce il myme-type
 * @method cleanBase64 : verifica che il base64 abbia il corretto formato e, se necessario, lo corregge
 *
 * PLUGIN CORDOVA NECESSARI
 * cordova plugin add cordova-plugin-camera
 * cordova plugin add cordova-plugin-file
 * cordova plugin add cordova-plugin-file-transfer
 * cordova plugin add cordova-plugin-file-opener2
 * cordova plugin add cordova-plugin-device
 * cordova plugin add cordova-plugin-inappbrowser
 */
import Service from '@ember/service';
import { service } from '@ember/service';

export default class DownloadService extends Service {
  /**
   * Metodo da chiamare per scaricare il file, sia per web che per app Android/iOS.
   *
   * @param {string} base64 : base64 del file da scaricare
   * @param {string} filename : nome da attribuire al file da scarcare, comprensivo di estensione
   */
  @service dialogs;

  async download(base64, filename) {
    //base64 = this.cleanBase64(base64);

    if (
      typeof window.cordova !== 'undefined' &&
      // eslint-disable-next-line no-undef
      ['android', 'ios'].includes(device.platform.toLowerCase())
    ) {
      // AMBIENTE CORDOVA (piattaforme Android e iOS, non browser)
      await this.saveAndOpenFile(base64, filename);
    } else {
      // AMBIENTE WEB
      // creo il blob dal base64
      const blob = await this.base64ToBlob(base64, filename);
      // creao un URL temporaneo
      const blobUrl = URL.createObjectURL(blob);
      await this.downloadWeb(blobUrl, filename);
    }
  }

  cleanBase64(base64String) {
    // Verifica che la lunghezza sia un multiplo di 4
    if (base64String.length % 4 !== 0) {
      base64String += '='.repeat(4 - (base64String.length % 4));
    }

    // Rimuove eventuali spazi bianchi
    base64String = base64String.replace(/\s/g, '');

    // Controlla se è presente un data URI e rimuovilo
    const matches = base64String.match(/^data:([A-Za-z-+\/]+);base64,(.+)$/);
    if (matches) {
      base64String = matches[2];
    }

    // Verifica che la stringa non contenga caratteri non validi
    if (!/^[A-Za-z0-9+/=]*$/.test(base64String)) {
      throw new Error('La stringa Base64 contiene caratteri non validi.');
    }

    return base64String;
  }

  // Converte la stringa base64 in un Blob
  // Tramite Blob è possibile creare un URL per il download
  async base64ToBlob(base64, mimeType) {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: mimeType });
  }

  // download effettivo - per ambiente web
  // creo un elemento di ancoraggio (<a>) e simulo un click per scaricare il file
  async downloadWeb(blobUrl, filename) {
    const a = document.createElement('a');
    a.href = blobUrl;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }

  // scarica ed apre il file - par ambiente Cordova
  async saveAndOpenFile(base64String, filename) {
    let self = this;
    const extension = filename.split('.').pop();
    const mimeType = this.getMimeType(extension);

    if (window.device.platform.toLowerCase() === 'android') {
      // APP ANDROID
      window.resolveLocalFileSystemURL(
        cordova.file.externalRootDirectory,
        function (dirEntry) {
          dirEntry.getDirectory(
            'Download',
            { create: true },
            function (subDirEntry) {
              subDirEntry.getFile(
                filename,
                { create: true, exclusive: false },
                function (fileEntry) {
                  fileEntry.createWriter(async function (fileWriter) {
                    let writeFailed = false;

                    fileWriter.onwriteend = async function () {
                      if (!writeFailed) {
                        console.log('Scrittura file completata.');
                        // Verifica che il file esista prima di tentare di aprirlo
                        fileEntry.file(async function (file) {
                          console.log('File esiste, dimensione: ' + file.size);
                          await self.openFile(fileEntry, mimeType); // Apri il file dopo averlo scritto
                        }, onError);
                      }
                    };

                    fileWriter.onerror = function (e) {
                      console.error('Scrittura file fallita su android:', e);
                      writeFailed = true; // Imposta lo stato di errore
                      onError(e); // Usa la funzione onError per gestire l'errore
                    };
                    // Converte la stringa base64 in un Blob e scrive il Blob nel file
                    const dataBlob = await self.base64ToBlob(
                      base64String,
                      mimeType
                    );
                    fileWriter.write(dataBlob);
                  }, onError);
                },
                onError
              );
            },
            onError
          );
        },
        onError
      );
    } else {
      // APP IOS
      window.resolveLocalFileSystemURL(
        cordova.file.documentsDirectory,
        function (dirEntry) {
          dirEntry.getFile(
            'miofile.txt',
            { create: true, exclusive: false },
            function (fileEntry) {
              fileEntry.createWriter(async function (fileWriter) {
                let writeFailed = false;
                fileWriter.onwriteend = function () {
                  if (!writeFailed) {
                    console.log('Scrittura file completata.');
                    // Verifica che il file esista prima di tentare di aprirlo
                    fileEntry.file(async function (file) {
                      console.log('File esiste, dimensione: ' + file.size);

                      await self.openFile(fileEntry, mimeType); // Apri il file dopo averlo scritto
                    }, onError);
                  }
                };
                fileWriter.onerror = function (e) {
                  console.error('Scrittura file fallita su ios:', e);
                  writeFailed = true; // Imposta lo stato di errore
                  onError(e); // Usa la funzione onError per gestire l'errore
                };
                // Converte la stringa base64 in un Blob e scrive il Blob nel file
                const dataBlob = await self.base64ToBlob(
                  base64String,
                  mimeType
                );
                fileWriter.write(dataBlob);
              }, onError);
            },
            onError
          );
        },
        onError
      );
    }

    function onError(error) {
      console.error('FileSystem Error', error);
      self.dialogs.toast(
        'Si è verificato un errore nel salvataggio del file: ' + error.code,
        'error',
        'bottom-right',
        4
      );
    }
  }

  // apre il file in ambiente Cordova
  async openFile(fileEntry, mimeType) {
    //let self = this;
    let nativePath = fileEntry.nativeURL || fileEntry.toURL();

    // eslint-disable-next-line no-undef
    cordova.plugins.fileOpener2.open(nativePath, mimeType, {
      error: function () {
        console.log('Failed to open file: ' + error);
      },
      success: function () {
        console.log('File open successfully');
      },
    });
    /*
    if (window.device.platform.toLowerCase() === 'android') {
    } else {
      var iab = cordova.InAppBrowser;
      iab.open(nativePath);
    }
    */
  }

  // ricava il myme-type a partire dall'estensione dei file
  getMimeType(extension) {
    return (
      this.mimeTypes[extension.toLowerCase()] || 'application/octet-stream'
    ); // 'application/octet-stream' è un tipo generico per dati binari sconosciuti
  }

  // mapping tra estensioni dei file e relativo mime-type
  mimeTypes = {
    txt: 'text/plain',
    htm: 'text/html',
    html: 'text/html',
    php: 'text/php',
    css: 'text/css',
    js: 'application/javascript',
    json: 'application/json',
    xml: 'application/xml',
    swf: 'application/x-shockwave-flash',
    flv: 'video/x-flv',

    // immagini
    png: 'image/png',
    jpe: 'image/jpeg',
    jpeg: 'image/jpeg',
    jpg: 'image/jpeg',
    gif: 'image/gif',
    bmp: 'image/bmp',
    ico: 'image/vnd.microsoft.icon',
    tiff: 'image/tiff',
    tif: 'image/tiff',
    svg: 'image/svg+xml',
    svgz: 'image/svg+xml',

    // archivi
    zip: 'application/zip',
    rar: 'application/x-rar-compressed',
    exe: 'application/x-msdownload',
    msi: 'application/x-msdownload',
    cab: 'application/vnd.ms-cab-compressed',

    // audio/video
    mp3: 'audio/mpeg',
    qt: 'video/quicktime',
    mov: 'video/quicktime',

    // adobe
    pdf: 'application/pdf',
    psd: 'image/vnd.adobe.photoshop',
    ai: 'application/postscript',
    eps: 'application/postscript',
    ps: 'application/postscript',

    // ms office
    doc: 'application/msword',
    rtf: 'application/rtf',
    xls: 'application/vnd.ms-excel',
    ppt: 'application/vnd.ms-powerpoint',

    // open office
    odt: 'application/vnd.oasis.opendocument.text',
    ods: 'application/vnd.oasis.opendocument.spreadsheet',
  };
}
