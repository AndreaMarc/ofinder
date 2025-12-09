/**
 * Componente che implementa il CRUD sui media e ne consente il caricamento.
 *
 * Esempio di utilizzo:
 * <Standard::Core::Media/>
 */

/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import config from 'poc-nuovo-fwk/config/environment';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import $ from 'jquery';

export default class StandardCoreMediaFilesComponent extends Component {
  @service('siteSetup') setup;
  @service jsUtility;
  @service fileQueue;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  @tracked pageSize = '5';
  @tracked availablePageSize = ['5', '10', '25', '50', '100'];
  @tracked availableContentType = [];
  @tracked availableTypologies = [];
  @tracked availableCategories = [];
  @tracked availableAlbums = [];
  @tracked filter = new TrackedObject({
    pageSize: 5,
    imageSize: 'sm',
    primaryContentType: '',
    imageType: '',
    imageCategory: '',
    imageAlbum: '',
    imageDate: '',
    imageGuid: '',
  });
  @tracked currentPage = 1;
  @tracked totalPages = 1;
  @tracked medias;
  @tracked serviceAvailable = 'waiting';
  @tracked editingMediaId = null;
  @tracked originalMedia;

  @tracked uploadParams = {
    imageType: '',
    imageCategory: '',
    imageAlbum: '',
    imageAlt: '',
    imageTag: '',
  };
  @tracked showUploadArea = false;
  clipboard;

  constructor(...args) {
    super(...args);
    this.start();

    $(document).on('click', '.switchSelectInput', function () {
      let $this = $(this);
      let ref = $this.attr('data-ref');
      $(`.${ref}`).val('').toggle();
    });
  }

  async start() {
    await this.getRoots();
    await this.fetchMedia();
  }

  // #region OPERAZIONI PRELIMINARI
  async getRoots() {
    let tenantId = this.session.get('data.tenantId');

    this.availableTypologies = await this.store.query('media-category', {
      filter: `and(equals(tenantId,'${tenantId}'),equals(type,'typology'))`,
      sort: 'order',
    });
    let self = this;

    this.fetch
      .call('fileUpload/roots', 'GET', null, {}, true, this.session)
      .then((data) => {
        let temp = [];
        if (data.primaryContentType && data.primaryContentType.length > 0) {
          temp = [];
          data.primaryContentType.forEach((element) => {
            temp.push(element);
          });
          self.availableContentType = temp;
        }
        /*
        if (data.typology && data.typology.length > 0) {
          temp = [];
          data.typology.forEach((element) => {
            temp.push(element);
          });
          self.availableTypologies = temp;
        }
        if (data.category && data.category.length > 0) {
          temp = [];
          data.category.forEach((element) => {
            temp.push(element);
          });
          self.availableCategories = temp;
        }
        if (data.album && data.album.length > 0) {
          temp = [];
          data.album.forEach((element) => {
            temp.push(element);
          });
          self.availableAlbums = temp;
        }
        */
      })
      .catch(() => {
        self.serviceAvailable = 'unavailable';
      });
  }

  async getCategory(typologyId) {
    let tenantId = this.session.get('data.tenantId');

    this.availableCategories = await this.store.query('media-category', {
      filter: `and(equals(tenantId,'${tenantId}'),equals(type,'category'),equals(parentMediaCategory,'${typologyId}'))`,
      sort: 'order',
    });
  }

  async getAlbums(categoryId) {
    let tenantId = this.session.get('data.tenantId');

    this.availableAlbums = await this.store.query('media-category', {
      filter: `and(equals(tenantId,'${tenantId}'),equals(type,'album'),equals(parentMediaCategory,'${categoryId}'))`,
      sort: 'order',
    });
  }

