import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Helper | convertdate2', function (hooks) {
  setupRenderingTest(hooks);

  // TODO: Replace this with your real tests.
  test('it renders', async function (assert) {
    this.set('inputValue', '2023-12-29T02:43:01.015Z');

    await render(hbs`{{convertdate2 this.inputValue}}`);

    assert.dom(this.element).hasText('29 dicembre 2023 alle ore 03:43');
  });
});
