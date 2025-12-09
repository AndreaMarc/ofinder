import Model, { attr, belongsTo } from '@ember-data/model';

export default class UserRoleModel extends Model {
  @belongsTo('user', { async: true, inverse: 'userRoles' }) user;
  @belongsTo('role', { async: true, inverse: 'userRoles' }) role;

  @attr('string', { defaultValue: '' }) roleId;
  @attr('string', { defaultValue: '' }) userId;
  @attr('number', { defaultValue: 0 }) tenantId;
}
