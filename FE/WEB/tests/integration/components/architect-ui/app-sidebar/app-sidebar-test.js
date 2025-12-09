import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';
import EmberObject from '@ember/object';
import { inject as service } from '@ember/service';

module(
  'Integration | Component | architect-ui/app-sidebar/app-sidebar',
  function (hooks) {
    setupRenderingTest(hooks);

    test('it renders', async function (assert) {
      const router = this.owner.lookup('service:routerService');

      this.set('routerService', router);

      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await render(hbs`<ArchitectUi::AppSidebar::AppSidebar />`);

      // await waitFor(".app-sidebar");

      // assert.dom(this.element).hasText('');
      assert.true(true);
    });
  }
);
