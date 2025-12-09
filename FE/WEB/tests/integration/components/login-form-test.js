import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render, pauseTest } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';
import Service from '@ember/service';

module('Integration | Component | login-form', function (hooks) {
  setupRenderingTest(hooks);

  hooks.beforeEach(function () {
    // Stub del servizio siteSetup
    this.owner.register(
      'service:siteSetup',
      class extends Service {
        siteSetup = {
          thirdPartsAccesses: {
            facebookEnabled: false,
            googleEnabled: false,
          },
        };
      }
    );
  });

  test('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });

    await render(hbs`<Standard::Core::LoginForm />`);

    assert.dom('input[type=email]').exists('input Email esiste');

    assert.dom('input[type=password]').exists('input password esiste');

    assert.dom('#login-form-btn').exists('Bottone submit Accedi esiste!');
  });
});
