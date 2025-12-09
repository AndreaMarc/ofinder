/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { service } from '@ember/service';
import { action } from '@ember/object';
import { htmlSafe } from '@ember/template';
import { tracked } from '@glimmer/tracking';
import config from 'poc-nuovo-fwk/config/environment';
import { task } from 'ember-concurrency';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import $ from 'jquery';

export default class StandardCoreUserProfileComponent extends Component {
  @service('siteSetup') stp;
  @service siteLayout;
  @service fileQueue;
  @service jsUtility;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  @tracked medias;
  @tracked profileImageAvailable = 'waiting';
  @tracked currentPic = ''; // per componente ShowImage nel template
  @tracked currentPicByUrl = true; // per componente ShowImage nel template
  @tracked showUploadArea = false;
  @tracked profileImageId = '';
  @tracked selectedPic;

  @tracked profileDataAvailable = 'waiting';
  @tracked registrationFields = null;
  @tracked currentTenant = null;
  @tracked oldValue = null;
  @tracked newValue = null; // usato nella modifica del record
  @tracked nations = [];
  @tracked editingRecord = null;

  constructor(...args) {
    super(...args);

    this.registrationFields = this.stp.siteSetup.registrationFields.profile;
    this.currentTenant = this.session.get('data.tenantId');
    this.nations = this.jsUtility.nations();

    setTimeout(() => {
      this.setupSelect2('#usersCrudBirthState');
      this.setupSelect2('#usersCrudResidenceState');
    }, 80);

    this.editRecord();
    this.fetchMedia();
  }

  get activeFieldsNumber() {
    let count = 0;
    for (const key in this.registrationFields) {
      if (
        Object.prototype.hasOwnProperty.call(this.registrationFields, key) &&
        this.registrationFields[key] !== '0'
      ) {
        count++;
      }
    }
    return count;
  }

  get lastName() {
    return this.session.get('data.lastName');
  }
  get firstName() {
    return this.session.get('data.firstName');
  }

  // ASPETTO
  // #region ASPETTO
  get getTextColor() {
    let out =
      this.siteLayout.headerLight === 'white' ? 'text-white' : 'text-dark';
    return htmlSafe(out);
  }
  get getBgColor() {
    let out = this.siteLayout.headerBackground;
    return htmlSafe(out);
  }

  get logoutStyle() {
    let out = '';
    if (this.siteLayout.headerLight === 'white') {
      out = `border: 1px solid #ffffff`;
    }
    return htmlSafe(out);
  }
  // #endregion

