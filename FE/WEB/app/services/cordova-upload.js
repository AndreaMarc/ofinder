/* eslint-disable no-undef */
/**
 * SERVIZIO CORDOVA-UPLOAD
 * Contiene tutti i metodi necessari per scattare foto o
 * prelevare un'immagine dalla gallery del telefono e
 * caricarla sul server (Per Cordova).
 *
 * METODI PUBBLICI:
 * @method getPicture : per scattare una foto o prelevarla dalla galleria.
 *                      Restituisce:
 *                      - un array contenente:
 *                        -- il file (che può essere caricato sul server)
 *                        -- il base64
 *                      OPPURE
 *                      - il fileURI
 *
 * @param {string} origin : stabilisce l'originie del file:
 *                          camera : scatta una foto per creare il file
 *                          photo: scegli un'immagine dalla galleria
 *                          album: Scegli un'immagine solo dall'album Rullino fotografico del dispositivo
 * @param {string} cameraDirection : quale fotocamera utilizzare. Valori: back/front
 * @param {number} quality : 0-100. Stabilisce la qualità dell'immagine (solo quando uso la fotocamera. Ignorato per imamgini prelevate dalla galleria)
 * @param {number} maxDimension : Dimensione in pixel per ridimensionare l'immagine. Le proporzioni rimangono costanti
 * @param {bool} saveToPhotoAlbum : Indica se memorizzare nel dispositivo l'immagine scattata
 * @param {number} returnType : 'base64'/'uri'. Indica cosa deve restituire la funzione
 *
 *
 * @method uploadPicture : invia al server un'immagine partendo dal suo base64
 *
 * PLUGIN CORDOVA NECESSARI
 * cordova plugin add cordova-plugin-camera
 * cordova plugin add cordova-plugin-device
 *
 * ESEMPIO DI UTILIZZO PER CARICARE UN'IMMAGINE SUL SERVER
 * let data = this.cordovaUpload.getPicture('camera', 'back', 90, 1500, false, 'base64)
 *  .then((data) => {
        let base64 = data[1];
        return this.cordovaUpload.uploadPicture(base64, this.typology.id, this.category.id, this.album.id, false, this.session);
      })
      .then(async (res) => {
        ...
      })
      .catch((e) => {
        ...
      });
 *
 *
 *
 * IMPORTANTE
 * Aggiungere queste righe al config.xml dell'app (per iOS)
 *
    <edit-config target="NSCameraUsageDescription" file="*-Info.plist" mode="merge">
        <string>need camera access to take pictures</string>
    </edit-config>
    <edit-config target="NSPhotoLibraryUsageDescription" file="*-Info.plist" mode="merge">
        <string>need photo library access to get pictures from there</string>
    </edit-config>
    <edit-config target="NSLocationWhenInUseUsageDescription" file="*-Info.plist" mode="merge">
        <string>need location access to find things nearby</string>
    </edit-config>
    <edit-config target="NSPhotoLibraryAddUsageDescription" file="*-Info.plist" mode="merge">
        <string>need photo library access to save pictures there</string>
    </edit-config>
 */
import Service from '@ember/service';
import { service } from '@ember/service';
import config from 'poc-nuovo-fwk/config/environment';
import { v4 } from 'ember-uuid';

export default class CordovaUploadService extends Service {
  @service dialogs;
  @service fetch;

  async uploadPicture(base64, typologyArea, category, album, global, session) {
    let d = new Date();
    let endpoint = `${config.apiHost}/${config.namespaceHost}/fileUpload`;

    // il backend si aspetta il data sottoforma di form-data
    const formData = new FormData();
    let data = {
      base64String: base64,
      base64Name: `${v4()}.jpg`,
      typologyArea: typologyArea,
      category: category,
      album: album,
      tenantId: session.get('data.tenantId').toString(),
      userGuid: session.get('data.id').toString(),
      alt: '',
      tag: '',
      extension: '',
      base64: '',
      fileUrl: '',
      mongoGuid: '',
      originalFileName: '',
      uploadDate: `${d.getFullYear()}/${
        d.getMonth() + 1
      }/${d.getDate()} ${d.getHours()}:${d.getMinutes()}:${d.getSeconds()}`,
      type: 'mediaFiles',
      global: global || false,
    };

    // Aggiungi dati al form data
    for (const [key, value] of Object.entries(data)) {
      formData.append(key, value);
    }

    let file = await this.processImageData(base64);
    formData.append('file', file[0]);

    return this.fetch.call(endpoint, 'POST', formData, {}, true, session);
  }

