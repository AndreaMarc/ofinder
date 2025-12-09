import { module, test } from 'qunit';
import { setupTest } from 'poc-nuovo-fwk/tests/helpers';

module('Unit | Route | ticket-create', function (hooks) {
  setupTest(hooks);

  test('it exists', function (assert) {
    let route = this.owner.lookup('route:ticket-create');
    assert.ok(route);
  });
});
