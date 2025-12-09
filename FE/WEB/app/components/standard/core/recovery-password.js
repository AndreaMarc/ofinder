/* eslint-disable no-undef */
import Component from '@glimmer/component';
import { action } from '@ember/object';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { MD5 } from 'crypto-js';
import { task } from 'ember-concurrency';
import { htmlSafe } from '@ember/template';

export default class StandardCoreRecoveryPasswordComponent extends Component {
  //@service translation;
  @service('siteSetup') stp;
  @service jsUtility;
  @service session;
  @service dialogs;
  @service router;
  @service fetch;

  @tracked helperText = ``;
  @tracked disabledSend1 = true;
  @tracked colorClass = '';
  @tracked outcome = '';
  @tracked showOutcome = false;
  @tracked userEmail = '';
  @tracked ph = 1;

  @tracked disabledSend2 = true;
  newPassword = '';
  @tracked newPasswordStatus = '';
  newPasswordConfirm = '';
  @tracked newPasswordConfirmStatus = '';
  @tracked showPwd = false;
  @tracked pwdType = 'password';
  @tracked userEmail2 = '';
  @tracked matchPassword = '';

  constructor(...attributes) {
    super(...attributes);
    this.showOutcome = false;
    this.outcome = '';
    this.helperText = htmlSafe(`<br />`);
    this.disabledSend1 = true;
    this.colorClass = htmlSafe('text-success');

    if (
      this.router.currentRoute.queryParams.ph &&
      this.router.currentRoute.queryParams.ph === '2'
    ) {
      this.userEmail2 = this.router.currentRoute.queryParams.email;
      this.otp = this.router.currentRoute.queryParams.otp;
      if (this.userEmail2 !== '' && this.otp !== '') {
        this.ph = 2;
      }
    }
  }

  // #region STEP 1
  // catturo e controllo l'email inserita
  @action
  updateEmail(e) {
    this.userEmail = e.target.value.trim();
    this.outcome = '';
    this.showOutcome = false;

    if (this.userEmail === '') {
      this.helperText = htmlSafe(`<br />`);
      this.disabledSend1 = true;
    } else {
      let regex = this.jsUtility.regex('email');

      if (!regex.test(this.userEmail)) {
        this.helperText = htmlSafe(`Inserire un'email valida!`);
        this.disabledSend1 = true;
      } else {
        this.helperText = htmlSafe(`<br />`);
        this.disabledSend1 = false;
      }
    }
  }

  // invio e-mail all'utente per il recupero della password
  recoveryStep1 = task({ drop: true }, async () => {
    let self = this;
    this.outcome = ``;
    this.showOutcome = false;

    let d = new Date();
    let y = this.jsUtility.data(d, { year: 'numeric' });
    let m = this.jsUtility.data(d, { month: '2-digit' });
    let g = this.jsUtility.data(d, { day: '2-digit' });
    let h = d.getUTCHours() > 12 ? d.getUTCHours() - 12 : d.getUTCHours();

    // "0x81{model.Username}{DateTime.UtcNow.ToString("yyyyMMddhhmm")}eAa9Bf80"
    // eslint-disable-next-line prettier/prettier
    let hash = `0x81${this.userEmail}${y}${m}${g}${('0' + h.toString()).slice(-2)}${('0' + d.getUTCMinutes().toString()).slice(-2)}eAa9Bf80`;

    let data = {
      securityToken: MD5(hash).toString(),
      username: this.userEmail,
    };

    await this.fetch
      .call('account/reset-password', 'POST', data, {}, false, this.session)
      .then(() => {
        // TODO : migliorare il messaggio
        Swal.fire({
          icon: 'success',
          title: 'Operazione riuscita',
          text: `Riceverai un'email contenente il link per creare la nuova password`,
        });
      })
      .catch((e) => {
        if (e.status === 404) {
          Swal.fire({
            icon: 'error',
            title: 'E-mail inesistente',
            text: `Assicurati di aver digitato l'email correttamente`,
          });
        } else {
          self.dialogs.toast(
            'Si è verificato un errore. Riprovare!',
            'error',
            'bottom-right',
            3
          );
        }
      });
  });
  // #endregion

  // #region STEP 2
  // memorizzo i valori inseriti
  @action
  updateValues(variableName, event) {
    this.errorMessage = '';
    let val = event.target.value.trim();
    this[variableName] = val;

    this.validPassword(val, `${variableName}Status`);

    let self = this;
    setTimeout(() => {
      if (
        self.newPassword !== '' &&
        self.newPasswordStatus.toString() === '' &&
        self.newPasswordConfirm !== '' &&
        self.newPasswordConfirmStatus.toString() === '' &&
        self.newPassword === self.newPasswordConfirm
      ) {
        self.disabledSend2 = false;
        this.matchPassword = '';
      } else {
        self.disabledSend2 = true;

        if (
          self.newPassword !== '' &&
          self.newPasswordConfirm !== '' &&
          self.newPassword !== self.newPasswordConfirm
        ) {
          this.matchPassword = htmlSafe(`Le due password non corrispondono`);
        }
      }
    }, 100);
  }

  validPassword(password, responseVariableName) {
    let res = this.jsUtility.verifyPassword(password);
    this[responseVariableName] = htmlSafe(res);
    if (res !== '') this.disabledSend2 = true;
  }

  @action
  updateShowPwd(e) {
    this.showPwd = e.target.checked;
    this.pwdType = this.showPwd ? 'text' : 'password';
  }

  recoveryStep2 = task({ drop: true }, async () => {
    let self = this;

    if (this.newPassword !== this.newPasswordConfirm) {
      this.disabledSend2 = true;
      self.dialogs.toast(
        'Le due nuove password non corrispondono',
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    let data = {
      otp: this.otp,
      md5Password: MD5(this.newPassword).toString(),
      email: this.userEmail2,
    };

    await this.fetch
      .call('account/reset-password-otp', 'POST', data, {}, false, this.session)
      .then(() => {
        Swal.fire({
          icon: 'success',
          title: 'Password aggiornata',
          text: `Ora puoi effettuare l'accesso con la tua nuova password`,
          confirmButtonText: 'Vai al login',
          allowOutsideClick: false,
          allowEscapeKey: false,
          allowEnterKey: false,
        }).then((result) => {
          if (result.isConfirmed) {
            this.router.transitionTo('login');
          }
        });
      })
      .catch((e) => {
        console.error(e);
        if ([400, 404].includes(e.status)) {
          Swal.fire({
            icon: 'error',
            title: 'Link non più valido',
            html: `Il link selezionato non è valido.<br />Puoi richiedere l'invio di un nuovo link.`,
            confirmButtonText: 'Ok',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
          });
        } else if (e.status === 409) {
          Swal.fire({
            icon: 'error',
            title: 'Password già utilizzata',
            html: `La nuova password deve essere diversa dalle ${this.stp.siteSetup.previousPasswordsStored} precedenti`,
            confirmButtonText: 'Ok',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
          });
        } else if (e.status === 406) {
          Swal.fire({
            icon: 'error',
            title: 'Link non più valido',
            html: `Il link selezionato è scaduto o è già stato utilizzato.<br />Puoi richiedere l'invio di un nuovo link.`,
            confirmButtonText: 'Ok',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
          });
        } else {
          self.dialogs.toast(
            'Si è verificato un errore. Riprovare!',
            'error',
            'bottom-right',
            3
          );
        }
      });
  });
  // #endregion
}
