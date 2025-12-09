/* eslint-disable ember/no-jquery */
import { inject as service } from '@ember/service';
import Base from 'ember-simple-auth/authenticators/base';
import config from 'poc-nuovo-fwk/config/environment';
import $ from 'jquery';

export default class OAuth2ImplicitGrantAuthenticator extends Base {
  // Base oppure OAuth2ImplicitGrant
  @service session;

  serverTokenEndpoint = `${config.apiHost}/${config.namespaceHost}/account/login`;
  serverTokenRevocationEndpoint = `${config.apiHost}/${config.namespaceHost}/revoke`;

  authenticate(email, pwd) {
    return this.login(email, pwd);
  }

  login(email, pwd) {
    return new Promise((resolve, reject) => {
      $.ajax({
        url: this.serverTokenEndpoint,
        type: 'POST',
        contentType: 'application/vnd.api+json',
        data: JSON.stringify({
          email: email,
          password: pwd,
          fingerprint: this.session.getFingerprint(),
        }),
      })
        .then((response) => {
          console.log(response);
          let returnData = {
            access_token: response.data.authorizationBearer,
            refresh_token: response.data.authorizationRefreshBearer,
            authorizationExpiresIn: response.data.authorizationExpiresIn,
            authorizationRefreshExpiresIn:
              response.data.authorizationRefreshExpiresIn,
            fullName: response.data.fullName,
            tenantId: response.data.tenantId,
            permissions: ['perm_a', 'perm_b', 'isSuperAdmin'],
            id: response.data.id ? response.data.id : 0,
          };
          resolve(returnData);
        })
        .catch((e) => {
          reject(e);
        });
    });
  }

  restore(data) {
    return new Promise((resolve, reject) => {
      if (data.access_token) {
        //console.log('RESTORE: found access token ');
        resolve(data);
      } else {
        //console.log('RESTORE: no token found');
        reject();
      }
    });
  }
}
