import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/legals-details-crud',
  function (hooks) {
    setupRenderingTest(hooks);

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      const session = this.owner.lookup('service:session');
      await session.authenticate(
        'authenticator:jwt',
        'unit.test@maestrale.it',
        '28ec245edc1a1d9813acb28441ae4313'
      );

      let legalModel = {
        title: 'Informativa sulla Privacy',
        note: 'Inserire la privacy policy del sito.',
        code: 'privacyPolicy',
        language: 'it',
        content: '<p></p>',
        active: true,
        version: '1.0',
        dataActivation: '2023-09-16T08:02:11.573',
      };

      this.set('legalModel', legalModel);

      await render(
        hbs`<Standard::Core::LegalsDetailsCrud @model={{this.legalModel}} />`
      );

      assert.dom('.template-details').exists('Componente esiste');

      assert.dom('.ckeditor-component').exists('Editor html esiste');

      //controllo sul numero di input e select presenti nella pagina
      //cambiare questa value se si aggiungono o tolgono campi
      let ExpectedInputCount = 4;

      let ActualInputCount = 0;

      $('input, textarea').each(function (i, e) {
        //input con id che comincia con "registration..."
        if ($(e).hasClass('form-control')) ActualInputCount++;
      });

      assert.deepEqual(
        ActualInputCount,
        ExpectedInputCount,
        'Numero di input corrisponde a quelli aspettati: ' + ExpectedInputCount
      );
    });
  }
);
