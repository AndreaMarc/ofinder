import Model, { attr, belongsTo } from '@ember-data/model';

export default class BannedUserModel extends Model {
  @belongsTo('user', { async: true, inverse: 'bannedUser' }) user;

  @attr('string', { defaultValue: '' }) userId;
  @attr('number', { defaultValue: 0 }) tenantId;
  @attr('string', { defaultValue: '' }) supervisorId;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  lockStart;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  lockEnd;
  @attr('number', { defaultValue: 0 }) lockDays;
  @attr('boolean', { defaultValue: true }) lockActive;
  @attr('boolean', { defaultValue: false }) crossTenantBanned;
}
