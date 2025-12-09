import { module, test, skip } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Component | standard/core/help-desk', function (hooks) {
  setupRenderingTest(hooks);

  QUnit.skip('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });
    const session = this.owner.lookup('service:session');
    await session.authenticate(
      'authenticator:jwt',
      'unit.test@maestrale.it',
      '28ec245edc1a1d9813acb28441ae4313'
    );

    await render(hbs`<Standard::Core::HelpDesk />`);

    await waitFor('.scroll-area-lg');

    assert.dom('button[test-aziendali]').exists();
    assert.dom('button[test-personali]').exists();

    assert.dom('.scroll-area-lg').exists();

    let ExpectedInputCount = 3;

    let ActualInputCount = 0;

    $('input, textarea').each(function (i, e) {
      if ($(e).hasClass('form-control')) ActualInputCount++;
    });

    assert.dom('button[test-segnalazione]').exists();

    assert.deepEqual(ActualInputCount, ExpectedInputCount);
  });
});
