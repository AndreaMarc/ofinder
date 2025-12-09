import Model, { attr, belongsTo } from '@ember-data/model';

/**
 * Performer Review Model
 * Raccoglie i voti e le recensioni dei performer espressi dagli utenti loggati
 *
 * NOTA: Vincolo UNIQUE su coppia (performer_id, user_id)
 * Un utente può recensire un performer una sola volta
 */
export default class PerformerReviewModel extends Model {
  // ============================================================
  // RELATIONSHIPS
  // ============================================================

  /**
   * FK to performer
   */
  @belongsTo('performer', { async: true, inverse: 'reviews' }) performer;

  /**
   * FK to user (autore della recensione)
   */
  @belongsTo('user', { async: true, inverse: null }) user;

  // ============================================================
  // ATTRIBUTES
  // ============================================================

  /**
   * Rating (1-5 stelle)
   */
  @attr('number') rating;

  /**
   * Testo della recensione (opzionale)
   */
  @attr('string') reviewText;

  /**
   * Timestamps
   */
  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;

  // ============================================================
  // COMPUTED PROPERTIES
  // ============================================================

  /**
   * Rating come numero intero (per stelle piene)
   * @returns {number}
   */
  get ratingStars() {
    return Math.round(this.rating || 0);
  }

  /**
   * Check se ha testo recensione
   * @returns {boolean}
   */
  get hasReviewText() {
    return !!(this.reviewText && this.reviewText.trim().length > 0);
  }

  /**
   * Nome dell'autore (da user)
   * @returns {string}
   */
  get authorName() {
    const userProfile = this.user?.get('userProfile');
    if (!userProfile) return 'Anonimo';

    const firstName = userProfile.get('firstName') || '';
    const lastName = userProfile.get('lastName') || '';

    return `${firstName} ${lastName}`.trim() || 'Anonimo';
  }

  /**
   * Data recensione formattata
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
   * Label classe CSS per rating
   * @returns {string}
   */
  get ratingClass() {
    const rating = this.rating || 0;

    if (rating >= 4.5) return 'rating-excellent';
    if (rating >= 3.5) return 'rating-good';
    if (rating >= 2.5) return 'rating-average';
    if (rating >= 1.5) return 'rating-poor';
    return 'rating-bad';
  }
}
