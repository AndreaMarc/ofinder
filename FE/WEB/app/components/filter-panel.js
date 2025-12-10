import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

/**
 * FilterPanel Component
 * Pannello laterale con filtri avanzati
 *
 * @param {Object} filters - Oggetto filtri attivi
 * @param {Function} onApply - Callback applica filtri
 * @param {Function} onClear - Callback cancella filtri
 * @param {Boolean} isMobile - Layout mobile
 */
export default class FilterPanelComponent extends Component {
  @tracked selectedCountry = 'italia';
  @tracked selectedRegion = null;
  @tracked selectedProvince = null;
  @tracked selectedPlatforms = [];
  @tracked selectedContentTypes = [];
  @tracked maxPrice = 25;
  @tracked selectedRating = null;
  @tracked onlyVerified = false;
  @tracked onlyNew = false;
  @tracked onlyActiveToday = false;

  // Dati geolocalizzazione
  countries = [{ id: 'italia', name: 'Italia' }];

  regions = [
    { id: 'abruzzo', name: 'Abruzzo' },
    { id: 'basilicata', name: 'Basilicata' },
    { id: 'calabria', name: 'Calabria' },
    { id: 'campania', name: 'Campania' },
    { id: 'emilia-romagna', name: 'Emilia-Romagna' },
    { id: 'friuli-venezia-giulia', name: 'Friuli-Venezia Giulia' },
    { id: 'lazio', name: 'Lazio' },
    { id: 'liguria', name: 'Liguria' },
    { id: 'lombardia', name: 'Lombardia' },
    { id: 'marche', name: 'Marche' },
    { id: 'molise', name: 'Molise' },
    { id: 'piemonte', name: 'Piemonte' },
    { id: 'puglia', name: 'Puglia' },
    { id: 'sardegna', name: 'Sardegna' },
    { id: 'sicilia', name: 'Sicilia' },
    { id: 'toscana', name: 'Toscana' },
    { id: 'trentino-alto-adige', name: 'Trentino-Alto Adige' },
    { id: 'umbria', name: 'Umbria' },
    { id: 'valle-daosta', name: "Valle d'Aosta" },
    { id: 'veneto', name: 'Veneto' },
  ];

  // Mappa province per regione (alcune principali per esempio)
  provincesByRegion = {
    lombardia: [
      { id: 'MI', name: 'Milano' },
      { id: 'BG', name: 'Bergamo' },
      { id: 'BS', name: 'Brescia' },
      { id: 'CO', name: 'Como' },
      { id: 'CR', name: 'Cremona' },
      { id: 'LC', name: 'Lecco' },
      { id: 'LO', name: 'Lodi' },
      { id: 'MN', name: 'Mantova' },
      { id: 'MB', name: 'Monza e Brianza' },
      { id: 'PV', name: 'Pavia' },
      { id: 'SO', name: 'Sondrio' },
      { id: 'VA', name: 'Varese' },
    ],
    lazio: [
      { id: 'RM', name: 'Roma' },
      { id: 'FR', name: 'Frosinone' },
      { id: 'LT', name: 'Latina' },
      { id: 'RI', name: 'Rieti' },
      { id: 'VT', name: 'Viterbo' },
    ],
    campania: [
      { id: 'NA', name: 'Napoli' },
      { id: 'AV', name: 'Avellino' },
      { id: 'BN', name: 'Benevento' },
      { id: 'CE', name: 'Caserta' },
      { id: 'SA', name: 'Salerno' },
    ],
    // Aggiungi altre regioni secondo necessità
  };

  // Use passed platformFilters from args, or fallback to defaults
  get platformFilters() {
    return (
      this.args.platformFilters || [
        { id: 'onlyfans', name: 'OnlyFans', emoji: '🔵', count: 1234 },
        { id: 'fansly', name: 'Fansly', emoji: '🟣', count: 567 },
        { id: 'instagram', name: 'Instagram', emoji: '📸', count: 890 },
        { id: 'tiktok', name: 'TikTok', emoji: '🎵', count: 456 },
      ]
    );
  }

