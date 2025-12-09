/**
 * Quando un tenant prova ad iscrivere un utente che è già registrato ad altro tenant dello stesso sito,
 * l'utente riceve un'email di accettazione che punta a questa pagina.
 * In tal pagina l'utente accetta o rifiuta l'associazione al tenant richiedente.
 */
import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';

export default class AccessPermissionsRoute extends Route {
  @service session;
  //@service router;
  @service store;

  async beforeModel(transition) {
    await this.session.setup();
    // Memorizzo l'intento di transizione per poterlo utilizzare dopo l'autenticazione
    await this.session.setAfterLogin(transition);
    this.session.requireAuthentication(transition, 'login'); // solo utenti loggati
  }

  model(_, transition) {
    return {
      otp: transition.to.queryParams.otp,
    };
  }

  @action
  setupController(controller, model) {
    super.setupController(controller, model);
    controller.loadData();
  }
}
