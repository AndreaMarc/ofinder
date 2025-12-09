/**
 * Inserisce una textarea con funzione di autosize
 * (autosize fornito dal plugin http://www.jacklmoore.com/autosize/).
 *
 * Può essere definita un'altezza massima per la textarea.
 * Può aggiunge un pulsante per espandere la textarea in modo che l'utente possa scegliere di ignorare l'altezza massima.
 *
 * @param {number} @rows : altezza di default della textarea.
 * @param {string} @size : dimensione della textarea. 'sm' per dimensione ridotta. Vuoto, omesso o altri valori per dimensione normale.
 * @param {string} @autosize :  Stabilisce se usare l'autosize.
 *                              'fixed' : viene usato l'autosize e non è possibile disattivarlo
 *                              'on' : viene usato l'autosize e compare un pulsante per disattivarlo/attivarlo; quando disattivato l'altezza sarà quella del parametro @rows
 *                              'off' : di default non viene usato l'autosize ma compare un pulsante per attivarlo/disattivarlo.
 *                              '', omesso o altri valori per non usare l'autosize
 * @param {function} @cb : callback eseguita quando viene modificato il contenuto della textarea. Viene passato il messaggio digitato nella textarea.
 * @param {string} @value : valore di default della textarea
 * @param {string} @type : Laddove sia necessario processare un @value di tipo json o array, settare @type a 'other'.
 *                          Omesso o stringa vuota per non manipolare il valore assegnato.
 * @param {string} @disabled : stringa vuota = false. Qualunque altro valore = true.
 *
 * Esempio di utilizzo:
 * <Standard::TextareaAutosize @disabled="" @value="" @cb="" id="help-desk-message" @rows="8" @autosize="fixed"/>
 */
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';

export default class StandardTextareaAutosizeComponent extends Component {
  @tracked textareaId = '';
  @tracked rows = '1';
  @tracked smallSize = false;
  @tracked autosizeMode = '';
  @tracked value = '';
  @tracked disabled = false;
  autosizeActive = false;
  cb = null;
  type = false;

  constructor(...attributes) {
    super(...attributes);

    // l'id viene generato come combinazione di un numero casuale e di un guid
    let casualCode = '';
    for (let i = 0; i < 6; i++) {
      casualCode += Math.floor(Math.random() * 10);
    }
    this.textareaId = `sel2_${casualCode}_${v4()}`;

    // altezza di default
    if (
      this.args.rows &&
      !isNaN(parseFloat(this.args.rows)) &&
      isFinite(this.args.rows)
    ) {
      this.rows = parseInt(this.args.rows).toString();
    }

    // comportamento della textarea
    if (
      this.args.autosize &&
      ['fixed', 'on', 'off'].includes(this.args.autosize)
    ) {
      this.autosizeMode = this.args.autosize;
    }

    // dimensione della textarea
    if (this.args.size && this.args.size === 'sm') {
      this.smallSize = true;
    }

    // eventuale callback
    if (this.args.cb && typeof this.args.cb === 'function') {
      this.cb = this.args.cb;
    }

    // eventuale valore iniziale
    if (this.args.type && this.args.type === 'other') {
      this.type = true;
    }

    // eventuale valore iniziale
    this.updateValue();

    if (this.args.disabled) {
      this.disabled = !!this.args.disabled;
    }

    setTimeout(() => {
      this.start();
    }, 20);
  }

  start() {
    if (
      this.autosizeMode !== '' &&
      ['fixed', 'on'].includes(this.autosizeMode)
    ) {
      // eslint-disable-next-line no-undef
      autosize(document.getElementById(this.textareaId));
      this.autosizeActive = true;
    }
  }

  @action
  updateDisabled() {
    this.disabled = !!this.args.disabled;
  }

  @action
  updateValue() {
    if (this.args.value && this.args.value !== '') {
      if (this.type) {
        // in input ho un array o un oggetto
        //this.value = JSON.stringify(this.args.value);
        this.value = JSON.stringify(this.args.value, undefined, 2);
      } else {
        this.value = this.args.value;
      }
    } else {
      this.value = '';
    }
  }

  @action
  changeActive() {
    if (this.autosizeActive) {
      // eslint-disable-next-line no-undef
      autosize.destroy(document.getElementById(this.textareaId));
    } else {
      // eslint-disable-next-line no-undef
      autosize(document.getElementById(this.textareaId));
    }
    this.autosizeActive = !this.autosizeActive;
  }

  @action
  onInput(event) {
    if (this.cb) {
      if (this.type) {
        // trasformo il valore in array/oggetto
        let out = event.target.value;
        try {
          out = JSON.parse(out);
        } catch (e) {
          //
        }
        this.cb(out);
      } else {
        this.cb(event.target.value);
      }
    }
  }
}
