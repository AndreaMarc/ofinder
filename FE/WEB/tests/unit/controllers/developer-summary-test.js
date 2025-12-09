import { module, test } from 'qunit';
import { setupTest } from 'poc-nuovo-fwk/tests/helpers';

module('Unit | Controller | developer-summary', function (hooks) {
  setupTest(hooks);

  // TODO: Replace this with your real tests.
  test('it exists', function (assert) {
    let controller = this.owner.lookup('controller:developer-summary');
    assert.ok(controller);
  });
});
