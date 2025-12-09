import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

/**
 * SearchBar Component
 * Barra di ricerca con autocomplete e filtri rapidi
 *
 * @param {String} placeholder - Testo placeholder
 * @param {Array} quickFilters - Array filtri rapidi
 * @param {Function} onSearch - Callback ricerca
 */
export default class SearchBarComponent extends Component {
  @tracked searchQuery = '';
  @tracked showFiltersPanel = false;
  @tracked activeQuickFilters = [];

  get placeholder() {
    return (
      this.args.placeholder ||
      'Cerca performer per nome, categoria, tag...'
    );
  }

  get quickFilters() {
    return this.args.quickFilters || [];
  }

  get activeFiltersCount() {
    return this.activeQuickFilters.length;
  }

  @action
  handleInput(event) {
    this.searchQuery = event.target.value;
  }

  @action
  handleKeydown(event) {
    if (event.key === 'Enter') {
      this.performSearch();
    }
  }

  @action
  toggleFilters() {
    this.showFiltersPanel = !this.showFiltersPanel;
  }

  @action
  toggleQuickFilter(filter) {
    if (this.activeQuickFilters.includes(filter.id)) {
      this.activeQuickFilters = this.activeQuickFilters.filter(
        (id) => id !== filter.id
      );
    } else {
      this.activeQuickFilters = [...this.activeQuickFilters, filter.id];
    }

    // Notify parent component
    if (this.args.onFilterToggle) {
      this.args.onFilterToggle(filter);
    }
  }

  @action
  performSearch() {
    if (this.args.onSearch) {
      this.args.onSearch(this.searchQuery);
    }
  }

  get isFilterActive() {
    return (filterId) => this.activeQuickFilters.includes(filterId);
  }
}
