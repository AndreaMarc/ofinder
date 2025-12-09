import Component from '@glimmer/component';
import { action } from '@ember/object';

/**
 * PerformerCard Component
 * Card responsiva per mostrare informazioni performer
 *
 * @param {Object} performer - Oggetto performer con dati
 * @param {String} variant - Variante: 'default' | 'compact' | 'featured'
 * @param {Function} onViewProfile - Callback click su bottone
 */
export default class PerformerCardComponent extends Component {
  get variant() {
    return this.args.variant || 'default';
  }

  get avatarHeight() {
    const heights = {
      compact: '200px',
      default: '300px',
      featured: '400px',
    };
    return heights[this.variant] || heights.default;
  }

  get showBio() {
    return this.variant !== 'compact' && this.args.performer.bio;
  }

  get truncatedBio() {
    const bio = this.args.performer.bio || '';
    if (this.variant === 'featured') {
      return bio.length > 200 ? bio.substring(0, 200) + '...' : bio;
    }
    return bio.length > 100 ? bio.substring(0, 100) + '...' : bio;
  }

  @action
  handleViewProfile() {
    if (this.args.onViewProfile) {
      this.args.onViewProfile(this.args.performer);
    }
  }
}
