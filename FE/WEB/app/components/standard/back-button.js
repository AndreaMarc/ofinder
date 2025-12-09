/**
 * Componente che crea un pulsante per tornare alla pagina precedente.
 *
 * Esempio di utilizzo:
 * <Standard::BackButton class="btn-hover-shine btn btn-sm btn-light">Torna indietro</Standard::BackButton>
 *
 * La classe è facoltativa.
 */
import Component from '@glimmer/component';
import { action } from '@ember/object';

export default class StandardBackButtonComponent extends Component {
  @action
  goBack() {
    window.history.back();
  }
}
