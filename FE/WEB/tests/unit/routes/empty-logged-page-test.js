import { module, test } from 'qunit';
import { setupTest } from 'poc-nuovo-fwk/tests/helpers';

module('Unit | Route | empty-logged-page', function (hooks) {
  setupTest(hooks);

  test('it exists', function (assert) {
    let route = this.owner.lookup('route:empty-logged-page');
    assert.ok(route);
  });
});
