/**
 * CREA IL FORM DI LOGIN
 *
 * NOTA:
 * Se il sito PREVEDE l'addon dei Ticket, NON usare questo componente e
 * sostituirlo con il componente login-form-ticket
 *
 *
 * Gestisce:
 * - procedura di login;
 * - visibilità dei link di cambio password al primo accesso e in caso di password scaduta;
 * - credenziali di login errate
 * - segnalazione dei tentativi di accesso rimanenti
 * - segnalazione di utenza bloccata
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::Core::LoginForm/>
 *
 * @param {string} disableSocialLogin permette di disattivare i pulsanti di accesso-social (quando attivi in Setup). Stringa "true" per disattivarli, altrimenti stringa vuota.
 * @param {string} dialogsError se "true", i messaggi di errori vengono mostrati in una modale, se stringa vuota vengono mostrati in un div.
 */
import { inject as service } from '@ember/service';
import Component from '@glimmer/component';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
//import config from 'poc-nuovo-fwk/config/environment';
import { htmlSafe } from '@ember/template';
import { MD5 } from 'crypto-js';
import { task } from 'ember-concurrency';

export default class StandardCoreLoginFormComponent extends Component {
  @service('siteSetup') stp;
  @service translation;
  @service jsUtility;
  @service session;
  @service dialogs;
  @service router;
  @service fetch;

  @tracked showSocialLogin = true;
  @tracked dialogsError = false;

  @tracked errorMessage;
  @tracked identification = '';
  @tracked password = '';
  @tracked freePassword = '';
  @tracked pwdType = 'password';
  @tracked linkToChangePwd = false;

  constructor(...attributes) {
    super(...attributes);

    if (typeof this.args.disableSocialLogin !== 'undefined') {
      this.showSocialLogin = !this.args.disableSocialLogin;
    }
    if (typeof this.args.dialogsError !== 'undefined') {
      this.dialogsError = !!this.args.dialogsError;
    }
  }