  // Use passed contentTypeFilters from args, or fallback to defaults
  get contentTypeFilters() {
    return (
      this.args.contentTypeFilters || [
        { id: 'foto', name: 'Foto', count: 234 },
        { id: 'video', name: 'Video', count: 189 },
        { id: 'live', name: 'Live', count: 156 },
        { id: 'abbigliamento', name: 'Abbigliamento', count: 45 },
        { id: 'contenuti-extra', name: 'Contenuti Extra', count: 78 },
      ]
    );
  }

  ratingFilters = [
    { value: 5, label: '5 stelle' },
    { value: 4, label: '4+ stelle' },
    { value: 3, label: '3+ stelle' },
  ];

  get availableProvinces() {
    if (!this.selectedRegion) {
      return [];
    }
    return this.provincesByRegion[this.selectedRegion] || [];
  }

  get hasActiveFilters() {
    return (
      this.selectedRegion !== null ||
      this.selectedProvince !== null ||
      this.selectedPlatforms.length > 0 ||
      this.selectedContentTypes.length > 0 ||
      this.maxPrice < 50 ||
      this.selectedRating !== null ||
      this.onlyVerified ||
      this.onlyNew ||
      this.onlyActiveToday
    );
  }

  get activeFiltersCount() {
    let count = 0;
    if (this.selectedRegion) count++;
    if (this.selectedProvince) count++;
    count += this.selectedPlatforms.length;
    count += this.selectedContentTypes.length;
    if (this.maxPrice < 50) count++;
    if (this.selectedRating) count++;
    if (this.onlyVerified) count++;
    if (this.onlyNew) count++;
    if (this.onlyActiveToday) count++;
    return count;
  }

  @action
  selectCountry(event) {
    this.selectedCountry = event.target.value;
  }

  @action
  selectRegion(event) {
    const value = event.target.value;
    this.selectedRegion = value === '' ? null : value;
    // Reset provincia quando cambi regione
    this.selectedProvince = null;
  }

  @action
  selectProvince(event) {
    const value = event.target.value;
    this.selectedProvince = value === '' ? null : value;
  }

  @action
  togglePlatform(platform) {
    if (this.selectedPlatforms.includes(platform.id)) {
      this.selectedPlatforms = this.selectedPlatforms.filter(
        (id) => id !== platform.id
      );
    } else {
      this.selectedPlatforms = [...this.selectedPlatforms, platform.id];
    }
  }

  @action
  toggleContentType(contentType) {
    if (this.selectedContentTypes.includes(contentType.id)) {
      this.selectedContentTypes = this.selectedContentTypes.filter(
        (id) => id !== contentType.id
      );
    } else {
      this.selectedContentTypes = [...this.selectedContentTypes, contentType.id];
    }
  }

  @action
  updatePriceFilter(event) {
    this.maxPrice = parseInt(event.target.value);
  }

  @action
  selectRating(value) {
    this.selectedRating = value;
  }

  @action
  toggleVerified() {
    this.onlyVerified = !this.onlyVerified;
  }

  @action
  toggleNew() {
    this.onlyNew = !this.onlyNew;
  }

  @action
  toggleActiveToday() {
    this.onlyActiveToday = !this.onlyActiveToday;
  }

  @action
  applyFilters() {
    // Notify parent with current filter state
    this.notifyFilterChange();
  }

  @action
  clearAllFilters() {
    this.selectedRegion = null;
    this.selectedProvince = null;
    this.selectedPlatforms = [];
    this.selectedContentTypes = [];
    this.maxPrice = 25;
    this.selectedRating = null;
    this.onlyVerified = false;
    this.onlyNew = false;
    this.onlyActiveToday = false;

    // Notify parent
    this.notifyFilterChange();
  }

  /**
   * Notify parent component of filter changes
   */
  notifyFilterChange() {
    if (this.args.onFilterChange) {
      this.args.onFilterChange({
        country: this.selectedCountry,
        region: this.selectedRegion,
        province: this.selectedProvince,
        platforms: this.selectedPlatforms,
        contentTypes: this.selectedContentTypes,
        maxPrice: this.maxPrice,
        minRating: this.selectedRating,
        onlyVerified: this.onlyVerified,
        onlyNew: this.onlyNew,
        onlyActiveToday: this.onlyActiveToday,
      });
    }
  }

  get isPlatformSelected() {
    return (platformId) => this.selectedPlatforms.includes(platformId);
  }

  get isContentTypeSelected() {
    return (contentTypeId) => this.selectedContentTypes.includes(contentTypeId);
  }
}
