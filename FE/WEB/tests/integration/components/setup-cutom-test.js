import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Component | setup-custom', function (hooks) {
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

    this.set('recordWeb', { maintenance: false });
    this.set('recordApp', { maintenance: false });

    await render(
      hbs`<SetupCustom @recordWeb={{this.recordWeb}} @recordApp={{this.recordApp}} @serviceAvailable="available"/>`
    );

    let ActualSelectCount = 0;

    $('select.form-control').each(function () {
      ActualSelectCount++;
    });

    assert.deepEqual(
      ActualSelectCount,
      2,
      'Numero di input corrisponde a quelli aspettati: ' + 2
    );

    assert.dom('button[type=submit].btn-primary').exists();
  });
});
