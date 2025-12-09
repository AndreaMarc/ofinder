import Model, { attr, belongsTo } from '@ember-data/model';

/**
 * Channel Pricing Model
 * Tariffe e abbonamenti specifici per ciascun canale
 * Range di prezzi (from/to) per ogni tipologia di servizio
 */
export default class ChannelPricingModel extends Model {
  // ============================================================
  // RELATIONSHIPS
  // ============================================================

  /**
   * FK to channel (1:1 relationship)
   */
  @belongsTo('channel', { async: true, inverse: 'pricing' }) channel;

  // ============================================================
  // ATTRIBUTES - Price Ranges
  // ============================================================

  /**
   * Abbonamento mensile
   */
  @attr('number') monthlySubscriptionFrom;
  @attr('number') monthlySubscriptionTo;

  /**
   * Vendita foto
   */
  @attr('number') photoSaleFrom;
  @attr('number') photoSaleTo;

  /**
   * Vendita video
   */
  @attr('number') videoSaleFrom;
  @attr('number') videoSaleTo;

  /**
   * Live public
   */
  @attr('number') livePublicFrom;
  @attr('number') livePublicTo;

  /**
   * Live private
   */
  @attr('number') livePrivateFrom;
  @attr('number') livePrivateTo;

  /**
   * Vendita abbigliamento
   */
  @attr('number') clothingSalesFrom;
  @attr('number') clothingSalesTo;

  /**
   * Contenuti extra
   */
  @attr('number') extraContentFrom;
  @attr('number') extraContentTo;

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
  // HELPER METHODS
  // ============================================================

  /**
   * Formatta un range di prezzi
   * @param {number} from - Prezzo minimo
   * @param {number} to - Prezzo massimo
   * @returns {string|null} Range formattato o null
   */
  priceRange(from, to) {
    if (!from && !to) return null;
    if (from === to) return `€${from}`;
    if (!to) return `da €${from}`;
    if (!from) return `fino a €${to}`;
    return `€${from} - €${to}`;
  }

  // ============================================================
  // COMPUTED PROPERTIES
  // ============================================================

  /**
   * Range abbonamento mensile formattato
   */
  get monthlySubscriptionRange() {
    return this.priceRange(
      this.monthlySubscriptionFrom,
      this.monthlySubscriptionTo
    );
  }

  /**
   * Range vendita foto formattato
   */
  get photoSaleRange() {
    return this.priceRange(this.photoSaleFrom, this.photoSaleTo);
  }

  /**
   * Range vendita video formattato
   */
  get videoSaleRange() {
    return this.priceRange(this.videoSaleFrom, this.videoSaleTo);
  }

  /**
   * Range live public formattato
   */
  get livePublicRange() {
    return this.priceRange(this.livePublicFrom, this.livePublicTo);
  }

  /**
   * Range live private formattato
   */
  get livePrivateRange() {
    return this.priceRange(this.livePrivateFrom, this.livePrivateTo);
  }

  /**
   * Range vendita abbigliamento formattato
   */
  get clothingSalesRange() {
    return this.priceRange(this.clothingSalesFrom, this.clothingSalesTo);
  }

  /**
   * Range contenuti extra formattato
   */
  get extraContentRange() {
    return this.priceRange(this.extraContentFrom, this.extraContentTo);
  }

  /**
   * Check se ha almeno un prezzo impostato
   * @returns {boolean}
   */
  get hasPricing() {
    return !!(
      this.monthlySubscriptionFrom ||
      this.monthlySubscriptionTo ||
      this.photoSaleFrom ||
      this.photoSaleTo ||
      this.videoSaleFrom ||
      this.videoSaleTo ||
      this.livePublicFrom ||
      this.livePublicTo ||
      this.livePrivateFrom ||
      this.livePrivateTo ||
      this.clothingSalesFrom ||
      this.clothingSalesTo ||
      this.extraContentFrom ||
      this.extraContentTo
    );
  }
}
