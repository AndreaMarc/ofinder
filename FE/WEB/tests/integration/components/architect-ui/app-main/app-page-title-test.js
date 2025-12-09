import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | architect-ui/app-main/app-page-title',
  function (hooks) {
    setupRenderingTest(hooks);

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await render(
        hbs`<ArchitectUi::AppMain::AppPageTitle @title="Test"  @description="Sottotitolo test"/>`
      );

      assert.dom(this.element).hasText('Test Sottotitolo test');
    });
  }
);
