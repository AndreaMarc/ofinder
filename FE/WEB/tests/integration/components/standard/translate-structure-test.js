import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/translate-structure',
  function (hooks) {
    setupRenderingTest(hooks);

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await this.set('serviceAvailable', 'available');

      const session = this.owner.lookup('service:session');
      await session.authenticate(
        'authenticator:jwt',
        'unit.test@maestrale.it',
        '28ec245edc1a1d9813acb28441ae4313'
      );

      await render(
        hbs`<Standard::Core::TranslateStructure @serviceAvailable={{this.serviceAvailable}}/>`
      );

      assert.dom('.main-card').exists();

      assert.dom('.card-header.text-success').exists();

      assert.dom('.card-header.text-primary').exists();

      assert.dom('button.collapsed').exists();

      assert.dom('input[type=text]').exists();

      assert.dom('button.btn-success').exists('bottone aggiungi esiste');

      assert.dom('select.form-control').exists();
    });
  }
);
