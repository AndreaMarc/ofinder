import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/setup-multi-language',
  function (hooks) {
    setupRenderingTest(hooks);

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await this.set('serviceAvailable', 'available');

      await this.set('recordWeb', {
        availableLanguages: [
          { code: 'it', label: 'Italiano', active: true },
          { code: 'en', label: 'English', active: true },
        ],
      });

      const session = this.owner.lookup('service:session');
      await session.authenticate(
        'authenticator:jwt',
        'unit.test@maestrale.it',
        '28ec245edc1a1d9813acb28441ae4313'
      );

      await render(
        hbs`<Standard::Core::SetupMultiLanguage   @recordWeb={{this.recordWeb}} @serviceAvailable={{this.serviceAvailable}}/>`
      );

      assert.dom('#accordion').exists('Accordion informazioni esiste');

      //conto il numero di input
      //cambiare il numero se si aggiungono o rimuovono Input
      let InputExpected = 4;

      let InputCounter = 0;

      $('input, select').each(function (i, e) {
        if ($(e).hasClass('form-control')) InputCounter++;
      });

      assert.deepEqual(
        InputCounter,
        InputExpected,
        'Numero input uguale a quelli aspettati: ' + InputExpected
      );

      assert.dom('button[type=submit]').exists('bottone salva esiste!');
    });
  }
);
