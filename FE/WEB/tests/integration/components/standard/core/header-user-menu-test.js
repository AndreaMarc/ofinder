import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render, waitFor, click } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/header-user-menu',
  function (hooks) {
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

      await render(hbs`<Standard::Core::HeaderUserMenu />`);

      await waitFor('a[test-usermenu]');

      assert.dom('a[test-usermenu]').exists();

      await click('a[test-usermenu]');

      assert.dom('.widget-heading ').exists();

      assert.dom('.widget-heading ').includesText('Test Unit');
    });
  }
);
