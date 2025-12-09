import Model, { attr, belongsTo } from '@ember-data/model';

export default class TemplateModel extends Model {
  @belongsTo('category', { async: true, inverse: 'template' }) category;

  @attr('string', { defaultValue: '' }) name;
  @attr('string', { defaultValue: '' }) description;
  @attr('string', { defaultValue: '' }) objectText;
  @attr('string', { defaultValue: '' }) content;
  @attr('string', { defaultValue: '' }) contentNoHtml;
  @attr('number', { defaultValue: 0 }) categoryId;
  @attr('boolean', { defaultValue: false }) active;
  @attr('string', { defaultValue: '' }) tags;
  @attr('string', { defaultValue: '' }) code;
  @attr('string', { defaultValue: '' }) language;
  @attr('boolean', { defaultValue: false }) erasable;
  @attr('boolean', { defaultValue: true }) copyInNewTenants; // indica se va copiato nei nuovi tenant

  @attr('boolean', { defaultValue: false }) erased;
  @attr('number', { defaultValue: 0 }) order;
  @attr('string', { defaultValue: '' }) featuredImage;
  @attr('arrays', { defaultValue: '{}' }) freeField;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  createdAt;
}
