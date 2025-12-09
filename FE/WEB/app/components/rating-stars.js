import Component from '@glimmer/component';

/**
 * RatingStars Component
 * Visualizza stelle di rating con voto numerico e conteggio recensioni
 *
 * @param {Number} rating - Voto da 0 a 5
 * @param {Number} reviewCount - Numero recensioni (opzionale)
 * @param {String} size - Dimensione: 'sm' | 'md' | 'lg' (default: 'md')
 * @param {Boolean} interactive - Stelle cliccabili (default: false)
 * @param {Function} onChange - Callback quando rating cambia (se interactive)
 */
export default class RatingStarsComponent extends Component {
  get formattedRating() {
    return this.args.rating?.toFixed(1) || '0.0';
  }

  get starsArray() {
    const rating = this.args.rating || 0;
    const stars = [];

    for (let i = 1; i <= 5; i++) {
      if (i <= Math.floor(rating)) {
        // Stella piena
        stars.push({
          index: i,
          iconClass: 'pe-7s-star',
          colorClass: 'ofinder-text-accent',
          filled: true,
        });
      } else if (i === Math.ceil(rating) && rating % 1 !== 0) {
        // Mezza stella (mostrata con opacity)
        stars.push({
          index: i,
          iconClass: 'pe-7s-star',
          colorClass: 'ofinder-text-accent',
          filled: true,
          style: 'opacity: 0.5;',
        });
      } else {
        // Stella vuota
        stars.push({
          index: i,
          iconClass: 'pe-7s-star',
          colorClass: 'text-muted',
          filled: false,
        });
      }
    }

    return stars;
  }

  get fontSize() {
    const size = this.args.size || 'md';
    const sizes = {
      sm: '16px',
      md: '20px',
      lg: '28px',
    };
    return sizes[size] || sizes.md;
  }

  get textSize() {
    const size = this.args.size || 'md';
    const sizes = {
      sm: '14px',
      md: '16px',
      lg: '20px',
    };
    return sizes[size] || sizes.md;
  }
}
