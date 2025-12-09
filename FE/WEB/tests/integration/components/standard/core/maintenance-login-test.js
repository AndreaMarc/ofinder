import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render, waitUntil } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/maintenance-login',
  function (hooks) {
    setupRenderingTest(hooks);

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await render(hbs`<Standard::Core::MaintenanceLogin />`);

      await waitUntil(async () => {
        await new Promise((resolve) => setTimeout(resolve, 1000));
        return true;
      });

      assert.dom('.modal-dialog').exists();

      assert.dom('a.btn').exists();
    });
  }
);
