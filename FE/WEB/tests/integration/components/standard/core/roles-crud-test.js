import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Component | standard/core/roles-crud', function (hooks) {
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

    this.set('lastUpdateRole', '');

    this.set('updateRole', () => {
      this.lastUpdateRole = new Date().getTime();
    });

    await render(
      hbs`<Standard::Core::RolesCrud @currentTenant="1" @updateRole={{this.updateRole}} />`
    );

    //conto il numero di input
    //cambiare il numero se si aggiungono o rimuovono Input
    let InputExpected = 7;

    let InputCounter = 0;

    $('input, select').each(function (i, e) {
      if ($(e).hasClass('form-control')) InputCounter++;
    });

    //minimo di input rispettato
    assert.ok(
      InputCounter >= InputExpected,
      'Numero input maggiore o uguale a quelli aspettati: ' + InputExpected
    );

    assert.dom('button.btn-save-field').exists('bottone salva esiste!');
  });
});
