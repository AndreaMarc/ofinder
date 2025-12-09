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
  get showBio() {
    return this.args.showBio !== false && this.args.performer?.bio;
  }

  get truncatedBio() {
    const bio = this.args.performer?.bio || '';
    return bio.length > 100 ? bio.substring(0, 100) + '...' : bio;
  }

  @action
  handleViewProfile() {
    if (this.args.onViewProfile) {
      this.args.onViewProfile(this.args.performer);
    }
  }
}
