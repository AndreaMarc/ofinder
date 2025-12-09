import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | architect-ui/app-footer/app-footer',
  function (hooks) {
    setupRenderingTest(hooks);

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await render(hbs`<ArchitectUi::AppFooter::AppFooter />`);

      assert.dom('.app-footer__inner').exists();
      assert.dom('.app-footer-left').exists();
      assert.dom('.app-footer-right').exists();
    });
  }
);
