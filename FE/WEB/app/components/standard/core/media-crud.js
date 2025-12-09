/**
 * Componente che implementa il CRUD sui media e ne consente il caricamento.
 *
 * Esempio di utilizzo:
 * <Standard::Core::MediaCrud/>
 */

import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import config from 'poc-nuovo-fwk/config/environment';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { task } from 'ember-concurrency';

export default class StandardCoreMediaCrudComponent extends Component {
  @service('siteSetup') setup;
  @service jsUtility;
  @service fileQueue;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  @tracked serviceAvailable = 'waiting';
  @tracked currentPage = 1;
  @tracked totalPages = 1;

  @tracked filter = new TrackedObject({
    pageSize: 10,
    imageSize: 'sm',
    primaryContentType: '',
    imageType: '',
    imageCategory: '',
    imageAlbum: '',
    imageDate: '',
    imageGuid: '',
  });

  @tracked availablePageSize = [5, 10, 25, 50, 100];
  @tracked availableContentType = [];
  @tracked availableTypologies = [];
  @tracked availableCategories = [];
  @tracked availableAlbums = [];
  @tracked editingMediaId = null;
  @tracked originalMedia;
  @tracked medias;
  clipboard;

  @tracked uploadParams = {
    imageType: '',
    imageCategory: '',
    imageAlbum: '',
    imageAlt: '',
    imageTag: '',
  };
  @tracked showUploadArea = false;

  constructor(...attributes) {
    super(...attributes);
    this.start.perform();
  }

  // All'avvio, inizializzo il ClipboardJS (copia negli appunti)
  @action
  afterRenderHook() {
    // Abilito i pulsanti di copia dei guid
    let self = this;
    // eslint-disable-next-line no-undef
    this.clipboard = new ClipboardJS('.copy-guid-media-crud');
    this.clipboard.off('success');
    this.clipboard.on('success', function (e) {
      self.dialogs.toast('copiato', 'message', 'bottom-right', 1);
      e.clearSelection();
    });
  }

  start = task({ drop: true }, async () => {
    try {
      // Estraggo i dati necessari al filtro di ricerca
      await this.getRoots();

      // Estraggo l'elenco file
      await this.fetchMedia.perform();
      this.serviceAvailable = 'available';
    } catch (e) {
      console.error(e);
      this.serviceAvailable = 'unavailable';
    }
  });

  // Estrae dal DB le Tipologie disponibili in questo tenant,
  // nonchè i content-type disponibili, così da popolare i filtri di ricerca.
  async getRoots() {
    try {
      let self = this;
      let tenantId = await this.session.get('data.tenantId');

      this.availableTypologies = await this.store.query('media-category', {
        filter: `and(equals(tenantId,'${tenantId}'),equals(type,'typology'))`,
        sort: 'order',
      });

      // estraggo i content-type disponibili (tra i file già caricati)
      await this.fetch
        .call('fileUpload/roots', 'GET', null, {}, true, this.session)
        .then((data) => {
          let temp = [];
          if (data.primaryContentType && data.primaryContentType.length > 0) {
            data.primaryContentType.forEach((element) => {
              temp.push(element);
            });
            self.availableContentType = temp;
          }
        })
        .catch(() => {
          throw new Error('unable get roots');
        });
    } catch (e) {
      throw new Error(e);
    }
  }

  // Modifica dei filtri di ricerca
  @action
  changeFilter(field, event) {
    this.currentPage = 1;
    let val =
      event.target.value && event.target.value !== ''
        ? event.target.value.trim()
        : '';
    if (field) {
      this.filter[field] = val;
    }

    if (field === 'imageType') {
      this.getCategory(val);
    } else if (field === 'imageCategory') {
      this.getAlbums(val);
    }

    this.fetchMedia.perform();
  }

  // al cambiare della Tipologia di filtro, estraggo le relative Categorie
  async getCategory(typologyId) {
    let tenantId = this.session.get('data.tenantId');

    this.availableCategories = await this.store.query('media-category', {
      filter: `and(equals(tenantId,'${tenantId}'),equals(type,'category'),equals(parentMediaCategory,'${typologyId}'))`,
      sort: 'order',
    });
  }

