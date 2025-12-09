import Model, { attr, belongsTo } from '@ember-data/model';

export default class OtpModel extends Model {
  //@belongsTo('tenant', { async: true, inverse: null }) tenant;

  @attr('string', { defaultValue: '' }) userId;
  @attr('string', { defaultValue: '' }) tenantId;
  @attr('string', { defaultValue: '' }) otpValue;
  @attr('string', { defaultValue: '' }) otpSended;
  @attr('date') creationDate;
  @attr('boolean', { defaultValue: true }) isValid;
}