  /* Authentications */
  authenticateJwt = task({ drop: true }, async () => {
    let self = this;
    this.errorMessage = '';
    this.linkToChangePwd = false;

    // verifico l'accettazione della privacy-policy
    let accepted =
      localStorage.getItem('poc-allow-gdpr') &&
      localStorage.getItem('poc-allow-gdpr') !== '';
    if (!accepted) {
      this.dialogs.confirm(
        `<h6>${this.translation.languageTranslation.component.loginForm.rulesAcceptance}</h6>`, // ACCETTAZIONE DELLA PRIVACY-POLICY
        `<p>${this.translation.languageTranslation.component.loginForm.rulesDetails}</p>`,
        () => {
          localStorage.setItem('poc-allow-gdpr', Date.now());
          localStorage.setItem('poc-allow-cookie', 'all');
          let loginBtn = document.getElementById('login-form-btn');
          loginBtn.click();
        },
        null,
        [
          this.translation.languageTranslation.component.loginForm.confirm,
          this.translation.languageTranslation.component.loginForm.cancel,
        ]
      );
      return;
    }

    if (this.identification === '' || this.password === '') {
      this.dialogs.toast(
        this.translation.languageTranslation.component.loginForm
          .mandatoryFields, // Email e password sono obbligatori
        'error',
        'bottom-right',
        3
      );
      return false;
    }

    let regex = this.jsUtility.regex('email');
    if (!regex.test(this.identification)) {
      this.dialogs.toast(
        this.translation.languageTranslation.component.loginForm.unvalidEmail, // Inserire un'email corretta
        'error',
        'bottom-right',
        3
      );
      return false;
    }

    try {
      let { identification, password } = this;
      await this.session.authenticate(
        'authenticator:jwt',
        identification,
        password
      );

      // Se è presente un intento di transizione memorizzato prima del login, reindirizzo verso esso.
      // Altrimenti redirect predefinito di ESA.
      //await this.jsUtility.sleep(1000);
      let attemptedTransition = localStorage.getItem('attemptedTransition');
      if (attemptedTransition && attemptedTransition !== '') {
        let savedTransitionInfo = JSON.parse(
          localStorage.getItem('attemptedTransition')
        );

        if (savedTransitionInfo) {
          const { targetRouteName, queryParams, params } = savedTransitionInfo;
          localStorage.removeItem('attemptedTransition');

          // Estrai i parametri di percorso come array
          const pathParamsArray = Object.values(params);

          // Controlla se queryParams è vuoto
          if (
            Object.keys(queryParams).length === 0 &&
            queryParams.constructor === Object
          ) {
            this.router.transitionTo(targetRouteName, ...pathParamsArray);
          } else {
            this.router.transitionTo(targetRouteName, ...pathParamsArray, {
              queryParams,
            });
          }
        }
      }
    } catch (e) {
      /**
       * risposta di errore del tipo:
       * {status: 401, body: { AccountLocked: true,AttemptsRemaining: 0, LockExpiresIn: "04/08/2023 09:30:48 +00:00" }}
       */
      //console.warn(e);
      let msg = '';
      if (e.status === 401) {
        // credenziali errate
        let body = e.body;
        let setup = this.stp.siteSetup;
        if (body.AccountLocked) {
          // account bloccato per superato numero di tentativi errati di login
          let expiration = new Date(body.LockExpiresIn);
          expiration.setMinutes(expiration.getMinutes() + 1); // Aggiungo un minuto

          let umanExpiration = self.jsUtility.data(expiration, {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
          });
          msg = `${this.translation.languageTranslation.component.loginForm.accountTemporarilyBlocked} ${umanExpiration}`; // Il tuo account è temporaneamente bloccato ...
        } else if (setup.failedLoginAttempts > 0) {
          // è attivo il conteggio dei tentativi rimanenti
          msg =
            this.translation.languageTranslation.component.loginForm
              .incorrectCredentials; // E-mail o password errati
          if (body.AttemptsRemaining === 1) {
            msg +=
              this.translation.languageTranslation.component.loginForm
                .oneAttemptRemaining; // Ti rimane <strong>1 tentativo</strong>
          } else {
            msg += `${this.translation.languageTranslation.component.loginForm.remain} <strong>${body.AttemptsRemaining} ${this.translation.languageTranslation.component.loginForm.attemptsLeft}</strong>`; // Ti rimangono X tentativi
          }
          msg += ` ${this.translation.languageTranslation.component.loginForm.beforeBlock}`;
        } else {
          // non è attivo il conteggio dei tentativi rimanenti
          msg =
            this.translation.languageTranslation.component.loginForm
              .incorrectCredentials; //`e-mail o password errati`;
        }
        this.showError(msg);
      } else if (e.status === 404) {
        // utente inesistente
        this.showError(
          `L'e-mail inserita non sembra essere registrata al nostro portale!`
        ); // L'e-mail inserita non sembra essere registrata al nostro portale!
        this.linkToChangePwd = false;
      } else if (e.status === 409) {
        // password scaduta
        this.showError(
          this.translation.languageTranslation.component.loginForm
            .expiredPassword
        ); // la password inserita è corretta ma scaduta
        this.linkToChangePwd = true;
      } else if (e.status === 412) {
        // primo accesso
        this.showError(
          this.translation.languageTranslation.component.loginForm
            .passwordToUpdate
        ); // la password inserita è corretta ma è necessario aggiornarla
        this.linkToChangePwd = true;
      } else if (e.status === 423) {
        // utente bannato
        this.showError(
          this.translation.languageTranslation.component.loginForm.banned
        ); // questa utenza è bloccata, accesso negato.
      } else if (e.status === 418) {
        // utente auto-registrato, che non ha ancora confermato l'email
        // eslint-disable-next-line no-undef
        Swal.fire({
          icon: 'error',
          title: 'Profilo non attivato',
          html: `Prima di accedere devi confermare il tuo indirizzo e-mail.<br /><br />
                Al momento della registrazione ti abbiamo inviato un'e-mail contenente le istruzioni.<br /><br />
                <small>Non hai ricevuto l'email? Premi il pulsante qui sotto!</small>`,
          showCancelButton: true,
          confirmButtonColor: '#3f6ad8',
          cancelButtonColor: '#6c757d',
          confirmButtonText: `Rimandami l'e-mail`,
          cancelButtonText: `Annulla`,
        }).then((result) => {
          if (result.isConfirmed) {
            let obj = { userEmail: this.identification };
            this.fetch
              .call(
                `account/confirmRegistrationAgainAfterLogin`,
                'POST',
                obj,
                {},
                false,
                self.session
              )
              .then(() => {
                this.dialogs.toast(
                  `Ti abbiamo rimandato l'e-mail con il link per l'attivazione`,
                  'success',
                  'bottom-right',
                  5
                );
              })
              .catch((e) => {
                console.error(e);
                this.dialogs.toast(
                  'Si è verificato un errore. Riprovare!',
                  'error',
                  'bottom-right',
                  3
                );
              });
          }
        });
      } else {
        this.showError(
          this.translation.languageTranslation.component.loginForm.accessError
        ); //si è verificato un errore; riprovare!
      }
    }
  });

