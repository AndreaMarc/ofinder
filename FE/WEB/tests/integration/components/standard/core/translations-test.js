import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render, fillIn, waitUntil } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/translations',
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

      await render(hbs`<Standard::Core::Translations />`);

      //conto il numero di input
      //cambiare il numero se si aggiungono o rimuovono Input
      let InputExpected = 2;

      let InputCounter = 0;

      $('select.form-control').each(function () {
        InputCounter++;
      });

      assert.deepEqual(
        InputCounter,
        InputExpected,
        'Numero select uguale a quelli aspettati: ' + InputExpected
      );

      assert.dom('.alert-info').exists('alert info esiste');

      $('select.form-control').each(async function (i, e) {
        if (i == 0) {
          await fillIn(e, 'it');
        } else {
          await fillIn(e, 'web');
        }
      });

      await waitUntil(() => $('.translation-key').length > 0, {
        timeout: 10000,
      });

      assert.dom('.translation-key').exists('Voce menu esistente!');

      assert.dom('input.find-primary-key').exists('Input ricerca esiste!');

      assert.dom('button.collapsed').exists('Bottone con icona occhio esiste!');
    });
  }
);
