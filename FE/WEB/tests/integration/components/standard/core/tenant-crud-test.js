import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Component | standard/core/tenant-crud', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders', async function (assert) {
    // Set any properties with this.set('myProperty', 'value');
    // Handle any actions with this.set('myAction', function(val) { ... });

    const session = this.owner.lookup('service:session');
    await session.authenticate(
      'authenticator:jwt',
      'unit.test@maestrale.it',
      '28ec245edc1a1d9813acb28441ae4313'
    );

    await render(hbs`<Standard::Core::TenantCrud />`);

    await waitFor('#tenantsTree');

    //conto il numero di input
    //cambiare il numero se si aggiungono o rimuovono Input
    let InputExpected = 4;

    let InputCounter = 0;

    $('input, select').each(function (i, e) {
      if ($(e).hasClass('form-control')) InputCounter++;
    });

    assert.strictEqual(
      InputCounter,
      InputExpected,
      'Numero input uguale a quelli aspettati: ' + InputExpected
    );

    assert.dom('button.btn-save-field').exists('bottone salva esiste!');

    assert.dom('#tenantsTree').exists('File tree dei teants esiste!');
  });
});
