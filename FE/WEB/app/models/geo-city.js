import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class GeoCityModel extends Model {
  @belongsTo('geo-country', { async: true, inverse: 'geoCities' }) geoCountry;
  @hasMany('geo-mapping', { async: true, inverse: 'GeoCity' })
  geoMappings;

  @attr('string', { defaultValue: '' }) name;
  @attr('number', { defaultValue: 0 }) geoCountryId;
}
