/* eslint-disable no-undef */
/**
 * COMPONENTO DI CAMBIO PASSWORD
 * Mostra il form per l'aggiornamento della password.
 *
 * Verifica se in query string sono passati i parametri 'email' e 'oldPassword'.
 * In caso affermativo popola i corrispondenti campi del form rendendoli nascosti.
 * In caso negativo, vengono mostrati i campi per inserire l'email e la password correnti.
 * (i campi di inserimento della nuova password sono sempre mostrati)
 *
 * Verifica chei dati inseriti siano corretti.
 *
 * NOTA:
 * I parametri in query string possono essere resi invisibili.
 * Codice di esempio (vedi componente login-form.js):
 *
 * this.router.transitionTo('update-password', {
      queryParams: {
        email: this.identification,
        oldPassword: this.password,
      },
    });
 *
 *
 * @queryString {string} @email email corrente dell'utente. Se fornita in queryString o se utente loggato, il campo email non viene mostrato.
 * @queryString {string} @oldPassword password corrente dell'utente (che deve essere aggiornata). Se fornito in queryString, il campo oldPassword non viene mostrato.
 * @param {string} showBack opzionale. Se inserito e strnga non vuota, viene il mostrato il tasto back
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::Core::UpdatePassword/>
 *
 */

import Component from '@glimmer/component';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { service } from '@ember/service';
import { MD5 } from 'crypto-js';
import { getOwner } from '@ember/application';

export default class StandardCoreUpdatePasswordComponent extends Component {
  @service('siteSetup') stp;
  @service jsUtility;
  @service session;
  @service dialogs;
  @service router;
  @service fetch;

  @tracked showCredentialEmail = '';
  @tracked showCredentialPwd = '';
  @tracked disabledSend = true;
  @tracked errorMessage = '';

  @tracked userEmail = '';
  @tracked userEmailStatus = '';

  @tracked userOldPassword = '';
  @tracked userOldPasswordStatus = '';

  newPassword = '';
  @tracked newPasswordStatus = '';

  newPasswordConfirm = '';
  @tracked newPasswordConfirmStatus = '';

  @tracked showPwd = false;
  @tracked pwdType = 'password';

  @tracked showBack = false;

  constructor(...attributes) {
    super(...attributes);

    if (this.router.currentRoute && this.router.currentRoute.queryParams) {
      if (this.router.currentRoute.queryParams.email) {
        this.userEmail = this.router.currentRoute.queryParams.email;
      } else if (
        this.session.isAuthenticated &&
        this.session.get('data.email') &&
        this.session.get('data.email') !== ''
      ) {
        this.userEmail = this.session.get('data.email');
      }
      if (this.router.currentRoute.queryParams.oldPassword) {
        this.userOldPassword = this.router.currentRoute.queryParams.oldPassword;
      }

      if (this.args.showBack && this.args.showBack !== '') {
        this.showBack = true;
      }
    }

    if (this.userEmail !== '') {
      this.showCredentialEmail = 'd-none';
    }
    if (this.userOldPassword !== '') {
      this.showCredentialPwd = 'd-none';
    }
  }

  // memorizzo i valori inseriti
  @action
  updateValues(variableName, event) {
    this.errorMessage = '';
    let val = event.target.value.trim();
    this[variableName] = val;

    if (variableName === 'userEmail') {
      let regex = this.jsUtility.regex('email');

      if (!regex.test(val)) {
        this.userEmailStatus = `Inserire un'email valida!`;
        this.disabledSend = true;
      }
    } else {
      this.validPassword(val, `${variableName}Status`);
    }

    let self = this;
    setTimeout(() => {
      if (
        self.userEmail !== '' &&
        self.userEmailStatus === '' &&
        self.userOldPassword !== '' &&
        self.userOldPasswordStatus === '' &&
        self.newPassword !== '' &&
        self.newPasswordStatus === '' &&
        self.newPasswordConfirm !== '' &&
        self.newPasswordConfirmStatus === ''
      ) {
        self.disabledSend = false;
      }
    }, 100);
  }

