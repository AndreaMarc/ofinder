import Model, { attr, belongsTo } from '@ember-data/model';

/**
 * Channel Schedule Model
 * Orari live o di attività specifici per ciascun canale
 */
export default class ChannelScheduleModel extends Model {
  // ============================================================
  // RELATIONSHIPS
  // ============================================================

  /**
   * FK to channel
   */
  @belongsTo('channel', { async: true, inverse: 'schedules' }) channel;

  // ============================================================
  // ATTRIBUTES
  // ============================================================

  /**
   * Giorno della settimana
   * 0 = Domenica, 1 = Lunedì, 2 = Martedì, ..., 6 = Sabato
   */
  @attr('number') dayOfWeek;

  /**
   * Ora inizio (formato HH:mm o time string)
   */
  @attr('string') startTime;

  /**
   * Ora fine (formato HH:mm o time string)
   */
  @attr('string') endTime;

  /**
   * Note aggiuntive su questo schedule
   */
  @attr('string') note;

  /**
   * Timestamps
   */
  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;

  // ============================================================
  // COMPUTED PROPERTIES
  // ============================================================

  /**
   * Nome del giorno della settimana (italiano)
   * @returns {string}
   */
  get dayName() {
    const days = [
      'Domenica',
      'Lunedì',
      'Martedì',
      'Mercoledì',
      'Giovedì',
      'Venerdì',
      'Sabato',
    ];
    return days[this.dayOfWeek] || '';
  }

  /**
   * Nome giorno abbreviato
   * @returns {string}
   */
  get dayNameShort() {
    const days = ['Dom', 'Lun', 'Mar', 'Mer', 'Gio', 'Ven', 'Sab'];
    return days[this.dayOfWeek] || '';
  }

  /**
   * Range orario formattato
   * Es: "14:00 - 18:00"
   * @returns {string}
   */
  get timeRange() {
    if (!this.startTime || !this.endTime) return '';
    return `${this.startTime} - ${this.endTime}`;
  }

  /**
   * Label completo
   * Es: "Lunedì 14:00 - 18:00"
   * @returns {string}
   */
  get scheduleLabel() {
    return `${this.dayName} ${this.timeRange}`;
  }
}
