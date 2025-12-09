/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import config from 'poc-nuovo-fwk/config/environment';
//import { htmlSafe } from '@ember/template';
import $ from 'jquery';

export default class StandardCoreTranslateStructureComponent extends Component {
  @service session;
  @service dialogs;
  @service store;
  @service fetch;
  @service translation;
  @service('siteSetup') stp;
  @service jsUtility;

  recordApp;
  recordWeb;
  @tracked languageSetup = [];
  @tracked languageVoiceExists = false;
  @tracked serviceAvailable = 'waiting';

  @tracked objectType = '';
  @tracked objectPrimaryRow = '';
  @tracked objectLabel = '';
  @tracked objectValue = '';

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
    this.serviceAvailable = 'waiting';
    try {
      this.recordWeb = await this.getRecord('web');
      this.recordApp = await this.getRecord('app');

      if (!this.recordWeb || !this.recordApp) {
        throw new Error('settings not available');
      } else {
        if (Object.keys(this.recordWeb.languageSetup).length > 0) {
          this.languageVoiceExists = true;
          this.languageSetup = this.recordWeb.languageSetup;
        } else {
          this.languageVoiceExists = false;
          this.languageSetup = [];
        }

        this.serviceAvailable = 'available';

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
      }
    } catch (e) {
      this.serviceAvailable = 'unavailable';
    }
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);
    $(document).off('change paste kayup', '.findPrimaryKey');
  }

  // scarica i record di setup
  async getRecord(environment) {
    let self = this;
    return new Promise((resolve) => {
      // recupero i record di setup
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

  // cambio tipologia del nuovo oggetto
  @action
  changeObjType(event) {
    this.objectType = event.target.value;
  }

  // cambio chiave primaria del nuovo oggetto
  @action
  changePrimaryRow(event) {
    this.objectPrimaryRow = event.target.value;
  }

  // cambio label del nuovo oggetto
  @action
  changeLabel(event) {
    this.objectLabel = event.target.value;
  }

  // cambio value del nuovo oggetto
  @action
  changeValue(event) {
    this.objectValue = event.target.value;
  }

  // creazione nuovo oggetto
  @action
  createObj(type, primaryRow, label, value) {
    if (!this.recordWeb) {
      this.dialogs.toast(
        'Si è verificato un errore. Ricaricare la pagina',
        'error',
        'bottom-right',
        5
      );
      this.serviceAvailable = 'unavailable';
      return false;
    }

    if (type) {
      this.objectType = type;
    }
    if (primaryRow) {
      this.objectPrimaryRow = primaryRow;
    }
    if (label) {
      this.objectLabel = label;
    }
    if (value) {
      this.objectValue = value;
    }

    let newVoice = {
      type: this.objectType,
      primaryRow: this.objectPrimaryRow,
      label: this.objectLabel,
      value: this.objectValue,
    };

    // verifica integrità dei dati
    if (newVoice.label === '') {
      this.dialogs.toast(
        'Il Nome del Campo è obbligatorio!',
        'error',
        'bottom-right',
        3
      );
      return false;
    }
    if (newVoice.type === '') {
      this.dialogs.toast(
        'La tipologia è obbligatoria!',
        'error',
        'bottom-right',
        3
      );
      return false;
    }

    let regex = this.jsUtility.regex('lnu');
    if (
      (newVoice.primaryRow !== '' && !regex.test(newVoice.primaryRow)) ||
      (value !== '' && !regex.test(newVoice.label))
    ) {
      this.dialogs.toast(
        'Nei campi nome/chiave sono consentite solo lettere, numeri e underscore',
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    /*if (this.languageSetup[newVoice.label]) {
      this.dialogs.toast(
        'La label scelta esiste già!',
        'error',
        'bottom-right',
        4
      );
      return false;
    }*/

    // chiamata http per memorizzare il dato nelle varie traduzioni esistenti
    let host = config.apiHost;
    let namespace = config.namespaceHost;
    let self = this;

    this.fetch
      .call('translations/addVoice', 'POST', newVoice, {}, true, this.session)
      .then((data) => {
        // Elabora i dati ricevuti
        console.log(data);

        // aggiorno il servzio siteSetup con il nuovo valore di languageSetup
        let newLanguageSetup = JSON.parse(data[0].languageSetup);
        self.recordWeb.languageSetup = newLanguageSetup;
        self.recordApp.languageSetup = newLanguageSetup;
        self.stp.setSetup('languageSetup', newLanguageSetup);
        self.languageSetup = newLanguageSetup;

        // scarico di nuovo la traduzione cosicché contenga il nuovo campo, con il suo valore di default
        self.translation.changeLanguage(self.translation.currentLang);
      })
      .catch((error) => {
        // Gestisci l'errore
        let msg = 'Si è verificato un errore. Riprovare!';
        if (error && error.title) {
          if (['Already exists'].includes(error.title)) {
            msg = 'Elemento già esistente';
          }
        }

        this.dialogs.toast(msg, 'error', 'bottom-right', 4);
      });
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
  }

  @action
  delVoice(key, subkey, finalkey) {
    if (!key || !subkey || key === '' || subkey === '') {
      this.dialogs.toast(
        'Si è verificato un errore. Ricaricare la pagina',
        'error',
        'bottom-right',
        5
      );
      return false;
    }

    this.dialogs.confirm(
      'Cancellazione del valore predefinito',
      `<div class="text-danger">
        <h3>ATTENZIONE</h3>
        <h5>Stai eliminando un valore predefinito dalla struttura. Questo comporta l'eliminazione della voce anche da tutti i file di traduzione</h5>
        <h5>Azione irreversibile.</h5>
        <br />
        <h6>CONFERMI?</h6>
      </div>`,
      () => {
        this.delVoiceConfirmed(key, subkey, finalkey);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  delVoiceConfirmed(key, subkey, finalkey) {
    let newVoice = {
      type: key,
      primaryRow: subkey,
      label: finalkey ? finalkey : '',
      value: '',
    };

    // chiamata http per cancellare la voce
    let host = config.apiHost;
    let namespace = config.namespaceHost;
    let self = this;

    this.fetch
      .call('translations/delVoice', 'DELETE', newVoice, {}, true, this.session)
      .then((data) => {
        // Elabora i dati ricevuti
        console.log(data);

        // aggiorno il servzio siteSetup con il nuovo valore di languageSetup
        let newLanguageSetup = JSON.parse(data[0].languageSetup);
        self.recordWeb.languageSetup = newLanguageSetup;
        self.recordApp.languageSetup = newLanguageSetup;
        self.stp.setSetup('languageSetup', newLanguageSetup);
        self.languageSetup = newLanguageSetup;

        // scarico di nuovo la traduzione
        self.translation.changeLanguage(self.translation.currentLang);
      })
      .catch((error) => {
        // Gestisci l'errore
        console.error(error);
        this.dialogs.toast(
          'Si è verificato un errore. Riprovare!',
          'error',
          'bottom-right',
          4
        );
      });
  }

  @action
  saveVoice(key, subkey, finalkey) {
    let selector = finalkey
      ? `${key}_${subkey}_${finalkey}`
      : `${key}_${subkey}`;

    let value = $(`.edit-field[data-key="${selector}"]`).val();

    let newVoice = {
      type: key,
      primaryRow: subkey,
      label: finalkey ? finalkey : value,
      value: finalkey ? value : '',
    };

    // chiamata http per cancellare la voce
    let host = config.apiHost;
    let namespace = config.namespaceHost;
    let self = this;

    this.fetch
      .call(
        'translations/patchVoice',
        'PATCH',
        newVoice,
        {},
        true,
        this.session
      )
      .then((data) => {
        // Elabora i dati ricevuti
        console.log(data);

        // aggiorno il servzio siteSetup con il nuovo valore di languageSetup
        let newLanguageSetup = JSON.parse(data[0].languageSetup);
        self.recordWeb.languageSetup = newLanguageSetup;
        self.recordApp.languageSetup = newLanguageSetup;
        self.stp.setSetup('languageSetup', newLanguageSetup);
        self.languageSetup = newLanguageSetup;

        // scarico di nuovo la traduzione
        self.translation.changeLanguage(self.translation.currentLang);

        // ripristino i pulsanti
        $(`.edit-field[data-key="${selector}"]`).prop('disabled', true);

        $(
          `.btn-edit-field[data-key="${selector}"], .btn-del-field[data-key="${selector}"], .btn-undo-field[data-key="${selector}"], .btn-save-field[data-key="${selector}"]`
        ).toggleClass('d-none');
      })
      .catch((error) => {
        // Gestisci l'errore
        console.error(error);
        this.dialogs.toast(
          'Si è verificato un errore. Riprovare!',
          'error',
          'bottom-right',
          4
        );
      });
  }

  // estrae lunghezza array
  getArrayLength(model) {
    let languageSetup = model.languageSetup;
    return languageSetup.length > 0 ? true : false;
  }

  @action
  encode(value) {
    return encodeURI(value);
  }

  @action
  toggleAddObj(key, subkey, type) {
    $('.toggleAdd').removeClass('show');
    $(
      `.toggleAdd[data-key="${this.encode(key)}"][data-subkey="${this.encode(
        subkey
      )}"][data-type="${type}"]`
    ).addClass('show');
    $('.toggleAddField').val('');
    this.objectType = '';
    this.objectPrimaryRow = '';
    this.objectLabel = '';
    this.objectValue = '';
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