  // al cambiare della Categoria di filtro, estraggo i relativi Album
  async getAlbums(categoryId) {
    let tenantId = this.session.get('data.tenantId');

    this.availableAlbums = await this.store.query('media-category', {
      filter: `and(equals(tenantId,'${tenantId}'),equals(type,'album'),equals(parentMediaCategory,'${categoryId}'))`,
      sort: 'order',
    });
  }

  // Estrae l'elenco dei file dal DB
  fetchMedia = task({ drop: true }, async () => {
    let self = this;
    try {
      this.editingMediaId = null;

      // compongo i filtri di ricerca
      let searchFilter = {
        page: {
          size: this.filter.pageSize,
          number: this.currentPage,
        },
        include: 'typologyAreaRel,categoryRel,albumRel',
        filter: [],
      };

      if (this.filter.imageSize) {
        searchFilter.size = this.filter.imageSize;
      }

      if (this.filter.primaryContentType) {
        searchFilter.filter.push({
          function: 'equals',
          column: 'primaryContentType',
          value: this.filter.primaryContentType,
        });
      }
      if (this.filter.imageType) {
        searchFilter.filter.push({
          function: 'equals',
          column: 'typologyArea',
          value: this.filter.imageType,
        });
      }
      if (this.filter.imageCategory) {
        searchFilter.filter.push({
          function: 'equals',
          column: 'category',
          value: this.filter.imageCategory,
        });
      }
      if (this.filter.imageAlbum) {
        searchFilter.filter.push({
          function: 'equals',
          column: 'album',
          value: this.filter.imageAlbum,
        });
      }
      if (this.filter.imageGuid) {
        searchFilter.filter.push({
          function: 'equals',
          column: 'id',
          value: this.filter.imageGuid,
        });
      }
      if (this.filter.imageDate) {
        searchFilter.filter.push({
          function: 'greaterOrEqual',
          column: 'uploadDate',
          value: `${this.filter.imageDate}-01`,
        });

        let d = this.filter.imageDate.split('-');
        let lastDay = new Date(d[0], d[1], 0).getDate();
        searchFilter.filter.push({
          function: 'lessOrEqual',
          column: 'uploadDate',
          value: `${this.filter.imageDate}-${lastDay}`,
        });
      }
      if (this.filter.imageTags) {
        searchFilter.filter.push({
          function: 'contains',
          column: 'tag',
          value: this.filter.imageTags,
        });
      }

      searchFilter.filter.push({
        function: 'equals',
        column: 'global',
        value: true,
      });

      if (searchFilter.filter.length > 0) {
        if (searchFilter.filter.length === 1) {
          let element = searchFilter.filter[0];
          searchFilter.filter = `${element.function}(${element.column},'${element.value}')`;
        } else {
          let subFilterStrings = searchFilter.filter.map((element) => {
            return `${element.function}(${element.column},'${element.value}')`;
          });
          let filterStrings = `and(${subFilterStrings.join()})`;

          searchFilter.filter = filterStrings;
        }
      }

      searchFilter.sort = '-uploadDate';
      //searchFilter.size = 'sm';

      const response = await this.store.query('media-file', searchFilter);

      this.medias = response.slice();
      this.totalPages =
        Math.ceil(response.meta.total / parseInt(this.filter.pageSize)) || 1;
    } catch (e) {
      e.then((error) => {
        // console.log(error);

        if (error.errors && error.errors.length > 0) {
          let isNotFound = error.errors.filter((item) => item.status === '404');
          if (isNotFound && isNotFound.length > 0) {
            this.medias = [];
            this.totalPages = 0;
          } else {
            self.dialogs.toast(
              'Si è verificato un errore. Riprovare!',
              'error',
              'bottom-right',
              4,
              null
            );
          }
        } else {
          self.dialogs.toast(
            'Si è verificato un errore. Riprovare!',
            'error',
            'bottom-right',
            4,
            null
          );
        }
      });
    }
  });

