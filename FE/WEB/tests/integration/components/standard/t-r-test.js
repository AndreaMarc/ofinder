import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Component | standard/t-r', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });

    this.set('text', 'Test');
    this.set('default', 'Unit Test');

    await render(
      hbs`<Standard::TR @key={{this.text}} @default={{this.default}}/>`
    );

    assert.dom(this.element).hasText('Unit Test');
  });
});
