import Model, { attr, belongsTo } from '@ember-data/model';

export default class UserTenantModel extends Model {
  @belongsTo('user', { async: true, inverse: 'userTenants' }) user;
  @belongsTo('tenant', { async: true, inverse: 'userTenant' }) tenant;

  @attr('string', { defaultValue: '' }) userId;
  @attr('number', { defaultValue: 0 }) tenantId;
  @attr('string', { defaultValue: 'pending' }) state; // possibili valori: accepted/pending/denied/ownerCreated/selfCreated
  @attr('string', { defaultValue: '' }) ip;
  @attr('date') acceptedAt;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  createdAt;
}
