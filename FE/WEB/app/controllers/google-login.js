import Controller from '@ember/controller';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { htmlSafe } from '@ember/template';

export default class GoogleLoginController extends Controller {
  @service translation;
  @service jsUtility;
  @service dialogs;
  @service session;
  @service router;
  @service fetch;

  @tracked state = 'waiting';
  @tracked errorMessage = '';
  otp = '';

  // invocato dal gestore di percordo quando il model è stato caricato
  // nota: this.model.otp ora è valorizzato e viene appunto ritornato dal metrodo model nel gestore di percorso
  async sendGoogleLoginData() {
    this.state = 'waiting';
    this.otp = this.model.otp;

    try {
      await this.session.authenticate('authenticator:jwtGoogle', this.otp);

      // Se è presente un intento di transizione memorizzato prima del login, reindirizzo verso esso.
      // Altrimenti redirect predefinito di ESA.
      await this.jsUtility.sleep(500);
      let attemptedTransition = localStorage.getItem('attemptedTransition');
      if (attemptedTransition && attemptedTransition !== '') {
        let savedTransitionInfo = JSON.parse(
          localStorage.getItem('attemptedTransition')
        );
        if (savedTransitionInfo) {
          const { targetRouteName, queryParams, params } = savedTransitionInfo; // Estrai eventuali parametri di percorso (path params) dalla rotta salvata
          const pathParams = Object.values(params[targetRouteName] || {});
          localStorage.setItem('attemptedTransition', '');

          if (
            Object.keys(queryParams).length === 0 &&
            queryParams.constructor === Object
          ) {
            this.router.transitionTo(targetRouteName, ...pathParams);
          } else {
            this.router.transitionTo(targetRouteName, ...pathParams, {
              queryParams,
            });
          }
        }
      }
      this.state = 'success';
    } catch (e) {
      /**
       * risposta di errore del tipo:
       * {status: 401, body: { AccountLocked: true,AttemptsRemaining: 0, LockExpiresIn: "04/08/2023 09:30:48 +00:00" }}
       */
      console.error(e);
      this.state = 'error';
      this.errorMessage = htmlSafe(`Si è verificato un errore.`);
    }
  }
}
