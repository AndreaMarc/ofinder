import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class GeoThirdDivisionModel extends Model {
  @belongsTo('geo-country', { async: true, inverse: 'geoFirstDivisions' })
  geoCountry;
  @hasMany('geo-first-division', { async: true, inverse: 'GeoSecondDivisions' })
  GeoFirstDivision;
  @hasMany('geo-second-division', { async: true, inverse: 'GeoThirdDivisions' })
  GeoSecondDivisions;
  @hasMany('geo-mapping', { async: true, inverse: 'GeoThirdDivision' })
  geoMappings;

  @attr('string', { defaultValue: '' }) name;
  @attr('number', { defaultValue: 0 }) geoCountryId;
  @attr('number', { defaultValue: 0 }) geoFirstDivisionId;
  @attr('number', { defaultValue: 0 }) geoSecondDivisionId;
}