  showError(msg) {
    if (this.dialogsError) {
      this.dialogs.alert(
        `<h6 class="text-danger">${this.translation.languageTranslation.component.loginForm.attention}</h6>`,
        msg,
        'Ok'
      );
    } else {
      this.errorMessage = htmlSafe(msg);
    }
  }

  @action
  goToUpdatePassword() {
    this.router.transitionTo('update-password', {
      queryParams: {
        email: this.identification,
        oldPassword: this.freePassword,
      },
    });
  }

  /* Handler */
  @action
  updateIdentification(e) {
    this.identification = e.target.value.trim();
  }

  @action
  updatePassword(e) {
    this.password = this.stp.siteSetup.useMD5
      ? MD5(e.target.value.trim()).toString()
      : e.target.value.trim();
    this.freePassword = e.target.value.trim();
  }

  @action
  updateShowPwd() {
    this.pwdType = this.pwdType === 'password' ? 'text' : 'password';
  }

  @action
  authenticateWithFacebook() {
    this.session.authenticate('authenticator:torii', 'facebook');
  }

  get showFacebook() {
    if (
      typeof this.stp !== 'undefined' &&
      typeof this.stp.siteSetup !== 'undefined' &&
      typeof this.stp.siteSetup.thirdPartsAccesses !== 'undefined' &&
      typeof this.stp.siteSetup.thirdPartsAccesses.facebookEnabled !==
        'undefined'
    ) {
      return this.stp.siteSetup.thirdPartsAccesses.facebookEnabled;
    } else return false;
  }
  get showGoogle() {
    if (
      typeof this.stp !== 'undefined' &&
      typeof this.stp.siteSetup !== 'undefined' &&
      typeof this.stp.siteSetup.thirdPartsAccesses !== 'undefined' &&
      typeof this.stp.siteSetup.thirdPartsAccesses.googleEnabled !== 'undefined'
    ) {
      return this.stp.siteSetup.thirdPartsAccesses.googleEnabled;
    } else return false;
  }

  // GOOGLE
  @action
  authenticateWithGoogleImplicitGrant() {
    let clientId = this.stp.siteSetup.googleCredentials.googleIdSite;
    let redirectURI = this.stp.siteSetup.googleCredentials.redirectUriLogin; // `${window.location.origin}/callback`;
    let scope = this.stp.siteSetup.googleCredentials.googleScopes;
    let url =
      `https://accounts.google.com/o/oauth2/v2/auth?` +
      `client_id=${clientId}` +
      `&redirect_uri=${redirectURI}` +
      `&response_type=code` + //  token
      `&scope=${scope}` +
      `&include_granted_scopes=true` +
      `&access_type=offline` +
      `&prompt=`;
    console.log(url);
    window.location.replace(encodeURI(url));
  }

  /*
  // spostata in jsUtility
  sleep(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
  */
}