  @action
  async fetchMedia() {
    this.editingMediaId = null;
    this.serviceAvailable = 'waiting';

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

    try {
      const response = await this.store.query('media-file', searchFilter);

      if (response) {
        this.medias = response.slice();
        this.totalPages =
          Math.ceil(response.meta.total / parseInt(this.filter.pageSize)) || 1;

        this.serviceAvailable = 'available';
      } else {
        this.serviceAvailable = 'unavailable';
      }
    } catch (e) {
      e.then((error) => {
        console.error(error);
        if (
          error.errors &&
          error.errors.length > 0 &&
          error.errors[0].status.toString() === '404'
        ) {
          this.serviceAvailable = 'available';
        } else {
          this.serviceAvailable = 'unavailable';
        }
      });
    }
  }
  // #endregion

  // #region VISUALIZZAZIONE MEDIA

  @action
  changeFilter(field, event) {
    this.currentPage = 1;
    let val =
      event.target.value && event.target.value !== ''
        ? event.target.value.trim()
        : '';
    if (field) {
      this.filter[field] = val;
      console.warn(this.filter[field]);
    }
    this.fetchMedia();

    if (field === 'imageType') {
      this.getCategory(val);
    } else if (field === 'imageCategory') {
      this.getAlbums(val);
    }
  }

  @action
  editMedia(media) {
    this.originalMedia = {
      alt: media.alt,
      tag: media.tag,
    };
    this.editingMediaId = media.id;
  }

  @action
  updateMediaField(media, field, event) {
    media[field] = event.target.value;
  }

  @action
  cancelEditMedia(media) {
    media.alt = this.originalMedia.alt;
    media.tag = this.originalMedia.tag;

    media.notifyPropertyChange('alt');
    media.notifyPropertyChange('tag');

    this.editingMediaId = null;
    this.originalMedia = null;
  }

  @action
  isEditing(mediaId) {
    return this.editingMediaId === mediaId;
  }

  @action
  async deleteMedia(media) {
    await media.destroyRecord();
    this.medias = this.medias.filter((m) => m.id !== media.id);
    this.getRoots();
  }

  @action
  async saveMedia(media) {
    this.editingMediaId = null;
    await media
      .save()
      .then(() => {
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

  @action
  async goToPage(where) {
    console.log('change page');
    if (where === 'previous' && this.currentPage !== 1) {
      this.currentPage--;
      await this.fetchMedia();
    } else if (where === 'next' && this.currentPage < this.totalPages) {
      this.currentPage++;
      await this.fetchMedia();
    }
  }

  @action
  afterRenderHook() {
    // Abilito i pulsanti di copia dei guid
    let self = this;
    // eslint-disable-next-line no-undef
    this.clipboard = new ClipboardJS('.copy-guid');
    this.clipboard.off('success');
    this.clipboard.on('success', function (e) {
      self.dialogs.toast('copiato', 'message', 'bottom-right', 1);
      e.clearSelection();
    });
  }

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

  @action
  noUrlAvailable() {
    // eslint-disable-next-line no-undef
    Swal.fire(
      'Non disponibile!',
      'Le impostazioni del sito non consentono il riferimento alle immagini via url. Usa il Base64!',
      'warning'
    );
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);

    // Rimuovi il listener quando il componente viene distrutto
    this.clipboard.off('success');
  }

  //#endregion

  // #region UPLOAD MEDIA
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

    if (field === 'imageTypology') {
      this.getCategory(val);
    } else if (field === 'imageCategory') {
      this.getAlbums(val);
    }
  }

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
      //'Content-Type': 'application/vnd.api+json',
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
        if (response && response.ok) {
          return response.json();
        } else {
          throw new Error('Errore nella richiesta HTTP');
        }
      })
      .then(async () => {
        $(`#uploadAlt, #uploadTag`).val('');
        await self.getRoots();
        self.fetchMedia();
      })
      .catch(() => {
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

  // errore di caricamento della foto
  @action
  async uploadFailed(file) {
    console.warn(file);
  }

  @action
  async uploadSucceeded(file) {
    console.warn(file);
  }

  // verifica il formato della foto da caricare (jpg, png)
  @action
  validateFile(file) {
    const allowedTypes = ['image/jpeg', 'image/png', 'application/pdf'];
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
  //#endregion
}
