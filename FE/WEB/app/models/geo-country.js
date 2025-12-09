import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class GeoCountryModel extends Model {
  @belongsTo('geo-region', { async: true, inverse: 'geoCountries' }) geoRegion;
  @belongsTo('geo-subregion', { async: true, inverse: 'GeoCountries' })
  geoSubregion;
  @hasMany('geo-city', { async: true, inverse: 'geoCountry' }) geoCities;
  @hasMany('geo-first-division', { async: true, inverse: 'geoCountry' })
  geoFirstDivisions;
  @hasMany('geo-second-division', { async: true, inverse: 'geoCountry' })
  geoSecondDivisions;
  @hasMany('geo-third-division', { async: true, inverse: 'geoCountry' })
  geoThirdDivisions;
  @hasMany('geo-mapping', { async: true, inverse: 'geoCountry' }) geoMappings;

  @attr('string', { defaultValue: '' }) name;
  @attr('string', { defaultValue: '' }) iso3;
  @attr('string', { defaultValue: '' }) numericCode;
  @attr('string', { defaultValue: '' }) iso2;
  @attr('string', { defaultValue: '' }) phoneCode;
  @attr('string', { defaultValue: '' }) capital;
  @attr('string', { defaultValue: '' }) currency;
  @attr('string', { defaultValue: '' }) currencyName;
  @attr('string', { defaultValue: '' }) currencySymbol;
  @attr('string', { defaultValue: '' }) internetDomain;
  @attr('string', { defaultValue: '' }) native;
  @attr('number', { defaultValue: 0 }) geoRegionId;
  @attr('number', { defaultValue: 0 }) geoSubregionId;
  @attr('string', { defaultValue: '' }) timeZones;
  @attr('string', { defaultValue: '' }) translations;
  @attr('number', { defaultValue: 0 }) numberOfDivisions;
}
