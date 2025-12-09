import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render, waitFor } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/setup-social',
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

      this.set('recordWeb', { thirdPartsAccesses: {} });
      this.set('recordApp', { thirdPartsAccesses: {} });

      await render(
        hbs`<Standard::Core::SetupSocial @recordApp={{this.recordApp}} @recordWeb={{this.recordWeb}}/>`
      );

      await waitFor('.nav');

      let tabs = 0;
      let tabsExpected = 3;

      $('li.nav-item').each(function () {
        tabs++;
      });

      assert.deepEqual(tabs, tabsExpected, 'Tabs presenti');
    });
  }
);
