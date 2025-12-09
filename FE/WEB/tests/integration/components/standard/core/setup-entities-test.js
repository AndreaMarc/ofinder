import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/setup-entities',
  function (hooks) {
    setupRenderingTest(hooks);

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await this.set('serviceAvailable', 'available');

      await this.set('recordWeb', {
        entitiesList: [
          {
            entity: 'categories',
            title: 'Categorie',
            key: '',
            description: 'Gestione delle Categorie',
            keyDescription: '',
          },
        ],
      });

      const session = this.owner.lookup('service:session');
      await session.authenticate(
        'authenticator:jwt',
        'unit.test@maestrale.it',
        '28ec245edc1a1d9813acb28441ae4313'
      );

      await render(
        hbs`<Standard::Core::SetupEntities @recordWeb={{this.recordWeb}} @serviceAvailable={{this.serviceAvailable}}/>`
      );

      //conto il numero di input
      //cambiare il numero se si aggiungono o rimuovono Input
      let InputExpected = 10;
      let InputDisabledExpected = 5;

      let InputCounter = 0;
      let InputDisabledCounter = 0;

      $('input, select').each(function (i, e) {
        if ($(e).hasClass('form-control')) {
          if ($(e).is('[disabled]')) {
            InputDisabledCounter++;
          }

          InputCounter++;
        }
      });

      assert.deepEqual(
        InputCounter,
        InputExpected,
        'Numero input uguale a quelli aspettati: ' + InputExpected
      );

      assert.deepEqual(
        InputDisabledCounter,
        InputDisabledExpected,
        'Numero input disabilitati uguale a quelli aspettati: ' +
          InputDisabledExpected
      );

      assert.dom('button.btn-save-field').exists('bottone salva esiste!');
    });
  }
);
