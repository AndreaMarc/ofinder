import Model, { attr, belongsTo } from '@ember-data/model';

export default class GeoMappingModel extends Model {
  @belongsTo('geo-country', { async: true, inverse: 'geoMappings' })
  geoCountry;
  @belongsTo('geo-city', { async: true, inverse: 'geoMappings' })
  GeoCity;
  @belongsTo('geo-first-division', { async: true, inverse: 'geoMappings' })
  GeoFirstDivision;
  @belongsTo('geo-second-division', { async: true, inverse: 'geoMappings' })
  GeoSecondDivision;
  @belongsTo('geo-third-division', { async: true, inverse: 'geoMappings' })
  GeoThirdDivision;

  @attr('string', { defaultValue: '' }) zipCode;
  @attr('number', { defaultValue: 0 }) geoFirstDivisionId;
  @attr('number', { defaultValue: 0 }) geoSecondDivisionId;
  @attr('number', { defaultValue: 0 }) geoThirdDivisionId;
  @attr('number', { defaultValue: 0 }) geoCityId;
  @attr('number', { defaultValue: 0 }) geoCountryId;
}
