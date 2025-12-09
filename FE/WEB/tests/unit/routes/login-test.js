import { module, test } from 'qunit';
import { setupTest } from 'poc-nuovo-fwk/tests/helpers';

module('Unit | Route | login', function (hooks) {
  setupTest(hooks);

  test('it exists', async function (assert) {
    let route = this.owner.lookup('route:login');

    assert.ok(route, 'la route esiste');
  });
});
