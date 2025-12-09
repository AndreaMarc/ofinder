/**
 * FETCH WRAPPING
 * Ritorna una Promise.
 *
 * Nota: quando possibile, preferire l'utilizzo di ember-data.
 * Le chiamate fetch vanno utilizzate solo se strettamente necessario, ad esempio
 * quando il server non risponde in formato JSON:API e sono generalmente più
 * lente.
 *
 *
 * @method call : METODO UNICO DA CHIAMARE
 *
 * @param {string} endpoint :       ENDPOINT da chiamare (es: "authenticate")
 * @param {string} method :         "GET" (default), "POST", "PUT", "DELETE"
 * @param {object} data :           Valori da inviare nel body, in formato Oggetto (es: {foo: bar})
 * @param {null/object} headers :   Parametri da inviare nell'header, in formato oggetto. Se null, l'header non viene inviato.
 *                                  Se oggetto, viene inviato un header di default con l'aggiunta degli eventuali parametri
 *                                  dell'oggetto passato (se non vuoto).
 * @param {bool} authorization :    Indica se aggiungere i dati di autenticazione nell'header (bearer, fingerprint...).
 *                                  Richiede header non nullo.
 * @param {service} session :       E' il servizio session di ember-simple-auth (this.session)
 *
 *
 * ESEMPI DI UTILIZZO:
 * @service fetch;
 * ...
 * this.fetch.call('endpoint', 'GET', null, {}, true, this.session);
 * this.fetch.call('endpoint2', 'POST', {foo: bar}, {'Content-Type': 'application/json'}, true, this.session);
 */
import Service from '@ember/service';
import config from 'poc-nuovo-fwk/config/environment';
//import { inject as service } from '@ember/service';
//import { tracked } from '@glimmer/tracking';

export default class FetchService extends Service {
  session = null;

  retry401 = 0;

  constructor() {
    super();
    this.retry401 = 0;
  }

  parseData(data) {
    if (data) {
      let qString = '';
      for (var property in data) {
        if (Object.prototype.hasOwnProperty.call(data, property)) {
          qString += `${property}=${encodeURIComponent(data[property])}&`;
        }
      }
      if (qString === '') {
        return null;
      }
      return qString.substring(0, qString.length - 1);
    }
    return null;
  }

  setHeaders(headers, authorization, isFormData) {
    let self = this;
    let head = null;

    if (headers && typeof headers == 'object') {
      let baseEndpoint = '';
      if (typeof window.cordova === 'undefined') {
        let currentLocation = window.location;
        baseEndpoint = `${currentLocation.protocol}//${
          currentLocation.hostname
        }${currentLocation.port !== '' ? ':' + currentLocation.port : ''}`;
      } else {
        baseEndpoint = config.feHost;
      }

      head = {
        platform: typeof window.cordova !== 'undefined' ? 'app' : 'web',
        'access-Control-Allow-Origin': '*',
        fingerprint: self.session.getFingerprint(),
        baseEndpoint: baseEndpoint,
        timezoneOffset: new Date().getTimezoneOffset(),
      };

      if (!isFormData) {
        head.accept = 'application/vnd.api+json';
        head['content-Type'] = 'application/vnd.api+json';
      }

      if (authorization && self.session && self.session.isAuthenticated) {
        head.authorization = self.session.get('data.access_token');
        //head.fingerprint = self.session.getFingerprint();
        head.tenantId = self.session.get('data.tenantId') || 1;
      }

      // aggiungo eventuali headers aggiuntivi passati dall'utente
      if (Object.keys(headers).length > 0) {
        for (let property in headers) {
          if (Object.prototype.hasOwnProperty.call(headers, property)) {
            head[property] = headers[property];
          }
        }
      }
    }

    return head;
  }

