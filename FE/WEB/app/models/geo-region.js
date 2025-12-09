import Model, { attr, hasMany } from '@ember-data/model';

export default class GeoRegionModel extends Model {
  @hasMany('geo-country', { async: true, inverse: 'geoRegion' })
  geoCountries;
  @hasMany('geo-subregion', { async: true, inverse: null })
  geoSubregions;

  @attr('string', { defaultValue: '' }) name;
  @attr('string', { defaultValue: '' }) translations;
}
