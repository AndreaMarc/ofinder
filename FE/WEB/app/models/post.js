import Model, { attr } from '@ember-data/model';
import { inject as service } from '@ember/service';

export default class PostModel extends Model {
  @service session;

  async beforeModel(transition) {
    await this.session.setup();
    this.session.requireAuthentication(transition, 'login');
  }

  @attr title;
  @attr body;
  @attr('date') publishedAt;
}
