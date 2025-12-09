import Model, { attr, belongsTo } from '@ember-data/model';

export default class ErpExtrenalWorkerDetailModel extends Model {
  @belongsTo('erp-employee', { async: true, inverse: null }) employee; // Relazione con l'employee
  @belongsTo('tenant', { async: true, inverse: null }) tenant;

  @attr('number') tenantId; // Identificativo del tenant
  @attr('string') vatNumber; // Partita IVA
  @attr('string') contractDetails; // Dettagli del contratto
  @attr('string', { defaultValue: 'hourly' }) paymentFrequency; // Frequenza di pagamento
  @attr('number') hourlyRate; // Tariffa oraria
  @attr('date-utc') createdAt; // Data di creazione del record
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  updatedAt; // Data di ultima modifica del record
}
