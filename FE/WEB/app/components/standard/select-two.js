/* eslint-disable ember/no-jquery */
/**
 * Genera una select gestita da select2
 *
 *
 * @param {string} @addEmptyOption : Indica se aggiungere l'opzione vuota. "" = false, "1" = true.
 * @param {array} @options : array in formato stringa contenente tutte le opzioni da mostrare nella select.
 *                                  Vedere il formato a fondo pagina.
 * @param {string-object} @select2Options : opzionale. Oggetto in formato stringa contenente eventuali
 *                                  parametri di setup di select2
 * @param {function} changeCB : funzione di call-back da invocare quando l'utente modifica la select. Vengono
 *                              passati 2 argomenti: il value e il text del valore selezionato.
 *                              Es: changeCG(selectId, selectedValue) { ... }
 *
 * - I parametri @options e @addEmptyOption sono monitorati (al cambiamento, la select viene rigenerata.
 * - L'eventuale opzione vuota creata con @addEmptyOption ha selected=true per default. In tal caso le
 *   options passate non devono avere il parametro selected a true.
 *
 * Formato dell'array @options (da convertire in stringa):
 * [
 *  { id: 'id1', value: 'value1', selected: true },
 *  { id: 'id2', value: 'value2' }
 * ]
 *
 * (se il parametro selected viene omesso, verrà considerato come false)
 *
 *
 * Esempio di utilizzo:
 * <Standard::SelectTwo @addEmptyOption="1" @changeCB="" @options='[{"id":"id1","value":"value1","selected":false},{"id":"id2","value":"value2"}]'/>
 *
 */
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { task } from 'ember-concurrency';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import $ from 'jquery';

export default class SelectTwoComponent extends Component {
  @tracked selectId = '';
  @tracked optionsList = [];

  changeCB = null;
  addEmptyOption = false;
  s2Started = false;
  select2Options = {
    theme: 'bootstrap4',
    width: '100%',
  };

  constructor(...attributes) {
    super(...attributes);
    // l'id viene generato come combinazione di un numero casuale e di un guid
    let casualCode = '';
    for (let i = 0; i < 6; i++) {
      casualCode += Math.floor(Math.random() * 10);
    }
    this.selectId = `sel2_${casualCode}_${v4()}`;

    // verifico che il parametro @select2Options sia in formato stringa
    if (
      this.args.select2Options &&
      typeof this.args.select2Options !== 'string'
    ) {
      console.error(
        `L'attributo @select2Options del componente select-two deve essere in formato stringa!`
      );
      return false;
    }
    // memorizzo eventuali settaggi per select2
    if (this.args.select2Options && this.args.select2Options !== '') {
      try {
        let s2o = JSON.parse(this.args.select2Options);
        this.select2Options = {
          ...this.select2Options,
          ...s2o,
        };
      } catch (e) {
        console.error(e);
      }
    }

    // memorizzo la call-back di cambiamento
    if (this.args.changeCB) {
      this.changeCB = this.args.changeCB;
    }
    this.start.perform();
  }

  start = task({ drop: true }, async () => {
    this.optionsList = [];

    // verifico che il parametro @options sia in formato stringa
    if (
      this &&
      this.args &&
      this.args.options &&
      typeof this.args.options !== 'string'
    ) {
      console.error(
        `L'attributo @options del componente select-two deve essere in formato stringa!`
      );
      return false;
    }

    // se il componente era già stato avviato, lo distruggo
    try {
      if (this.s2Started) {
        await this.teardownSelect2();
      }
    } catch (e) {
      //console.error(e);
    }

    // verifico se deve essere aggiunta l'opzione vuota
    if (this.args.addEmptyOption) {
      this.addEmptyOption = !!this.args.addEmptyOption;
    }

    // popolo la lista delle opzioni
    try {
      if (this.args.options) {
        let firstOpt = [];
        if (this.addEmptyOption) {
          firstOpt = [{ id: '', value: '- -', selected: true }];
        }

        let opts = JSON.parse(this.args.options);

        opts = [...firstOpt, ...opts];

        let hasSelected = false;
        opts.map((item) => {
          // best-practies di Select2 - conversione in stringa del parametro id
          item.id = item.id.toString();
          // mi assicuro che ci sia una sola opzione "selected"
          if (Object.prototype.hasOwnProperty.call(item, 'selected')) {
            if (!hasSelected) {
              item.selected = true;
              hasSelected = true;
            } else {
              item.selected = false;
            }
          } else item.selected = false;
        });

        this.optionsList = opts;
      } else {
        //throw new Error('No options');
        if (this.addEmptyOption) {
          this.optionsList = [{ id: '', value: '- -', selected: true }];
        }
      }

      // verifico che un solo elemento sia selected
      /*
      let sel = this.optionsList.filter((item) => item.selected === true);
      if (sel.length > 0) {
        this.selectedElement = sel[0].id;
      }
      */

      this.setupSelect2();
    } catch (e) {
      console.warn(e);
      if (e !== 'No options') console.error(e);
      this.optionsList = [];
      this.setupSelect2();
    }
  });

  // Applica select2 e ne gestisce la variazione
  @action
  setupSelect2() {
    setTimeout(() => {
      let element = `#${this.selectId}`;
      $(element).empty();
      let self = this;

      // popolo le options
      $.each(this.optionsList, (key, item) => {
        $(element).append(
          new Option(
            item.value,
            item.id,
            item.selected ? true : false,
            item.selected ? true : false
          )
        );
      });

      $(element).select2(this.select2Options);
      this.s2Started = true;

      // Make sure to sync changes made by Select2 with Ember
      $(element).on('change', () => {
        let $element = $(element);
        if (self.changeCB) self.changeCB($element.val(), $element.text());
      });
    }, 80);
  }

  @action
  async teardownSelect2() {
    let element = `#${this.selectId}`;
    $(element).off('change');
    $(element).select2('destroy');
    $(element).empty();
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);
    this.teardownSelect2(); // Rimuovi il listener quando il componente viene distrutto
  }
}
