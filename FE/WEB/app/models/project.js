import Model, { attr } from '@ember-data/model';

export default class ProjectModel extends Model {
  @attr('string', { defaultValue: '' }) name;
  @attr('number', { defaultValue: 0 }) tenantId;
}
