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

  @tracked searchQuery = '';
  @tracked activeFilters = {
    platforms: [],
    categories: [],
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
   */
  get filteredPerformers() {
    let performers = this.model.performers || [];

    // Apply search query filter
    if (this.searchQuery && this.searchQuery.length > 0) {
      const query = this.searchQuery.toLowerCase();
      performers = performers.filter((p) => {
        return (
          p.displayName.toLowerCase().includes(query) ||
          (p.bio && p.bio.toLowerCase().includes(query))
        );
      });
    }

    // Apply verified filter
    if (this.activeFilters.verified) {
      performers = performers.filter((p) => p.verified === true);
    }

    // Apply premium filter
    if (this.activeFilters.premium) {
      performers = performers.filter((p) => p.isPremium === true);
    }

    // Apply top-rated filter
    if (this.activeFilters.topRated) {
      performers = performers.filter((p) => p.averageRating >= 4.5);
    }

    // Apply platform filters
    if (this.activeFilters.platforms.length > 0) {
      performers = performers.filter((p) => {
        return p.channels?.some((ch) =>
          this.activeFilters.platforms.includes(ch.platformType)
        );
      });
    }

    // Apply rating filter
    if (this.activeFilters.minRating > 0) {
      performers = performers.filter(
        (p) => p.averageRating >= this.activeFilters.minRating
      );
    }

    // Apply price filter
    if (this.activeFilters.maxPrice < 50) {
      performers = performers.filter(
        (p) => p.minPrice <= this.activeFilters.maxPrice
      );
    }

    // Pagination
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return performers.slice(startIndex, endIndex);
  }

  /**
   * Total count of filtered performers (before pagination)
   */
  get filteredPerformersCount() {
    let performers = this.model.performers || [];

    if (this.searchQuery && this.searchQuery.length > 0) {
      const query = this.searchQuery.toLowerCase();
      performers = performers.filter((p) => {
        return (
          p.displayName.toLowerCase().includes(query) ||
          (p.bio && p.bio.toLowerCase().includes(query))
        );
      });
    }

    if (this.activeFilters.verified) {
      performers = performers.filter((p) => p.verified === true);
    }

    if (this.activeFilters.premium) {
      performers = performers.filter((p) => p.isPremium === true);
    }

    if (this.activeFilters.topRated) {
      performers = performers.filter((p) => p.averageRating >= 4.5);
    }

    if (this.activeFilters.platforms.length > 0) {
      performers = performers.filter((p) => {
        return p.channels?.some((ch) =>
          this.activeFilters.platforms.includes(ch.platformType)
        );
      });
    }

    return performers.length;
  }

  /**
   * Check if there are any results
   */
  get hasResults() {
    return this.filteredPerformersCount > 0;
  }

  /**
   * Calculate total pages
   */
  get totalPages() {
    return Math.ceil(this.filteredPerformersCount / this.itemsPerPage);
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
   * Handle advanced filter changes
   */
  @action
  handleFilterChange(filterType, value) {
    if (filterType === 'platforms' || filterType === 'categories') {
      // Toggle array filters
      const index = this.activeFilters[filterType].indexOf(value);
      if (index > -1) {
        this.activeFilters[filterType].splice(index, 1);
      } else {
        this.activeFilters[filterType].push(value);
      }
    } else {
      // Set scalar filters
      this.activeFilters[filterType] = value;
    }
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
    alert(`Profilo di ${performer.displayName} - TODO: Implementare pagina dettaglio`);
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
