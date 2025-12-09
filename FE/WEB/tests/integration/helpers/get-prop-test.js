import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Helper | get-prop', function (hooks) {
  setupRenderingTest(hooks);

  // TODO: Replace this with your real tests.
  test('it renders', async function (assert) {
    //creo oggetto
    this.set('object', {
      id: 1,
      name: 'nome',
    });

    await render(hbs`<p>{{get-prop this.object "name"}}</p>`);

    assert.dom(this.element).hasText('nome');
  });
});
