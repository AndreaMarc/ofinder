import { module, test } from 'qunit';
import { setupRenderingTest } from 'poc-nuovo-fwk/tests/helpers';
import { render } from '@ember/test-helpers';
import { hbs } from 'ember-cli-htmlbars';

module('Integration | Helper | convertdate', function (hooks) {
  setupRenderingTest(hooks);

  // TODO: Replace this with your real tests.
  test('it renders', async function (assert) {
    this.set('data', new Date());
    this.set('format', 'YYYY-MM-DD');

    await render(hbs`{{convertdate this.data this.format}}`);

    const res = moment.parseZone(this.data).local().format(this.format);

    assert.dom(this.element).hasText(res);
  });
});
