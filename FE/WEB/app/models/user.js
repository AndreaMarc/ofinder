import Model, { attr, hasMany, belongsTo } from '@ember-data/model';

export default class UserModel extends Model {
  @hasMany('user-role', { async: true, inverse: 'user' }) userRoles;
  @hasMany('user-tenant', { async: true, inverse: 'user' }) userTenants;
  @belongsTo('user-profile', { async: true, inverse: 'user' }) userProfile;
  @hasMany('banned-user', { async: true, inverse: 'user' }) bannedUser;

  @attr('number', { defaultValue: 0 }) accessFailedCount;
  @attr('date') lockoutEnd;
  @attr('date') lastAccess;
  @attr('date') passwordLastChange;
  @attr('boolean', { defaultValue: true }) isPasswprdMd5;
  @attr('boolean', { defaultValue: false }) fakeEmail;

  @attr('string', { defaultValue: '' }) email;
  @attr('date') lockOutEnd; // per bannaggio utente
  @attr('string', { defaultValue: '' }) userName;
  @attr('number', { defaultValue: 0 }) tenantId;
  @attr('boolean', { defaultValue: false }) deleted;
  @attr('string', { defaultValue: '' }) freeFieldString1;
  @attr('string', { defaultValue: '' }) freeFieldString2;
  @attr('string', { defaultValue: '' }) freeFieldString3;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  freeFieldDateTime;
  @attr('number', { defaultValue: 0 }) freeFieldInt2;
  @attr('number', { defaultValue: 0 }) freeFieldInt1;
  @attr('boolean', { defaultValue: false }) freeFieldBoolean;
}
