/**
 * Crea una select contenente i filtri tipici di una query (uguale, maggiore, contiene, ecc)
 *
 * @param {function} @cbChange : callback da invocare al cambiamento. Vengono passati i seguenti parametri:
 *    @param {string} column : nome della colonna che è stata modificata
 *    @param {string} filterType : tipo di filtro selezionao (es: equals)
 *    @param {general} value : valore del filtro
 *
 * @param {function} @cbOrder : callback da invocare per cambiare l'ordine di visualizzazione. Parametri:
 *    @param {string} column : nome della colonna di cui modificare l'ordine
 *    @param {string} order : ordine scelto. Es: se column è 'id' e l'ordine scelto è down, il parametro order sarà '-id'
 *
 * @param {string} @column : nome della colonna a cui il filtro si riferisce
 * @param {string} @size : dimensione dei bottoni. 'sm' per dimensione ridotta. Vuoto, omesso o altri valori per dimensione normale.
 * @param {string} @inputType : indica il tipo di dato. Modifica le opzioni di ricerca. Es:
 *                              se 'string', non vengono mostrate le opzioni maggiore, minore ecc.
 *                              se 'bool' viene mostrata solo l'opzione 'checked'. Ecc...
 * @param {general} @value : valore di default (opzionale)
 * @param {string} @autosize :  Laddove il campo di immissione sia di tipo text-area, stabilisce se usare l'autosize.
 *                              'fixed' : viene usato l'autosize e non è possibile disattivarlo
 *                              'on' : viene usato l'autosize e compare un pulsante per disattivarlo/attivarlo; quando disattivato l'altezza sarà quella del parametro @rows
 *                              'off' : di default non viene usato l'autosize ma compare un pulsante per attivarlo/disattivarlo.
 *                              '', omesso o altri valori per non usare l'autosize
 * @param {bool} disabled
 *
 * Possibili valori dei filtri:
 * - equals
 * - not-equals
 * - lessThan
 * - lessOrEqual
 * - greaterThan
 * - greaterOrEqual
 * - contains
 * - checked
 *
 * Possibili valori del parametro inputType:
 * - string
 * - number
 * - date
 * - boolean
 * - other
 */
import Component from '@glimmer/component';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';

export default class StandardCoreFilterSelectComponent extends Component {
  cbChange = null;
  cbOrder = null;
  @tracked column = '';
  filterType = 'equals';
  @tracked smallSize = false;
  @tracked inputType = 'string';
  @tracked defaultValue = '';
  @tracked order = '';
  @tracked autosize = '';
  @tracked disabled = false;

  constructor(...attributes) {
    super(...attributes);

    if (this.args.column) {
      this.column = this.args.column;
    }
    if (this.args.defaultValue) {
      this.defaultValue = this.args.defaultValue;
    }

    if (this.args.cbChange && typeof this.args.cbChange === 'function') {
      this.cbChange = this.args.cbChange;
    }

    if (this.args.cbOrder && typeof this.args.cbOrder === 'function') {
      this.cbOrder = this.args.cbOrder;
    }

    if (this.args.size && this.args.size === 'sm') {
      this.smallSize = true;
    }

    if (
      this.args.inputType &&
      ['string', 'number', 'date', 'boolean', 'other'].includes(
        this.args.inputType
      )
    ) {
      this.inputType = this.args.inputType;
    }

    if (this.args.autosize) {
      this.autosize = this.args.autosize;
    }

    if (this.args.disabled) {
      this.disabled = !!this.args.disabled;
    }
  }

  @action
  update() {
    this.disabled = !!this.args.disabled;
  }

  @action
  changeValue(event) {
    let val = '';
    try {
      val = event.target.checked;
    } catch (e) {
      val = event;
    }

    if (this.inputType === 'boolean') {
      this.defaultValue = val;
      this.filterType = 'equals';
    } else {
      this.defaultValue = val;
    }
    this.sendFilter();
  }
  @action
  emptyValue() {
    this.order = '';
    this.filterType = 'equals';
    this.defaultValue = this.inputType === 'boolean' ? false : '';
    this.sendFilter();
  }

  @action
  changeFilterType(event) {
    this.filterType = event.target.value;
    if (this.defaultValue !== '') this.sendFilter();
  }

  sendFilter() {
    if (this.cbChange) {
      this.cbChange(this.column, this.filterType, this.defaultValue);
    }
  }

  @action
  changeOrder(event) {
    this.order = event.target.value;

    if (this.cbOrder) {
      this.cbOrder(this.column, this.order);
    }
  }
}
