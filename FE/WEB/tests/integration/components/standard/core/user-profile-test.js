import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { click, pauseTest, render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';
import Service from '@ember/service';
module(
  'Integration | Component | standard/core/user-profile',
  function (hooks) {
    setupRenderingTest(hooks);

    hooks.beforeEach(function () {
      // Stub del servizio siteSetup
      this.owner.register(
        'service:siteSetup',
        class extends Service {
          siteSetup = {
            publicRegistration: true,
            registrationFields: { profile: {} },
          };
        }
      );
    });

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });
      const session = this.owner.lookup('service:session');
      await session.authenticate(
        'authenticator:jwt',
        'unit.test@maestrale.it',
        '28ec245edc1a1d9813acb28441ae4313'
      );

      await render(hbs`<Standard::Core::UserProfile />`);

      assert.dom('form').exists('Form info utente presente');

      //Faccio comparire anche il componente per caricare il file
      await click('button');

      // await waitFor('.upload-area');

      // assert.dom('input[type=file]').exists('input carica foto presente');

      //controllo sul numero di input e select presenti nella pagina
      //cambiare questa value se si aggiungono o tolgono campi
      let ExpectedInputCount = 37;

      let ActualInputCount = 0;

      $('input, select').each(function (i, e) {
        ActualInputCount++;
      });

      assert.deepEqual(
        ActualInputCount,
        ExpectedInputCount,
        'Numero di input corrisponde a quelli aspettati: ' + ExpectedInputCount
      );
    });
  }
);
