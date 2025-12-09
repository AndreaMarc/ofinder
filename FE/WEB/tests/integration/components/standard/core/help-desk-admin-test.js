import { module, test, skip } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/help-desk-admin',
  function (hooks) {
    setupRenderingTest(hooks);

    skip('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });
      const session = this.owner.lookup('service:session');
      await session.authenticate(
        'authenticator:jwt',
        'unit.test@maestrale.it',
        '28ec245edc1a1d9813acb28441ae4313'
      );

      await render(hbs`<Standard::Core::HelpDeskAdmin />`);

      await waitFor('.tickets-table');

      assert.dom('.tickets-table').exists();
      assert.dom('input[test-search]').exists();

      assert.dom('ul.nav').exists('sidebar dei filtri esiste');

      var expectedTabs = 11;
      var actualTabs = 0;

      $('li.nav-item').each(() => {
        actualTabs++;
      });

      assert.deepEqual(actualTabs, expectedTabs);
    });
  }
);
