import Model, { attr, belongsTo } from '@ember-data/model';

export default class MediaFileModel extends Model {
  @belongsTo('media-category', { async: true, inverse: 'typologyMediaFiles' })
  typologyAreaRel;
  @belongsTo('media-category', { async: true, inverse: 'categoryMediaFiles' })
  categoryRel;
  @belongsTo('media-category', { async: true, inverse: 'albumMediaFiles' })
  albumRel;

  @attr('string', { defaultValue: '' }) album;
  @attr('string', { defaultValue: '' }) alt;
  @attr('string', { defaultValue: '' }) base64;
  @attr('string', { defaultValue: '' }) category;
  @attr('string', { defaultValue: '' }) extension;
  @attr('string', { defaultValue: '' }) primaryContentType;
  @attr('string', { defaultValue: '' }) fileUrl;
  @attr('string', { defaultValue: '' }) originalFileName;
  @attr('string', { defaultValue: '' }) tag;
  @attr('number', { defaultValue: '' }) tenantId;
  @attr('string', { defaultValue: '' }) typologyArea;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  uploadDate;
  @attr('string', { defaultValue: '' }) userGuid;
  @attr('boolean', { defaultValue: false }) global;
}
