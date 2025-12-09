import Model, { attr, hasMany, belongsTo } from '@ember-data/model';

export default class MediaCategoryModel extends Model {
  @hasMany('media-file', { async: true, inverse: 'typologyAreaRel' })
  typologyMediaFiles;
  @hasMany('media-file', { async: true, inverse: 'categoryRel' })
  categoryMediaFiles;
  @hasMany('media-file', { async: true, inverse: 'albumRel' }) albumMediaFiles;
  @belongsTo('tenant', { async: true, inverse: 'mediaCategory' }) tenant;

  @attr('string', { defaultValue: '' }) name;
  @attr('string', { defaultValue: '' }) code;
  @attr('string', { defaultValue: '' }) description;
  @attr('number', { defaultValue: 0 }) tenantId;
  @attr('boolean', { defaultValue: true }) erasable;
  @attr('boolean', { defaultValue: false }) copyInNewTenant;
  @attr('string', { defaultValue: '' }) parentMediaCategory;
  @attr('string', { defaultValue: '' }) type; // 'typology', 'category', 'album'
  @attr('number', { defaultValue: 0 }) order;
}
