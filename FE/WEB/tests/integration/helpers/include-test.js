import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Helper | include', function (hooks) {
  setupRenderingTest(hooks);

  // TODO: Replace this with your real tests.
  test('it works', async function (assert) {
    this.set('myArray', [1, 2, 3]);
    this.set('myValue', 2);

    await render(hbs`{{include this.myArray this.myValue}}`);

    assert.dom(this.element).hasText('true');
  });
});
