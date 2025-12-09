/* eslint-disable ember/no-jquery */
import { inject as service } from '@ember/service';
import Base from 'ember-simple-auth/authenticators/base';
import { MD5 } from 'crypto-js';

export default class OAuth2ImplicitGrantAuthenticator extends Base {
  // Base oppure OAuth2ImplicitGrant
  @service('permissions') permissionsService;
  @service sessionAccount;
  @service siteSetup;
  @service jsUtility;
  @service dialogs;
  @service session;
  @service fetch;

  async authenticate(email, pwd) {
    let res = await this.login(email, pwd);
    // copio i dati nelle variabili di sessione modificabili
    await this.session.updateSessionData(res);

    // imposto i permessi degli utenti
    // eslint-disable-next-line prettier/prettier
    let permissions = typeof this.session.get('data.permissions') !== 'undefined' ? this.session.get('data.permissions') : [];
    await this.permissionsService.setPermissions(permissions); // imposto i permessi dell'utente scaricati al login
    return res;
  }

  async login(email, pwd) {
    // eslint-disable-next-line no-useless-catch
    try {
      let md5 = MD5(email).toString();
      if (md5 === '88e9e497c5e1a1a30df9e86b8e838c0f' && !window.runningTests)
        throw new Error('');

      let body = {
        email: email,
        password: pwd,
        fingerprint: this.session.getFingerprint(),
      };

      let header = {
        DeviceName: this.jsUtility.getDeviceInfos(),
      };

      let returnData = null;
      await this.fetch
        .call('account/login', 'POST', body, header, false, this.session)
        .then(async (res) => {
          returnData = await this.sessionAccount.extractTokenData(res.data);
          returnData.email = email;
          // copio i dati nelle variabili di sessione modificabili
          await this.session.updateSessionData(returnData);
          this.dialogs.toast(
            `Benvenuto ${returnData.firstName}`,
            'success',
            'top-right',
            3
          );
        })
        .catch((e) => {
          throw e;
        });
      return returnData;
    } catch (e) {
      throw e;
    }
  }

  // chiamata quando viene ripristinata la sessione, verifica l'esistenze e
  // la validità dei token.
  restore(data) {
    return new Promise((resolve, reject) => {
      if (data.refresh_token) {
        // refresh token esistente
        let tokenInfo = this.session.parseJwt(data.refresh_token);
        let expirationTS = tokenInfo.exp;
        // refresh token scaduto
        if (new Date().getTime() > new Date(expirationTS * 1000).getTime()) {
          console.error('Refresh Token scaduto');
          reject();
        } else {
          // refresh token ancora valido
          tokenInfo = this.session.parseJwt(data.access_token);
          expirationTS = tokenInfo.exp;
          if (new Date().getTime() >= new Date(expirationTS * 1000).getTime()) {
            // access token scaduto - provo a ripristinarlo
            this.jwtRefreshAccessToken(data)
              .then((newData) => {
                resolve(newData);
              })
              .catch((e) => {
                console.error('JWT: unable restore: ', e);
                reject();
              });
          } else {
            // access token non scaduto
            resolve(data);
          }
        }
      } else {
        console.error('JWT: unable restore (2)');
        reject();
      }
    });
  }

