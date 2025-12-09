import Model, { attr, belongsTo } from '@ember-data/model';

export default class ErpEmployeeModel extends Model {
  @belongsTo('user', { async: true, inverse: null }) user;
  @belongsTo('tenant', { async: true, inverse: null }) tenant;

  @attr('number') tenantId; // Identificativo numerico del tenant
  @attr('string') userId; // Identificativo dell'utente associato
  @attr('boolean', { defaultValue: true }) isActive; // Stato del dipendente (attivo o no)
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  hiredDate; // Data di assunzione
  @attr('date-utc') terminatedDate; // Data di cessazione (se applicabile)
  @attr('string', { defaultValue: 'full-time' }) contractType; // Tipo di contratto ('full-time', 'part-time')
  @attr('string', { defaultValue: 'internal' }) employeeType; // Tipo di dipendente ('internal' o 'external')
  @attr('date-utc') createdAt; // Data di creazione del record
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  updatedAt; // Data di ultima modifica del record
}
