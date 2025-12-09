import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import Session from 'ember-simple-auth/services/session';
import { v4 } from 'ember-uuid';
import config from 'poc-nuovo-fwk/config/environment';
import { cancel } from '@ember/runloop';
import { getIncomplete } from 'poc-nuovo-fwk/utility/utils-incomplete-config';
import { getAdvices } from 'poc-nuovo-fwk/utility/utils-get-advices';
import { setupGetData } from 'poc-nuovo-fwk/utility/utils-startup';
import { task } from 'ember-concurrency';

export default class SessionService extends Session {
  @service('permissions') permissionsService;
  @service sessionAccount;
  @service session;
  @service dialogs;
  @service header;
  @service fetch;
  @service store;

  refreshTokenTimeout = null;
  serverTokenRefreshEndpoint = `${config.apiHost}/${config.namespaceHost}/refreshtoken`;

  // se un utente non loggato tenta di visualizzare una pagina riservata e vogliamo che dopo il login
  // riatterri in essa, aggiungiamo tale metodo nel beforeModel della pagina
  // nota: tali pagine vanno inoltre aggiunte nell'array unloggedEnabledPages di router/application.js
  async setAfterLogin(transition) {
    if (!this.isAuthenticated) {
      const targetRouteName = transition.to.name;
      const queryParams = transition.to.queryParams;
      const params = transition.to.params;
      localStorage.setItem(
        'attemptedTransition',
        JSON.stringify({ targetRouteName, queryParams, params })
      );
      console.log('REDIRECT POST-LOGIN SETTED');
    }
    return;
  }

  // tutte le chiamate autenticate richiedono un fingerprint che identifica univocamente ogni sessione.
  // Questa funzione lo recupera (e lo genera se non era ancora stato definito)
  getFingerprint() {
    let fp = localStorage.getItem('poc-fingerprint');
    if (!fp || fp === '') {
      fp = v4();
      localStorage.setItem('poc-fingerprint', fp);
      return fp;
    } else {
      return fp;
    }
  }

  // estrapola i dati contenuti nei token di autenticazione
  parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(
      window
        .atob(base64)
        .split('')
        .map(function (c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        })
        .join('')
    );

    return JSON.parse(jsonPayload);
  }

  async refreshAccessToken() {
    try {
      let response = await this.fetch.call(
        'refreshtoken',
        'GET',
        null,
        { authorization: this.data.authenticated.refresh_token },
        true,
        this
      );
      let returnData = await this.sessionAccount.extractTokenData(
        response.data
      );

      // Aggiorno ciascuna proprietà di data.authenticated individualmente
      this.data.authenticated.access_token = returnData.access_token;
      this.data.authenticated.refresh_token = returnData.refresh_token;
      this.data.authenticated.authorizationExpiresIn =
        returnData.authorizationExpiresIn;
      this.data.authenticated.authorizationRefreshExpiresIn =
        returnData.authorizationRefreshExpiresIn;
      this.data.authenticated.fullName = returnData.fullName;
      this.data.authenticated.tenantId = returnData.tenantId;
      this.data.authenticated.permissions = returnData.permissions;
      this.data.authenticated.profileImageId = returnData.profileImageId;
      this.data.authenticated.currentTenantActive =
        returnData.currentTenantActive;

      // copio i dati nelle variabili di sessione modificabili
      await this.updateSessionData(returnData);
    } catch (e) {
      console.error(e);
      this.invalidate();
    }
  }

  async handleAuthentication() {
    super.handleAuthentication(...arguments);

    await this.sessionAccount.loadCurrentUser().catch(() => {
      console.error('NON VALIDATO!');
      this.invalidate();
    });

    let self = this;
    setTimeout(async () => {
      self.initializeLogged.perform(
        self.store,
        self,
        self.header,
        self.permissionsService,
        self.fetch
      );
    }, 2500);
  }

  initializeLogged = task(
    { drop: true },
    async (store, session, header, permissionsService, sFetch) => {
      try {
        // aggiorna l'elenco dei messaggi e delle notifiche non lette
        let stp = await setupGetData(store);
        header.internalChat = stp.internalChat;
        header.internalNotifications = stp.internalNotifications;
        header.search = stp.search;

        header.updatingAdvices = true;
        let al = await getAdvices(store, session, header);
        header.advicesList = al;
        header.notifications = al.notifications.length;
        header.messages = al.messages.length;
        header.updatingAdvices = false;

        let hasP = await permissionsService.hasPermissions([
          'canSeeIncompleteConfigurations',
        ]);
        if (hasP) {
          header.incomplete = await getIncomplete(sFetch, this);
        }
      } catch (e) {
        console.error('Errore nel recupero delle IncompleteConfigurations');
      }
    }
  );

  @action
  invalidateSessionGeneral() {
    this.dialogs.confirm(
      'USCITA',
      `Confermi il log-out?`,
      () => {
        cancel(this.refreshTokenTimeout);
        this.session.invalidate();
      },
      null,
      ['Sì', 'No']
    );
  }

  // aggiorna i dati di sessione
  async updateSessionData(data) {
    try {
      if (data) {
        if (typeof data.email !== 'undefined')
          this.session.set('email', data.email);
        if (typeof data.id !== 'undefined') this.session.set('id', data.id);
        if (typeof data.firstName !== 'undefined')
          this.session.set('firstName', data.firstName);
        if (typeof data.lastName !== 'undefined')
          this.session.set('lastName', data.lastName);
        if (typeof data.profileImageId !== 'undefined')
          this.session.set('profileImageId', data.profileImageId);
        if (typeof data.tenantId !== 'undefined')
          this.session.set('tenantId', data.tenantId);
        if (typeof data.associatedTenants !== 'undefined')
          this.session.set('associatedTenants', data.associatedTenants);
        if (typeof data.currentTenantActive !== 'undefined')
          this.session.set('currentTenantActive', data.currentTenantActive);
        if (typeof data.permissions !== 'undefined')
          this.session.set('permissions', data.permissions);
        if (typeof data.access_token !== 'undefined')
          this.session.set('access_token', data.access_token);
        if (typeof data.refresh_token !== 'undefined')
          this.session.set('refresh_token', data.refresh_token);
        if (typeof data.authorizationExpiresIn !== 'undefined') {
          this.session.set(
            'authorizationExpiresIn',
            data.authorizationExpiresIn
          );
          this.session.set(
            'authorizationExpires',
            new Date(new Date().getTime() + data.authorizationExpiresIn * 60000)
          );
        }
        if (typeof data.authorizationRefreshExpiresIn !== 'undefined') {
          this.session.set(
            'authorizationRefreshExpiresIn',
            data.authorizationRefreshExpiresIn
          );
          this.session.set(
            'authorizationRefreshExpires',
            new Date(
              new Date().getTime() + data.authorizationRefreshExpiresIn * 60000
            )
          );
        }
        if (typeof data.termsAcceptanceDate !== 'undefined') {
          this.session.set(
            'termsAcceptanceDate',
            new Date(data.termsAcceptanceDate)
          );
        }
      }
      return;
    } catch (e) {
      console.error(e);
      return;
    }
  }
}
