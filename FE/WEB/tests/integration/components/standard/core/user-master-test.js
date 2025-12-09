import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';
import Service from '@ember/service';

module('Integration | Component | standard/core/user-master', function (hooks) {
  setupRenderingTest(hooks);

  hooks.beforeEach(function () {
    // Stub del servizio siteSetup
    this.owner.register(
      'service:siteSetup',
      class extends Service {
        siteSetup = {
          publicRegistration: true,
          registrationFields: { registration: [] },
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

    await render(hbs`<Standard::Core::UsersMaster />`);

    assert.dom('.users-master').exists('Componente usersMaster esiste');

    assert.dom('ul.body-tabs').exists('tabs esistono');

    assert.dom('.tab-content').exists('contenuto componente esiste');
  });
});