  // crea il link completo al file per il pulsante copia-url
  @action
  baseEndpoint(url, fileName, ext) {
    return `${config.feHost}/assets/virtual${url}/${fileName}.${ext}`;
  }

  // formatta la data di caricamento dei file
  @action
  uploadDate(data) {
    try {
      let d = new Date(data);
      return this.jsUtility.data(d, {
        day: '2-digit',
        month: 'short',
        year: '2-digit',
      });
    } catch (e) {
      return '';
    }
  }

  // attiva la modifica di un record
  @action
  editMedia(media) {
    this.originalMedia = {
      alt: media.alt,
      tag: media.tag,
    };
    this.editingMediaId = media.id;
  }

  // annulla la modifica di un record
  @action
  cancelEditMedia(media) {
    media.alt = this.originalMedia.alt;
    media.tag = this.originalMedia.tag;

    media.notifyPropertyChange('alt');
    media.notifyPropertyChange('tag');

    this.editingMediaId = null;
    this.originalMedia = null;
  }

  // elimina un record
  @action
  async deleteMedia(media) {
    let self = this;
    this.dialogs.confirm(
      '<h4 class="text-danger">Cancellazione del file</h4>',
      `<p class="text-danger">La foto verrà cancellata definitivamente. CONFERMI?</p>`,
      async () => {
        await self.deleteConfirmed(media);
      },
      null,
      ['Sì, cancella il file', 'No']
    );
  }

  async deleteConfirmed(media) {
    await media.destroyRecord();
    this.medias = this.medias.filter((m) => m.id !== media.id);
    this.getRoots();
  }

  // riceve le modifiche al record
  @action
  updateMediaField(media, field, event) {
    media[field] = event.target.value;
  }

  // salva le modifiche ai record
  @action
  async saveMedia(media) {
    await media
      .save()
      .then(() => {
        this.editingMediaId = null;
        this.dialogs.toast(
          'Salvataggio completato',
          'success',
          'bottom-right',
          3,
          null
        );
      })
      .catch((e) => {
        console.error(e);
        this.dialogs.toast(
          'Errore nel salvataggio. Riprovare!',
          'error',
          'bottom-right',
          3,
          null
        );
      });
  }

  // alla pressione di copia-url se il puntamento via url non è disponibile
  @action
  noUrlAvailable() {
    // eslint-disable-next-line no-undef
    Swal.fire(
      'Non disponibile!',
      'Le impostazioni del sito non consentono il riferimento alle immagini via url. Usa il Base64!',
      'warning'
    );
  }

  // pressione dei pulsanti di paginazione
  @action
  async goToPage(where) {
    if (where === 'previous' && this.currentPage !== 1) {
      this.currentPage--;
      await this.fetchMedia.perform();
    } else if (where === 'next' && this.currentPage < this.totalPages) {
      this.currentPage++;
      await this.fetchMedia.perform();
    }
  }

