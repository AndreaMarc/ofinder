import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

/**
 * Channel Model
 * Ogni canale/social/piattaforma gestito dal performer
 * Es: OnlyFans, Fansly, Telegram, Instagram, ecc.
 */
export default class ChannelModel extends Model {
  // ============================================================
  // RELATIONSHIPS
  // ============================================================

  /**
   * FK to performer
   */
  @belongsTo('performer', { async: true, inverse: 'channels' }) performer;

  /**
   * Has many schedules (orari live per questo canale)
   */
  @hasMany('channel-schedule', { async: true, inverse: 'channel' }) schedules;

  /**
   * Has many content types (tipi di contenuti pubblicati)
   */
  @hasMany('channel-content-type', { async: true, inverse: 'channel' })
  contentTypes;

  /**
   * Has one pricing (tariffe per questo canale)
   */
  @belongsTo('channel-pricing', { async: true, inverse: 'channel' }) pricing;

  // ============================================================
  // ATTRIBUTES
  // ============================================================

  /**
   * Nome piattaforma
   * ENUM: OnlyFans, Fansly, Telegram, Instagram, Facebook, X, ecc.
   */
  @attr('string') platform;

  /**
   * Username/handle su quella piattaforma
   * Es: @username
   */
  @attr('string') usernameHandle;

  /**
   * Link diretto al profilo
   * Es: https://onlyfans.com/username
   */
  @attr('string') profileLink;

  /**
   * Note aggiuntive
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
   * Icona CSS class per la piattaforma
   * @returns {string} CSS class name
   */
  get platformIcon() {
    const icons = {
      OnlyFans: 'icon-onlyfans',
      Fansly: 'icon-fansly',
      Telegram: 'icon-telegram',
      Instagram: 'icon-instagram',
      Facebook: 'icon-facebook',
      X: 'icon-x',
      Twitter: 'icon-twitter',
    };
    return icons[this.platform] || 'icon-link';
  }

  /**
   * Colore brand della piattaforma (per badge/UI)
   * @returns {string} Hex color
   */
  get platformColor() {
    const colors = {
      OnlyFans: '#00AFF0',
      Fansly: '#E91E63',
      Telegram: '#0088CC',
      Instagram: '#E4405F',
      Facebook: '#1877F2',
      X: '#000000',
      Twitter: '#1DA1F2',
    };
    return colors[this.platform] || '#6c757d';
  }

  /**
   * Check se il canale ha orari live
   * @returns {boolean}
   */
  get hasSchedules() {
    return this.schedules?.get('length') > 0;
  }

  /**
   * Check se il canale ha prezzi
   * @returns {boolean}
   */
  get hasPricing() {
    return !!this.pricing;
  }

  /**
   * Label per il link esterno
   * @returns {string}
   */
  get linkLabel() {
    if (this.usernameHandle) {
      return `${this.platform} - ${this.usernameHandle}`;
    }
    return this.platform;
  }
}
