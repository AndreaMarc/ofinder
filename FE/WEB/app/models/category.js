import Model, { attr, hasMany, belongsTo } from '@ember-data/model';

export default class CategoryModel extends Model {
  @hasMany('template', { async: true, inverse: 'category' }) template;
  @belongsTo('tenant', { async: true, inverse: 'category' }) tenant;

  @attr('string', { defaultValue: '' }) name;
  @attr('string', { defaultValue: '' }) code;
  @attr('string', { defaultValue: '' }) description;
  @attr('string', { defaultValue: '' }) type;
  @attr('number', { defaultValue: 0 }) parentCategory;
  @attr('number', { defaultValue: '' }) tenantId;
  @attr('boolean', { defaultValue: false }) copyInNewTenants; // indica se la categoria e i suoi template vanno copiati nei nuovi tenant

  @attr('number', { defaultValue: 0 }) order;
  @attr('boolean', { defaultValue: true }) erasable;
}
