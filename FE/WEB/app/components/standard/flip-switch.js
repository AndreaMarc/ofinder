/* eslint-disable ember/no-jquery */
/**
 * Componente flip-switch bootstrap.
 * La versione originale è disponibile qui: https://gitbrent.github.io/bootstrap4-toggle/
 *
 * @param {string} size : dimensione. Valori ammessi: 'lg', '', 'sm', 'xs'
 * @param {bool} checked: (tracked).
 * @param {string} onstyle : opzionale. 'primary', 'secondary', 'success', 'danger', 'warning', 'info', 'light', 'dark' o gli stessi preceduti da 'outline-'
 * @param {string} offstyle : opzionale. 'primary', 'secondary', 'success', 'danger', 'warning', 'info', 'light', 'dark' o gli stessi preceduti da 'outline-'
 * @param {string} ontext : testo per pulsante attivo
 * @param {string} offtext : testo per pulsante disattivo
 * @param {number} width : larghezza in pixel. Da utilizzare quando uno dei due testi ha una lunghezza molto diversa dall'altro. Usare "full" per '100%'
 * @param {funciton} cb : callback da invocare al cambiamento del flip-switch
 * @param {bool} disabled : (tracked).
 *
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::FlipSwitch @checked="true" @size="sm" @offstyle="warning" @onstyle="success" @offtext="No" @ontext="Sì" @width="80" @cb=""/>
 *
 */
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import { task } from 'ember-concurrency';
import $ from 'jquery';

export default class StandardFlipSwitchComponent extends Component {
  @tracked id = '';
  @tracked cid = '';
  @tracked size = '';
  @tracked disabled;
  @tracked checked;
  @tracked onStyle = '';
  @tracked offStyle = '';
  @tracked onText = 'on';
  @tracked offText = 'off';
  isInitialized = false;
  width = 0;
  cb = null;

  constructor(...attributes) {
    super(...attributes);
    this.cid = `cfs-${v4()}-${Math.floor(100000 + Math.random() * 900000)}`;
    this.id = `fs-${v4()}-${Math.floor(100000 + Math.random() * 900000)}`;

    if (typeof this.args.width !== 'undefined' && this.args.width !== '')
      this.width =
        this.args.width === 'full' ? '100%' : `${parseInt(this.args.width)}px`;

    this.size = '';
    if (
      typeof this.args.size !== 'undefined' &&
      ['lg', 'sm', 'xs'].includes(this.args.size.toString())
    )
      this.size = this.args.size;

    if (typeof this.args.onstyle !== 'undefined' && this.args.onstyle !== '')
      this.onStyle = this.args.onstyle;

    if (typeof this.args.offstyle !== 'undefined' && this.args.offstyle !== '')
      this.offStyle = this.args.offstyle;

    let onText = 'on';
    if (typeof this.args.ontext !== 'undefined' && this.args.ontext !== '')
      onText = this.args.ontext;

    let offText = 'off';
    if (typeof this.args.offtext !== 'undefined' && this.args.offtext !== '')
      offText = this.args.offtext;

    let diff = onText.length - offText.length;
    if (diff > 0) {
      // Aggiungi spazi speciali a this.offText
      offText += '&nbsp;'.repeat(diff);
    } else if (diff < 0) {
      // Aggiungi spazi speciali a this.onText
      onText += '&nbsp;'.repeat(-diff);
    }
    this.onText = onText;
    this.offText = offText;

    this.checked =
      typeof this.args.checked !== 'undefined' &&
      this.args.checked.toString() === 'true'
        ? true
        : false;

    this.disabled =
      typeof this.args.disabled !== 'undefined' &&
      this.args.disabled.toString() === 'true'
        ? true
        : false;

    if (this.args.cb && typeof this.args.cb === 'function')
      this.cb = this.args.cb;
  }

  start = task({ drop: true }, async () => {
    // avvio il toggle
    let $el = $(`#${this.id}`);
    if (this.size !== '') $el.attr('data-size', this.size);
    if (this.onStyle !== '') $el.attr('data-onstyle', this.onStyle);
    if (this.offStyle !== '') $el.attr('data-offstyle', this.offStyle);
    if (
      ['light', 'dark', 'outline-light', 'outline-dark'].includes(
        this.onStyle
      ) ||
      ['light', 'dark', 'outline-light', 'outline-dark'].includes(this.offStyle)
    )
      $el.attr('data-style', 'border');
    if (this.onText !== '') $el.attr('data-on', this.onText);
    if (this.offText !== '') $el.attr('data-off', this.offText);

    setTimeout(() => {
      $el.bootstrapToggle();

      if (this.disabled) {
        $el.bootstrapToggle('disable');
      }

      // imposto il listener
      this.startListener();

      // imposto la dimensione del contenitore
      let $cel = $(`#${this.cid}`);
      setTimeout(() => {
        $cel
          .find('[data-toggle="toggle"]')
          .css({ width: this.width, height: '1em' });
        this.isInitialized = true;
      }, 30);
    }, 10);
  });

  startListener() {
    let self = this;
    let $el = $(`#${this.id}`);
    $el.on('change', function () {
      self.callCb($(this).prop('checked'));
    });
  }
  stopListener() {
    let $el = $(`#${this.id}`);
    $el.off('change');
  }

  callCb(checked) {
    this.checked = checked;
    if (!this.isInitialized) return false;
    if (this.cb) {
      this.cb(checked);
    } else {
      console.warn('La callback di flip-switch non è definita!');
    }
  }

  // invocata quando viene modificato l'attributo disabled
  @action
  changeAttr() {
    if (!this.isInitialized) return false;

    let oldDisabled = this.disabled;
    let oldChecked = this.checked;

    let disabled =
      typeof this.args.disabled !== 'undefined' &&
      this.args.disabled.toString() === 'true'
        ? true
        : false;

    let checked =
      typeof this.args.checked !== 'undefined' &&
      this.args.checked.toString() === 'true'
        ? true
        : false;

    let $el = $(`#${this.id}`);
    if (checked !== oldChecked) {
      this.stopListener();
      this.checked = checked;
      if (this.checked) {
        $el.bootstrapToggle('on');
      } else {
        $el.bootstrapToggle('off');
      }

      this.startListener();
    }

    if (disabled !== oldDisabled) {
      this.disabled = disabled;
      if (this.disabled) {
        $el.bootstrapToggle('disable');
      } else {
        $el.bootstrapToggle('enable');
      }
    }
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);
    $(`#${this.id}`).off('change').bootstrapToggle('destroy');
  }
}
