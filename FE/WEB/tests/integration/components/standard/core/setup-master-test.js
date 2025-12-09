import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/setup-master',
  function (hooks) {
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

      await render(hbs`<Standard::Core::SetupMaster />`);
      await waitFor('.nav');

      //conto il numero di voci di setup
      //cambiare il numero se si aggiungono voci
      let NavItemExpected = 14;
      let PaneExpected = 14;

      let NavItemCounter = 0;
      let PaneCounter = 0;

      $('#setup-master-ul-primary > li.nav-item').each(function () {
        NavItemCounter++;
      });

      $('#setup-master-container-primary > .tab-pane').each(function () {
        PaneCounter++;
      });

      assert.deepEqual(
        PaneCounter,
        PaneExpected,
        'Numero pannelli corrispondono a quelli aspettati: ' + PaneExpected
      );
      assert.deepEqual(
        NavItemCounter,
        NavItemExpected,
        'Numero nav-item corrispondono a quelli aspettati: ' + NavItemExpected
      );
      assert.deepEqual(
        PaneCounter,
        NavItemCounter,
        'Ogni Nav.item corrisponde ad un Pannello'
      );
    });
  }
);
