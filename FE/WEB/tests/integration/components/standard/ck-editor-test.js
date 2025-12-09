import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Component | standard/ck-editor', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });

    this.set('changeHtml', function (html) {
      let par = { target: { value: html } };
      this.storeNewValue('content', par);
    });

    await render(
      hbs`<Standard::CkEditor  @changeCallback={{this.changeHtml}} />`
    );

    assert.dom('.ckeditor-component').exists();
  });
});
