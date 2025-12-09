import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Helper | number-format', function (hooks) {
  setupRenderingTest(hooks);

  // TODO: Replace this with your real tests.
  test('it renders', async function (assert) {
    // Salva l'originale Intl.NumberFormat
    const originalNumberFormat = Intl.NumberFormat;

    // Sovrascrivi Intl.NumberFormat per questo test
    Intl.NumberFormat = function (locale, options) {
      return new originalNumberFormat('en-US', options);
    };

    this.set('inputValue', 1234);

    await render(hbs`{{number-format this.inputValue}}`);

    assert.dom(this.element).hasText('1,234');

    // Ripristina l'originale Intl.NumberFormat dopo il test
    Intl.NumberFormat = originalNumberFormat;
  });
});
