import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/templates-crud',
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

      await render(
        hbs`<Standard::Core::TemplatesCrud @excludedCategory='1,16' @title="LISTA DEGLI ARTICOLI"/>`
      );

      await waitFor('.templates-crud');

      assert.dom('.templates-crud').exists();

      await waitFor('#templateTree');

      assert.dom('#templateTree').exists();
    });
  }
);
