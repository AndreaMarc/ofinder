import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';

/**
 * Age Gate Route
 * Verifica età obbligatoria per accesso al sito (AGCOM Delibera 96/25/CONS)
 *
 * NOTA: Usa localStorage invece di cookies per compatibilità con Cordova
 */
export default class AgeGateRoute extends Route {
  @service router;

  beforeModel() {
    // Controlla se l'utente ha già verificato l'età (localStorage)
    const ageVerified = localStorage.getItem('ofinder-age-verified');

    // Se già verificato, redirect alla home
    if (ageVerified === 'true') {
      this.router.transitionTo('index');
    }
  }
}
