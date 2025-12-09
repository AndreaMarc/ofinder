/**
 * SERVIZIO HEADER
 * Contiene tutte le informazioni relative agli elementi dell'Header.
 * Le variabili tracked qui presenti vengono popolate da routes/application.js all'avvio dell'applicazione
 *
 * @tracked {bool} internalNotifications : indica se il servizio di comunicazioni interne è attivo
 * @tracked {int} notifications : numero di notifiche non lette
 *
 * @tracked {int} internalChat : indica se il servizio di chat è attivo (0 = non attivo, 1 = attivo solo tra utenti e admin, 2 = attivo tratutti)
 * @tracked {int} messages : Numero di messaggi di chat non letti
 *
 * @tracked {bool} search : indica se mostrare il campo di ricerca ( TODO )
 *
 * @tracked {array} incomplete : Contiene l'elenco delle configurazioni essenziali mancanti
 *
 * @tracked {object} advicesList :  Contiene la lista di messaggi non letti e di notifiche non lette.
 *                                  Valorizzato dall'utility utils-get-advices.js e mostrati dal componente messages-notifications.js nell'header
 *                                  { messages: [], notifications: [] }
 *
 * @tracked {bool} updatingAdvices : da porre a true durante l'aggiornamento di advicesList, fa comparire un loading nel componente di notifiche/messagi dell'header
 * NOTA:
 * All'interno del sito, gli eventuali metodi che interferiscono sulla valore delle
 * variabili, devono occuparsi della loro ri-valorizzazione.
 *
 * Es:
 * Se modifico un Template portando lo stato a 'non-active', devo ricalcolare la variabile 'incomplete':
 *
 * import { getIncomplete } from 'poc-nuovo-fwk/utility/utils-incomplete-config';
 * ...
 * this.header.incomplete = await getIncomplete(this.fetch, this.session);
 *
 */

import Service from '@ember/service';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';

export default class HeaderService extends Service {
  @service('siteSetup') stp;
  //@service fetch;
  session = null;
  store = null;

  @tracked notifications = 0;
  @tracked messages = 0;
  @tracked search = false;
  @tracked incomplete = [];
  @tracked advicesList = { messages: [], notifications: [] };
  @tracked updatingAdvices = false;

  @tracked internalChat = false;
  @tracked internalNotifications = false;

  constructor() {
    super();
  }

  // verifica se ci sono messaggi, notifiche o configurazioni incomplete
  // (usato in visualizzazione mobile per mostrare icona di avviso)
  get totalAdvise() {
    return (
      this.messages > 0 ||
      this.notifications > 0 ||
      (this.incomplete && this.incomplete.length > 0)
    );
  }

  // verifica se ci sono messaggi o notifiche
  getMessagesNotifications() {
    return this.messages > 0 || this.notifications > 0;
  }
}
