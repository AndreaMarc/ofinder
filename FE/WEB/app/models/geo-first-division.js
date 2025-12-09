import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class GeoFirstDivisionModel extends Model {
  @belongsTo('geo-country', { async: true, inverse: 'geoFirstDivisions' })
  geoCountry;

  @hasMany('geo-second-division', { async: true, inverse: 'GeoFirstDivision' })
  GeoSecondDivisions;
  @hasMany('geo-third-division', { async: true, inverse: null })
  GeoThirdDivisions;
  @hasMany('geo-mapping', { async: true, inverse: 'GeoFirstDivision' })
  geoMappings;

  @attr('string', { defaultValue: '' }) name;
  @attr('number', { defaultValue: 0 }) geoCountryId;
}
