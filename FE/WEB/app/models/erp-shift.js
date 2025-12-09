import Model, { attr, belongsTo } from '@ember-data/model';

export default class ErpShiftModel extends Model {
  @belongsTo('tenant', { async: true, inverse: null }) tenant;

  @attr('number') tenantId; // Identificativo del tenant
  @attr('string') name; // Nome del turno
  @attr('string') description; // Descrizione del turno
  @attr('string') startTime; // Ora di inizio del turno
  @attr('number', { defaultValue: 480 }) standardWorkingTime; // Minuti lavorativi standard
  @attr('date-utc') createdAt; // Data di creazione del record
  @attr('date-utc', {
    defaultValue() {
      return new Date();
    },
  })
  updatedAt; // Data di ultima modifica del record

  // Calcola l'orario di fine in base a startTime e standardWorkingTime
  get calculatedEndTime() {
    let startDateTime = new Date(`1970-01-01T${this.startTime}:00`);
    let endDateTime = new Date(
      startDateTime.getTime() + this.standardWorkingTime * 60 * 60 * 1000
    );
    return endDateTime.toTimeString().slice(0, 5); // Restituisce il formato HH:mm
  }
}
