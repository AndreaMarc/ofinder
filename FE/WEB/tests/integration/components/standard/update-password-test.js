import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Component | standard/update-password', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });

    await render(hbs`<Standard::Core::UpdatePassword />`);

    //controllo sul numero di input e select presenti nella pagina
    //cambiare questa value se si aggiungono o tolgono campi
    let ExpectedInputCount = 5;

    let ActualInputCount = 0;

    $('input').each(function (i, e) {
      if ($(e).hasClass('form-control') || $(e).hasClass('form-check-input'))
        ActualInputCount++;
    });

    assert.strictEqual(
      ActualInputCount,
      ExpectedInputCount,
      'Numero di input corrisponde a quelli aspettati: ' + ExpectedInputCount
    );

    assert.dom('button[disabled]').exists();
  });
});
