import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import {
  click,
  pauseTest,
  render,
  waitFor,
  waitUntil,
} from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';
import Service from '@ember/service';

module('Integration | Component | standard/gdpr', function (hooks) {
  setupRenderingTest(hooks);

  hooks.beforeEach(function () {
    // Stub del servizio siteSetup
    this.owner.register(
      'service:translation',
      class extends Service {
        languageTranslation = {
          component: {
            gdpr: {
              waitPlease: 'Attendere prego...',
            },
          },
        };
      }
    );
  });

  test('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });
    localStorage.setItem('poc-allow-gdpr', Date.now());
    localStorage.setItem('poc-allow-cookie', 'all');

    await render(hbs`<Standard::Core::Gdpr />`);

    await waitFor('.gdpr-block');

    assert.dom('.gdpr-block').exists();

    assert.dom('button[test-none]').exists();
    assert.dom('button[test-limited]').exists();
    assert.dom('button[test-all]').exists();
  });
});
