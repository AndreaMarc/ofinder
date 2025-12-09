/**
 * @tracked {bool} languageAvailable : indica se il servizio di traduzione è disponibile
 *          Se false, il componente di traduzione usera i soli valori di default.
 * @tracked {string} currentLang : indica il codice della lingua corrente
 * @tracked {array} languageTranslation : contiene le traduzioni nella lingua corrente
 * @tracked {bool} translationOnTheFly : indica se il servizio di traduzione 'al volo' è attivo
 */

import Service from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { inject as service } from '@ember/service';

export default class TranslationService extends Service {
  @service store;

  @tracked languageTranslation = {};
  @tracked currentLang = 'it';
  @tracked languageAvailable = false;
  @tracked translationOnTheFly = false;

  async setCurrentLanguage(languageCode) {
    await this.changeLanguage(languageCode);
  }

  // chiamato dal componente choose-language, invoca il cambio lingua e,
  // se il dowload riesce, aggiorna la variabile '@tracked translation' contenente le traduzioni,
  // così da aggiornare nel DOM tutti i valori del componente di traduzione

  // scarica dal server la traduzione impostata dal componente di traduzione
  async changeLanguage(languageCode) {
    let self = this;

    return new Promise((resolve) => {
      console.log('TRANSLATION: scarico lingua ' + languageCode);
      // scarico dal server la traduzione
      // se la chiamata fallisce imposto: self.languageAvailable = false;
      // e self.languageTranslation = {};
      //
      // se tutto ok imposto in self.languageTranslation la traduzione scaricata
      // e self.languageAvailable = true;
      // e self.currentLang = languageCode

      self.store
        .queryRecord('translation', {
          filter: `equals(languageCode,'${languageCode}')`,
        })
        .then(function (data) {
          //console.log('TRANSLATION DATA: ', data);

          //this.session.set('data.setup', data);
          let newLang =
            typeof window.cordova !== 'undefined'
              ? data.translationApp
              : data.translationWeb;
          self.languageTranslation = newLang;
          self.languageAvailable = true;
          self.currentLang = languageCode;
          localStorage.setItem('poc-user-lang', languageCode);
          console.log('TRANSLATION: completato');
          resolve();
        })
        // eslint-disable-next-line no-unused-vars
        .catch((e) => {
          //console.error(e);

          // eslint-disable-next-line no-unused-vars
          self.store.push({
            data: {
              id: 1,
              type: 'translation',
              attributes: {
                translationApp: {},
                translationWeb: {},
                languageCode: 'it',
              },
            },
          });

          self.languageTranslation = '{}';
          self.languageAvailable = false;
          self.currentLang = languageCode;
          localStorage.setItem('poc-user-lang', languageCode);

          console.log('TRANSLATION: completato con valore di default');
          resolve();
        });
    });
  }

  activateTranslation(status) {
    this.translationOnTheFly = !!status;
  }
}
