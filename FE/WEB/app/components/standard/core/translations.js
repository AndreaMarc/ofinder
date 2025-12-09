/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import $ from 'jquery';

export default class StandardCoreTranslationsComponent extends Component {
  //@service session;
  @service dialogs;
  @service store;
  //@service fetch;
  @service translation;
  //@service('siteSetup') stp;
  //@service jsUtility;

  recordWeb;
  recordTranslation;
  translationApp;
  translationWeb;
  @tracked selectedLanguage = '';
  @tracked selectedEnvironment = '';
  @tracked availableLanguages = {};
  @tracked languageSetup = {};
  languageSetupTemp = {};
  @tracked languageVoiceExists = false;
  @tracked serviceAvailable = 'waiting';
  @tracked changedValue = 0;

  @tracked currentRow = '';
  @tracked currentFilter = '';

  constructor(...attributes) {
    super(...attributes);

    // funzione di ricerca
    $(document).on('change paste keyup', '.find-primary-key', function () {
      let $this = $(this);
      let ref = $this.attr('data-ref');
      let value = $this.val().toLowerCase();

      let $primaryRows = $(`.primary-filterable[data-ref="${ref}"]`);
      $primaryRows.filter(function () {
        $(this).toggle(
          $(this).attr('data-value').toLowerCase().indexOf(value) > -1
        );
      });
    });

    this.start();
  }

  @action
  async start() {
    this.selectedLanguage = '';
    this.serviceAvailable = 'waiting';
    try {
      this.recordWeb = await this.getRecord('web');

      if (!this.recordWeb) {
        throw new Error('settings not available');
      } else {
        if (
          Object.keys(this.recordWeb.languageSetup).length === 0 ||
          Object.keys(this.recordWeb.availableLanguages).length === 0
        ) {
          this.languageVoiceExists = false;
          this.languageSetup = {};
          this.languageSetupTemp = {};
        } else {
          this.languageVoiceExists = true;
          this.languageSetup = {};
          this.languageSetupTemp = this.recordWeb.languageSetup;
          this.availableLanguages = this.recordWeb.availableLanguages;

          this.serviceAvailable = 'available';
        }
      }
    } catch (e) {
      this.serviceAvailable = 'unavailable';
    }
  }

  @action
  changeLanguage(event) {
    let value = event.target.value;
    if (value === '') {
      this.selectedLanguage = '';
    } else {
      this.selectedLanguage = value;
      if (this.selectedLanguage !== '' && this.selectedEnvironment !== '') {
        this.getTranslation();
      }
    }
  }

  @action
  changeEnvironment(event) {
    let value = event.target.value;
    if (value === '') {
      this.selectedEnvironment = '';
    } else {
      this.selectedEnvironment = value;
      if (this.selectedLanguage !== '' && this.selectedEnvironment !== '') {
        this.getTranslation();
      }
    }
  }

  async getTranslation() {
    if (this.selectedLanguage === '' || this.selectedEnvironment === '') {
      return false;
    }
    this.languageSetup = {};
    this.changedValue = 0;

    let lang = await this.getRecordJsonApi(this.selectedLanguage);
    if (lang) {
      this.recordTranslation = lang;
      this.translationApp = lang.translationApp;
      this.translationWeb = lang.translationWeb;

      this.languageSetup = this.languageSetupTemp;

      // textarea autosize
      setTimeout(() => {
        document.querySelectorAll('textarea').forEach(function (element) {
          element.style.boxSizing = 'border-box';
          var offset = element.offsetHeight - element.clientHeight;
          element.addEventListener('input', function (event) {
            event.target.style.height = 'auto';
            event.target.style.height =
              event.target.scrollHeight + offset + 'px';
          });
          element.removeAttribute('data-autoresize');
        });
      }, 500);
    } else {
      this.serviceAvailable = 'unavailable';
    }
    return;
  }

  @action
  getTranslationValue(type, primaryRow, label) {
    let ret = '';
    try {
      let record =
        this.selectedEnvironment === 'app'
          ? this.translationApp
          : this.translationWeb;

      if (type && typeof record[type] !== 'undefined') {
        ret = record[type];
        if (primaryRow && typeof ret[primaryRow] !== 'undefined') {
          ret = ret[primaryRow];

          if (label && typeof ret[label] !== 'undefined') {
            ret = ret[label];
          }
        }
      }
    } catch (e) {
      console.warn(e);
    }
    return ret;
  }