  getPicture(
    origin,
    cameraDirection,
    quality,
    maxDimension,
    saveToPhotoAlbum,
    returnType
  ) {
    return new Promise((resolve, reject) => {
      if (typeof window.cordova === 'undefined') {
        this.dialogs.toast(
          `Il servizio Upload è disponibile solo in ambiente App`,
          'error',
          'bottom-right',
          4
        );
        return reject(new Error('Cordova not defined.'));
      }

      if (
        typeof navigator === 'undefined' ||
        typeof navigator.camera === 'undefined'
      ) {
        this.dialogs.toast(
          `Non è stato trovato il plugin Camera!`,
          'error',
          'bottom-right',
          4
        );
        return reject(new Error('Camera plugin not installed.'));
      }

      let cameraOptions = this.setupCameraOptions(
        origin,
        cameraDirection,
        quality,
        maxDimension,
        saveToPhotoAlbum,
        returnType
      );

      navigator.camera.getPicture(
        (imageData) => {
          if (cameraOptions.destinationType === 0) {
            // imageData è il base64 dell'immagine catturata
            console.log('Base64 Image Data:', imageData.substring(0, 100));

            if (!imageData.toString().includes('data:image/jpeg;base64')) {
              imageData = `data:image/jpeg;base64,${imageData}`;
            }
            this.processImageData(imageData)
              .then((res) => {
                // ritorna un array con un file e il base64
                resolve(res);
              })
              .catch((err) => {
                reject(err);
              });
          } else {
            console.log('URI:', imageData);
            this.processFileURI(imageData)
              .then((res) => {
                console.log(res);
                resolve(res);
              })
              .catch((e) => {
                console.error(e);
                reject(e);
              });
          }
        },
        (error) => {
          this.dialogs.toast(
            `Errore durante l'acquisizione.`,
            'error',
            'bottom-right',
            4
          );
          reject(error);
        },
        cameraOptions
      );
    });
  }

  // Funzione per processare il file URI
  async processFileURI(fileURI) {
    return new Promise((resolve, reject) => {
      window.resolveLocalFileSystemURL(
        fileURI,
        async function (fileEntry) {
          await fileEntry.file(
            function (file) {
              resolve(file);
            },
            function (error) {
              console.error('Errore nel leggere il file: ', error);
              reject(error);
            }
          );
        },
        function (error) {
          console.error('Errore nel risolvere il file URI: ', error);
          reject(error);
        }
      );
    });
  }

  // Questa funzione si occupa di salvare l'immagine come file e poi convertirla in un oggetto File
  async processImageData(base64Data) {
    try {
      const fileEntry = await this.saveBase64AsFile(base64Data, 'photo.jpg');
      console.log('File saved successfully:', fileEntry.nativeURL);

      const file = await this.convertFileEntryToFile(fileEntry);
      if (file.size > 0) {
        console.log('File created successfully:', file);
        return [file, base64Data];
      } else {
        throw new Error('File created is empty or invalid');
      }
    } catch (error) {
      console.error('Process Image Data Error:', error);
      throw error;
    }
  }

  // Converti un FileEntry in un oggetto File
  async convertFileEntryToFile(fileEntry) {
    return new Promise((resolve, reject) => {
      fileEntry.file(resolve, reject);
    });
  }

