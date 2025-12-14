import Controller from '@ember/controller';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

/**
 * Search Controller
 * Manages search page state and interactions
 */
export default class SearchController extends Controller {
  @service router;
  @service performerFilter;

  @tracked searchQuery = '';
  @tracked activeFilters = {
    searchType: null, // 'CamGirl', 'Performer', 'Escort' - null until user selects
    platforms: [],
    contentTypes: [],
    scheduleDays: [], // Giorni della settimana
    timeSlot: null, // Fascia oraria
    verified: false,
    premium: false,
    new: false,
    topRated: false,
    minRating: 0,
    maxPrice: 50,
  };
  @tracked viewMode = 'grid'; // 'grid' or 'list'
  @tracked currentPage = 1;
  @tracked itemsPerPage = 6;

  /**
   * Filtered performers based on search query and active filters
   * Delegates filtering logic to performerFilter service
   */
  get filteredPerformers() {
    const performers = this.model.performers || [];

    // Use service to filter performers
    const filtered = this.performerFilter.filterPerformers(
      performers,
      this.activeFilters,
      this.searchQuery
    );

    // Apply pagination
    return this.performerFilter.paginatePerformers(
      filtered,
      this.currentPage,
      this.itemsPerPage
    );
  }

  /**
   * Total count of filtered performers (before pagination)
   * Delegates counting to performerFilter service
   */
  get filteredPerformersCount() {
    const performers = this.model.performers || [];
    return this.performerFilter.countFilteredPerformers(
      performers,
      this.activeFilters,
      this.searchQuery
    );
  }

  /**
   * Check if there are any results
   */
  get hasResults() {
    return this.filteredPerformersCount > 0;
  }

  /**
   * Calculate total pages
   * Delegates calculation to performerFilter service
   */
  get totalPages() {
    return this.performerFilter.calculateTotalPages(
      this.filteredPerformersCount,
      this.itemsPerPage
    );
  }

  /**
   * Check if on first page
   */
  get isFirstPage() {
    return this.currentPage === 1;
  }

  /**
   * Check if on last page
   */
  get isLastPage() {
    return this.currentPage >= this.totalPages;
  }

  /**
   * Generate page numbers array for pagination
   */
  get pageNumbers() {
    const pages = [];
    for (let i = 1; i <= this.totalPages; i++) {
      pages.push(i);
    }
    return pages;
  }

  /**
   * Handle search input
   */
  @action
  handleSearch(query) {
    this.searchQuery = query;
    this.currentPage = 1; // Reset to first page on new search
  }

  /**
   * Handle quick filter toggle
   */
  @action
  handleQuickFilterToggle(filter) {
    this.activeFilters[filter.id] = !this.activeFilters[filter.id];
    this.currentPage = 1; // Reset to first page on filter change
  }

  /**
   * Handle advanced filter changes from FilterPanel
   * Receives complete filter object from FilterPanel component
   */
  @action
  handleFilterChange(filters) {
    // Update activeFilters with all values from FilterPanel
    this.activeFilters = {
      searchType: filters.searchType,
      platforms: filters.platforms || [],
      contentTypes: filters.contentTypes || [],
      scheduleDays: filters.scheduleDays || [],
      timeSlot: filters.timeSlot || null,
      verified: filters.onlyVerified || false,
      premium: this.activeFilters.premium, // Keep quick filter state
      new: filters.onlyNew || false,
      topRated: this.activeFilters.topRated, // Keep quick filter state
      minRating: filters.minRating || 0,
      maxPrice: filters.maxPrice || 50,
    };
    this.currentPage = 1; // Reset to first page on filter change
  }

  /**
   * Set view mode (grid or list)
   */
  @action
  setViewMode(mode) {
    this.viewMode = mode;
  }

  /**
   * Navigate to specific page
   */
  @action
  goToPage(pageNum) {
    if (pageNum >= 1 && pageNum <= this.totalPages) {
      this.currentPage = pageNum;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  /**
   * Navigate to previous page
   */
  @action
  goToPreviousPage(event) {
    event.preventDefault();
    if (!this.isFirstPage) {
      this.currentPage -= 1;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  /**
   * Navigate to next page
   */
  @action
  goToNextPage(event) {
    event.preventDefault();
    if (!this.isLastPage) {
      this.currentPage += 1;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  /**
   * Handle performer profile view
   * TODO: Create performer detail page
   */
  @action
  handleViewProfile(performer) {
    console.log('View profile for performer:', performer.id);
    // TODO: Navigate to performer detail page
    // this.router.transitionTo('performer-detail', performer.id);
    alert(
      `Profilo di ${performer.displayName} - TODO: Implementare pagina dettaglio`
    );
  }

  /**
   * Navigate to login page
   */
  @action
  goToLogin() {
    this.router.transitionTo('login');
  }

  /**
   * Navigate to registration page
   */
  @action
  goToRegistration() {
    this.router.transitionTo('registration');
  }

  /**
   * Navigate to user profile (for logged users)
   */
  @action
  goToProfile() {
    this.router.transitionTo('user-profile');
  }
}
