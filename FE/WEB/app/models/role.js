import Model, { attr, hasMany } from '@ember-data/model';

export default class RoleModel extends Model {
  @hasMany('user-role', { async: true, inverse: 'role' }) userRoles;

  @attr('string', { defaultValue: '' }) name;
  @attr('number', { defaultValue: 0 }) tenantId;
  @attr('boolean', { defaultValue: false }) needful; // se true, il ruolo non può essere cancellato da interfaccia
  @attr('string', { defaultValue: '' }) typology; // nome dell'addon che ha aggiunto il ruolo, in formato camelcase
  @attr('string', { defaultValue: '' }) initials; // eventule sigla del ruolo (es: CEO)
  @attr('number', { defaultValue: 0 }) level;
}
