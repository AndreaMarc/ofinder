import Model, { attr, belongsTo } from '@ember-data/model';

export default class UserProfileModel extends Model {
  @belongsTo('user', { async: true, inverse: 'userProfile' }) user;

  @attr('string', { defaultValue: '' }) userId;
  @attr('string', { defaultValue: '' }) contactEmail;
  @attr('string', { defaultValue: '' }) firstName;
  @attr('string', { defaultValue: '' }) lastName;
  @attr('string', { defaultValue: '' }) nickName;
  @attr('string', { defaultValue: '' }) fixedPhone;
  @attr('string', { defaultValue: '' }) mobilePhone;
  @attr('string', { defaultValue: '' }) sex;
  @attr('string', { defaultValue: '' }) taxId;

  @attr('date') birthDate;
  @attr('string', { defaultValue: '' }) birthCity;
  @attr('string', { defaultValue: '' }) birthProvince;
  @attr('string', { defaultValue: '' }) birthZIP;
  @attr('string', { defaultValue: '' }) birthState;

  @attr('string', { defaultValue: '' }) residenceCity;
  @attr('string', { defaultValue: '' }) residenceProvince;
  @attr('string', { defaultValue: '' }) residenceZIP;
  @attr('string', { defaultValue: '' }) residenceState;
  @attr('string', { defaultValue: '' }) residenceAddress;
  @attr('string', { defaultValue: '' }) residenceHouseNumber;

  @attr('string', { defaultValue: '' }) occupation;
  @attr('string', { defaultValue: '' }) description;

  @attr('string', { defaultValue: '' }) profileImageId;

  @attr('string', { defaultValue: '' }) profileFreeFieldString1;
  @attr('string', { defaultValue: '' }) profileFreeFieldString2;
  @attr('string', { defaultValue: '' }) profileFreeFieldString3;
  @attr('date') profileFreeFieldDateTime;
  @attr('number', { defaultValue: 0 }) profileFreeFieldInt1;
  @attr('number', { defaultValue: 0 }) profileFreeFieldInt2;
  @attr('boolean', { defaultValue: false }) profileFreeFieldBoolean;

  @attr('string', { defaultValue: '' }) userLang;

  @attr('string', { defaultValue: 'limited' }) cookieAccepted;
  @attr('boolean', { defaultValue: false }) termsAccepted;
  @attr('date') termsAcceptanceDate;
  @attr('date') registrationDate;
}
