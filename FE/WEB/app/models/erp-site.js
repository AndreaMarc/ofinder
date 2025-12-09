import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class ErpSiteModel extends Model {
  @belongsTo('erp-site', { async: true, inverse: 'childSites' }) parentSite; // Relazione con la sede madre
  @hasMany('erp-site', { async: true, inverse: 'parentSite' }) childSites; // Relazione con le sotto-sedi

  @attr('number') tenantId; // Identificativo del tenant
  @attr('string', { defaultValue: '' }) name; // Nome della sede
  @attr('string') address; // Indirizzo della sede
  @attr('number') addressNumber; // Numero civico
  @attr('string') phone; // Numero di telefono
  @attr('string') city; // Città
  @attr('string') province; // Provincia
  @attr('string') zip; // Codice postale
  @attr('string') state; // Stato (es. 'IT', 'US')
  @attr('boolean', { defaultValue: false }) administrativeHeadquarters; // Sede amministrativa
  @attr('boolean', { defaultValue: false }) registeredOffice; // Sede legale
  @attr('boolean', { defaultValue: false }) operationalHeadquarters; // Sede operativa
  @attr('string') parentSiteId; // Guid della sede madre
  @attr('date-utc') createdAt; // Data di creazione del record
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  updatedAt; // Data di ultima modifica del record
}
