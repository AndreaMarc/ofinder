import Model, { attr, belongsTo } from '@ember-data/model';

/**
 * Channel Content Type Model
 * Tipi di contenuti pubblicati su quel canale
 */
export default class ChannelContentTypeModel extends Model {
  // ============================================================
  // RELATIONSHIPS
  // ============================================================

  /**
   * FK to channel
   */
  @belongsTo('channel', { async: true, inverse: 'contentTypes' }) channel;

  // ============================================================
  // ATTRIBUTES
  // ============================================================

  /**
   * Tipo di contenuto
   * Es: "foto erotiche", "video gallery", "live public", "live private",
   * "vendita abbigliamento", "contenuti extra", ecc.
   */
  @attr('string') contentType;

  /**
   * Descrizione del tipo di contenuto
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
   * Label per visualizzazione UI
   * @returns {string}
   */
  get label() {
    return this.contentType || '';
  }

  /**
   * Icon class basato sul tipo di contenuto
   * @returns {string}
   */
  get iconClass() {
    const contentType = (this.contentType || '').toLowerCase();

    if (contentType.includes('foto')) return 'fa fa-camera';
    if (contentType.includes('video')) return 'fa fa-video';
    if (contentType.includes('live')) return 'fa fa-broadcast-tower';
    if (contentType.includes('abbigliamento')) return 'fa fa-tshirt';

    return 'fa fa-file';
  }
}
