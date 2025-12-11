import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { inject as service } from '@ember/service';

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
  @service store;

  @tracked searchType = null; // 'CamGirl', 'Performer', 'Escort'
  @tracked selectedCountryId = null; // ID del paese selezionato
  @tracked selectedRegionId = null; // ID della regione selezionata
  @tracked selectedProvinceId = null; // ID della provincia selezionata
  @tracked regions = []; // Lista regioni caricate dal DB
  @tracked provinces = []; // Liste province caricate dal DB
  @tracked selectedPlatforms = [];
  @tracked selectedContentTypes = [];
  @tracked selectedScheduleDays = []; // Giorni per filtro orari live
  @tracked selectedTimeSlot = null; // Fascia oraria
  @tracked maxPrice = 50; // Valore massimo di default (nessun filtro attivo)
  @tracked selectedRating = null;
  @tracked onlyVerified = false;
  @tracked onlyNew = false;
  @tracked onlyActiveToday = false;

  // Opzioni "Cosa cerchi?"
  searchTypes = [
    {
      id: 'CamGirl',
      label: 'Cam-girl',
      description: 'Live streaming su piattaforme cam',
    },
    {
      id: 'Performer',
      label: 'Performer (OF, Fansly, ecc)',
      description: 'Content creator su OnlyFans, Fansly, ecc.',
    },
    { id: 'Escort', label: 'Escort', description: 'Servizi escort' },
  ];

  // Content types per Performer
  performerContentTypes = [
    { id: 'foto-erotiche', name: 'Foto erotiche', count: 234 },
    { id: 'video-gallery', name: 'Video gallery', count: 189 },
    { id: 'live-public', name: 'Live public', count: 156 },
    { id: 'live-private', name: 'Live private', count: 98 },
    { id: 'vendita-abbigliamento', name: 'Vendita abbigliamento', count: 45 },
    { id: 'contenuti-extra', name: 'Contenuti extra', count: 78 },
  ];

  // Content types per CamGirl (con categorie)
  camGirlContentTypes = [
    // Aspetto & Performance
    { id: 'face', name: 'Face', count: 145, category: 'Aspetto & Performance' },
    { id: 'body', name: 'Body', count: 230, category: 'Aspetto & Performance' },
    { id: 'dance', name: 'Dance', count: 156, category: 'Aspetto & Performance' },
    { id: 'cosplay', name: 'Cosplay', count: 45, category: 'Aspetto & Performance' },

    // Formato Show
    { id: 'public-show', name: 'Public Show', count: 320, category: 'Formato Show' },
    { id: 'private-show', name: 'Private Show', count: 285, category: 'Formato Show' },
    { id: 'tip-controlled', name: 'Tip Controlled', count: 178, category: 'Formato Show' },
    { id: 'interactive-toy', name: 'Interactive Toy', count: 195, category: 'Formato Show' },

    // Interazione & Esperienza
    { id: 'vge', name: 'VGE (Virtual Girlfriend)', count: 87, category: 'Interazione & Esperienza' },
    { id: 'asmr', name: 'ASMR', count: 64, category: 'Interazione & Esperienza' },
    { id: 'custom-request', name: 'Custom Request', count: 142, category: 'Interazione & Esperienza' },
    { id: 'roleplay', name: 'Roleplay', count: 134, category: 'Interazione & Esperienza' },

    // Numero Partecipanti
    { id: 'single', name: 'Single', count: 412, category: 'Numero Partecipanti' },
    { id: 'couple', name: 'Couple', count: 78, category: 'Numero Partecipanti' },
    { id: 'lesbo', name: 'Lesbo', count: 93, category: 'Numero Partecipanti' },
    { id: 'group', name: 'Group', count: 41, category: 'Numero Partecipanti' },

    // Atti Sessuali Base
    { id: 'oral', name: 'Oral', count: 267, category: 'Atti Sessuali Base' },
    { id: 'anal', name: 'Anal', count: 89, category: 'Atti Sessuali Base' },
    { id: 'squirt', name: 'Squirt', count: 92, category: 'Atti Sessuali Base' },

    // Atti Sessuali Intensi
    { id: 'deep-penetration', name: 'Deep Penetration', count: 123, category: 'Atti Sessuali Intensi' },
    { id: 'double-penetration', name: 'Double Penetration', count: 67, category: 'Atti Sessuali Intensi' },
    { id: 'large-toys', name: 'Large Toys', count: 98, category: 'Atti Sessuali Intensi' },
    { id: 'fisting', name: 'Fisting', count: 34, category: 'Atti Sessuali Intensi' },

    // Setting & Fetish
    { id: 'foot', name: 'Foot', count: 103, category: 'Setting & Fetish' },
    { id: 'oil-cream', name: 'Oil/Cream', count: 88, category: 'Setting & Fetish' },
    { id: 'shower-bath', name: 'Shower/Bath', count: 112, category: 'Setting & Fetish' },
    { id: 'outdoor', name: 'Outdoor', count: 56, category: 'Setting & Fetish' },

    // Dominazione Verbale & Taboo
    { id: 'dom-sub', name: 'Dom/Sub', count: 87, category: 'Dominazione Verbale & Taboo' },
    { id: 'dirty-talk', name: 'Dirty Talk', count: 201, category: 'Dominazione Verbale & Taboo' },
    { id: 'joi', name: 'JOI (Jerk Off Instructions)', count: 72, category: 'Dominazione Verbale & Taboo' },
    { id: 'sph', name: 'SPH (Small Penis Humiliation)', count: 34, category: 'Dominazione Verbale & Taboo' },
    { id: 'cei', name: 'CEI (Cum Eating Instructions)', count: 28, category: 'Dominazione Verbale & Taboo' },
    { id: 'golden-shower-scat', name: 'Golden Shower/Scat', count: 19, category: 'Dominazione Verbale & Taboo' },
  ];

  // Giorni della settimana per filtro orari
  weekDays = [
    { id: 0, name: 'Domenica', shortName: 'Dom' },
    { id: 1, name: 'Lunedì', shortName: 'Lun' },
    { id: 2, name: 'Martedì', shortName: 'Mar' },
    { id: 3, name: 'Mercoledì', shortName: 'Mer' },
    { id: 4, name: 'Giovedì', shortName: 'Gio' },
    { id: 5, name: 'Venerdì', shortName: 'Ven' },
    { id: 6, name: 'Sabato', shortName: 'Sab' },
  ];

  // Fasce orarie
  timeSlots = [
    {
      id: 'morning',
      label: 'Mattina (06:00-12:00)',
      start: '06:00',
      end: '12:00',
    },
    {
      id: 'afternoon',
      label: 'Pomeriggio (12:00-18:00)',
      start: '12:00',
      end: '18:00',
    },
    {
      id: 'evening',
      label: 'Sera (18:00-00:00)',
      start: '18:00',
      end: '00:00',
    },
    { id: 'night', label: 'Notte (00:00-06:00)', start: '00:00', end: '06:00' },
  ];

  /**
   * Constructor - carica le regioni dell'Italia all'avvio
   */
  constructor() {
    super(...arguments);
    this.loadRegions();
  }

  /**
   * Carica le regioni (GeoFirstDivision) per l'Italia
   */
  async loadRegions() {
    try {
      // Per ora hardcodiamo l'ID dell'Italia - TODO: prendere dall'utente o da configurazione
      // Assumiamo che l'ID dell'Italia sia 107 (standard ISO)
      const italyId = 107;
      this.selectedCountryId = italyId;

      const regions = await this.store.query('geo-first-division', {
        filter: { geoCountryId: italyId },
        sort: 'name',
      });

      this.regions = regions.toArray();
    } catch (error) {
      console.error('Errore nel caricamento delle regioni:', error);
      this.regions = [];
    }
  }

  /**
   * Carica le province (GeoSecondDivision) per una regione specifica
   */
  async loadProvinces(regionId) {
    try {
      const provinces = await this.store.query('geo-second-division', {
        filter: { geoFirstDivisionId: regionId },
        sort: 'name',
      });

      this.provinces = provinces.toArray();
    } catch (error) {
      console.error('Errore nel caricamento delle province:', error);
      this.provinces = [];
    }
  }

  /**
   * Opzioni regioni in formato JSON per SelectTwo
   */
  get regionsOptions() {
    const options = this.regions.map((region) => ({
      id: region.id,
      value: region.name,
      selected: this.selectedRegionId === region.id,
    }));
    return JSON.stringify(options);
  }

  /**
   * Opzioni province in formato JSON per SelectTwo
   */
  get provincesOptions() {
    const options = this.provinces.map((province) => ({
      id: province.id,
      value: province.name,
      selected: this.selectedProvinceId === province.id,
    }));
    return JSON.stringify(options);
  }

  /**
   * Opzioni Select2 per la select province (disabilita se nessuna regione selezionata)
   */
  get provincesSelect2Options() {
    return JSON.stringify({
      disabled: !this.selectedRegionId,
    });
  }

  get availableProvinces() {
    return this.provinces;
  }

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

  /**
   * Content types basati sul tipo di ricerca
   */
  get contentTypeFilters() {
    if (this.searchType === 'CamGirl') {
      return this.camGirlContentTypes;
    } else if (this.searchType === 'Performer') {
      return this.performerContentTypes;
    }
    // Per Escort non mostriamo content types
    return [];
  }

  /**
   * Content types raggruppati per categoria
   */
  get contentTypesByCategory() {
    const types = this.contentTypeFilters;
    const grouped = {};

    types.forEach((type) => {
      const category = type.category || 'Altri';
      if (!grouped[category]) {
        grouped[category] = [];
      }
      grouped[category].push(type);
    });

    // Converti in array di oggetti per facilitare l'iterazione nel template
    return Object.keys(grouped).map((categoryName) => ({
      name: categoryName,
      items: grouped[categoryName],
    }));
  }

  /**
   * Mostra filtro Piattaforme solo per Performer
   */
  get showPlatformsFilter() {
    return this.searchType === 'Performer';
  }

  /**
   * Mostra filtro Tipi di Contenuto per CamGirl e Performer
   */
  get showContentTypesFilter() {
    return this.searchType === 'CamGirl' || this.searchType === 'Performer';
  }

  /**
   * Mostra filtro Orari per CamGirl e Performer
   * In realtà dovrebbe controllare anche se hanno live, ma per ora lo mostriamo sempre
   */
  get showScheduleFilter() {
    return this.searchType === 'CamGirl' || this.searchType === 'Performer';
  }

  /**
   * Mostra filtro Prezzo (con riserve per Performer e CamGirl)
   */
  get showPriceFilter() {
    // Mostra sempre, ma l'utente ha riserve per Performer e CamGirl
    return true;
  }

  get hasActiveFilters() {
    return (
      this.searchType !== null ||
      this.selectedRegionId !== null ||
      this.selectedProvinceId !== null ||
      this.selectedPlatforms.length > 0 ||
      this.selectedContentTypes.length > 0 ||
      this.selectedScheduleDays.length > 0 ||
      this.selectedTimeSlot !== null ||
      this.maxPrice < 50 ||
      this.selectedRating !== null ||
      this.onlyVerified ||
      this.onlyNew ||
      this.onlyActiveToday
    );
  }

  get activeFiltersCount() {
    let count = 0;
    if (this.searchType) count++;
    if (this.selectedRegionId) count++;
    if (this.selectedProvinceId) count++;
    count += this.selectedPlatforms.length;
    count += this.selectedContentTypes.length;
    count += this.selectedScheduleDays.length;
    if (this.selectedTimeSlot) count++;
    if (this.maxPrice < 50) count++;
    if (this.selectedRating) count++;
    if (this.onlyVerified) count++;
    if (this.onlyNew) count++;
    if (this.onlyActiveToday) count++;
    return count;
  }

  /**
   * Callback quando cambia la selezione della regione
   * @param {string} regionId - ID della regione selezionata
   */
  @action
  async onRegionChange(regionId) {
    this.selectedRegionId = regionId || null;
    this.selectedProvinceId = null; // Reset provincia
    this.provinces = []; // Svuota lista province

    if (regionId) {
      await this.loadProvinces(regionId);
    }
  }

  /**
   * Callback quando cambia la selezione della provincia
   * @param {string} provinceId - ID della provincia selezionata
   */
  @action
  onProvinceChange(provinceId) {
    this.selectedProvinceId = provinceId || null;
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
      this.selectedContentTypes = [
        ...this.selectedContentTypes,
        contentType.id,
      ];
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
  selectSearchType(searchTypeId) {
    this.searchType = searchTypeId;
    // Reset filtri specifici quando cambi tipo di ricerca
    this.selectedPlatforms = [];
    this.selectedContentTypes = [];
    this.selectedScheduleDays = [];
    this.selectedTimeSlot = null;
  }

  @action
  toggleScheduleDay(dayId) {
    if (this.selectedScheduleDays.includes(dayId)) {
      this.selectedScheduleDays = this.selectedScheduleDays.filter(
        (id) => id !== dayId
      );
    } else {
      this.selectedScheduleDays = [...this.selectedScheduleDays, dayId];
    }
  }

  @action
  selectTimeSlot(slotId) {
    this.selectedTimeSlot = slotId;
  }

  @action
  applyFilters() {
    // Notify parent with current filter state
    this.notifyFilterChange();
  }

  @action
  clearAllFilters() {
    this.searchType = null;
    this.selectedRegionId = null;
    this.selectedProvinceId = null;
    this.provinces = []; // Svuota lista province
    this.selectedPlatforms = [];
    this.selectedContentTypes = [];
    this.selectedScheduleDays = [];
    this.selectedTimeSlot = null;
    this.maxPrice = 50; // Reset al valore massimo (nessun filtro)
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
        searchType: this.searchType,
        countryId: this.selectedCountryId,
        regionId: this.selectedRegionId,
        provinceId: this.selectedProvinceId,
        platforms: this.selectedPlatforms,
        contentTypes: this.selectedContentTypes,
        scheduleDays: this.selectedScheduleDays,
        timeSlot: this.selectedTimeSlot,
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

  get isScheduleDaySelected() {
    return (dayId) => this.selectedScheduleDays.includes(dayId);
  }

  get isTimeSlotSelected() {
    return (slotId) => this.selectedTimeSlot === slotId;
  }
}
