import Model, { attr } from '@ember-data/model';

export default class UserDeviceModel extends Model {
  @attr('string', { defaultValue: '' }) userId;
  @attr('string', { defaultValue: '' }) deviceHash;
  @attr('string', { defaultValue: '' }) salt;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  lastAccess;
  @attr('string', { defaultValue: '' }) pushToken;
  @attr('string', { defaultValue: '' }) appleToken;
  @attr('string', { defaultValue: '' }) googleToken;
  @attr('string', { defaultValue: '' }) facebookToken;
  @attr('string', { defaultValue: '' }) twitterToken;
  @attr('string', { defaultValue: '' }) platform;
  @attr('string', { defaultValue: '' }) deviceName;
}