  @action
  editVoice(key, subkey, finalkey) {
    if (!key || !subkey || key === '' || subkey === '') {
      return false;
    }

    let selector = finalkey
      ? `${key}_${subkey}_${finalkey}`
      : `${key}_${subkey}`;

    $(`.edit-field[data-key="${selector}"]`).prop('disabled', false);

    $(
      `.btn-edit-field[data-key="${selector}"], .btn-del-field[data-key="${selector}"], .btn-undo-field[data-key="${selector}"], .btn-save-field[data-key="${selector}"]`
    ).toggleClass('d-none');
    this.changedValue++;
  }

  @action
  undoVoice(key, subkey, finalkey, oldValue) {
    let selector = finalkey
      ? `${key}_${subkey}_${finalkey}`
      : `${key}_${subkey}`;

    $(`.edit-field[data-key="${selector}"]`)
      .prop('disabled', true)
      .val(oldValue);

    $(
      `.btn-edit-field[data-key="${selector}"], .btn-del-field[data-key="${selector}"], .btn-undo-field[data-key="${selector}"], .btn-save-field[data-key="${selector}"]`
    ).toggleClass('d-none');
    this.changedValue--;
  }

  @action
  saveVoices(environment) {
    let subMsg = '';
    if (environment === 'both') {
      subMsg = `<br />
        <p class="bg-warning text-dark p-2"><strong>NOTA:</strong><br />L'intera traduzione dell'ambiente "App" verrà sostituita da quella per il "Web"</p>`;
    }
    this.dialogs.confirm(
      'Salvataggio modifiche',
      `<div class="text-danger">
        <h5>Azione irreversibile. Confermi il salvataggio?</h5>
        <h6>Le modifiche saranno subito disponibili.</h6>
        ${subMsg}`,
      () => {
        this.saveConfirmed(environment);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  saveConfirmed(environment) {
    // ricostruisco il json
    let translations = {};
    let $key = $('.translation-key');
    $.each($key, function () {
      let $this = $(this);
      let key = $this.attr('data-key');
      translations[key] = {};

      let _subkey = `.translation-primary-row[data-parent-key='${key}']`;
      let $subKey = $(_subkey);
      $.each($subKey, function () {
        let $this2 = $(this);
        let subKey = $this2.attr('data-subkey');
        translations[key][subKey] = {};

        let _labels = `.translation-label[data-parent-key='${key}'][data-parent-pr='${subKey}']`;
        let $labels = $(_labels);
        $.each($labels, function () {
          let $this3 = $(this);
          let type = $this3.attr('data-type');

          if (type === 'obj') {
            // è un oggetto
            let label = $this3.attr('data-label');
            let value = $this3.val();
            translations[key][subKey][label] = value;
          } else {
            // è una stringa
            translations[key][subKey] = $this3.val();
          }
        });
      });
    });

    if (environment === 'web') {
      this.recordTranslation.translationWeb = translations;
    }
    if (environment === 'both') {
      this.recordTranslation.translationWeb = translations;
      this.recordTranslation.translationApp = translations;
    }
    if (environment === 'app') {
      this.recordTranslation.translationApp = translations;
    }

    try {
      this.recordTranslation.save();
      this.getTranslation();

      // se sto modificando la lingua corrente, aggiorno la traduzione nel servizio dedicato,
      // così da rendere disponibili le modifiche senza ricaricare la pagina
      if (this.translation.currentLang === this.selectedLanguage) {
        this.translation.changeLanguage(this.translation.currentLang);
      }
    } catch (e) {
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        4
      );
    }
  }

  // scarica i record di setup
  async getRecord(environment) {
    let self = this;
    return new Promise((resolve) => {
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

  // scarica il record di traduzione
  async getRecordJsonApi(code) {
    let self = this;
    return new Promise((resolve) => {
      try {
        self.store
          .queryRecord('translation', {
            filter: `equals(languageCode,'${code}')`,
          })
          .then(function (record) {
            resolve(record);
          });
      } catch (e) {
        resolve(null);
      }
    });
  }

  @action
  encode(value) {
    return encodeURI(value);
  }

  @action
  openRow(key) {
    if (this.currentRow !== key) {
      this.currentRow = key;
    } else {
      this.currentRow = '';
    }
  }

  @action
  insertFilter(event) {
    this.currentFilter = event.target.value;
  }

  // triggerato all'inserimento di ciascuna riga. Grazie a questa funzione,
  // se avevo un filtro di ricerca impostato, esso viene ripristinato
  @action
  triggerFilter() {
    $('.find-primary-key').trigger('change');
  }
}
