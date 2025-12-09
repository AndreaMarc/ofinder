/**
 * CREA IL COMPONENTE PER IL SETUP DEGLI SLIDER
 * Gestisce la visibilità, la posizione, le foto e i testi degli sliders delle
 * pagine di login, di registrazione e di termini&condizioni.
 *
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::Core::SetupSliders/>
 *
 * NOTA:
 * L'elenco delle foto degli sliders è contenuto nella variabile sliderPics
 * di Setup. Le foto sono caricate tutte nell'album "Welcome".
 */

/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import config from 'poc-nuovo-fwk/config/environment';
import { htmlSafe } from '@ember/template';
import { task } from 'ember-concurrency';
import $ from 'jquery';
import { v4 } from 'ember-uuid';

export default class StandardCoreSetupSlidersComponent extends Component {
  @service('siteLayout') serviceLayout;
  @service jsUtility;
  @service session;
  @service dialogs;
  @service store;
  @service fileQueue;

  recordApp = null;
  recordWeb = null;
  siteLayout;

  @tracked savedChanges = true;
  @tracked serviceAvailable = 'waiting';
  @tracked sliderPositionApp = '';
  @tracked sliderPositionWeb = '';
  @tracked sliderRegistrationPositionWeb = '';
  @tracked sliderTermsPositionWeb = '';
  @tracked sliderPics;
  @tracked numberPicsLogin = 0;
  @tracked numberPicsRegistration = 0;
  @tracked numberPicsTerms = 0;

  @tracked monitoredSlider = 'login';
  @tracked typologyId = '';
  @tracked categoryId = '';
  @tracked albumId = '';

  // helper locale di sanificazione stile css
  htmlsafe = (string) => htmlSafe(`background-image: url(${string})`);

  /**
   * INIZIALIZZAZIONE
   */

  constructor(...attributes) {
    super(...attributes);
    this.siteLayout = this.serviceLayout;
    this.start();
  }

  @action
  async start() {
    try {
      // recupero i record di setup
      this.recordWeb = this.args.recordWeb;
      this.recordApp = this.args.recordApp;
      this.serviceAvailable = this.args.serviceAvailable;

      // recupero gli id degli album di carimento foto
      let typologyArea = await this.store.queryRecord('media-category', {
        filter: `and(equals(tenantId,'1'),equals(type,'typology'),equals(code,'system'))`,
      });
      this.typologyId = typologyArea.id;

      let category = await this.store.queryRecord('media-category', {
        filter: `and(equals(tenantId,'1'),equals(type,'category'),equals(code,'sliders'),equals(parentMediaCategory,'${typologyArea.id}'))`,
      });
      this.categoryId = category.id;

      let album = await this.store.queryRecord('media-category', {
        filter: `and(equals(tenantId,'1'),equals(type,'album'),equals(code,'welcome'),equals(parentMediaCategory,'${this.categoryId}'))`,
      });
      this.albumId = album.id;

      if (this.serviceAvailable === 'available') {
        this.valueAdapter();
      }
      // eslint-disable-next-line no-empty
    } catch (e) {}
  }

  valueAdapter() {
    this.sliderPositionApp = this.recordApp.sliderPosition;
    this.sliderPositionWeb = this.recordWeb.sliderPosition;
    this.sliderRegistrationPositionWeb =
      this.recordWeb.sliderRegistrationPosition;
    this.sliderTermsPositionWeb = this.recordWeb.sliderTermsPosition;
    let self = this;

    // non necessario (attualmente) distinguere tra foto per web ed app
    let sp = this.recordWeb.sliderPics;

    if (sp && sp !== '' && sp !== '""') {
      this.sliderPics =
        typeof this.recordWeb.sliderPics === 'string'
          ? JSON.parse(this.recordWeb.sliderPics)
          : this.recordWeb.sliderPics;

      this.numberPicsLogin = this.sliderPics.filter((item) => {
        return item.page === 'login' && item.active;
      }).length;

      let registrationPics = this.sliderPics.filter((item) => {
        return item.page === 'registration' && item.active;
      });
      this.numberPicsRegistration = registrationPics
        ? registrationPics.length
        : 0;

      let termsPics = this.sliderPics.filter((item) => {
        return item.page === 'terms' && item.active;
      });
      this.numberPicsTerms = termsPics ? termsPics.length : 0;
    } else this.sliderPics = [];

    // abilito i toggle
    setTimeout(() => {
      $(`.slider-active-pic`).bootstrapToggle();
      $(`.slider-byguid-pic`)
        .bootstrapToggle()
        .change(function () {
          let $this = $(this);
          let byUrl = $this.prop('checked');
          let index = $this.attr('data-index');
          let value = $(`.slider-url[data-index="${index}"]`).val();
          self.subChangeUrl(byUrl, index, value);
        });
    }, 100);
  }

