import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | architect-ui/app-drawer/app-drawer',
  function (hooks) {
    setupRenderingTest(hooks);

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await render(hbs`<ArchitectUi::AppDrawer::AppDrawer />`);

      assert.dom('h3.drawer-heading').hasText('Utility');

      assert
        .dom('li.nav-item > a ')
        .hasText('Traduzioni', 'Il tab per le traduzioni esiste!');

      assert
        .dom('input[type=checkbox]')
        .exists('Il toggle per le traduziony on-the-fly esiste!');
    });
  }
);
