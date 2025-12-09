import { module, test } from 'qunit';

import { setupTest } from 'poc-nuovo-fwk/tests/helpers';

module('Unit | Model | fwk addon', function (hooks) {
  setupTest(hooks);

  // Replace this with your real tests.
  test('it exists', function (assert) {
    let store = this.owner.lookup('service:store');
    let model = store.createRecord('fwk-addon', {});
    assert.ok(model);
  });
});
