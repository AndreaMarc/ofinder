import { inject as service } from '@ember/service';
import Route from '@ember/routing/route';
import ENV from 'poc-nuovo-fwk/config/environment';
import { registerDestructor } from '@ember/destroyable';
import { setupGetData } from 'poc-nuovo-fwk/utility/utils-startup';
import { languageManager } from 'poc-nuovo-fwk/utility/utils-translation';
import { getIncomplete } from 'poc-nuovo-fwk/utility/utils-incomplete-config';
import { getAdvices } from 'poc-nuovo-fwk/utility/utils-get-advices';
import { getOwner } from '@ember/application';
import { task } from 'ember-concurrency';

export default class ApplicationRoute extends Route {
  @service('permissions') permissionsService;
  @service('router') routerService;
  @service pushNotifications;
  @service sessionAccount;
  @service statusService;
  @service pushCallback;
  @service translation;
  @service addonConfig;
  @service siteLayout;
  @service siteSetup;
  @service session;
  @service header;
  @service router;
  @service fetch;
  @service store;

  // pagine che gli utenti non loggati possono visualizzare, anche quando in setup è attivo il forceLoginRedirect
  unloggedEnabledPages = [
    'registration',
    'access-permissions',
    'confirm-registration',
    'google-login',
    //'google-registration',
    'google-error',
    'recovery-password',
    'welcome-slider',
    'help-desk-unlog',
    'maintenance',
    'user-profile', // necessaria per il redirect post-login
    'age-gate', // Age verification (AGCOM)
    'terms', // Legal documents (Privacy, Terms)
  ];

  async beforeModel(transition) {
    if (window.cordova) {
      console.warn('STARTUP PER AMBIENTE CORDOVA');
      document.addEventListener(
        'deviceready',
        async () => {
          await this.execBeforeModel.perform(transition);
        },
        false
      );
      setTimeout(() => {
        this.execBeforeModel.perform(transition);
      }, 1000);
    } else {
      console.warn('STARTUP PER AMBIENTE WEB');
      await this.execBeforeModel.perform(transition);
    }
  }

