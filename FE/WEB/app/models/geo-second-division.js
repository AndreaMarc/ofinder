import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class GeoSecondDivisionModel extends Model {
  @belongsTo('geo-country', { async: true, inverse: 'geoSecondDivisions' })
  geoCountry;
  @hasMany('geo-first-division', { async: true, inverse: 'GeoSecondDivisions' })
  GeoFirstDivision;
  @hasMany('geo-third-division', { async: true, inverse: 'GeoSecondDivisions' })
  GeoThirdDivisions;
  @hasMany('geo-mapping', { async: true, inverse: 'GeoSecondDivision' })
  geoMappings;

  @attr('string', { defaultValue: '' }) name;
  @attr('number', { defaultValue: 0 }) geoCountryId;
  @attr('number', { defaultValue: 0 }) geoFirstDivisionId;
}
