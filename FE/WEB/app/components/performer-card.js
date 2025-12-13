import Component from '@glimmer/component';
import { action } from '@ember/object';

/**
 * PerformerCard Component
 * Card responsiva per mostrare informazioni performer
 *
 * @param {Object} performer - Oggetto performer con dati
 * @param {String} variant - Variante: 'default' | 'compact' | 'featured'
 * @param {String} searchType - Tipo di ricerca: 'CamGirl' | 'Performer' | 'Escort'
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

  get showScheduleChart() {
    const searchType = this.args.searchType;
    return (
      (searchType === 'CamGirl' || searchType === 'Performer') &&
      this.args.performer?.schedules &&
      this.args.performer.schedules.length > 0
    );
  }

  @action
  handleViewProfile() {
    if (this.args.onViewProfile) {
      this.args.onViewProfile(this.args.performer);
    }
  }
}
