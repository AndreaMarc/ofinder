/**
 * CREA UN GRAFICO APEX-CHARTS
 *
 * @param {object-string} @options : le opzioni del grafo in formato stringa
 */
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';

export default class StandardChartsApexChartsComponent extends Component {
  @tracked chartId = '';
  @tracked options = null;
  @tracked error = '';

  constructor(...attributes) {
    super(...attributes);

    this.chartId = `apex-charts-${v4()}`;
  }

  @action
  renderChart(element) {
    try {
      this.options =
        typeof this.args.options !== 'undefined' &&
        typeof this.args.options === 'string'
          ? this.args.options
          : null;

      if (!this.options) throw new Error('not available Apex-Charts datas');
      let opts = JSON.parse(this.options);
      // eslint-disable-next-line no-undef
      let chart = new ApexCharts(element, opts);
      chart.render();
    } catch (e) {
      console.error(e);
      this.error = 'Errore nella configurazione di Apex-Charts';
    }
  }
}