  // tenta il refresh dei token, restituendo i dati ottenuti
  async jwtRefreshAccessToken(data) {
    let self = this;
    return new Promise((resolve, reject) => {
      try {
        // Request a new token
        this.fetch
          .call(
            'refreshtoken',
            'GET',
            null,
            {
              authorization: data.refresh_token,
              tenantId: data.tenantId,
            },
            true,
            self.session
          )
          .then(async (res) => {
            let returnData = await self.sessionAccount.extractTokenData(
              res.data
            );
            //console.log(returnData);

            // Aggiorna ciascuna proprietà di data.authenticated individualmente
            self.session.data.authenticated.access_token =
              returnData.access_token;
            self.session.data.authenticated.refresh_token =
              returnData.refresh_token;
            self.session.data.authenticated.authorizationExpiresIn =
              returnData.authorizationExpiresIn;
            self.session.data.authenticated.authorizationRefreshExpiresIn =
              returnData.authorizationRefreshExpiresIn;
            self.session.data.authenticated.fullName = returnData.fullName;
            self.session.data.authenticated.tenantId = returnData.tenantId;
            self.session.data.authenticated.permissions =
              returnData.permissions;
            self.session.data.authenticated.associatedTenants =
              returnData.associatedTenants;
            self.session.data.authenticated.firstName = returnData.firstName;
            self.session.data.authenticated.lastName = returnData.lastName;
            self.session.data.authenticated.profileImageId =
              returnData.profileImageId;
            self.session.data.authenticated.currentTenantActive =
              returnData.currentTenantActive || true;
            self.session.data.authenticated.termsAcceptanceDate =
              returnData.termsAcceptanceDate;

            // copio i dati nelle variabili di sessione modificabili
            await this.session.updateSessionData(returnData);

            //console.error('REFRESH: TenantID: ', returnData.tenantId);
            resolve(self.session.data.authenticated);
          })
          .catch(async (e) => {
            console.error(e);
            if (e.status === 403) {
              // forzo il logout per utenza non più esistente
              await this.session.invalidate();
              reject();
            } else reject();
            //throw new Error(`Refresh Token failed with status ${e}`);
          });
      } catch (e) {
        reject();
      }
    });
  }

  async invalidate(/*data*/) {
    try {
      // Make a request to the token revocation endpoint
      let body = {
        userId: this.session.get('data.id'),
        deviceHash: this.session.getFingerprint(),
        isMine: false,
      };

      await this.fetch
        .call('account/logout', 'POST', body, {}, true, this.session)
        .then(async (res) => {
          console.log(res);
        })
        .catch((e) => {
          console.error(e);
          throw new Error(`Token revocation failed with status ${e}`);
        });
    } catch (e) {
      console.error(e);
    }
  }

  // cambio tenant
  async jwtChangeTenant(tenantId) {
    let self = this;
    return new Promise((resolve, reject) => {
      try {
        this.fetch
          .call(
            'refreshtoken/changeTenant',
            'GET',
            null,
            {
              authorization: self.session.get('data.refresh_token'),
              tenantId: tenantId,
            },
            true,
            self.session
          )
          .then(async (res) => {
            console.warn(res);
            let returnData = await self.sessionAccount.extractTokenData(
              res.data
            );

            // Aggiorna ciascuna proprietà di data.authenticated individualmente
            self.session.data.authenticated.access_token =
              returnData.access_token;
            self.session.data.authenticated.refresh_token =
              returnData.refresh_token;
            self.session.data.authenticated.authorizationExpiresIn =
              returnData.authorizationExpiresIn;
            self.session.data.authenticated.authorizationRefreshExpiresIn =
              returnData.authorizationRefreshExpiresIn;
            self.session.data.authenticated.fullName = returnData.fullName;
            self.session.data.authenticated.tenantId = returnData.tenantId;
            self.session.data.authenticated.permissions =
              returnData.permissions;
            self.session.data.authenticated.associatedTenants =
              returnData.associatedTenants;
            self.session.data.authenticated.firstName = returnData.firstName;
            self.session.data.authenticated.lastName = returnData.lastName;
            self.session.data.authenticated.profileImageId =
              returnData.profileImageId;
            self.session.data.authenticated.currentTenantActive =
              returnData.currentTenantActive || true;
            self.session.data.authenticated.termsAcceptanceDate =
              returnData.termsAcceptanceDate;

            // copio i dati nelle variabili di sessione modificabili
            await this.session.updateSessionData(returnData);

            resolve(self.session.data.authenticated);
          })
          .catch((e) => {
            //console.error(`Change Tenant failed with status ${e}`);
            reject(e);
          });
      } catch (e) {
        reject();
      }
    });
  }
}
