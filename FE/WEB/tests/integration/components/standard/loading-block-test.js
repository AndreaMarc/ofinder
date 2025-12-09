import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Component | standard/loading-block', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });

    await render(hbs`<Standard::LoadingBlock />`);

    assert.dom(this.element).hasText('');

    // Template block usage:
    await render(hbs`
      <Standard::LoadingBlock>
        template block text
      </Standard::LoadingBlock>
    `);

    assert.dom(this.element).hasText('template block text');
  });
});
