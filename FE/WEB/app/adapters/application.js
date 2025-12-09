/* eslint-disable ember/no-jquery */
import JSONAPIAdapter from '@ember-data/adapter/json-api';
import config from 'poc-nuovo-fwk/config/environment';
import { inject as service } from '@ember/service';

export default class ApplicationAdapter extends JSONAPIAdapter {
  // Adapter
  @service jsUtility;
  @service session;

  host = config.apiHost;
  namespace = config.namespaceHost;

  // estrapola gli Headers da passare alle chiamate
  get headers() {
    let baseEndpoint = '';
    if (typeof window.cordova === 'undefined') {
      let currentLocation = window.location;
      baseEndpoint = `${currentLocation.protocol}//${currentLocation.hostname}${
        currentLocation.port !== '' ? ':' + currentLocation.port : ''
      }`;
    } else {
      baseEndpoint = config.feHost;
    }

    let headers = {};
    headers['timezoneOffset'] = new Date().getTimezoneOffset();
    headers['baseEndpoint'] = baseEndpoint;
    headers['access-Control-Allow-Origin'] = '*';
    headers['deviceName'] = this.jsUtility.getDeviceInfos();
    headers['platform'] = 'web';
    headers['appPlatform'] = null;
    if (typeof window.cordova !== 'undefined') {
      headers['platform'] = 'app';

      headers['appPlatform'] =
        // eslint-disable-next-line no-undef
        typeof device !== 'undefined' ? device.platform : 'unknown';
    }

    if (this.session.isAuthenticated) {
      headers['authorization'] = `${this.session.get('data.access_token')}`;
      headers['fingerprint'] = this.session.getFingerprint();
      headers['tenantId'] = this.session.get('data.tenantId');
    }
    return headers;
  }

  /*ajaxOptions(url, type, options) {
    let hash = super.ajaxOptions(url, type, options);

    console.warn(
      'TENANT-ID: ',
      this.session.get('data.tenantId'),
      ` (Ember - ${url})`
    );
    return hash;
  }*/

  // Intercetta le risposte di ciascuna chiamata e, in caso di 401, tenta il refresh del token
  async handleResponse(status, headers, payload, requestData) {
    //console.log('HANDLE RESPONSE');
    if (status === 401 && this.session.isAuthenticated) {
      console.warn(
        'ERRORE 401: PROVO UN REFRESH-TOKEN PER RIPRISTINARE IL BEARER'
      );
      await this.session.refreshAccessToken();
    }
    return super.handleResponse(...arguments);
  }
}