  // al cambio dell'url, aggiorno l'anteprima della foto
  @action
  changeUrl(index, event) {
    // verifico se l'immagine corrente è basata su url o su guid
    let byUrl = $(`.slider-byguid-pic[data-index="${index}"]`).prop('checked');
    console.warn('test', byUrl, index, event.target.value);
    this.subChangeUrl(byUrl, index, event.target.value);
  }

  subChangeUrl(byUrl, index, value) {
    if (byUrl) {
      // è basata su url
      $(`.slider-image-preview[data-index="${index}"]`).css(
        'background-image',
        `url(${htmlSafe(value)})`
      );
    } else {
      // è basata su guid, scarico le informazioni della foto dal server
      // a58c55ef-6005-4c24-a833-08db63cb5b4c
      this.store
        .queryRecord(
          'media-file',
          {
            filter: `equals(id,'${value}')`,
            size: 'md',
          },
          { reload: true }
        )
        .then(function (fileInfo) {
          if (fileInfo.base64 !== '') {
            $(`.slider-image-preview[data-index="${index}"]`).css(
              'background-image',
              `url(${fileInfo.base64})`
            );
          } else {
            $(`.slider-image-preview[data-index="${index}"]`).css(
              'background-image',
              `url('assets/images/sliders/undefined_image.png')`
            );
            throw new Error(`No base64 found for guid image '${value}'`);
          }
        })
        .catch(() => {
          $(`.slider-image-preview[data-index="${index}"]`).css(
            'background-image',
            `url('assets/images/sliders/undefined_image.png')`
          );
        });
    }
  }

  // selezione dello slider su cui gestire le foto
  @action
  updateMonitored(evt) {
    let value = evt.target.value;
    this.monitoredSlider = value;

    this.getAlbumId();
  }

  // sposta in alto una foto
  @action
  moveUp(index, page) {
    console.warn(index, page);
    let sliderPics = this.sliderPics;

    let currentPic = sliderPics[index];
    for (let i = 0; i < sliderPics.length; i++) {
      if (i === index - 1 && sliderPics[i].page === page) {
        // la foto precedente è dello stesso slider, quindi posso procedere
        let previous = sliderPics[i];
        sliderPics[i] = currentPic;
        sliderPics[i + 1] = previous;
      }
    }

    this.sliderPics = sliderPics;
  }

  // cancella una foto
  @action
  deletePic(index) {
    let sliderPics = this.sliderPics;
    sliderPics.splice(index, 1);
    this.sliderPics = sliderPics;
    console.log(this.sliderPics);
  }

  // crea una nuova slide vuota
  @action
  newPic() {
    return new Promise((resolve) => {
      let slider = this.monitoredSlider;
      let sliderPics = this.sliderPics;
      sliderPics.push({
        page: slider,
        title: '',
        titleKey: '',
        description: '',
        descriptionKey: '',
        url: '',
        active: false,
        byUrl: false,
      });
      this.sliderPics = sliderPics;

      // abilito i toggle
      setTimeout(() => {
        $(`.slider-active-pic, .slider-byguid-pic`).bootstrapToggle();
        resolve();
      }, 200);
    });
  }

