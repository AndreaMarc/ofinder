import Model, { attr, belongsTo } from '@ember-data/model';

/**
 * Performer View Model
 * Registra ogni click sul profilo del performer (tracking visualizzazioni)
 *
 * NOTA: Vincolo UNIQUE su coppia (user_id, performer_id)
 * per evitare duplicati per utente
 */
export default class PerformerViewModel extends Model {
  // ============================================================
  // RELATIONSHIPS
  // ============================================================

  /**
   * FK to performer
   */
  @belongsTo('performer', { async: true, inverse: 'views' }) performer;

  /**
   * FK to user (nullable per utenti anonimi)
   * inverse: null perché è tracking unidirezionale
   */
  @belongsTo('user', { async: true, inverse: null }) user;

  // ============================================================
  // ATTRIBUTES
  // ============================================================

  /**
   * Timestamp visualizzazione
   */
  @attr('date-utc') viewedAt;

  // ============================================================
  // COMPUTED PROPERTIES
  // ============================================================

  /**
   * Check se è una view da utente anonimo
   * @returns {boolean}
   */
  get isAnonymous() {
    return !this.user;
  }

  /**
   * Check se è una view da utente registrato
   * @returns {boolean}
   */
  get isRegistered() {
    return !!this.user;
  }

  /**
   * Data visualizzazione formattata
   * @returns {string}
   */
  get formattedDate() {
    if (!this.viewedAt) return '';

    const date = new Date(this.viewedAt);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');

    return `${day}/${month}/${year} ${hours}:${minutes}`;
  }
}
