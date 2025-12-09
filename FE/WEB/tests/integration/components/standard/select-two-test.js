import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Component | select-two', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });
    this.set(
      'options',
      '[{"id":"key1","value":"value1","selected":false},{"id":"key2","value":"value2"}]'
    );
    await render(hbs`<Standard::SelectTwo @options={{this.options}} />`);

    await waitFor('option[value=key1]');

    assert.dom('select.form-control').exists('');
    assert.dom('option[value=key1]').exists('');
    assert.dom('option[value=key2]').exists('');
  });
});
