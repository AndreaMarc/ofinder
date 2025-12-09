import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Component | standard/setup-security', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });

    await this.set('serviceAvailable', 'available');

    await this.set('recordWeb', {
      defaultClaims: [
        {
          name: 'canBypassMaintenance',
          description: 'Accede anche quando il sito è in manutenzione',
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
      hbs`<Standard::Core::SetupSecurity @serviceAvailable={{this.serviceAvailable}} @recordWeb={{this.recordWeb}}/>`
    );

    assert
      .dom(this.element)
      .containsText(
        'Convertitore giorni/minuti',
        'Convertitore giorni/minuti presente'
      );

    //controllo sul numero di input nel componente
    //modificare in caso di aggiunta o rimozione campo
    let inputExpected = 20;

    let inputCounter = 0;

    $('input, select').each((i, e) => {
      if ($(e).hasClass('form-control')) {
        //controlliamo che gli input abbiano classe bootstrap
        inputCounter++;
      }
    });

    assert.deepEqual(
      inputCounter,
      inputExpected,
      'Il numero di input è quello aspettato: ' + inputExpected
    );

    assert.dom('button[type=submit]').exists('bottone salva esiste!');
  });
});
