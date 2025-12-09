import Controller from '@ember/controller';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { htmlSafe } from '@ember/template';
import { task } from 'ember-concurrency';

export default class ConfirmRegistrationController extends Controller {
  @service dialogs;
  @service session;
  @service router;
  @service fetch;

  @tracked available = 'waiting';
  @tracked error = '';
  @tracked showResend = false;

  otp = '';
  // invocato dal gestore di percordo quando il model è stato caricato
  // nota: this.model.otp ora è valorizzato e viene appunto ritornato dal metrodo model nel gestore di percorso
  async loadData() {
    this.available = 'waiting';
    this.otp = this.model.otp;
    let obj = { otp: this.otp };

    // verifico l'otp
    await this.fetch
      .call(`account/confirmRegistration`, 'POST', obj, {}, false, this.session)
      .then(() => {
        // eslint-disable-next-line no-undef
        Swal.fire({
          icon: 'success',
          title: 'Profilo attivato',
          text: `Ora puoi effettuare l'accesso`,
          confirmButtonText: 'Ok',
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
        switch (e.status) {
          case 404:
            this.error = 'Il link selezionato non é valido.';
            break;
          case 406:
            this.error = htmlSafe(`Il link selezionato non é più valido.`);
            this.showResend = true;
            break;
          default:
            this.error = 'Si è verificato un errore. Riprovare!';
        }

        this.available = 'unavailable';
      });
  }

  // richiesta di invio e-mail con nuovo OTP
  resend = task({ drop: true }, async () => {
    let obj = { otp: this.otp };
    await this.fetch
      .call(
        `account/confirmRegistrationAgain`,
        'POST',
        obj,
        {},
        false,
        this.session
      )
      .then(() => {
        // eslint-disable-next-line no-undef
        Swal.fire({
          icon: 'success',
          title: 'Email inviata',
          text: `troverai il nuovo link di attivazione nell'email`,
          confirmButtonText: 'Ok',
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
        this.dialogs.toast(
          'Si è verificato un errore. Riprova!',
          'error',
          'bottom-right',
          3
        );
      });
  });
}
