import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';

export default class LegalsDetailsRoute extends Route {
  @service statusService;
  @service session;
  @service store;
  @service router;

  async model(params) {
    let { legal_id } = params;

    let record = this.store.peekRecord('legal-term', legal_id);
    if (record) {
      return { record: record };
    } else {
      let r = await this.store.findRecord('legal-term', legal_id);
      return { record: r };
    }
  }

  @action
  willTransition(transition) {
    if (this.statusService.isLegalModified) {
      let confirmed = window.confirm(
        `Hai delle modifiche non salvate. Sei sicuro di voler abbandonare la pagina?`
      );

      if (!confirmed) {
        transition.abort();
        this.statusService.isLegalModified = false;
        this.refresh();
      } else {
        // resetta il flag
        this.statusService.isLegalModified = false;
      }
    }
  }
}
