/* eslint-disable ember/no-jquery */
/**
 * CREA UN CIRCLE PROGRESS
 *
 * @param {string} @style : stile da applicare. Vedi elenco sottostante, non esaustivo
 * @param {number} @value : valore (0 - 1)
 * @param {string} @valueType : "percentage" (default) = il valore va espresso in percentuale
 *                              "absolute" = il valore va espresso come numero assoluto
 * @param {number} @size : dimensione in pixel. Se omesso, viene estratto il valore di default associato al parametro @style
 * @param {string} @fontSize : dimensione del testo, espresso in fsize (es: fsize-2). Se omesso, viene estratto il valore di default associato al parametro @style
 * @param {number} @startAngle : angolo iniziale
 * @param {bool} @reverse : animazione inversa e disegno dell'arco
 * @param {number} @thickness : spessore dell'arco. Opzionale.
 * @param {string} @lineCap : Estremità della linea dell'arco: "butt" (default), "round" or "square"
 * @param {string} @emptyFill : colore dell'arco vuoto
 * @param {string} @color1 : opzionale. Se vuoi sovrascrivere il colore di default è possibile definire un colore custom
 * @param {string} @color2 : opzionale. Per definire un gradiente custom. Viene ignorato se color1 non è fornito.
 *
 *
 * STYLE:
 * circle-progress-primary, circle-progress-success, ecc (vedi getter getStyle)
 *
 */
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import $ from 'jquery';

export default class StandardChartsCircleProgressComponent extends Component {
  @tracked id = null;
  @tracked style = '';
  value = '';
  valueType = 'percentage';

  constructor(...attributes) {
    super(...attributes);

    this.id = `circle-progress-${v4()}`;
  }

  @action
  setCircleProgress() {
    let self = this;
    if (
      this.args.style &&
      typeof this.args.style !== 'undefined' &&
      this.args.style !== ''
    ) {
      this.style = this.args.style;
    }

    let circleProfressSetup = this.getStyle;

    $(`#${this.id}`)
      .circleProgress(circleProfressSetup)
      .on('circle-animation-progress', function (event, progress, stepValue) {
        if (self.valueType === 'percentage') {
          $(this)
            .find('small')
            .html(
              `<span class="${circleProfressSetup.fsize}">${stepValue
                .toFixed(2)
                .substr(2)}%<span>`
            );
        } else {
          if (progress === 1) {
            if (
              isNaN(circleProfressSetup.value) ||
              (!isNaN(circleProfressSetup.value) &&
                parseFloat(circleProfressSetup.value) > 1)
            ) {
              circleProfressSetup.value = 1;
            }

            $(this)
              .find('small')
              .html(
                `<span class="${circleProfressSetup.fsize}">${self.value}<span>`
              );
          }
        }
      });
  }

