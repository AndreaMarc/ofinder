import Model, { attr } from '@ember-data/model';

export default class ThirdPartsTokenModel extends Model {
  @attr('string', { defaultValue: '' }) userId;
  @attr('string', { defaultValue: '' }) accessToken;
  @attr('string', { defaultValue: '' }) refreshToken;
  @attr('string', { defaultValue: '' }) otpId;
  @attr('string', { defaultValue: '' }) email;
  @attr('string', { defaultValue: '' }) accessType;
  @attr('utc-date') nextRefreshTokenDateTime; // nel futuro
}
