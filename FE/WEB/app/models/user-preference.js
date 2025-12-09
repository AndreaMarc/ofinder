import Model, { attr } from '@ember-data/model';

export default class UserPreferenceModel extends Model {
  @attr('string', { defaultValue: '' }) userId;
  @attr('number', { defaultValue: 0 }) tenantId;
  @attr('string', { defaultValue: '' }) prefKey;
  @attr('string', { defaultValue: '' }) prefValue;
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  createdAt;
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  updatedAt;
}
