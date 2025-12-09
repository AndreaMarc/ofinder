import Model, { attr, belongsTo } from '@ember-data/model';

export default class ErpSiteUserMappingModel extends Model {
  @belongsTo('erp-site', { async: true, inverse: null }) site; // Relazione con la sede
  @belongsTo('user', { async: true, inverse: null }) user; // Relazione con l'utente

  @attr('number') tenantId; // Identificativo numerico del tenant
  @attr('string') siteId; // Identificativo della sede
  @attr('string') employeeId; // Identificativo del dipendente
  @attr('boolean', { defaultValue: false }) isPrimary; // Indica se è la sede principale per l’utente
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  mappingStartDate; // Data di inizio del collegamento
  @attr('date-utc') mappingEndDate; // Data di fine del collegamento
  @attr('date-utc') createdAt; // Data di creazione del record
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  updatedAt; // Data di ultima modifica del record
}