  // #region FOTO PROFILO
  // DOWNLOAD DELLE FOTO PROFILO
  @action
  async fetchMedia() {
    this.profileImageId = this.session.get('data.profileImageId') || '';

    // compongo i filtri di ricerca
    let searchFilter = {
      filter: [],
      size: 'sm',
    };

    searchFilter.filter.push({
      function: 'equals',
      column: 'primaryContentType',
      value: 'image',
    });

    searchFilter.filter.push({
      function: 'equals',
      column: 'typologyArea',
      value: 'userPics',
    });

    searchFilter.filter.push({
      function: 'equals',
      column: 'category',
      value: 'userPics',
    });

    searchFilter.filter.push({
      function: 'equals',
      column: 'album',
      value: 'profilePics',
    });

    searchFilter.filter.push({
      function: 'equals',
      column: 'userGuid',
      value: this.session.get('data.id'),
    });

    searchFilter.filter.push({
      function: 'equals',
      column: 'tenantId',
      value: this.session.get('data.tenantId'),
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

    try {
      const response = await this.store.query('media-file', searchFilter);

      if (response) {
        this.medias = response.slice();

        if (this.medias.length === 0) {
          this.currentPic = '/assets/images/avatars/user-icon.png';
          this.currentPicByUrl = true;
        } else {
          this.currentPic = this.profileImageId;
          this.currentPicByUrl = false;
        }

        this.profileImageAvailable = 'available';
      } else {
        this.profileImageAvailable = 'unavailable';
      }
    } catch (e) {
      if (e instanceof Promise) {
        e.then((error) => {
          if (
            error.errors &&
            error.errors[0] &&
            error.errors[0].status === '404'
          ) {
            // non ha alcuna foto profilo
            this.medias = [];
            this.currentPic = '/assets/images/avatars/user-icon.png';
            this.currentPicByUrl = true;
            this.profileImageAvailable = 'available';
          } else {
            // errore generico
            this.profileImageAvailable = 'unavailable';
          }
        });
      } else {
        console.error(e);
        this.medias = [];
        this.currentPic = '/assets/images/avatars/user-icon.png';
        this.currentPicByUrl = true;
        this.profileImageAvailable = 'available';
      }
    }
  }

  get numberPics() {
    return this.medias.length;
  }

  @action
  enableUpload() {
    this.showUploadArea = !this.showUploadArea;
  }

  @action
  triggerFileInput() {
    let fileInput = document.getElementById('user-profile-input');
    if (fileInput) {
      fileInput.click(); // Triggers the file input click event
    }
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

  // caricamento foto profilo
  uploadPhoto = task({ drop: true }, async (file) => {
    let self = this;
    let queue = this.fileQueue.find('userProfile');

    let d = new Date();
    let endpoint = `${config.apiHost}/${config.namespaceHost}/fileUpload`;
    let currentLocation = window.location;

    let baseEndpoint = '';
    if (typeof window.cordova === 'undefined') {
      baseEndpoint = `${currentLocation.protocol}//${currentLocation.hostname}${
        currentLocation.port !== '' ? ':' + currentLocation.port : ''
      }`;
    } else {
      baseEndpoint = config.feHost;
    }

    file
      .upload(endpoint, {
        method: 'POST',
        headers: {
          authorization: `${this.session.get('data.access_token')}`,
          accept: 'application/vnd.api+json',
          platform: typeof window.cordova !== 'undefined' ? 'app' : 'web',
          'access-Control-Allow-Origin': '*',
          fingerprint: self.session.getFingerprint(),
          baseEndpoint: baseEndpoint,
          tenantId: self.session.get('data.tenantId') || 1,
        },
        data: {
          typologyArea: 'userPics',
          category: 'userPics',
          album: 'profilePics',
          tenantId: this.session.get('data.tenantId').toString(),
          userGuid: this.session.get('data.id').toString(),
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
          global: false,
        },
      })
      .then((response) => {
        if (response && response.ok) {
          return response.json();
        } else {
          throw new Error('Errore nella richiesta HTTP');
        }
      })
      .then(async (res) => {
        // caricamento foto avvenuto correttamente
        res = res.data.attributes;
        let imageId = res.StringId;
        this.showUploadArea = false;
        // Aggiorno il campo profileImageId dei dati utente con il valore StringId restituito
        await self.store
          .queryRecord('user-profile', {
            filter: `equals(userId,'${this.session.get('data.id')}')`,
          })
          .then(function (userProfile) {
            userProfile.profileImageId = imageId;
            userProfile.save();
          })
          .catch((e) => {
            console.error(e);
          });

        self.session.set('data.profileImageId', imageId);

        // aggiorno anche immagine profilo in header
        let base64 = await this.getRemotePic(imageId);
        let headImg = document.getElementsByClassName('header-user-menu-img');
        for (var i = 0; i < headImg.length; i++) {
          headImg[i].src = base64;
        }

        self.fetchMedia();
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
  });

  // errore di caricamento della foto
  @action
  async uploadFailed(file) {
    //console.warn(file);
  }

  @action
  async uploadSucceeded(file) {
    //console.warn(file);
  }

  // uso una foto esistente come foto profilo attuale
  choicePic = task({ drop: true }, async (picId, base64) => {
    try {
      this.selectedPic = picId;
      await this.store
        .queryRecord('user-profile', {
          filter: `equals(userId,'${this.session.get('data.id')}')`,
        })
        .then(async (post) => {
          post.profileImageId = picId;
          await post.save();
          this.session.set('data.profileImageId', picId);
        });

      this.profileImageId = picId;
      this.currentPic = base64;
      this.currentPicByUrl = true;

      // aggiorno anche immagine profilo in header
      let headImg = document.getElementsByClassName('header-user-menu-img');
      for (var i = 0; i < headImg.length; i++) {
        headImg[i].src = this.currentPic;
      }
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprovare!',
        'error',
        'bottom-right',
        3,
        null
      );
    }
  });

  delPic = task({ drop: true }, async (picId) => {
    this.selectedPic = picId;

    try {
      let pic = this.store.peekRecord('media-file', picId);
      await pic.destroyRecord();

      if (this.session.get('data.profileImageId') === picId) {
        // sto cancellando l'attuale foto profilo -> aggiorno tabella user-profile
        await this.store
          .queryRecord('user-profile', {
            filter: `equals(userId,'${this.session.get('data.id')}')`,
          })
          .then(async (post) => {
            post.profileImageId = '';
            await post.save();
          });

        this.profileImageId = '';
        this.currentPic = '/assets/images/avatars/user-icon.png';
        this.currentPicByUrl = true;
        this.session.set('data.profileImageId', '');

        // aggiorno anche immagine profilo in header
        let headImg = document.getElementsByClassName('header-user-menu-img');
        for (var i = 0; i < headImg.length; i++) {
          headImg[i].src = '/assets/images/avatars/user-icon.png';
        }
      }

      this.fetchMedia();
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprovare!',
        'error',
        'bottom-right',
        3,
        null
      );
    }
  });

  // recupero dal DB il base64 della foto profilo
  getRemotePic = async (imageId) => {
    let out = '/assets/images/avatars/user-icon.png';
    console.warn(
      'Recupero nuova foto profilo per Header',
      this.session.get('data.profileImageId')
    );

    if (
      this.session.get('data.profileImageId') &&
      this.session.get('data.profileImageId') !== ''
    ) {
      await this.store
        .findRecord('media-file', imageId)
        .then((response) => {
          out = response.base64;
        })
        .catch((e) => {
          console.error('errore recupero foto profilo per pagina profilo', e);
        });
    }
    return out;
  };

  // TODO : aggiungere visualizzazione ingrandita delle foto al click su di esse
  // #endregion

  // #region DATI PERSONALI
  // abilito modifica di un record
  @action
  async editRecord() {
    await this.store
      .queryRecord('user', {
        include: 'userProfile',
        filter: `equals(id,'${this.session.get('data.id')}')`,
      })
      .then((rec) => {
        this.editingRecord = rec;
        rec.get('userProfile').then((userProfile) => {
          let userProfileData = userProfile.serialize();

          this.newValue = this.initializeRecord();
          this.oldValue = this.initializeRecord();
          Object.keys(userProfileData.data.attributes).forEach((key) => {
            if (key === 'birthDate') {
              let val = null;
              if (userProfileData.data.attributes[key]) {
                let offset = new Date().getTimezoneOffset(); // sottraggo offset del fuso orario
                val = new Date(
                  new Date(userProfileData.data.attributes[key]).getTime() -
                    offset * 60 * 1000
                );
              }
              this.newValue[key] = val;
              this.oldValue[key] = val;
            } else {
              this.newValue[key] = userProfileData.data.attributes[key];
              this.oldValue[key] = userProfileData.data.attributes[key];
            }
          });
          this.newValue.id = userProfile.id;
          this.newValue.email = rec.email;
          this.profileDataAvailable = 'available';
        });
      })
      .catch((e) => {
        console.error(e);
        this.profileDataAvailable = 'unavailable';
        this.dialogs.toast(
          'Si è verificato un errore. Riprova!',
          'error',
          'bottom-right',
          3
        );
      });
  }

  // cattura il valore inserito dall'utente nella modifica del record
  @action
  storeNewValue(field, event) {
    let val = '';
    if (field === 'birthDate') {
      try {
        val = new Date(event.target.value + 'T00:00:00Z'); // Crea un oggetto data con orario impostato a mezzanotte
      } catch (e) {
        console.error(e);
        val = null;
      }
    } else val = event.target.value.trim();

    this.newValue[field] = val;
  }

  // chiede conferma per salvataggio modifiche al record
  @action
  saveVoice() {
    let errors = 0;
    Object.keys(this.registrationFields).forEach((key) => {
      let required = this.registrationFields[key] === '2';
      if (required) {
        if (!this.newValue[key] || this.newValue[key].toString() === '') {
          errors++;
        }
      }
    });

    if (errors > 0) {
      this.dialogs.toast(
        `Tutti i campi contrassegnati dall'asterisco sono obbligatori`,
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    let regex;
    errors = [];

    // verifica lunghezza nome e cognome
    if (
      (this.newValue.lastName !== '' && this.newValue.lastName.length < 2) ||
      (this.newValue.firstName !== '' && this.newValue.firstName.length < 2)
    ) {
      errors.push(`Inserisci almeno due caratteri nei campi Nome e Cognome`);
    }

    // verifica del codice fiscale
    regex = this.jsUtility.regex('taxId');
    console.warn('taxId', this.newValue.taxId);
    if (
      this.newValue.taxId &&
      this.newValue.taxId !== '' &&
      !regex.test(this.newValue.taxId)
    ) {
      errors.push(`Il formato del campo 'Codice Fiscale' non è corretto`);
    }

    // TODO : aggiungere verifiche e blocchi sull'eventuale campo nick-name

    if (errors.length > 0) {
      this.dialogs.toast(errors[0], 'error', 'bottom-right', 4);
      return false;
    }

    let self = this;
    this.dialogs.confirm(
      '<h6>Modifica dati personali</h6>',
      `<p>Confermi la modifica?</p>`,
      () => {
        self.updateUser.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  // aggiorna i dati dell'utente nel DB
  updateUser = task({ drop: true }, async () => {
    let self = this;
    try {
      let existingUser = await this.store.findRecord(
        'user-profile',
        this.newValue.id
      );

      Object.keys(self.newValue).forEach((key) => {
        if (key !== 'id') {
          existingUser[key] = self.newValue[key];
        }
      });

      await existingUser
        .save()
        .then(() => {
          this.dialogs.toast(
            `Salvataggio eseguito con successo`,
            `success`,
            'bottom-right',
            3
          );
        })
        .catch(() => {
          this.dialogs.toast(
            `Si è verificato un errore. Riprova!`,
            `error`,
            'bottom-right',
            3
          );
        });

      this.fetchMedia();
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprova!`,
        'error',
        'bottom-right',
        4
      );
    }
  });

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord('user-profile'); // Crea una nuova istanza del modello (ad esempio, 'user')
    // Crea un semplice oggetto JavaScript con tutte le proprietà del modello
    let modelData = {};
    modelInstance.eachAttribute((key) => {
      modelData[key] = modelInstance.get(key);
    });

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }

  // Applica select2 e ne gestisce la variazione
  @action
  setupSelect2(element) {
    $(element).select2({
      theme: 'bootstrap4',
      width: '100%',
    });

    // Make sure to sync changes made by Select2 with Ember
    $(element).on('change', () => {
      let field = $(element).attr('data-field');
      let selectedValue = $(element).val();
      // eslint-disable-next-line ember/jquery-ember-run
      this.newValue[field] = selectedValue;
    });
  }

  @action
  teardownSelect2(element) {
    $(element).off('change');
    $(element).select2('destroy');
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);
    // Rimuovi il listener quando il componente viene distrutto
    this.teardownSelect2('#usersCrudBirthState');
    this.teardownSelect2('#usersCrudResidenceState');
  }
  // #endregion

  @action
  requestDeleteProfile() {
    let self = this;
    this.dialogs.confirm(
      '<h6>Cancellazione del profilo</h6>',
      `<p>Stai richiedendo la Cancellazione del tuo profilo. Non potrai più accedere al portale.<br />Sei sicuro?</p>
        <p><small>Il nostro responsabile del Trattamento dei Dati riceverà la tua richiesta e ti comunicherà l'avvenuta cancellazione nei tempi previsti dalla Legge</small></p>`,
      () => {
        self.deleteProfile();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  deleteProfile() {
    this.fetch
      .call('deleteSelf', 'DELETE', {}, {}, true, this.session)
      .then(async () => {
        this.session.invalidateSessionGeneral();
        await this.jsUtility.sleep(2500);
      })
      .catch((e) => {
        this.dialogs.toast(
          `Si è verificato un errore. Riprova!`,
          'error',
          'bottom-right',
          4
        );
      });
  }
}
