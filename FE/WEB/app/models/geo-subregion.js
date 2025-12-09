import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class GeoSubregionModel extends Model {
  @belongsTo('geo-region', { async: true, inverse: 'geoSubregions' }) geoRegion;
  @hasMany('geo-country', { async: true, inverse: 'geoSubregion' })
  GeoCountries;

  @attr('string', { defaultValue: '' }) name;
  @attr('string', { defaultValue: '' }) translations;
  @attr('number', { defaultValue: 0 }) geoRegionId;
}
