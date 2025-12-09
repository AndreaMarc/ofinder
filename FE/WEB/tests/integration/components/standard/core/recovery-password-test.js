import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';
import Service from '@ember/service';
module(
  'Integration | Component | standard/core/recovery-password',
  function (hooks) {
    setupRenderingTest(hooks);

    hooks.beforeEach(function () {
      // Stub del servizio siteSetup
      this.owner.register(
        'service:router',
        class extends Service {
          currentRoute = {
            queryParams: {
              ph: '2',
            },
          };
        }
      );
    });

    QUnit.skip('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await render(hbs`<Standard::Core::RecoveryPassword />`);

      assert.dom('#identification').exists();

      assert.dom('.text-danger').exists();

      assert.dom('button[disabled]').exists();
    });
  }
);
