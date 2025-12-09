import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/media-categories',
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
      await render(hbs`<Standard::Core::MediaCategories />`);

      await waitFor('input[type=search]');
      assert.dom('.media-categories').exists();
      assert.dom('input[type=search]').exists();
      assert.dom('table#categoriesTree').exists();
      assert.dom('ul.nav').exists();
    });
  }
);
