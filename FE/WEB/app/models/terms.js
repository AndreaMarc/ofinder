import Model, { attr } from '@ember-data/model';

export default class TermsModel extends Model {
  @attr('string') description;
  @attr('boolean', { defaultValue: false }) active;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  lastUpdate;
}
