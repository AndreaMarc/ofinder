/**
 * CREA IL COMPONENTE DROPDOWN CHE INDICA SE CI SONO CONFIGURAZIONI MANCANTI
 * Per segnalare a chi gestisce il sito che qualcosa non è completo!
 *
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::Core::IncompleteConfiguration/>
 *
 */
import Component from '@glimmer/component';
import { action } from '@ember/object';
//import { tracked } from '@glimmer/tracking';
import { inject as service } from '@ember/service';
import { getIncomplete } from 'poc-nuovo-fwk/utility/utils-incomplete-config';
import { task } from 'ember-concurrency';

export default class StandardCoreIncompleteConfigurationComponent extends Component {
  @service permissions;
  @service session;
  @service router;
  @service header;
  @service fetch;

  constructor(...attributes) {
    super(...attributes);
    //this.incomplete = this.header.incomplete;
  }

  // leggo dal servizio header l'elenco delle configurazioni mancanti (variabile tracked del servizio)
  get incomplete() {
    return this.header.incomplete;
  }

  // estrae il numero di configurazioni mancanti
  get incompleteLength() {
    if (!this.incomplete) return 0;
    return this.incomplete.length;
  }

  // al click su una delle voci mancanti, fa il redirect opportuno
  /*
  @action
  goTo(page) {
    this.router.transitionTo(page);
  }
  */

  refresh = task({ drop: true }, async () => {
    if (this.permissions.hasPermissions(['canSeeIncompleteConfigurations'])) {
      this.header.incomplete = await getIncomplete(this.fetch, this.session);
    }
  });
}
