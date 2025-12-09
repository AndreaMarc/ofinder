/**
 * CALLBACK PER NOTIFICHE-PUSH e WEB-SOCKET
 *
 * Tutti i metodi prevedono una firma di questo tipo:
 *
 * (res, translation, store, session, header, getAdvices, router)
 */
import Service from '@ember/service';
import { service } from '@ember/service';

export default class PushCallbackService extends Service {
  //@service statusService; // decommentare se necessario
  @service audio;

  // eslint-disable-next-line no-undef
  cordovaDevice = typeof device !== 'undefined' ? device : null; // device è variabile cordova

  // Scarico l'elenco delle notifiche/messaggi non letti e aggiorno il servizio header.
  // Questo aggiornerà l'elenco dei messaggi visibili nel componente in header.
  // eslint-disable-next-line prettier/prettier
  async refreshHeaderNotifications(res, translation, store, session, header, getAdvices, router) {
    header.updatingAdvices = true;
    header.advicesList = await getAdvices(store, session, header);
    header.notifications = header.advicesList.notifications.length;
    header.messages = header.advicesList.messages.length;
    header.updatingAdvices = false;
  }
}
