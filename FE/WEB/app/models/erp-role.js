import Model, { attr, belongsTo } from '@ember-data/model';

export default class ErpRoleModel extends Model {
  @belongsTo('tenant', { async: true, inverse: null }) tenant;
  @belongsTo('role', { async: true, inverse: null }) role;

  @attr('number') tenantId; // Identificativo numerico del tenant
  @attr('string') roleId; // Identificativo del ruolo del FWK
  @attr('boolean', { defaultValue: false }) isManagerial; // indica se il ruolo ha responsabilità manageriali
  @attr('date-utc') createdAt; // Data di creazione del record
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  updatedAt; // Data di ultima modifica del record
}
