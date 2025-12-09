import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';

export default class TemplateDetailsRoute extends Route {
  @service statusService;
  @service session;
  @service store;
  @service router;

  showObject = false;
  showTags = false;
  showFeaturedImage = false;

  async beforeModel(transition) {
    await this.session.setup();
    this.session.requireAuthentication(transition, 'login');

    let queryParams = transition.to.queryParams;
    this.showObject = false;
    if (
      queryParams.showObject &&
      queryParams.showObject !== '' &&
      queryParams.showObject !== 'false'
    )
      this.showObject = !!queryParams.showObject;

    this.showTags = false;
    if (
      queryParams.showTags &&
      queryParams.showTags !== '' &&
      queryParams.showTags !== 'false'
    )
      this.showTags = !!queryParams.showTags;

    this.showFeaturedImage = false;
    if (
      queryParams.showFeaturedImage &&
      queryParams.showFeaturedImage !== '' &&
      queryParams.showFeaturedImage !== 'false'
    )
      this.showFeaturedImage = !!queryParams.showFeaturedImage;
  }

  async model(params) {
    let { template_id } = params;
    //let r = await this.store.findRecord('template', template_id);
    let r = await this.store.queryRecord('template', {
      filter: `equals(id,'${template_id}')`,
      include: 'category',
    });
    return {
      record: r,
      renderKey: Date.now(),
      showObject: this.showObject,
      showTags: this.showTags,
      showFeaturedImage: this.showFeaturedImage,
    };
  }

  @action
  willTransition(transition) {
    if (this.statusService.isTemplateModified) {
      let confirmed = window.confirm(
        `Hai delle modifiche non salvate. Sei sicuro di voler abbandonare la pagina?`
      );

      if (!confirmed) {
        transition.abort();
        this.statusService.isTemplateModified = false;
        this.refresh();
      } else {
        // resetta il flag
        this.statusService.isTemplateModified = false;
      }
    }
  }
}
