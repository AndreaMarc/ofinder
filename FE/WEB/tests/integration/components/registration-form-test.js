import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';
import Service from '@ember/service';

module('Integration | Component | registration-form', function (hooks) {
  setupRenderingTest(hooks);

  hooks.beforeEach(function () {
    // Stub del servizio siteSetup
    this.owner.register(
      'service:siteSetup',
      class extends Service {
        siteSetup = {
          publicRegistration: true,
          registrationFields: { registration: {} },
        };
      }
    );
  });

  test('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });

    await render(hbs`<Standard::Core::RegistrationForm/>`);

    //controllo sul numero di input e select presenti nella pagina
    //cambiare questa value se si aggiungono o tolgono campi
    let ExpectedInputCount = 31;

    let ActualInputCount = 0;

    $('input, select').each(function (i, e) {
      //input con id che comincia con "registration..."
      if ($(e).attr('id').includes('registration')) ActualInputCount++;
    });

    assert.deepEqual(
      ActualInputCount,
      ExpectedInputCount,
      'Numero di input corrisponde a quelli aspettati: ' + ExpectedInputCount
    );

    assert
      .dom('button[data-test-registrati]')
      .exists('Bottone registrazione esiste');
  });
});
