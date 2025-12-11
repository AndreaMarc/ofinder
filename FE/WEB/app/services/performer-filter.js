import Service from '@ember/service';

/**
 * PerformerFilterService
 * Servizio centralizzato per la logica di filtraggio dei performers
 * Riutilizzabile in qualsiasi parte dell'applicazione
 */
export default class PerformerFilterService extends Service {
  /**
   * Filtra una lista di performers in base ai filtri specificati
   *
   * @param {Array} performers - Array di performers da filtrare
   * @param {Object} filters - Oggetto contenente i filtri da applicare
   * @param {string} searchQuery - Testo di ricerca (opzionale)
   * @returns {Array} Array di performers filtrati
   */
  filterPerformers(performers, filters = {}, searchQuery = '') {
    if (!performers || performers.length === 0) {
      return [];
    }

    let filtered = [...performers];

    // Apply search query filter
    if (searchQuery && searchQuery.length > 0) {
      filtered = this._filterBySearchQuery(filtered, searchQuery);
    }

    // Apply verified filter
    if (filters.verified) {
      filtered = this._filterByVerified(filtered);
    }

    // Apply premium filter
    if (filters.premium) {
      filtered = this._filterByPremium(filtered);
    }

    // Apply top-rated filter
    if (filters.topRated) {
      filtered = this._filterByTopRated(filtered);
    }

    // Apply search type filter (channelType: CamGirl, Performer, Escort)
    if (filters.searchType) {
      filtered = this._filterBySearchType(filtered, filters.searchType);
    }

    // Apply platform filters (only for Performer)
    if (filters.platforms && filters.platforms.length > 0) {
      filtered = this._filterByPlatforms(filtered, filters.platforms);
    }

    // Apply content type filters (for CamGirl and Performer)
    if (filters.contentTypes && filters.contentTypes.length > 0) {
      filtered = this._filterByContentTypes(filtered, filters.contentTypes);
    }

    // Apply schedule day filters (for CamGirl and Performer)
    if (filters.scheduleDays && filters.scheduleDays.length > 0) {
      filtered = this._filterByScheduleDays(filtered, filters.scheduleDays);
    }

    // Apply time slot filter
    if (filters.timeSlot) {
      filtered = this._filterByTimeSlot(filtered, filters.timeSlot);
    }

    // Apply rating filter
    if (filters.minRating && filters.minRating > 0) {
      filtered = this._filterByMinRating(filtered, filters.minRating);
    }

    // Apply price filter
    if (filters.maxPrice !== undefined && filters.maxPrice < 50) {
      filtered = this._filterByMaxPrice(filtered, filters.maxPrice);
    }

    // Apply "new" filter (last 30 days)
    if (filters.new) {
      filtered = this._filterByNew(filtered);
    }

    // Apply "active today" filter
    if (filters.activeToday) {
      filtered = this._filterByActiveToday(filtered);
    }

    return filtered;
  }

  /**
   * Conta i performers filtrati (senza paginazione)
   */
  countFilteredPerformers(performers, filters = {}, searchQuery = '') {
    return this.filterPerformers(performers, filters, searchQuery).length;
  }

  // ============================================================
  // PRIVATE FILTER METHODS
  // ============================================================

  _filterBySearchQuery(performers, query) {
    const lowerQuery = query.toLowerCase();
    return performers.filter((p) => {
      return (
        p.displayName?.toLowerCase().includes(lowerQuery) ||
        p.bio?.toLowerCase().includes(lowerQuery)
      );
    });
  }

  _filterByVerified(performers) {
    return performers.filter((p) => p.verified === true);
  }

  _filterByPremium(performers) {
    return performers.filter((p) => p.isPremium === true);
  }

  _filterByTopRated(performers) {
    return performers.filter((p) => p.averageRating >= 4.5);
  }

  _filterBySearchType(performers, searchType) {
    return performers.filter((p) => {
      return p.channels?.some((ch) => ch.channelType === searchType);
    });
  }

  _filterByPlatforms(performers, platforms) {
    return performers.filter((p) => {
      return p.channels?.some((ch) => platforms.includes(ch.platform));
    });
  }

  _filterByContentTypes(performers, contentTypes) {
    return performers.filter((p) => {
      return p.channels?.some((ch) =>
        ch.contentTypes?.some((ct) => contentTypes.includes(ct.contentType))
      );
    });
  }

  _filterByScheduleDays(performers, scheduleDays) {
    return performers.filter((p) => {
      return p.channels?.some((ch) =>
        ch.schedules?.some((schedule) =>
          scheduleDays.includes(schedule.dayOfWeek)
        )
      );
    });
  }

  _filterByTimeSlot(performers, timeSlot) {
    // TODO: Implementare logica per filtrare per fascia oraria
    // Richiede parsing degli orari startTime/endTime degli schedule
    // e verifica se sono inclusi nella fascia oraria selezionata
    return performers;
  }

  _filterByMinRating(performers, minRating) {
    return performers.filter((p) => p.averageRating >= minRating);
  }

  _filterByMaxPrice(performers, maxPrice) {
    return performers.filter((p) => p.minPrice <= maxPrice);
  }

  _filterByNew(performers) {
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);

    return performers.filter((p) => {
      if (!p.createdAt) return false;
      const createdDate = new Date(p.createdAt);
      return createdDate >= thirtyDaysAgo;
    });
  }

  _filterByActiveToday(performers) {
    // TODO: Implementare logica per "attivi oggi"
    // Potrebbe basarsi su lastActivityDate o altri metadati
    return performers;
  }

  /**
   * Applica paginazione ai performers filtrati
   */
  paginatePerformers(performers, page = 1, itemsPerPage = 6) {
    const startIndex = (page - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    return performers.slice(startIndex, endIndex);
  }

  /**
   * Calcola il numero totale di pagine
   */
  calculateTotalPages(totalCount, itemsPerPage = 6) {
    return Math.ceil(totalCount / itemsPerPage);
  }
}
