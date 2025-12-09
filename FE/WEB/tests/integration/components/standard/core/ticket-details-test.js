import { module, test, todo } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | standard/core/ticket-details',
  function (hooks) {
    setupRenderingTest(hooks);

    QUnit.skip('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      await render(hbs`<Standard::Core::TicketDetails />`);

      assert.dom(this.element).hasText('');

      // Template block usage:
      await render(hbs`
      <Standard::Core::TicketDetails>
        template block text
      </Standard::Core::TicketDetails>
    `);

      assert.dom(this.element).hasText('template block text');
    });
  }
);