  savePics = task({ drop: true }, async () => {
    let arr = [];
    let $pics = $('.slider-pic-area');
    $.each($pics, function () {
      let $this = $(this);
      let index = $this.attr('data-index');
      let url = $(`.slider-url[data-index="${index}"]`).val();

      let pic = {
        page: $this.attr('data-page'),
        title: $(`.slider-title[data-index="${index}"]`).val(),
        titleKey: $(`.slider-title-key[data-index="${index}"]`).val(),
        description: $(`.slider-description[data-index="${index}"]`).val(),
        descriptionKey: $(
          `.slider-description-key[data-index="${index}"]`
        ).val(),
        url: url,
        active:
          url !== ''
            ? $(`.slider-active-pic[data-index="${index}"]`).prop('checked')
            : false,
        byUrl: $(`.slider-byguid-pic[data-index="${index}"]`).prop('checked'),
      };

      arr.push(pic);
    });
    //console.warn(arr);
    let evt = { target: { value: arr } };
    this.changeValue(null, 'both', 'sliderPics', evt);
    await this.save.perform();
    this.valueAdapter();
  });

  // aggiorna localmente i record di setup
  @action
  changeValue(dataType, environment, paramName, event) {
    let value = event.target.value;

    this.serviceLayout.updateParam(paramName, value);
    if (environment === 'app') {
      this.recordApp[paramName] = value;
    } else if (environment === 'web') {
      this.recordWeb[paramName] = value;
    } else {
      this.recordApp[paramName] = value;
      this.recordWeb[paramName] = value;
    }

    if (
      this.recordApp.hasDirtyAttributes ||
      this.recordWeb.hasDirtyAttributes
    ) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }
  }

  // salva su server le modifiche apportate
  save = task({ drop: true }, async () => {
    try {
      await this.recordWeb.save({ adapterOptions: { method: 'PATCH' } });
      await this.recordApp.save({ adapterOptions: { method: 'PATCH' } });
      this.savedChanges = true;
      this.dialogs.toast(
        'Salvataggio completato',
        'success',
        'bottom-right',
        3,
        null
      );
    } catch (e) {
      this.dialogs.toast(
        'Errore nel salvataggio. Riprovare!',
        'error',
        'bottom-right',
        4,
        null
      );
    }
  });

  /**
   * CARICAMENTO FOTO
   */

  // verifica il formato della foto da caricare (jpg, png)
  @action
  validateFile(file) {
    const allowedTypes = ['image/jpeg', 'image/png'];
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

  // caricamento della foto
  // a58c55ef-6005-4c24-a833-08db63cb5b4c
  @action
  async uploadPhoto(file) {
    let queue = this.fileQueue.find('sliders');

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
          typologyArea: this.typologyId,
          category: this.categoryId,
          album: this.albumId,
          tenantId: this.session.get('data.tenantId').toString(),
          userGuid: this.session.get('data.id').toString(),
          alt: '',
          tag: '',
          extension: '',
          base64: '',
          fileUrl: '',
          mongoGuid: '',
          originalFileName: '',
          uploadDate: `${d.getFullYear()}/${d.getMonth() + 1}/${d.getDate()}`,
          type: 'mediaFiles',
          global: true,
        },
      })
      .then((response) => {
        if (response.ok) {
          return response.json();
        } else {
          throw new Error('Errore nella richiesta HTTP');
        }
      })
      .then((response) => {
        this.newPic().then(() => {
          setTimeout(() => {
            console.warn('valorizzo il guid');
            let $el = $('.slider-url').last();
            let index = $el.attr('data-index');

            $(`.slider-byguid-pic[data-index="${index}"]`)
              .bootstrapToggle('off')
              .change();

            setTimeout(() => {
              $el.val(response.data.attributes.StringId).trigger('keyup');

              this.subChangeUrl(
                false,
                index,
                response.data.attributes.StringId
              );
            }, 150);
          }, 250);
        });
      })
      .catch((e) => {
        console.warn(e);
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
}