  validPassword(password, responseVariableName) {
    let res = this.jsUtility.verifyPassword(password);
    this[responseVariableName] = res;
    if (res !== '') this.disabledSend = true;
  }

  @action
  updateShowPwd(e) {
    this.showPwd = e.target.checked;
    this.pwdType = this.showPwd ? 'text' : 'password';
  }

  @action
  sendForm(event) {
    event.preventDefault();
    this.errorMessage = '';

    if (this.newPassword !== this.newPasswordConfirm) {
      this.disabledSend = true;
      this.errorMessage = 'Le due nuove password non corrispondono';
      return false;
    }

    if (
      this.stp.previousPasswordsStored &&
      this.userOldPassword === this.newPassword
    ) {
      this.disabledSend = true;
      this.errorMessage =
        'Le nuova password deve essere diversa dalla precedente!';
      return false;
    }

    // sicurezza ridondante!
    if (
      this.userEmail === '' ||
      this.userOldPassword === '' ||
      this.newPassword === '' ||
      this.newPasswordConfirm === ''
    )
      return false;

    let data = {
      oldPassword: this.stp.siteSetup.useMD5
        ? MD5(this.userOldPassword).toString()
        : this.userOldPassword,
      newPassword: this.stp.siteSetup.useMD5
        ? MD5(this.newPassword).toString()
        : this.newPassword,
      confirmPassword: this.stp.siteSetup.useMD5
        ? MD5(this.newPasswordConfirm).toString()
        : this.newPasswordConfirm,
      username: this.userEmail,
    };

    this.fetch
      .call(
        'account-management/change-password',
        'POST',
        data,
        {},
        true,
        this.session
      )
      .then(async () => {
        try {
          await this.session.authenticate(
            'authenticator:jwt',
            this.userEmail,
            this.stp.siteSetup.useMD5
              ? MD5(this.newPassword).toString()
              : this.newPassword
          );
        } catch (e) {
          console.error(e);
          // redirect al login
        }
      })
      .catch((e) => {
        console.error(e);
        if (e.status === 428) {
          Swal.fire({
            title: 'Operazione parzialmente riuscita',
            html: `La password è stata cambiata correttamente.<br />L'invio dell'e-mail informativa non è riuscito.<br /><br />`,
            icon: 'warning',
            showCancelButton: false,
          }).then(async () => {
            try {
              await this.session.authenticate(
                'authenticator:jwt',
                this.userEmail,
                this.stp.siteSetup.useMD5
                  ? MD5(this.newPassword).toString()
                  : this.newPassword
              );
            } catch (e) {
              console.error(e);
              // redirect al login
            }
          });
        } else if (e.status === 401) {
          Swal.fire({
            icon: 'error',
            title: 'Non accettabile',
            text: `La vecchia password non è corretta`,
          });
        } else if (e.status === 409) {
          Swal.fire({
            icon: 'error',
            title: 'Non accettabile',
            text: `Scegli una password diversa dalle ultime ${this.stp.siteSetup.previousPasswordsStored} utilizzate`,
          });
        } else if (e.status === 406) {
          Swal.fire({
            icon: 'error',
            title: 'Non accettabile',
            text: `Il formato della password non è corretto`,
          });
        } else {
          this.dialogs.toast(
            'Si è verificato un errore. Riprovare!',
            'error',
            'bottom-right',
            3
          );
        }
      });
  }

  async forceRefreshTokens() {
    let owner = getOwner(this);
    let authenticator = owner.lookup('authenticator:jwt');
    let currentData = this.session.data.authenticated;
    try {
      await authenticator.jwtRefreshAccessToken(currentData);
    } catch (error) {
      console.error(error);
      console.error('Failed to refresh token in application.js:', error);
    }
  }
}
