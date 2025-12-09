import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

/**
 * Performer Model
 * Dati del performer (estende user-profile)
 *
 * NOTA: name, surname, email, birthDate, sex, description, residenceCity
 * sono su user e user-profile del framework, non duplicati qui.
 */
export default class PerformerModel extends Model {
  // ============================================================
  // RELATIONSHIPS
  // ============================================================

  /**
   * FK to user (framework)
   * Il performer è legato ad un user del framework
   */
  @belongsTo('user', { async: true, inverse: null }) user;

  /**
   * FK to geo-country (nazione: Italia, Francia, ecc.)
   */
  @belongsTo('geo-country', { async: true, inverse: null }) geoCountry;

  /**
   * FK to geo-first-division (regione: Lombardia, Lazio, ecc.)
   */
  @belongsTo('geo-first-division', { async: true, inverse: null })
  geoFirstDivision;

  /**
   * FK to geo-second-division (provincia: Milano, Roma, ecc.)
   */
  @belongsTo('geo-second-division', { async: true, inverse: null })
  geoSecondDivision;

  /**
   * FK to geo-city (città - preparato ma non usato per privacy)
   */
  @belongsTo('geo-city', { async: true, inverse: null }) geoCity;

  /**
   * Has many channels (OnlyFans, Fansly, Telegram, ecc.)
   */
  @hasMany('channel', { async: true, inverse: 'performer' }) channels;

  /**
   * Has many reviews
   */
  @hasMany('performer-review', { async: true, inverse: 'performer' }) reviews;

  /**
   * Has many services (model, escort, ecc.)
   */
  @hasMany('performer-service', { async: true, inverse: 'performer' }) services;

  /**
   * Has many views (tracking visualizzazioni profilo)
   */
  @hasMany('performer-view', { async: true, inverse: 'performer' }) views;

  // ============================================================
  // ATTRIBUTES
  // ============================================================

  /**
   * Timestamps
   */
  @attr('date-utc') createdAt;
  @attr('date-utc') updatedAt;

  /**
   * Computed attributes (calcolati dal backend e inviati nell'API)
   * Questi vengono calcolati con query aggregate sul backend
   */
  @attr('number') avgRating; // Media recensioni (da performer_review)
  @attr('number') totalReviews; // Numero totale recensioni
  @attr('number') totalViews; // Numero totale visualizzazioni profilo

  // ============================================================
  // COMPUTED PROPERTIES
  // ============================================================

  /**
   * Nome completo del performer
   * Accede a user.userProfile.firstName e lastName
   * @returns {string} "Nome Cognome"
   */
  get fullName() {
    const userProfile = this.user?.get('userProfile');
    if (!userProfile) return '';

    const firstName = userProfile.get('firstName') || '';
    const lastName = userProfile.get('lastName') || '';
    return `${firstName} ${lastName}`.trim();
  }

  /**
   * Nome del performer (firstName)
   * @returns {string}
   */
  get name() {
    return this.user?.get('userProfile')?.get('firstName') || '';
  }

  /**
   * Cognome del performer (lastName)
   * @returns {string}
   */
  get surname() {
    return this.user?.get('userProfile')?.get('lastName') || '';
  }

  /**
   * Email del performer
   * @returns {string}
   */
  get email() {
    return this.user?.get('email') || '';
  }

  /**
   * Data di nascita
   * @returns {Date|null}
   */
  get birthDate() {
    return this.user?.get('userProfile')?.get('birthDate') || null;
  }

  /**
   * Sesso (M/F)
   * @returns {string}
   */
  get sex() {
    return this.user?.get('userProfile')?.get('sex') || '';
  }

  /**
   * Descrizione/Bio del performer
   * @returns {string}
   */
  get description() {
    return this.user?.get('userProfile')?.get('description') || '';
  }

  /**
   * Città di residenza
   * @returns {string}
   */
  get residenceCity() {
    return this.user?.get('userProfile')?.get('residenceCity') || '';
  }

  /**
   * Calcola l'età dal birthDate
   * @returns {number|null} Età in anni
   */
  get age() {
    const birthDate = this.birthDate;
    if (!birthDate) return null;

    const today = new Date();
    const birth = new Date(birthDate);
    let age = today.getFullYear() - birth.getFullYear();
    const monthDiff = today.getMonth() - birth.getMonth();

    if (
      monthDiff < 0 ||
      (monthDiff === 0 && today.getDate() < birth.getDate())
    ) {
      age--;
    }

    return age;
  }

  /**
   * Location string completo (per visualizzazione)
   * Es: "Milano, Lombardia, Italia"
   * @returns {string}
   */
  get locationString() {
    const parts = [];

    // Provincia (geo-second-division)
    const provincia = this.geoSecondDivision?.get('name');
    if (provincia) parts.push(provincia);

    // Regione (geo-first-division)
    const regione = this.geoFirstDivision?.get('name');
    if (regione) parts.push(regione);

    // Nazione (geo-country)
    const nazione = this.geoCountry?.get('name');
    if (nazione) parts.push(nazione);

    return parts.join(', ');
  }

  /**
   * Rating con stelle (per UI)
   * @returns {number} Rating arrotondato
   */
  get ratingStars() {
    return Math.round(this.avgRating || 0);
  }

  /**
   * Check se performer ha recensioni
   * @returns {boolean}
   */
  get hasReviews() {
    return (this.totalReviews || 0) > 0;
  }

  /**
   * Check se performer è popolare (molte visualizzazioni)
   * @returns {boolean}
   */
  get isPopular() {
    return (this.totalViews || 0) > 1000;
  }

  /**
   * URL foto profilo (da user-profile.profileImageId)
   * @returns {string}
   */
  get profileImageUrl() {
    const imageId = this.user?.get('userProfile')?.get('profileImageId');
    if (!imageId) return '/assets/images/default-avatar.png'; // placeholder
    return `/api/media/${imageId}`; // esempio URL
  }
}
