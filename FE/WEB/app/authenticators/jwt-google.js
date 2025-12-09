import { inject as service } from '@ember/service';
import Base from 'ember-simple-auth/authenticators/base';

export default class OAuth2ImplicitGrantAuthenticator extends Base {
  @service('permissions') permissionsService;
  @service sessionAccount;
  @service jsUtility;
  @service dialogs;
  @service session;
  @service fetch;

  async authenticate(otp) {
    let res = await this.loginGoogle(otp);
    // copio i dati nelle variabili di sessione modificabili
    await this.session.updateSessionData(res);

    // imposto i permessi degli utenti
    // eslint-disable-next-line prettier/prettier
    let permissions = typeof this.session.get('data.permissions') !== 'undefined' ? this.session.get('data.permissions') : [];
    await this.permissionsService.setPermissions(permissions); // imposto i permessi dell'utente scaricati al login
    return res;
  }

  async loginGoogle(otp) {
    // eslint-disable-next-line no-useless-catch
    try {
      let body = {
        otp: otp,
        fingerprint: this.session.getFingerprint(),
      };

      let header = {
        DeviceName: this.jsUtility.getDeviceInfos(),
      };

      let returnData = null;
      await this.fetch
        .call('account/loginOtp', 'POST', body, header, false, this.session)
        .then(async (res) => {
          //console.log(res);
          returnData = await this.sessionAccount.extractTokenData(res.data);
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
}
