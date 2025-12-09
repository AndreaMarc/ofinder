import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

/**
 * Age Gate Component
 * Gestisce la verifica età e il consenso dell'utente
 *
 * Features:
 * - Checkbox per conferma 18+
 * - Salvataggio in localStorage (Cordova compatible)
 * - Link a Privacy Policy e Terms (sistema framework)
 * - Exit button per chi non accetta
 */
export default class AgeGateComponent extends Component {
  @service router;

  @tracked accepted = false;
  @tracked errorMessage = '';

  /**
   * Toggle checkbox accettazione
   */
  @action
  toggleAccepted() {
    this.accepted = !this.accepted;
    this.errorMessage = '';
  }

  /**
   * Conferma età e salva in localStorage
   */
  @action
  confirmAge() {
    if (!this.accepted) {
      this.errorMessage =
        'Devi confermare di avere almeno 18 anni per continuare.';
      return;
    }

    // Salva verifica in localStorage (persiste con Cordova)
    localStorage.setItem('ofinder-age-verified', 'true');
    localStorage.setItem('ofinder-age-verified-date', new Date().toISOString());

    // Redirect alla pagina di ricerca pubblica
    this.router.transitionTo('search');
  }

  /**
   * Exit - Redirect a sito esterno
   */
  @action
  exitSite() {
    window.location.href = 'https://www.google.com';
  }

  /**
   * Naviga a Privacy Policy (route esistente del framework)
   */
  @action
  goToPrivacyPolicy(event) {
    event.preventDefault();
    this.router.transitionTo('terms', 'privacyPolicy');
  }

  /**
   * Naviga a Terms & Conditions (route esistente del framework)
   */
  @action
  goToTerms(event) {
    event.preventDefault();
    this.router.transitionTo('terms', 'termsEndConditions');
  }
}
