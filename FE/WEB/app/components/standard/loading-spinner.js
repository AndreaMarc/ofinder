/**
 * LOADING-SPINNER
 * Mostra un messaggio di attesa
 *
 * Esempio di utilizzo:
 * <Standard::LoadingSpinner @icon=true @msg="Attendere prego" @msgKey="component.loadingSpinner.message" @style="1"/>
 *
 * @param {string} @icon se '' non mostra l'icona rotante. '1' o altri valori per mostrarla
 * @param {string} @msgKey chiave del messaggio da mostrare
 * @param {string} @msg messaggio di default (mostrato se non è possibile recuperare quello corrispondente alla chiave)
 * @param {number} @style stile grafico del componente. Valori 1, 2, ... (si consiglia di mantenerlo costante nell'intero sito)
 *
 * DESCRIZIONE DEGLI STILI
 * 1) contenuto in un tag p 'inline'. E' possibile associargli una classe per definire lo stile del testo (es: text-success)
 * 1) contenuto in un tag p 'd-block' con testo centrato. E' possibile associargli una classe
 * 3) contenuto in un div con class 'alert' E' possibile associargli una classe (es:  alert-info)
 *
 * Se devi inserire html all'interno del componente, puoi utilizzarlo così:
 *  <Standard::LoadingSpinner @icon="" @style="3" class="alert-danger">
      <!-- qui l'html -->
    </Standard::LoadingSpinner>
 */

import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { htmlSafe } from '@ember/template';

export default class StandardLoadingSpinnerComponent extends Component {
  @service translation;

  @tracked icon = true;
  @tracked style = 1;
  key = null;
  default = '';
  @tracked textToShow = '';

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  async start() {
    await this.updateStyle();
    await this.updLangParams();
  }

  @action
  async updateStyle() {
    this.icon =
      typeof this.args.icon !== 'undefined' ? !!this.args.icon : false;
    this.style =
      typeof this.args.style !== 'undefined' ? parseInt(this.args.style) : 3;
  }

  @action
  updLangParams() {
    this.key = typeof this.args.msgKey !== 'undefined' ? this.args.msgKey : '';
    this.default = typeof this.args.msg !== 'undefined' ? this.args.msg : '';
    this.getTranslation();
  }

  getTranslation() {
    try {
      this.textToShow = '';

      if (this.msgKey && this.msgKey !== '') {
        this.textToShow = 'Attendere prego...';

        let languageAvailable = this.translation.languageAvailable;
        if (languageAvailable) {
          if (this.translation && this.translation.languageTranslation) {
            let value = this.translation.languageTranslation;
            let keys = this.msgKey.split('.');
            keys.forEach((key) => {
              value = value[key];
            });

            if (value && value !== 'undefined') {
              this.textToShow = htmlSafe(value);
            } else {
              throw new Error('Word not available');
            }
          }
        } else {
          throw new Error('Language not available');
        }
      } else {
        throw new Error('No key for loading-spinner');
      }
    } catch (e) {
      if (this.default && this.default !== '') {
        this.textToShow = htmlSafe(this.default);
      }
    }
  }
}
