import Model, { attr } from '@ember-data/model';

export default class RoleClaimModel extends Model {
  @attr('string', { defaultValue: '' }) roleId;
  @attr('string', { defaultValue: '' }) claimType;
  @attr('string', { defaultValue: '' }) claimValue;
}