  execBeforeModel = task({ enqueue: true }, async (transition) => {
    try {
      let self = this;
      if (window.cordova) console.log('EXEC DEVICE READY');

      // ============================================
      // AGE GATE CHECK (AGCOM Delibera 96/25/CONS)
      // Verifica età obbligatoria - Decreto Caivano
      // ============================================
      const ageVerified = localStorage.getItem('ofinder-age-verified');
      const destinationRoute = transition.to.name;

      // Usa unloggedEnabledPages esistente (contiene già age-gate e terms)
      if (
        ageVerified !== 'true' &&
        !this.unloggedEnabledPages.includes(destinationRoute)
      ) {
        this.router.transitionTo('age-gate');
        return false; // Stop execution
      }
      // ============================================

      await this.session.setup();

      // RECUPERO INFORMAZIONI SUGLI ADDON
      await this.addonConfig.getConfig();

      // RECUPERO DAL SERVER LE IMPOSTAZIONI DI CONFIGURAZIONE DEL SITO
      let stp = await setupGetData(this.store);
      this.siteSetup.changeSetup(stp);
      this.siteLayout.updateLayoutStyle(stp);

      // SE UTENTE LOGGATO, FORZO REFRESH-TOKEN (anche per verificare se Tenant è attivo)
      if (this.session.isAuthenticated) {
        try {
          await this.forceRefreshTokens();
        } catch (e) {
          console.error('Unable refresh token in application.js', e);
        }
      }

      // IMPOSTO IL SERVIZIO DI TRADUZIONE
      let langStp = await languageManager(stp);
      if (langStp.languageAvailable) {
        await this.translation.setCurrentLanguage(langStp.currentLang);
      }

      // #region PRIVILEGI DI ACCESSO
      if (this.session.isAuthenticated) {
        // eslint-disable-next-line prettier/prettier
        let permissions = typeof this.session.get('data.permissions') !== 'undefined' ? this.session.get('data.permissions') : [];
        this.permissionsService.setPermissions(permissions); // imposto i permessi dell'utente scaricati al login

        let p = await this.sessionAccount.routePermissionsProcessing(
          stp.routesList
        );
        this.permissionsService.setRoutePermissions(p); // imposto i permessi di rotta
        this.permissionsService.addRouteAccessDeniedHandler(
          this.sessionAccount.routeNotAllowed
        );
        registerDestructor(this, () => {
          this.permissionsService.removeRouteAccessDeniedHandler(
            this.sessionAccount.routeNotAllowed
          );
        });
        this.permissionsService.enableRouteValidation(transition);
      }
      // #endregion

      // VERIFICO SE LA MANUTENZIONE DEL SITO E' ATTIVA
      let stpCustom = await setupGetData(this.store, true);
      console.warn('MAINTENANCE:', stp.maintenance);
      if (stp.maintenance || (stpCustom && stpCustom.maintenanceAdmin)) {
        if (this.session.isAuthenticated) {
          let permissions = this.session.get('data.permissions');
          if (!permissions.includes('canBypassMaintenance')) {
            await this.session.invalidate();
          }
        } else {
          this.router.transitionTo('maintenance');
          if (navigator && navigator.splashscreen) {
            navigator.splashscreen.hide();
          }
          return false;
        }
      }

      // IMPOSTO I PARAMETRI DELL'HEADER (TEMA)
      if (this.session.isAuthenticated) {
        this.header.internalChat = stp.internalChat;
        this.header.internalNotifications = stp.internalNotifications;
        this.header.search = stp.search;

        // aggiorna l'elenco dei messaggi e delle notifiche non lette
        this.header.updatingAdvices = true;
        let al = await getAdvices(this.store, this.session, this.header);
        this.header.advicesList = al;
        this.header.notifications = al.notifications.length;
        this.header.messages = al.messages.length;
        this.header.updatingAdvices = false;

        let hasP = await this.permissionsService.hasPermissions([
          'canSeeIncompleteConfigurations',
        ]);
        if (hasP) {
          this.header.incomplete = await getIncomplete(
            this.fetch,
            this.session
          );
        }
      }

      // Disabilitiamo i log in produzione se le impostazioni di configurazione lo prevedono
      if (stp && stp.disableLog && ENV.environment === 'production') {
        window.console = {};
        // eslint-disable-next-line no-unused-vars
        window.console.log = (a) => {};
        // eslint-disable-next-line no-unused-vars
        window.console.warn = (a) => {};
        // eslint-disable-next-line no-unused-vars
        window.console.error = (a) => {};
      }

      // NOTA: se vuoi consentire di visualizzare pagine diverse da login/welcome-slider agli
      // utenti sloggati, usa l'opzione disponibile in Setup/Sicurezza
      let destinationPage = transition.to.name;
      if (this.session.isAuthenticated) {
        // al primo avvio dell'app per utenti loggati, impongo un redirect alla home per risolvere errore di rotta cordova
        if (
          typeof window.cordova !== 'undefined' &&
          transition.to.attributes &&
          transition.to.attributes.path &&
          transition.to.attributes.path === 'index.html'
        ) {
          this.router.transitionTo('/');
        }
      } else {
        if (
          stp.forceLoginRedirect &&
          !this.unloggedEnabledPages.includes(destinationPage)
        ) {
          if (
            typeof window.cordova !== 'undefined' &&
            stp.sliderPosition &&
            stp.sliderPosition !== ''
          ) {
            // In ambiente Cordova (cioè se il codice gira in un'App) ed utente non loggato, redirect a 'login' o a 'slider' in base alle configurazioni
            this.session.requireAuthentication(transition, 'welcome-slider');
          } else {
            this.session.requireAuthentication(transition, 'login');
          }
        }
      }

      if (typeof window.cordova !== 'undefined') {
        // eslint-disable-next-line no-undef
        if (device && device.platform !== 'browser') {
          // CHIUDO SPLASH-SCREEN
          navigator.splashscreen.hide();

          // CONTROLLO LA VERSIONE DELL'APP
          let wantedVersion = stp.minAppVersion;
          if (
            typeof BuildInfo !== 'undefined' &&
            // eslint-disable-next-line no-undef
            BuildInfo &&
            wantedVersion &&
            wantedVersion.toString() !== '' &&
            wantedVersion.toString() !== '0.0.0'
          ) {
            wantedVersion = wantedVersion.split('.'); // array
            let currentAppVersion = self.appVersion.split('.'); // array

            if (parseInt(wantedVersion[2]) > parseInt(currentAppVersion[2])) {
              // Aggiornamento obbligatorio
              this.router.transitionTo('update-app');
            } else if (
              parseInt(wantedVersion[1]) > parseInt(currentAppVersion[1])
            ) {
              // Aggiornamento obbligatorio
              this.router.transitionTo('update-app');
            } else if (
              parseInt(wantedVersion[0]) > parseInt(currentAppVersion[0])
            ) {
              // aggiornamento facoltativo
            }
          }

          // ATTIVO NOTIFICHE PUSH
          if (
            this.siteSetup.siteSetup.pushNotifications &&
            window.FirebasePlugin
          ) {
            this.pushNotifications.start(
              this.session,
              this.store,
              this.pushCallback,
              this.translation,
              this.header,
              getAdvices,
              // eslint-disable-next-line no-undef
              device,
              this.router
            );
          }

          // GESTISCO EVENTUALI CUSTOM-URL-SCHEME
          window.handleOpenURL = (url) => {
            // i Deep Link saranno della forma:
            // http://<base>>/<real-route>?uid=61
            // vedi servizio universal-deep-links.js per maggiori info
            setTimeout(() => {
              try {
                this.udlService.exec(
                  url,
                  this.session,
                  this.store,
                  this.pushCallback,
                  this.translation,
                  this.header,
                  getAdvices,
                  // eslint-disable-next-line no-undef
                  device,
                  this.router
                );
              } catch (e) {
                console.error('Errore in esecuzione c.b. di handleOpenURL', e);
              }
            }, 0);
          };
        }
      }

      return;
      // eslint-disable-next-line no-empty
    } catch (e) {}
  });

  async forceRefreshTokens() {
    let owner = getOwner(this);
    let authenticator = owner.lookup('authenticator:jwt');
    let currentData = this.session.data.authenticated;
    try {
      await authenticator.jwtRefreshAccessToken(currentData);
    } catch (error) {
      console.error('Failed to refresh token in application.js:', error);
    }
  }

  get appVersion() {
    try {
      if (this.isApp) {
        return typeof BuildInfo !== 'undefined' &&
          // eslint-disable-next-line no-undef
          BuildInfo &&
          // eslint-disable-next-line no-undef
          typeof BuildInfo.version !== 'undefined'
          ? // eslint-disable-next-line no-undef
            BuildInfo.version
          : '0.0.0';
      } else return '0.0.0';
    } catch (e) {
      console.error('Error in appVersion() (file authenticated.js)', e);
      return '0.0.0';
    }
  }

  get isApp() {
    return typeof window.cordova !== 'undefined';
  }
}
