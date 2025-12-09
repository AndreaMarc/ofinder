/* eslint-disable ember/no-jquery */
/**
 * COMPONENTE PER LE TRADUZIONI
 *
 * Ogni testo del sito va inserito nell'html come segue:
 *
 * <Standard::TR @key="post.example" @default="Questo è il valore di default!"/>
 *
 * @param {string} key è la chiave del json di traduzione
 * @param {string} default è il valore di default, utilizzato quando il servizio
 *                         di traduzione non è attivo, non è disponibile (problemi
 *                         di connessione al server) o non viene trovata la parola
 *                         corrispondente alla key
 */

import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import $ from 'jquery';
//import { tracked } from '@glimmer/tracking';

export default class StandardTRComponent extends Component {
  @service translation;
  @service session;
  @service dialogs;
  @service store;

  @tracked key = '';
  default = '';

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  start() {
    this.key = this.args.key;
    this.default = this.args.default;
  }

  get text() {
    let ret = '<span class="text-danger">Unknown translation</span>';
    try {
      if (
        (typeof this.key === 'undefined' || this.key === '') &&
        (typeof this.default === 'undefined' || this.default === '')
      ) {
        ret = '';
      } else {
        if (this.key !== '') {
          let languageAvailable = this.translation.languageAvailable;
          if (languageAvailable) {
            if (this.translation && this.translation.languageTranslation) {
              let value = this.translation.languageTranslation;
              let keys = this.key.split('.');
              keys.forEach((key) => {
                value = value[key];
              });

              if (value && value !== 'undefined') {
                ret = value;
              } else {
                throw new Error('Word not available');
              }
            }
          } else {
            throw new Error('Language not available');
          }
        } else throw new Error('Unknown Key');
      }
    } catch (e) {
      if (this.default && this.default !== '') {
        ret = this.default;
      }
    }

    return htmlSafe(ret);
  }

  get editOnTheFly() {
    if (!this.session.isAuthenticated) {
      return false;
    } else return this.translation.translationOnTheFly ? true : false;
  }

  @action
  changeText(key, event) {
    event.preventDefault();
    event.stopPropagation();

    let self = this;
    if (key && key !== '') {
      /*
      this.dialogs.confirm(
        'Modifica testo',
        `Vuoi davvero modificare la traduzione del testo selezionato?
        <input type="hidden" value="${key}" id="translate-popup-confirm-change">`,
        self.executeChange,
        null,
        ['Conferma', 'Annulla']
      );
      */
      self.executeChange(key);
    } else {
      this.dialogs.toast(
        'Testo non modificabile. Contattare un admin.',
        'warning',
        'bottom-right',
        3
      );
    }
  }

  @action
  async executeChange(key) {
    //this.dialogs.toast('Attendere prego...', 'warning', 'bottom-left', 2);
    if (!key) {
      key = $('#translate-popup-confirm-change').val();
    }

    // Scarico le traduzioni nella lingua corrente
    let lang = await this.getRecordJsonApi(this.translation.currentLang);
    try {
      if (lang) {
        let wordApp = lang.translationApp;
        let wordWeb = lang.translationWeb;

        let keys = key.split('.');
        keys.forEach((element) => {
          wordApp = wordApp[element];
          wordWeb = wordWeb[element];
        });

        // Valorizzo le text-area di traduzione in right-sidebar
        $('#change-translation-app').val(wordApp);
        $('#change-translation-web').val(wordWeb);

        // mostro l'area di modifica
        $('#edit-translation-area').show();
        // abilito textarea e pulsante di salvataggio
        $(
          '#change-translation-app, #change-translation-web, #translate-confirm-change'
        ).prop('disabled', false);

        // scrivo negli attributi del pulsante di salvataggio-traduzione (right-sidebar) le informazioni necessarie per il salvataggio
        $('#translate-confirm-change')
          .attr('data-record-id', lang.id)
          .attr('data-key', key);

        $('#change-translation-key').val(key); // input "Chiave" in right-sidebar

        // attivo la tab "Traduzioni" nella right-sidebar
        $('#to-tab-drawer-0').trigger('click');

        // apro la right-sidebar
        $('.open-right-drawer').addClass('is-active');
        $('.app-drawer-wrapper').addClass('drawer-open');
        $('.app-drawer-overlay').removeClass('d-none');
      } else {
        throw new Error('Unable to download current translation');
      }
    } catch (e) {
      console.error(e);
      $('#change-translation-app, #change-translation-web').val(''); // text-area di traduzione in right-sidebar
      $('#change-translation-key').val(''); // input "Chiave" in right-sidebar

      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        3
      );
    }
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
}
