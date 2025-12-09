/**
 * CREA IL COMPONENTE TERMINI E CONDIZIONI
 * Recupera dal server e mostra i Termini, Condizioni e Privacy Poolicy.
 *
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::Core::TermsConditions/>
 *
 */
import Component from '@glimmer/component';
//import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';
//import $ from 'jquery';

export default class StandardCoreTermsConditionsComponent extends Component {
  @service translation;
  @service jsUtility;
  @service store;

  legalCode = 'termsEndConditions';
  @tracked available = 'waiting';
  @tracked title = '';
  @tracked version = '';
  @tracked content = '';
  @tracked dataActivation = null;

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  //@action
  async start() {
    let self = this;

    if (this.args.code && this.args.code !== '') {
      this.legalCode = this.args.code;
    }

    if (
      this.legalCode !== 'privacyPolicy' &&
      this.legalCode !== 'termsEndConditions'
    ) {
      this.available = 'unknown';
      return false;
    }

    this.userLang =
      this.translation.currentLang ||
      (navigator.language || navigator.userLanguage).substring(0, 2); // es: 'it-IT' => 'it'

    await this.store
      .queryRecord('legal-term', {
        filter: `and(equals(language,'${self.userLang}'),equals(code,'${self.legalCode}'),equals(active,'true'))`,
        sort: '-version',
      })
      .then(function (data) {
        if (!data || data.length === 0 || data.content === '') {
          if (self.userLang !== 'it') {
            // provo a recuperare il documento in lingua italiana
            self.retry('it');
          } else self.available = 'unavailable';
        } else {
          self.title = data.title;
          self.version = data.version;
          self.content = htmlSafe(data.content);
          self.dataActivation = null;
          if (data.dataActivation) {
            let d = new Date(data.dataActivation);
            self.dataActivation = self.jsUtility.data(d, {
              day: '2-digit',
              month: 'short',
              year: 'numeric',
            });
          }
          self.available = 'available';
        }
      })
      .catch((e) => {
        console.error(e);

        if (this.userLang !== 'it') {
          // provo a recuperare il documento in lingua oitaliana
          self.retry('it');
        } else self.available = 'unavailable';
      });
  }

  async retry(lang) {
    let self = this;
    await this.store
      .queryRecord('legal-term', {
        filter: `and(equals(language,'${lang}'),equals(code,'${this.legalCode}'),equals(active,'true'))`,
        sort: '-version',
      })
      .then(function (data) {
        if (!data || data.length === 0 || data.content === '') {
          self.available = 'unavailable';
        } else {
          self.title = data.title;
          self.version = data.version;
          self.content = htmlSafe(data.content);
          self.dataActivation = null;
          if (data.dataActivation) {
            let d = new Date(data.dataActivation);
            self.dataActivation = self.jsUtility.data(d, {
              day: '2-digit',
              month: 'short',
              year: 'numeric',
            });
          }
          self.available = 'available';
        }
      })
      .catch((e) => {
        console.error(e);
        self.available = 'unavailable';
      });
  }
}
