/**
 * Contiene tutte le configurazioni di funzionamento.
 *
 * Gli addon devono essere installati, il che significa:
 * - renderne disponibile il codice FE nel fwk mediante installazione del pacchetto NPM
 * - renderne disponibile il codice BE nel fwk mediante installazione del pacchetto Nuget
 * - valorizzare manualmente la tabella 'fwk-addon' con il codice dell'addon.
 *
 * Una volta installato, il super-admin dovrà provvedere al Setup delle funzionalità dell'addon.
 *
 *
 * @return {object}:
 * {
 *    settings: {
 *      @param {bool} addonErp: indica se l'addon ERP è installato
 *    },
 *    license: {
 *      .
 *    },
 *    custom: { // attivazioni scelte dal tenant
 *      .
 *    },
 *    options: { // configurazioni scelte dal tenant
 *      .
 *    },
 *    customMenu: {
 *      @param {bool} active stabilisce se mostrare o meno il menù
        @param {string} icon l'icona del pulsante
        @param {array} label il testo del pulsante (desktop) e del popup (desktop e app)
        @param {array} subLabel il testo secondario nel popup (desktop e app)
        @param {string} colorClass la classe di colore per il popup. Lasciare vuoto per utilizzare i colori del tema
        @param {array} permissions i permessi necessari per visualizzarlo. Se array vuoto, è sempre visibile.
 *    }
 * }
 */
import Service from '@ember/service';
import { tracked } from '@glimmer/tracking';
import customMenu from 'poc-nuovo-fwk/_customs/customMenu';

export default class AddonConfigService extends Service {
  @tracked config = {
    settings: {
      addonErp: true, // Indica se l'addon ERP è installato
      /*
      addonHr: false, // Indica se l'addon HR è installato
      addonCrm: false, // Indica se l'addon CRM è installato
      addonProject: false, // Indica se l'addon Project è installato
      */
    },
    customMenu: customMenu.customMenu || {
      active: true,
      icon: 'fa fa-cogs',
      label: 'CUSTOM MENÚ',
      subLabel: 'Sub-title!',
      colorClass: '',
      permissions: [],
    },
  };

  async getConfig() {
    console.log('INSIDE', this.config);
  }
}
