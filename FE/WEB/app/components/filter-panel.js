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
  @tracked selectedPlatforms = [];
  @tracked selectedCategories = [];
  @tracked maxPrice = 25;
  @tracked selectedRating = null;
  @tracked onlyVerified = false;
  @tracked onlyNew = false;
  @tracked onlyActiveToday = false;

  // Use passed platformFilters from args, or fallback to defaults
  get platformFilters() {
    return this.args.platformFilters || [
      { id: 'onlyfans', name: 'OnlyFans', emoji: '🔵', count: 1234 },
      { id: 'fansly', name: 'Fansly', emoji: '🟣', count: 567 },
      { id: 'instagram', name: 'Instagram', emoji: '📸', count: 890 },
      { id: 'tiktok', name: 'TikTok', emoji: '🎵', count: 456 },
    ];
  }

  // Use passed categoryFilters from args, or fallback to defaults
  get categoryFilters() {
    return this.args.categoryFilters || [
      { id: 'fitness', name: 'Fitness', count: 234 },
      { id: 'gaming', name: 'Gaming', count: 156 },
      { id: 'lifestyle', name: 'Lifestyle', count: 345 },
      { id: 'cosplay', name: 'Cosplay', count: 123 },
      { id: 'art', name: 'Art & Design', count: 89 },
    ];
  }

  ratingFilters = [
    { value: 5, label: '5 stelle' },
    { value: 4, label: '4+ stelle' },
    { value: 3, label: '3+ stelle' },
  ];

  get hasActiveFilters() {
    return (
      this.selectedPlatforms.length > 0 ||
      this.selectedCategories.length > 0 ||
      this.maxPrice < 50 ||
      this.selectedRating !== null ||
      this.onlyVerified ||
      this.onlyNew ||
      this.onlyActiveToday
    );
  }

  get activeFiltersCount() {
    let count = 0;
    count += this.selectedPlatforms.length;
    count += this.selectedCategories.length;
    if (this.maxPrice < 50) count++;
    if (this.selectedRating) count++;
    if (this.onlyVerified) count++;
    if (this.onlyNew) count++;
    if (this.onlyActiveToday) count++;
    return count;
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
  toggleCategory(category) {
    if (this.selectedCategories.includes(category.id)) {
      this.selectedCategories = this.selectedCategories.filter(
        (id) => id !== category.id
      );
    } else {
      this.selectedCategories = [...this.selectedCategories, category.id];
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
    this.selectedPlatforms = [];
    this.selectedCategories = [];
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
        platforms: this.selectedPlatforms,
        categories: this.selectedCategories,
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

  get isCategorySelected() {
    return (categoryId) => this.selectedCategories.includes(categoryId);
  }
}
