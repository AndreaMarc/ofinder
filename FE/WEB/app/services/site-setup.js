/**
 * @tracked {siteSetup} contiene i parametri di funzionamento del sito, scaricati all'avvio
 *                      (o quelli di default se la connessione al server non è riuscita)
 * @tracked {availableActiveLanguages} array contenente l'elenco delle lingue attive
 */

import Service from '@ember/service';
import { tracked } from '@glimmer/tracking';

export default class SiteSetupService extends Service {
  @tracked siteSetup = {};
  @tracked availableActiveLanguages = [];

  // chiamato da utility/utils-startup.js, imposta la configurazione generale nella
  // variabile @tracked siteSetup, rendendola così disponibile per tutti i servizi
  // (componenti, controller ecc) che importano il servizio corrente.
  changeSetup(setup) {
    this.siteSetup = setup;
    this.updateAvailableActiveLanguages();
  }

  setSetup(param, value) {
    this.siteSetup[param] = value;

    if (param === 'availableLanguages') {
      this.updateAvailableActiveLanguages();
    }
  }

  updateAvailableActiveLanguages() {
    if (
      this.siteSetup &&
      typeof this.siteSetup.availableLanguages !== 'undefined'
    ) {
      let availableLanguages = this.siteSetup.availableLanguages;
      let langs = availableLanguages.filter((item) => {
        return item.active === true;
      });
      this.availableActiveLanguages = langs;
    }
  }

  getSetup() {
    return this.siteSetup;
  }

  constructor() {
    super();
    this.siteSetup = {};
  }
}
