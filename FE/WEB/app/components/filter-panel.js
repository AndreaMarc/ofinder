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
  @tracked minAge = 18; // Età minima (default: nessun filtro)
  @tracked maxAge = 60; // Età massima (default: nessun filtro)
  @tracked selectedPlatforms = [];
  @tracked selectedContentTypes = [];
  @tracked selectedScheduleDay = null; // Giorno per filtro orari live (singolo)
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

  // Content types per CamGirl (con categorie e tooltip)
  // Tipi di Show Live (channelType = CamGirl)
  liveShowContentTypes = [
    // Aspetto
    { id: 'face', name: 'Face', category: 'Aspetto' },
    { id: 'body', name: 'Body', category: 'Aspetto' },

    // Formato dello Show
    { id: 'public-show', name: 'Public Show', category: 'Formato dello Show' },
    {
      id: 'private-show',
      name: 'Private Show',
      category: 'Formato dello Show',
    },
    {
      id: 'tip-controlled',
      name: 'Tip Controlled',
      tooltip: 'Show controllato dai tips degli spettatori',
      category: 'Formato dello Show',
    },
    {
      id: 'interactive-toy',
      name: 'Interactive Toy',
      tooltip: 'Toys interattivi controllati dagli spettatori',
      category: 'Formato dello Show',
    },

    // Numero Partecipanti
    { id: 'single', name: 'Single', category: 'Numero Partecipanti' },
    { id: 'couple', name: 'Couple', category: 'Numero Partecipanti' },
    { id: 'lesbo', name: 'Lesbo', category: 'Numero Partecipanti' },
    { id: 'group', name: 'Group', category: 'Numero Partecipanti' },

    // Atti Sessuali Base
    {
      id: 'masturbation',
      name: 'Masturbation',
      category: 'Atti Sessuali Base',
    },
    { id: 'oral', name: 'Oral', category: 'Atti Sessuali Base' },
    { id: '69', name: '69', category: 'Atti Sessuali Base' },
    { id: 'vaginal', name: 'Vaginal', category: 'Atti Sessuali Base' },
    { id: 'anal-finger', name: 'Anal Finger', category: 'Atti Sessuali Base' },
    {
      id: 'anal-dildo-sex',
      name: 'Anal Dildo/Sex',
      category: 'Atti Sessuali Base',
    },
    { id: 'squirt', name: 'Squirt', category: 'Atti Sessuali Base' },
    { id: 'facial', name: 'Facial', category: 'Atti Sessuali Base' },
    { id: 'tit-cum', name: 'Tit Cum', category: 'Atti Sessuali Base' },

    // Atti Sessuali Intensi
    {
      id: 'deep-penetration',
      name: 'Deep Penetration',
      category: 'Atti Sessuali Intensi',
    },
    {
      id: 'double-penetration',
      name: 'Double Penetration',
      category: 'Atti Sessuali Intensi',
    },
    { id: 'large-toys', name: 'Large Toys', category: 'Atti Sessuali Intensi' },
    { id: 'fisting', name: 'Fisting', category: 'Atti Sessuali Intensi' },

    // Ambientazioni e Fetish
    { id: 'foot', name: 'Foot', category: 'Ambientazioni e Fetish' },
    { id: 'oil-cream', name: 'Oil/Cream', category: 'Ambientazioni e Fetish' },
    {
      id: 'shower-bath',
      name: 'Shower/Bath',
      category: 'Ambientazioni e Fetish',
    },
    { id: 'outdoor', name: 'Outdoor', category: 'Ambientazioni e Fetish' },

    // Dominazione Verbale & Taboo
    {
      id: 'dom-sub',
      name: 'Dom/Sub',
      tooltip: 'Dominazione e sottomissione',
      category: 'Dominazione Verbale & Taboo',
    },
    {
      id: 'dirty-talk',
      name: 'Dirty Talk',
      category: 'Dominazione Verbale & Taboo',
    },
    {
      id: 'joi',
      name: 'JOI',
      tooltip: 'Jerk Off Instructions',
      category: 'Dominazione Verbale & Taboo',
    },
    {
      id: 'sph',
      name: 'SPH',
      tooltip: 'Small Penis Humiliation',
      category: 'Dominazione Verbale & Taboo',
    },
    {
      id: 'cei',
      name: 'CEI',
      tooltip: 'Cum Eating Instructions',
      category: 'Dominazione Verbale & Taboo',
    },
    {
      id: 'golden-shower-scat',
      name: 'Golden Shower/Scat',
      tooltip: 'Giochi con urina e feci',
      category: 'Dominazione Verbale & Taboo',
    },

    // Interazione & Esperienza (ultima sezione)
    {
      id: 'vge',
      name: 'VGE',
      tooltip:
        'Virtual Girlfriend Experience - chiacchiere e interazioni romantiche simulate',
      category: 'Interazione & Esperienza',
    },
    { id: 'roleplay', name: 'Roleplay', category: 'Interazione & Esperienza' },
  ];

  // Tipi di Contenuto Performer (channelType = Performer)
  performerContentTypes = [
    // Foto & Video
    { id: 'foto', name: 'Foto', category: 'Foto & Video' },
    { id: 'video', name: 'Video', category: 'Foto & Video' },
    { id: 'video-custom', name: 'Video Custom', category: 'Foto & Video' },
    { id: 'live-public', name: 'Live Public', category: 'Foto & Video' },
    { id: 'live-private', name: 'Live Private', category: 'Foto & Video' },

    // Altro
    {
      id: 'vendita-abbigliamento',
      name: 'Vendita Abbigliamento',
      category: 'Altro',
    },
    { id: 'contenuti-extra', name: 'Contenuti Extra', category: 'Altro' },
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
      id: 'night',
      label: 'Notte 0:00 ÷ 6:00',
      shortLabel: '0÷6',
      start: '00:00',
      end: '06:00',
    },
    {
      id: 'morning',
      label: 'Mattina 6:00 ÷ 12:00',
      shortLabel: '6÷12',
      start: '06:00',
      end: '12:00',
    },
    {
      id: 'afternoon',
      label: 'Pomeriggio 12:00 ÷ 18:00',
      shortLabel: '12÷18',
      start: '12:00',
      end: '18:00',
    },
    {
      id: 'evening',
      label: 'Sera 18:00 ÷ 0:00',
      shortLabel: '18÷24',
      start: '18:00',
      end: '00:00',
    },
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
        filter: `equals(geoCountryId,'${italyId}')`,
        sort: 'name',
      });

      this.regions = regions.slice();
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
        filter: `equals(geoFirstDivisionId,'${regionId}')`,
        sort: 'name',
      });

      this.provinces = provinces.slice();
    } catch (error) {
      console.error('Errore nel caricamento delle province:', error);
      this.provinces = [];
    }
  }

  /**
   * Opzioni regioni in formato JSON per SelectTwo
   */
  get regionsOptions() {
    const options = this.regions.map((region) => {
      let obj = {
        id: region.id,
        value: region.name,
      };

      //if (String(this.selectedRegionId) === String(region.id)) obj.selected = true;
      return obj;
    });
    return JSON.stringify(options);
  }

  /**
   * Opzioni province in formato JSON per SelectTwo
   * Include selectedRegionId come marker per forzare il re-render quando cambia la regione
   */
  get provincesOptions() {
    const options = this.provinces.map((province) => {
      let obj = {
        id: province.id,
        value: province.name,
      };
      // Non impostiamo selected - lasciamo gestire a Select2
      //if (String(this.selectedProvinceId) === String(province.id)) obj.selected = true;
      return obj;
    });

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

  /**
   * Nome della regione selezionata
   */
  get selectedRegionName() {
    if (!this.selectedRegionId) return null;
    const region = this.regions.find(
      (r) => String(r.id) === String(this.selectedRegionId)
    );
    return region ? region.name : null;
  }

  /**
   * Nome della provincia selezionata
   */
  get selectedProvinceName() {
    if (!this.selectedProvinceId) return null;
    const province = this.provinces.find(
      (p) => String(p.id) === String(this.selectedProvinceId)
    );
    return province ? province.name : null;
  }

  /**
   * Label da visualizzare per la posizione selezionata
   * Priorità: provincia > regione
   */
  get locationLabel() {
    if (this.selectedProvinceName) {
      return this.selectedProvinceName;
    }
    if (this.selectedRegionName) {
      return this.selectedRegionName;
    }
    return null;
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
        { id: 'other', name: 'Altro', emoji: '🌐', count: 890 },
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
  /**
   * Raggruppa i tipi di show live per categoria
   */
  get liveShowTypesByCategory() {
    const types = this.liveShowContentTypes;
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
   * Raggruppa i tipi di contenuto performer per categoria
   */
  get performerContentTypesByCategory() {
    const types = this.performerContentTypes;
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
   * Mostra filtro Tipi di Show Live
   * - Sempre per CamGirl
   * - Per Performer solo se è selezionato live-public o live-private
   */
  get showLiveShowTypesFilter() {
    if (this.searchType === 'CamGirl') {
      return true;
    }
    if (this.searchType === 'Performer') {
      return (
        this.selectedContentTypes.includes('live-public') ||
        this.selectedContentTypes.includes('live-private')
      );
    }
    return false;
  }

  /**
   * Mostra filtro Tipi di Contenuto (solo per Performer)
   */
  get showPerformerContentTypesFilter() {
    return this.searchType === 'Performer';
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
      this.minAge > 18 ||
      this.maxAge < 60 ||
      this.selectedPlatforms.length > 0 ||
      this.selectedContentTypes.length > 0 ||
      this.selectedScheduleDay !== null ||
      this.selectedTimeSlot !== null ||
      this.maxPrice < 50 ||
      this.selectedRating !== null ||
      this.onlyVerified ||
      this.onlyNew
    );
  }

  get activeFiltersCount() {
    let count = 0;
    if (this.searchType) count++;
    if (this.selectedRegionId) count++;
    if (this.selectedProvinceId) count++;
    if (this.minAge > 18 || this.maxAge < 60) count++;
    count += this.selectedPlatforms.length;
    count += this.selectedContentTypes.length;
    if (this.selectedScheduleDay) count++;
    if (this.selectedTimeSlot) count++;
    if (this.maxPrice < 50) count++;
    if (this.selectedRating) count++;
    if (this.onlyVerified) count++;
    if (this.onlyNew) count++;
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

  /**
   * Cancella la selezione della posizione
   * Se c'è una provincia selezionata, la rimuove e mantiene la regione
   * Se c'è solo la regione, rimuove anche quella
   */
  @action
  async clearLocation(event) {
    // Previeni la propagazione dell'evento
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }

    if (this.selectedProvinceId) {
      // Se c'è una provincia, rimuovi solo quella e ricarica le province
      const regionId = this.selectedRegionId;
      this.selectedProvinceId = null;
      if (regionId) {
        await this.loadProvinces(regionId);
      }
    } else if (this.selectedRegionId) {
      // Se c'è solo la regione, rimuovi regione, province e ricarica le regioni
      this.selectedRegionId = null;
      this.selectedProvinceId = null;
      this.provinces = [];
      // Ricarica le regioni per forzare il re-render delle select
      await this.loadRegions();
    }
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
  updateMinAge(event) {
    const newMin = parseInt(event.target.value);
    this.minAge = newMin;
    // Assicura che minAge non superi maxAge
    if (this.minAge > this.maxAge) {
      this.maxAge = this.minAge;
    }
  }

  @action
  updateMaxAge(event) {
    const newMax = parseInt(event.target.value);
    this.maxAge = newMax;
    // Assicura che maxAge non sia inferiore a minAge
    if (this.maxAge < this.minAge) {
      this.minAge = this.maxAge;
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
    this.selectedScheduleDay = null;
    this.selectedTimeSlot = null;
    // Notifica immediatamente il parent del cambio
    this.notifyFilterChange();
  }

  @action
  selectScheduleDay(dayId) {
    this.selectedScheduleDay = dayId;
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
  clearAllFilters(event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }

    this.searchType = null;
    this.selectedRegionId = null;
    this.selectedProvinceId = null;
    this.provinces = []; // Svuota lista province
    this.minAge = 18; // Reset età minima
    this.maxAge = 60; // Reset età massima
    this.selectedPlatforms = [];
    this.selectedContentTypes = [];
    this.selectedScheduleDay = null;
    this.selectedTimeSlot = null;
    this.maxPrice = 50; // Reset al valore massimo (nessun filtro)
    this.selectedRating = null;
    this.onlyVerified = false;
    this.onlyNew = false;

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
        minAge: this.minAge,
        maxAge: this.maxAge,
        platforms: this.selectedPlatforms,
        contentTypes: this.selectedContentTypes,
        scheduleDay: this.selectedScheduleDay,
        timeSlot: this.selectedTimeSlot,
        maxPrice: this.maxPrice,
        minRating: this.selectedRating,
        onlyVerified: this.onlyVerified,
        onlyNew: this.onlyNew,
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
    return (dayId) => this.selectedScheduleDay === dayId;
  }

  get isTimeSlotSelected() {
    return (slotId) => this.selectedTimeSlot === slotId;
  }

  get selectedTimeSlotLabel() {
    if (!this.selectedTimeSlot) {
      return null;
    }
    const slot = this.timeSlots.find((s) => s.id === this.selectedTimeSlot);
    return slot ? slot.label : null;
  }

  /**
   * Label riassuntiva per orari live (giorno + fascia oraria)
   * Esempio: "LUN. MATTINA"
   */
  get scheduleSummaryLabel() {
    if (this.selectedScheduleDay === null && !this.selectedTimeSlot) {
      return null;
    }

    let label = '';

    // Aggiungi il giorno se selezionato (usa !== null perché 0 è un valore valido per Domenica)
    if (this.selectedScheduleDay !== null) {
      const day = this.weekDays.find((d) => d.id === this.selectedScheduleDay);
      if (day) {
        label += day.shortName.toUpperCase() + '.';
      }
    }

    // Aggiungi la fascia oraria se selezionata
    if (this.selectedTimeSlot) {
      const slot = this.timeSlots.find((s) => s.id === this.selectedTimeSlot);
      if (slot) {
        // Estrai solo la prima parola (es: "Mattina" da "Mattina 6:00 ÷ 12:00")
        const timeLabel = slot.label.split(' ')[0].toUpperCase();
        label += (label ? ' ' : '') + timeLabel;
      }
    }

    return label || null;
  }

  @action
  clearSchedule(event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    this.selectedScheduleDay = null;
    this.selectedTimeSlot = null;
  }

  /**
   * Numero di piattaforme selezionate
   */
  get selectedPlatformsCount() {
    return this.selectedPlatforms.length;
  }

  @action
  clearPlatforms(event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    this.selectedPlatforms = [];
  }

  /**
   * Numero di tipi di contenuto selezionati (solo performerContentTypes)
   */
  get selectedPerformerContentTypesCount() {
    const performerIds = this.performerContentTypes.map((t) => t.id);
    return this.selectedContentTypes.filter((id) =>
      performerIds.includes(id)
    ).length;
  }

  /**
   * Numero di tipi di show live selezionati (solo liveShowContentTypes)
   */
  get selectedLiveShowTypesCount() {
    const liveShowIds = this.liveShowContentTypes.map((t) => t.id);
    return this.selectedContentTypes.filter((id) => liveShowIds.includes(id))
      .length;
  }

  @action
  clearPerformerContentTypes(event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    // Rimuovi solo gli ID dei performerContentTypes
    const performerIds = this.performerContentTypes.map((t) => t.id);
    this.selectedContentTypes = this.selectedContentTypes.filter(
      (id) => !performerIds.includes(id)
    );
  }

  @action
  clearLiveShowTypes(event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    // Rimuovi solo gli ID dei liveShowContentTypes
    const liveShowIds = this.liveShowContentTypes.map((t) => t.id);
    this.selectedContentTypes = this.selectedContentTypes.filter(
      (id) => !liveShowIds.includes(id)
    );
  }

  /**
   * Label per il voto minimo selezionato
   */
  get selectedRatingLabel() {
    if (this.selectedRating === null) {
      return null;
    }
    const rating = this.ratingFilters.find((r) => r.value === this.selectedRating);
    return rating ? rating.label : null;
  }

  @action
  clearRating(event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    this.selectedRating = null;
  }

  /**
   * Label per lo stato selezionato (Verificati, New)
   */
  get selectedStatusLabel() {
    const statuses = [];
    if (this.onlyVerified) {
      statuses.push('Verificati');
    }
    if (this.onlyNew) {
      statuses.push('New');
    }
    return statuses.length > 0 ? statuses.join(', ') : null;
  }

  @action
  clearStatus(event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    this.onlyVerified = false;
    this.onlyNew = false;
  }

  /**
   * Label del prezzo basata sul tipo di ricerca
   */
  get priceLabel() {
    if (this.searchType === 'Performer') {
      return 'Prezzo (€/mese)';
    } else if (this.searchType === 'CamGirl') {
      return 'Prezzo (€/minuto)';
    } else if (this.searchType === 'Escort') {
      return 'Prezzo (€)';
    }
    return 'Prezzo ($/mese)'; // Default
  }

  /**
   * Label per il tipo di ricerca selezionato
   */
  get selectedSearchTypeLabel() {
    if (!this.searchType) {
      return null;
    }
    const searchType = this.searchTypes.find((st) => st.id === this.searchType);
    if (!searchType) return null;

    // Rimuove tutto ciò che è tra parentesi (es: "(OF, Fansly, ecc)")
    const labelWithoutParentheses = searchType.label.split('(')[0].trim();
    return labelWithoutParentheses.toUpperCase();
  }

  @action
  clearSearchType(event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    this.searchType = null;
    // Reset filtri specifici quando cancelli il tipo di ricerca
    this.selectedPlatforms = [];
    this.selectedContentTypes = [];
    this.selectedScheduleDay = null;
    this.selectedTimeSlot = null;
    // Notifica immediatamente il parent del cambio
    this.notifyFilterChange();
  }
}
