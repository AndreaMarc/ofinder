import Model, { attr, belongsTo } from '@ember-data/model';

export default class ErpEmployeeRoleModel extends Model {
  @belongsTo('erp-employee', { async: true, inverse: null }) employee; // Relazione con il dipendente
  @belongsTo('erp-role', { async: true, inverse: null }) role; // Relazione con il ruolo (erp-role)
  @belongsTo('erp-employee', { async: true, inverse: null }) manager; // Relazione con il manager
  @belongsTo('tenant', { async: true, inverse: null }) tenant;

  @attr('number') tenantId; // Identificativo numerico del tenant
  @attr('string') employeeId; // Identificativo del dipendente
  @attr('string') erpRoleId; // Identificativo del ruolo
  @attr('string') managerId; // Identificativo del manager
  @attr('date-utc') startDate; // Data di inizio validità del ruolo
  @attr('date-utc') endDate; // Data di fine validità del ruolo
  @attr('date-utc') createdAt; // Data di creazione del record
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  updatedAt; // Data di ultima modifica del record
}
