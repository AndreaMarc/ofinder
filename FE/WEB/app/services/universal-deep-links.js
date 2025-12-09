/**
 * Questo servizio gestisce i Deep Link (android) e gli Universal Link (iOS) di Cordova.
 *
 * Il metodo exec è chiamato automaticamente dal FWK in ambiente Cordova all'apertura di un deep-link.
 * (https://github.com/prageeth/cordova-universal-links-plugin)
 * Nel metodo exec vanno aggiunti i listener necessari per ciascuna rotta.
 *
 * I Deep Link saranno della forma:
 * http://<base>>/<real-route>?uid=61
 *
 * dove:
 * - <base> è la variabile URL_SCHEME configurata nel config.xml
 * - <real-route> è una rotta reale di questo sito ember
 *
 * i parametri sono facoltativi, e saranno presenti se previsti dalla funzionalità della rotta desiderata.
 */
import Service from '@ember/service';

export default class UniversalDeepLinksService extends Service {
  exec(
    url,
    session,
    store,
    pushCallback,
    translation,
    header,
    getAdvices,
    device,
    router
  ) {
    try {
      let res = this.parseDeepLink(url);
      console.log(res.path);
      console.log(res.params);

      /**
       * ****************************************************************
       * DEFINIRE QUI LE ROTTE PREVISTE DAI DEEP LINK E RELATIVE CALLBACK
       * ****************************************************************
       */
      switch (res.path) {
        case '/absence-planning': // esempio!
          // REDIRECT AD ASSENZA DI SOTTOPOSTO
          // eslint-disable-next-line no-case-declarations
          let qp = {};
          if (res.params.uid) qp.uid = res.params.uid;
          if (res.params.ref_date) qp.ref_date = res.params.ref_date;

          router.transitionTo('absence-planning', {
            queryParams: qp,
          });
          break;
      }

      /*
      this.refreshHeaderNotifications(
        res,
        translation,
        store,
        session,
        header,
        getAdvices,
        router
      );
      */
    } catch (e) {
      console.error(
        'Errore in netodo EXEC del servizio universal-deep-link',
        e
      );
    }
  }

  // Scarico l'elenco delle notifiche/messaggi non letti e aggiorno il Servizio header.
  // Questo aggiornerà l'elenco dei messaggi visibili nel componente in header.
  // eslint-disable-next-line prettier/prettier
  async refreshHeaderNotifications(res, translation, store, session, header, getAdvices, router) {
    header.updatingAdvices = true;
    header.advicesList = await getAdvices(store, session, header);
    header.notifications = header.advicesList.notifications.length;
    header.messages = header.advicesList.messages.length;
    header.updatingAdvices = false;
  }

  /**
   * PROCEESA IL CUSTOM URL *-*-*-* NON TOCCARE *-*-*-*
   * @param {string} url : indirizzo da processare
   * @returns {object} oggetto restuito:
   * {
   *    cmd: contiene l'indirizzo senza parametri e senza base url,
   *    params: oggetto in cui i parametri sono presentati come chiave-valore.,
   * }
   *
   * Ad esempio uando url = "http://maewayapp.app/any/link?a=3&b=5"
   * otteniamo:
   * { path: 'any/link', params: { a: 3, b: 5 } }
   *
   */
  parseDeepLink(url) {
    try {
      let parsedUrl = new URL(url);

      // Ottieni il path
      let path = parsedUrl.pathname;

      // Ottieni tutti i parametri della query string
      let queryParams = {};
      parsedUrl.searchParams.forEach((value, key) => {
        queryParams[key] = value;
      });

      return {
        path: path,
        params: queryParams,
      };
    } catch (e) {
      console.error("Errore nella gestione dell'URL:", e);
      return null;
    }
  }
}
