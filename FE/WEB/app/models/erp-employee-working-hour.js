import Model, { attr, belongsTo } from '@ember-data/model';

export default class ErpEmployeeWorkingHourModel extends Model {
  @belongsTo('erp-employee') employee; // Collegamento al dipendente

  @attr('string', { defaultValue: '' }) erpEmployeeId; // collegamento con il dipendente
  @attr('number') tenantId; // Identificativo numerico del tenant
  @attr('number', { defaultValue: 0 }) day; // Giorno della settimana (0 = Domenica, ..., 6 = Sabato)
  @attr('string', { defaultValue: '' }) startTime; // Ora di inizio personalizzata
  @attr('string', { defaultValue: '' }) endTime; // Ora di fine personalizzata
  @attr('number', { defaultValue: null }) startFlexibility; // Flessibilità per l'inizio (in minuti)
  @attr('number', { defaultValue: null }) endFlexibility; // Flessibilità per la fine (in minuti)
  @attr('number', { defaultValue: null }) minimumBreakDuration; // Durata minima della pausa (in minuti)
  @attr('number', { defaultValue: null }) maximumBreakDuration; // Durata massima della pausa (in minuti)
  @attr('number', { defaultValue: null }) dailyWorkingTime; // Minuti lavorativie giornalieri
  @attr('date-utc') createdAt; // Data di creazione del record
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  updatedAt; // Data di ultima modifica del record
}
