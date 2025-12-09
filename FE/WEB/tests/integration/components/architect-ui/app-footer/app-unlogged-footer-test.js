import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';
import Service from '@ember/service';

module(
  'Integration | Component | architect-ui/app-footer/app-unlogged-footer',
  function (hooks) {
    setupRenderingTest(hooks);

    hooks.beforeEach(function () {
      // Stub del servizio siteSetup
      this.owner.register(
        'service:siteSetup',
        class extends Service {
          siteSetup = {
            publicRegistration: true,
            registrationFields: { registration: {} },
          };
        }
      );
    });

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await render(hbs`<ArchitectUi::AppFooter::AppUnloggedFooter />`);

      assert.dom('.unlogged-footer-btn').exists();

      assert.dom(this.element).hasText('REGISTRATI ACCEDI');
    });
  }
);