  // Apertura immagine full-screen
  @action
  async preview(id) {
    try {
      const response = await this.store.queryRecord('media-file', {
        size: 'lg',
        filter: `equals(id,'${id}')`,
      });

      let img = new Image();
      img.src = response.base64;
      img.onload = function () {
        // dimensioni dell'immagine: this.width, this.height
        // eslint-disable-next-line no-undef
        Swal.fire({
          imageUrl: response.base64,
          width: '90%',
          background: '#efefef',
          showConfirmButton: true,
          confirmButtonText: 'OK',
          customClass: {
            popup: 'swal2-custom-popup',
            image: 'swal2-responsive-image',
            confirmButton: 'swal2-custom-confirm-button',
          },
          didOpen: () => {
            document.body.classList.add('swal2-open');
          },
          willClose: () => {
            document.body.classList.remove('swal2-open');
          },
        });
      };
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprovare!',
        'error',
        'bottom-right',
        4,
        null
      );
    }
  }

  /**
   * AREA UPLOAD FILES
   */

  // modifico la destinazione del file
  @action
  changeUpload(field, event) {
    let val = '';
    if (field) {
      val =
        event.target.value && event.target.value !== ''
          ? event.target.value.trim()
          : '';
      this.uploadParams[field] = val;
    }

    if (
      this.uploadParams.imageTypology !== '' &&
      this.uploadParams.imageCategory !== '' &&
      this.uploadParams.imageAlbum !== ''
    ) {
      this.showUploadArea = true;
    } else {
      this.showUploadArea = false;
    }

    this.filter.imageType = '';

    if (field === 'imageTypology') {
      this.getCategory(val);
    } else if (field === 'imageCategory') {
      this.getAlbums(val);
    }
  }

  // Caricamento nuovo file
  @action
  async uploadPhoto(file) {
    let self = this;
    let queue = this.fileQueue.find('uploader');

    // verifico campi obbligatori
    if (
      this.uploadParams.imageTypology === '' ||
      this.uploadParams.imageCategory === '' ||
      this.uploadParams.imageAlbum === ''
    ) {
      this.dialogs.toast(
        'Tipologia, Categoria ed Album sono obbligatori',
        'error',
        'bottom-right',
        4,
        null
      );
      queue.remove(file);
      return false;
    }

    let d = new Date();
    let endpoint = `${config.apiHost}/${config.namespaceHost}/fileUpload`;

    let headers = {
      authorization: `${this.session.get('data.access_token')}`,
      accept: 'application/vnd.api+json',
      deviceName: this.jsUtility.getDeviceInfos(),
      platform: 'web',
      appPlatform: null,
      fingerprint: this.session.getFingerprint(),
      tenantId: this.session.get('data.tenantId'),
    };
    if (typeof window.cordova !== 'undefined') {
      headers['platform'] = 'app';
      headers['appPlatform'] =
        // eslint-disable-next-line no-undef
        typeof device !== 'undefined' ? device.platform : 'unknown';
    }

    file
      .upload(endpoint, {
        method: 'POST',
        headers: headers,
        data: {
          typologyArea: self.uploadParams.imageTypology,
          category: self.uploadParams.imageCategory,
          album: self.uploadParams.imageAlbum,
          tenantId: this.session.get('data.tenantId').toString(),
          userGuid: this.session.get('data.id').toString(),
          alt: self.uploadParams.imageAlt || '',
          tag: self.uploadParams.imageTag || '',
          extension: '',
          base64: '',
          fileUrl: '',
          mongoGuid: '',
          originalFileName: '',
          uploadDate: `${d.getFullYear()}/${
            d.getMonth() + 1
          }/${d.getDate()} ${d.getHours()}:${d.getMinutes()}:${d.getSeconds()}`,
          type: 'mediaFiles',
          global: true,
        },
      })
      .then((response) => {
        console.log(response);
        if (response && response.ok) {
          return response.json();
        } else {
          throw new Error('Errore nella richiesta HTTP');
        }
      })
      .then(async () => {
        document.getElementById('uploadAlt').value = '';
        document.getElementById('uploadTag').value = '';
        await self.getRoots();
        self.fetchMedia.perform();
        this.dialogs.toast(
          'Caricamento riuscito.',
          'success',
          'bottom-right',
          4,
          null
        );
      })
      .catch((e) => {
        console.error(e);
        queue.remove(file);
        this.dialogs.toast(
          'Errore nel caricamento. Riprovare!',
          'error',
          'bottom-right',
          4,
          null
        );
      });
  }

  // errore di caricamento del file
  @action
  async uploadFailed(file) {
    console.warn(file);
  }

  // successo nel caricamento del file
  @action
  async uploadSucceeded(file) {
    console.warn(file);
  }

  // verifica il formato della foto da caricare (jpg, png)
  @action
  validateFile(file) {
    const allowedTypes = [
      'image/jpeg',
      'image/jpg',
      'image/png',
      'application/pdf',
    ];
    if (allowedTypes.includes(file.type)) {
      return true;
    } else {
      this.dialogs.toast(
        'Formato non consentito!<br />Sono ammesse solo immagini .jpg e .png',
        'error',
        'bottom-right',
        5,
        null
      );
      return false;
    }
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);

    // Rimuovi il listener quando il componente viene distrutto
    this.clipboard.off('success');
  }
}
