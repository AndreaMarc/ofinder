/* eslint-disable ember/no-jquery */
/**
 * CREA UN TOOLTIP
 * per visualizzare del testo al click del mouse su un elemento.
 *
 * @param {string} color colore dello sfondo. Valori: "black" (default), "white".
 * @param {string} title testo da visualizzare
 *
 * Esempio di utilizzo:
 * <Standard::ToolTip @color="black" @title="testo del tooltip">ABC</Standard::ToolTip>
 */

import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import $ from 'jquery';

export default class StandardToolTipComponent extends Component {
  @tracked title = '';
  @tracked style = 'tooltip';
  @tracked id = '';

  constructor(...attributes) {
    super(...attributes);
  }

  @action
  setup() {
    this.id = 'tooltip_' + v4();

    if (this.args.title && this.args.title !== '') {
      this.title = this.args.title;
    }

    if (this.args.color && this.args.color === 'white') {
      this.style = 'tooltip-light';
    }

    this.tooltip();
  }

  tooltip() {
    setTimeout(() => {
      if (this.style === 'tooltip') {
        $(`#${this.id}`).tooltip();
      } else {
        $(`#${this.id}`).tooltip({
          template:
            '<div class="tooltip tooltip-light"><div class="tooltip-inner"></div></div>',
        });
      }
    }, 100);
  }
}
