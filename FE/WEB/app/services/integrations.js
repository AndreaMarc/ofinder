/**
 * SERVIZIO PER LA GESTIONE DELLE INTEGRAZIONI CON APPLICATIVI ESTERNI DI TERZE PARTI
 *
 * @method {function} connect : task da chiamare per avviare la connessione al servizio estermo. Richiede il seguente parametro:
 *    @param {string} integrationId : id del record di integrazione
 */
import Service from '@ember/service';
import { task } from 'ember-concurrency';

export default class IntegrationsService extends Service {
  constructor() {
    super();
  }

  connect = task(
    { drop: true },
    async (integrationId, userId, fetch, dialogs, session, store) => {
      let obj = {
        integrationId: integrationId,
        userId: userId,
      };
      const result = await fetch
        .call('account/getExitingHash', 'POST', obj, {}, true, session)
        .then(async (res) => {
          console.warn(res);
          return this._callExternaSite(
            integrationId,
            res.Hash,
            fetch,
            dialogs,
            session,
            store
          );
        })
        .catch((e) => {
          console.error(e);
          if (e.status) {
            return `<span class="text-danger">${e.status} - ${e.statusText}</span>`;
          } else if (typeof e === 'object') {
            return `<span class="text-danger">${JSON.stringify(e)}</span>`;
          } else return `<span class="text-danger">${e}</span>`;
        });
      return result;
    }
  );

  async _callExternaSite(integrationId, hash, fetch, dialogs, session, store) {
    // ricavo dal DB le informazioni sul record di integrazione
    let record = await store.findRecord('integration', integrationId);
    let obj = {
      hash: hash,
      code: record.code,
    };

    const result = await fetch.call(
      record.url,
      'POST',
      obj,
      {},
      false,
      session
    );
    return result;
  }
}