  async saveBase64AsFile(base64Data, fileName) {
    return new Promise((resolve, reject) => {
      // Ottieni l'accesso al file system
      window.resolveLocalFileSystemURL(
        device.platform.toLowerCase() === 'android'
          ? cordova.file.externalApplicationStorageDirectory
          : cordova.file.dataDirectory,

        function (dirEntry) {
          // Crea un nuovo file nel file system
          dirEntry.getFile(
            fileName,
            { create: true, exclusive: false },
            function (fileEntry) {
              // Crea un FileWriter per scrivere nel file
              fileEntry.createWriter(
                function (fileWriter) {
                  fileWriter.onwriteend = function () {
                    console.log('File Write completed.');
                    resolve(fileEntry);
                  };

                  fileWriter.onerror = function (e) {
                    console.log('File Write failed: ' + e.toString());
                    reject(e);
                  };

                  // Crea un Blob dai dati base64
                  let data = base64Data.split(',')[1]; // Rimuove l'intestazione base64
                  let binary = atob(data);
                  let array = [];
                  for (let i = 0; i < binary.length; i++) {
                    array.push(binary.charCodeAt(i));
                  }
                  let blob = new Blob([new Uint8Array(array)], {
                    type: 'image/jpeg',
                  });

                  // Scrivi il Blob nel file
                  fileWriter.write(blob);
                },
                function (error) {
                  console.error('Could not create file writer, error: ', error);
                  reject(error);
                }
              );
            },
            function (error) {
              console.error('Could not create file, error: ', error);
              reject(error);
            }
          );
        },
        function (error) {
          console.error('Could not resolve file system, error: ', error);
          reject(error);
        }
      );
    });
  }

  setupCameraOptions(
    origin,
    cameraDirection,
    quality,
    maxDimension,
    saveToPhotoAlbum,
    returnType
  ) {
    let sourceType;
    switch (origin) {
      case 'camera':
        sourceType = Camera.PictureSourceType.CAMERA;
        break;
      case 'photo':
        sourceType = Camera.PictureSourceType.PHOTOLIBRARY;
        break;
      case 'album':
        sourceType = Camera.PictureSourceType.SAVEDPHOTOALBUM;
        break;
      default:
        sourceType = Camera.PictureSourceType.CAMERA;
    }

    let cameraDir =
      cameraDirection.toLowerCase() === 'front'
        ? Camera.Direction.FRONT
        : Camera.Direction.BACK;
    let parsedQuality = parseInt(quality);
    parsedQuality =
      !isNaN(parsedQuality) && parsedQuality >= 0 && parsedQuality <= 100
        ? parsedQuality
        : 90;
    let dimension = parseInt(maxDimension);
    dimension = !isNaN(dimension) && dimension > 0 ? dimension : 1500;

    let destinationType =
      !returnType || returnType === 'uri'
        ? Camera.DestinationType.FILE_URI
        : Camera.DestinationType.DATA_URL;

    return {
      quality: parsedQuality,
      destinationType: destinationType,
      sourceType: sourceType,
      allowEdit: false,
      encodingType: Camera.EncodingType.JPEG,
      targetWidth: dimension,
      targetHeight: dimension,
      mediaType: Camera.MediaType.PICTURE,
      correctOrientation: true,
      saveToPhotoAlbum: !!saveToPhotoAlbum,
      cameraDirection: cameraDir,
    };
  }

  cameraOptions = {
    quality: 90,
    destinationType: Camera.DestinationType.DATA_URL, // oppure Camera.DestinationType.FILE_URI
    sourceType: Camera.PictureSourceType.CAMERA, // oppure PHOTOLIBRARY oppure SAVEDPHOTOALBUM
    allowEdit: false,
    encodingType: Camera.EncodingType.JPEG, // oppure PNG
    targetWidth: 1500,
    targetHeight: 1500,
    mediaType: Camera.MediaType.PICTURE, // oppure VIDEO oppure ALLMEDIA
    correctOrientation: true,
    saveToPhotoAlbum: false,
    cameraDirection: Camera.Direction.BACK, // oppure FRONT
    popoverOptions: new CameraPopoverOptions(
      0, // coordinata x in pixel dell'elemento dello schermo su cui ancorare il popover.
      32, // coordinata y in pixel dell'elemento dello schermo su cui ancorare il popover.
      320, // larghezza, in pixel, dell'elemento dello schermo su cui ancorare il popover.
      480, // altezza, in pixel, dell'elemento dello schermo su cui ancorare il popover.
      Camera.PopoverArrowDirection.ARROW_ANY, // La direzione in cui dovrebbe puntare la freccia sul popover
      300, // larghezza del popover (0 o non specificato utilizzerà la larghezza predefinita di Apple).
      600 //altezza del popover (0 o non specificato utilizzerà l'altezza predefinita di Apple).
    ),
  };
}
