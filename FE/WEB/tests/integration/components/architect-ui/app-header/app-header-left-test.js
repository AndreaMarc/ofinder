import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { pauseTest, render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module(
  'Integration | Component | architect-ui/app-header/app-header-left',
  function (hooks) {
    setupRenderingTest(hooks);

    test('it renders', async function (assert) {
      // Set any properties with this.set('myProperty', 'value');
      // Handle any actions with this.set('myAction', function(val) { ... });

      const header = this.owner.lookup('service:header');
      header.search = true;
      this.set('service:header', header);

      await render(hbs`<ArchitectUi::AppHeader::AppHeaderLeft />`);

      assert.dom('.search-icon').exists();

      //Elementi commentanti da daniele nel componente, solo la ricerca esiste.
      // assert.dom("input.search-input").exists();

      // assert.dom(".header-megamenu").exists();
    });
  }
);