  get getStyle() {
    let self = this;
    let fill = {};
    let size = 52;
    let fsize = '';
    switch (this.style) {
      case 'circle-progress-primary':
        fill = { color: '#3f6ad8' };
        size = 52;
        break;
      case 'circle-progress-success':
        fill = { color: '#3ac47d' };
        size = 52;
        break;
      case 'circle-progress-danger':
        fill = { color: '#d92550' };
        size = 52;
        break;
      case 'circle-progress-info':
        fill = { color: '#16aaff' };
        size = 52;
        break;
      case 'circle-progress-alternate':
        fill = { color: '#6f42c1' };
        size = 52;
        break;
      case 'circle-progress-dark':
        fill = { color: '#6c757d' };
        size = 52;
        break;
      case 'circle-progress-warning':
        fill = { color: '#fd7e14' };
        break;
      case 'circle-progress-gradient':
        fill = { gradient: ['#ff1e41', '#ff8130'] };
        size = 52;
        break;
      // SM
      case 'circle-progress-gradient-alt-sm':
        fill = { gradient: ['#007bff', '#16aaff'] };
        size = 46;
        break;
      case 'circle-progress-danger-sm':
        fill = { color: '#d92550' };
        size = 46;
        break;
      case 'circle-progress-warning-sm':
        fill = { color: '#fd7e14' };
        size = 46;
        break;
      case 'circle-progress-success-sm':
        fill = { color: '#3ac47d' };
        size = 46;
        break;
      // LG
      case 'circle-progress-gradient-lg':
        fill = { gradient: ['#ff1e41', '#ff8130'] };
        size = 64;
        break;
      case 'circle-progress-gradient-alt-lg':
        fill = { gradient: ['#007bff', '#16aaff'] };
        size = 64;
        break;

      // XL
      case 'circle-progress-success-xl':
        fill = { color: '#3ac47d' };
        size = 114;
        fsize = 'fsize-2';
        break;
      case 'circle-progress-gradient-xl':
        fill = { gradient: ['#fdb83a', '#fd7e14'] };
        size = 114;
        fsize = 'fsize-2';
        break;
      case 'circle-progress-danger-xl':
        fill = { gradient: ['#ff1e41', '#ff8130'] };
        size = 114;
        fsize = 'fsize-2';
        break;
    }

    // colore custom
    let color1 = null,
      color2 = null;
    if (
      this.args.color1 &&
      typeof this.args.color1 !== 'undefined' &&
      this.args.color1 !== ''
    ) {
      color1 = this.args.color1;
    }

    if (
      this.args.color2 &&
      typeof this.args.color2 !== 'undefined' &&
      this.args.color2 !== ''
    ) {
      color2 = this.args.color2;
    }

    if (color1 && !color2) {
      fill = { color: color1 };
    } else if (color1 && color2) {
      fill = { gradient: [color1, color2] };
    }

    // valueType
    if (
      this.args.valueType &&
      typeof this.args.valueType !== 'undefined' &&
      this.args.valueType === 'percentage'
    ) {
      self.valueType = 'percentage';
    } else {
      self.valueType = 'absolute';
    }

    // valore
    let value;
    if (
      //this.args.value &&
      typeof this.args.value !== 'undefined'
    ) {
      if (self.valueType === 'percentage') {
        if (!isNaN(this.args.value)) {
          value = parseFloat(this.args.value);
        } else {
          value = 0;
        }
      } else {
        value = 1;
        self.value = this.args.value;
      }
    }

    // eventuale dimensione del testo
    if (this.args.fontSize && this.args.fontSize !== '') {
      fsize = this.args.fontSize;
    }

    // eventuale dimensione custom
    if (
      this.args.size &&
      !isNaN(this.args.size) &&
      parseInt(this.args.size) > 0
    ) {
      size = parseInt(this.args.size);
    }

    // angolo iniziale
    let startAngle = -Math.PI;
    if (typeof this.args.startAngle !== 'undefined') {
      startAngle = this.args.startAngle;
    }

    // animazione inversa
    let reverse = false;
    if (this.args.reverse) reverse = true;

    // spessore arco
    let thickness = 'auto';
    if (
      this.args.thickness &&
      !isNaN(this.args.thickness) &&
      parseInt(this.args.thickness) > 0
    ) {
      thickness = parseInt(this.args.thickness);
    }

    // estremità dell'arco
    let lineCap = 'butt';
    if (this.args.lineCap && ['round', 'square'].includes(this.args.lineCap)) {
      lineCap = this.args.lineCap;
    }

    let emptyFill = 'rgba(0, 0, 0, .1)';
    if (this.args.emptyFill && this.args.emptyFill !== '') {
      emptyFill = this.args.emptyFill;
    }

    return {
      value: value,
      size: size,
      fsize: fsize,
      fill: fill,
      startAngle: startAngle,
      reverse: reverse,
      thickness: thickness,
      lineCap: lineCap,
      emptyFill: emptyFill,
    };
  }
}
