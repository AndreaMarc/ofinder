/* eslint-disable ember/no-jquery */
/* eslint-disable no-undef */
import Component from '@glimmer/component';
import { action } from '@ember/object';
import { inject as service } from '@ember/service';
import $ from 'jquery';

export default class ArchitectUiAppDrawerAppDrawerComponent extends Component {
  @service store;
  @service translation;
  @service dialogs;

  @action
  start() {
    if ($('#drawer-scrollbar-container')[0]) {
      $('#drawer-scrollbar-container').each(function () {
        // eslint-disable-next-line no-undef, no-unused-vars
        const ps = new PerfectScrollbar($(this)[0], {
          wheelSpeed: 2,
          wheelPropagation: false,
          minScrollbarLength: 20,
        });
      });
    }

    // abilito gli slider
    setTimeout(() => {
      let self = this;
      $(`.drawer-lang-translation`)
        .bootstrapToggle()
        .change(function (evt) {
          let value = evt.target.checked;
          self.translation.activateTranslation(value);

          if (!value) {
            self.translateCancelChange();
          }
        });
    }, 10);
  }

  @action
  translateCancelChange(event) {
    try {
      event.preventDefault();
      // eslint-disable-next-line no-empty
    } catch (e) {}

    $('#edit-translation-area').hide();
    $('#change-translation-app, #change-translation-web').val('');
    $('#change-translation-key').val('');

    $(
      '#change-translation-app, #change-translation-web, #translate-confirm-change'
    ).prop('disabled', true);

    $('#translate-confirm-change')
      .attr('data-key', '')
      .attr('data-record-id', '');
  }

  @action
  translateComfirmChange(event) {
    event.preventDefault();
    let self = this;
    this.dialogs.confirm(
      '<h6>Modifica voce</h6>',
      `<p>Confermi la modifica al testo?<br />Azione irreversibile. Le modifiche avranno effetto immediato.</p>`,
      self.executeChange,
      null,
      ['Conferma', 'Annulla']
    );
  }

  @action
  executeChange() {
    let self = this;
    let $btn = $('#translate-confirm-change');
    let key = $btn.attr('data-key');
    let recordId = $btn.attr('data-record-id');

    this.store
      .findRecord('translation', recordId)
      .then(function (lang) {
        /*let wordApp = lang.translationApp;
        let wordWeb = lang.translationWeb;*/

        let wordApp = self.changeObjectValue(
          lang.translationApp,
          key,
          $('#change-translation-app').val()
        );
        let wordWeb = self.changeObjectValue(
          lang.translationWeb,
          key,
          $('#change-translation-web').val()
        );

        lang.translationApp = wordApp;
        lang.translationWeb = wordWeb;
        lang.save();

        self.translation.languageTranslation = wordWeb; // la modifica on-the-fly è disponibile solo su dispositivi desktop!
        self.dialogs.toast('Operazione riuscita', 'success', 'bottom-left', 3);
      })
      .catch((e) => {
        console.error(e);
        self.dialogs.toast(
          'Si è verificato un errore. Riprova!',
          'error',
          'bottom-left',
          4
        );
      });
  }

  changeObjectValue(obj, keyString, newValue) {
    const keys = keyString.split('.');
    const lastKey = keys.pop();
    const newObj = keys.reduce((acc, key) => acc[key], obj);
    newObj[lastKey] = newValue;
    return obj;
  }
}
