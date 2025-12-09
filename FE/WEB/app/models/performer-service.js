import Model, { attr, belongsTo } from '@ember-data/model';

/**
 * Performer Service Model
 * Indica servizi opzionali offerti dai performer
 * Es: "model", "escort", ecc.
 */
export default class PerformerServiceModel extends Model {
  // ============================================================
  // RELATIONSHIPS
  // ============================================================

  /**
   * FK to performer
   */
  @belongsTo('performer', { async: true, inverse: 'services' }) performer;

  // ============================================================
  // ATTRIBUTES
  // ============================================================

  /**
   * Tipo di servizio
   * Es: "model", "escort", "influencer", ecc.
   */
  @attr('string') serviceType;

  /**
   * Link esterno relativo al servizio
   * Es: portfolio, sito web, profilo agenzia, ecc.
   */
  @attr('string') link;

  /**
   * Descrizione del servizio
   */
  @attr('string') description;

  /**
   * Timestamps
   */
  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;

  // ============================================================
  // COMPUTED PROPERTIES
  // ============================================================

  /**
   * Label per visualizzazione
   * @returns {string}
   */
  get label() {
    return this.serviceType || '';
  }

  /**
   * Check se ha un link
   * @returns {boolean}
   */
  get hasLink() {
    return !!(this.link && this.link.trim().length > 0);
  }

  /**
   * Check se ha descrizione
   * @returns {boolean}
   */
  get hasDescription() {
    return !!(this.description && this.description.trim().length > 0);
  }

  /**
   * Icon class basato sul tipo di servizio
   * @returns {string}
   */
  get iconClass() {
    const serviceType = (this.serviceType || '').toLowerCase();

    if (serviceType.includes('model')) return 'fa fa-user-tie';
    if (serviceType.includes('escort')) return 'fa fa-heart';
    if (serviceType.includes('influencer')) return 'fa fa-star';
    if (serviceType.includes('content')) return 'fa fa-photo-video';

    return 'fa fa-briefcase';
  }

  /**
   * Badge color basato sul tipo
   * @returns {string}
   */
  get badgeClass() {
    const serviceType = (this.serviceType || '').toLowerCase();

    if (serviceType.includes('model')) return 'badge-primary';
    if (serviceType.includes('escort')) return 'badge-danger';
    if (serviceType.includes('influencer')) return 'badge-warning';

    return 'badge-secondary';
  }
}