  // metodo da chiamare per avviare una chiamata fetch
  async call(endpoint, method, data, headers, authorization, session) {
    this.session = session;
    let self = this;
    return new Promise((resolve, reject) => {
      self.retry401 = 0;
      // #region verifyConnection
      if (
        navigator &&
        navigator.connection &&
        navigator.connection.effectiveType
      ) {
        let networkState = navigator.connection.effectiveType;
        if (networkState.toLowerCase() === 'none') {
          reject('no_connection');
        }
      }
      // #endregion

      // #region verifyEndpoint
      if (!endpoint) {
        reject(new Error('An endpoint is needed to make a request.'));
      }

      if (typeof endpoint !== 'string') {
        reject(new Error('Unknown endpoint format.'));
      }
      // #endregion

      // #region verifyMethod
      if (
        !method ||
        !['GET', 'POST', 'DELETE', 'PUT', 'PATCH'].includes(
          method.toUpperCase()
        )
      ) {
        reject(new Error('unknown method.'));
      }
      // #endregion

      self
        .go(endpoint, method, data, headers, authorization)
        .then((res) => {
          resolve(res);
        })
        .catch((err) => {
          reject(err);
        });
    });
  }

  go = async (endpoint, method, data, headers, authorization) => {
    let self = this;
    return new Promise((resolve, reject) => {
      // #region prepareDate
      if (
        data &&
        typeof data === 'object' &&
        Object.keys(data).length > 0 &&
        method === 'GET'
      ) {
        // caso GET con parametri => trasformo l'oggetto in una query-string
        endpoint += endpoint.includes('?') ? '&' : '?' + self.parseData(data);
      }
      // #endregion

      let fetchSetup = { method: method };
      let head = self.setHeaders(
        headers,
        authorization,
        data && data.constructor && data.constructor.name === 'FormData'
      );
      //console.warn('TENANT-ID: ', head.tenantId, ` (Fetch - ${endpoint})`);
      if (head) fetchSetup.headers = head;

      if (
        method !== 'GET' &&
        data &&
        data.constructor &&
        data.constructor.name === 'FormData' &&
        [...data.keys()].length > 0
      ) {
        // oggetto formData
        fetchSetup.body = data;
      } else if (
        method !== 'GET' &&
        data &&
        typeof data === 'object' &&
        Object.keys(data).length > 0
      ) {
        // oggetto classico
        fetchSetup.body = JSON.stringify(data);
      }

      if (!endpoint.includes('http')) {
        let host = config.apiHost;
        let namespace = config.namespaceHost;
        endpoint = `${host}/${namespace}/${endpoint}`;
      }

      fetch(endpoint, fetchSetup)
        .then(async (response) => {
          if (response.ok) {
            if (response.status !== 204) {
              return response.json();
            } else {
              return null;
            }
          } else {
            try {
              // Creiamo un oggetto che contiene sia lo stato che il corpo del messaggio d'errore
              const errorBody = await response.json();
              return Promise.reject({
                status: response.status,
                body: errorBody,
              });
            } catch (e) {
              return await Promise.reject(response);
            }
          }
        })
        .then((res) => {
          resolve(res);
        })
        .catch(async (error) => {
          //console.log(error);
          if (
            error.status &&
            error.status === 401 &&
            self.session.isAuthenticated
          ) {
            // Sono loggato ed ho ottenuto 401 -> provo refreshToken e ripeto 1 volta la chiamata
            if (endpoint.includes('refreshtoken')) {
              reject(error);
            } else {
              if (self.retry401 > 0) {
                reject(error);
              } else {
                try {
                  await self.session.refreshAccessToken();
                  self.retry401++;
                  // eslint-disable-next-line prettier/prettier
                  return self.go(endpoint, method, data, headers, authorization);
                } catch (e) {
                  reject(e);
                  return new Promise.reject({ status: 'invalidate' });
                }
              }
            }
          } else if (error instanceof Promise) {
            error.then((errorData) => {
              reject(errorData);
            });
          } else {
            reject(error);
          }
        })
        .then((res) => {
          resolve(res);
        })
        .catch((e) => {
          if (e.status && e.status === 'invalidate') {
            console.error('FORZO LOGOUT PER ERRORE 401 RIPETUTO');
            this.session.invalidate();
          } else {
            reject(e);
          }
        });
    });
  };
}
