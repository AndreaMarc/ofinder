import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/general-crud',
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

      const stp = this.owner.lookup('service:siteSetup');
      stp.siteSetup.entitiesList = [];

      await render(hbs`<Standard::Core::GeneralCrud/>`);

      await waitFor('select.form-control');

      assert.dom('select.form-control').exists('');
    });
  }
);
