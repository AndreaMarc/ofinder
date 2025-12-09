import Model, { attr, belongsTo } from '@ember-data/model';

/**
 * User Favorite Model
 * Consente agli utenti loggati di crearsi una lista di performer preferiti
 *
 * NOTA: Vincolo UNIQUE su coppia (user_id, performer_id)
 */
export default class UserFavoriteModel extends Model {
  // ============================================================
  // RELATIONSHIPS
  // ============================================================

  /**
   * FK to user
   */
  @belongsTo('user', { async: true, inverse: null }) user;

  /**
   * FK to performer
   */
  @belongsTo('performer', { async: true, inverse: null }) performer;

  // ============================================================
  // ATTRIBUTES
  // ============================================================

  /**
   * Data aggiunta ai preferiti
   */
  @attr('date-utc') createdAt;

  // ============================================================
  // COMPUTED PROPERTIES
  // ============================================================

  /**
   * Data formattata
   * @returns {string}
   */
  get formattedDate() {
    if (!this.createdAt) return '';

    const date = new Date(this.createdAt);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
  }

  /**
   * Nome del performer (helper per UI)
   * @returns {string}
   */
  get performerName() {
    return this.performer?.get('fullName') || '';
  }
}
