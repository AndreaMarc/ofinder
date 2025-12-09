import { module, test } from 'qunit';
import { setupTest } from 'poc-nuovo-fwk/tests/helpers';

module('Unit | Route | ticket-details', function (hooks) {
  setupTest(hooks);

  test('it exists', function (assert) {
    let route = this.owner.lookup('route:ticket-details');
    assert.ok(route);
  });
});
