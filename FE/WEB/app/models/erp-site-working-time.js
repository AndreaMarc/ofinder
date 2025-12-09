import Model, { attr, belongsTo } from '@ember-data/model';

export default class ErpSiteWorkingTimeModel extends Model {
  @belongsTo('erp-site', { async: true, inverse: null }) site; // Collegamento alla sede (erp-site)
  @belongsTo('erp-shift', { async: true, inverse: null }) shift; // Collegamento al turno (erp-shift)
  @belongsTo('tenant', { async: true, inverse: null }) tenant;

  @attr('number') tenantId; // Identificativo del tenant
  @attr('number') day; // Giorno della settimana (0 = Domenica, ..., 6 = Sabato)
  @attr('number', { defaultValue: 0 }) startFlexibility; // Tolleranza per l'orario di inizio (in minuti)
  @attr('number', { defaultValue: 0 }) endFlexibility; // Tolleranza per l'orario di fine (in minuti)
  @attr('number', { defaultValue: 45 }) minimumBreakDuration; // Durata minima della pausa (in minuti)
  @attr('number', { defaultValue: 75 }) maximumBreakDuration; // Durata massima della pausa (in minuti)
  @attr('date-utc') createdAt; // Data di creazione del record
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  updatedAt; // Data di ultima modifica del record
}
