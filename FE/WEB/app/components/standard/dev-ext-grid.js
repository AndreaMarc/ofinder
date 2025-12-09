/**
 * WRAPPER PER IL COMPONENTE DATA-GRID DI DEV-EXTREME.
 * Info qui:
 * https://js.devexpress.com/jQuery/Documentation/Guide/UI_Components/DataGrid/Getting_Started_with_DataGrid/
 *
 * @param {object} options : oggetto di configurazione di data-grid.
 * @param {function} setIstance : callback a cui viene passata l'istanza della griglia,
 *                                per memorizzarla e usarla liberamente. Verrà ri-valorizzata a
 *                                null quando il componente DevExtGrid viene distrutto.
 * @param {null/number} height : altezza del container in pixel. Omettere il parametro per altezza automatica
 *
 *
 *
 * @callback setIstance : restituisce l'istanza della griglia
 *
 * I parametri possono essere variati semplicemente modificando l'oggetto options.
 * È inoltre possibile ottenere un'istanza della griglia e operare su quella, a tal
 * scopo consulare l'esempio che segue e la callBack setIstance.
 *
 *
 * NOTA:
 * Per l'export PD>F e EXCEL non è necessario importare alcuna libreria poiché
 * sono tutte già importate di default nel framework!
 *
 *
 * ESEMPIO DI UTILIZZO:
 * Ipotizziamo di avere in un nostro componente:
 * @tracked istance = null;
 * @action myIstance(val) {
 *    this.istance = val;
 * }
 *
 *
 * passando l'action al componete:
 * <Standard::DevExtGrid @setIstance={{this.myIstance}} @options="" @height="500"/>
 *
 * riceveremo l'istanza di dev-ext-grid nella variabile istance.
 */
/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { htmlSafe } from '@ember/template';
import { v4 } from 'ember-uuid';
import $ from 'jquery';

export default class StandardDevExtGridComponent extends Component {
  @tracked id = '';
  @tracked options = {};
  @tracked istance = null;
  height = 'auto';
  @tracked height = htmlSafe(`height: auto;`);

  setIstance = null;

  constructor(...attributes) {
    super(...attributes);

    try {
      this.id = `data-grid-${v4()}`;

      if (typeof this.args.options === 'undefined') {
        throw new Error('istanziato senza oggetto di configurazione!');
      } else if (typeof this.args.options === 'object') {
        this.options = this.args.options;
      } else if (typeof this.args.options === 'string') {
        this.options = JSON.parse(this.args.options);
      } else {
        throw new Error('oggetto di configurazione non cxorretto!');
      }

      this.setIstance =
        typeof this.args.setIstance !== 'undefined' &&
        typeof this.args.setIstance === 'function'
          ? this.args.setIstance
          : null;

      if (this.args.height && !isNaN(this.args.height)) {
        this.height = htmlSafe(`height: ${parseInt(this.args.height)}px;`);
      }
    } catch (e) {
      console.error('Errore nel componente Dev-Ext-Grid: ' + e.toString());
    }
  }

  @action
  start() {
    try {
      this.istance = $(`#${this.id}`)
        .dxDataGrid(this.options)
        .dxDataGrid('instance');

      if (this.setIstance) this.setIstance(this.istance);
    } catch (e) {
      console.error(e);
    }
  }

  @action
  changeParams() {
    let options = this.args.options || {};
    this.istance.option(options);
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);

    $(`#${this.id}`).dxDataGrid('dispose');
    $(`#${this.id}`).remove();
    if (this.setIstance) this.setIstance(null);
  }
}
