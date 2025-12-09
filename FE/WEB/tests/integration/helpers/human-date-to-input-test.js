import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Helper | human-date-to-input', function (hooks) {
  setupRenderingTest(hooks);

  // TODO: Replace this with your real tests.
  QUnit.skip('it renders', async function (assert) {
    this.set('inputValue', '1234');

    await render(hbs`{{human-date-to-input this.inputValue}}`);

    assert.dom(this.element).hasText('1234');
  });
});
