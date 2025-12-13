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
   * Es nel caso di PERFORMER:
   *  - Foto & Video: "foto", "video", "video-custom", "live-public", "live-private"
   *  - Altro: "vendita abbigliamento", "contenuti extra"
   * Es nel caso di CAMGIRL:
   *  - Aspetto & Performance: face, body, dance, cosplay
   *  - Formato Show: public-show, private-show, tip-controlled, interactive-toy
   *  - Interazione & Esperienza: vge (Virtual girlfriend experience, cioé chiacchiere
   *    interazioni romantiche simulate), asmr (suoni, sussurri, esperienza sensoriale), custom-request
   *  - Numero Partecipanti: single, couple, lesbo, group
   *  - Atti Sessuali Base: masturbation, oral, 69, vaginal, anal-finger, anal-dildo-sex, squirt, facial, tit-cum
   *  - Atti Sessuali Intensi: deep-penetration, double-penetration, large-toys, fisting
   *  - Setting & Fetish: foot, oil-cream, shower/bath, outdoor
   *  - Dominazione Verbale & Taboo: dom/sub, dirty-talk, joi (Jerk Off Instructions cioè istruzioni per
   *    masturbarsi), sph (Small Penis Humiliation), cei, golden-shower-scat
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
