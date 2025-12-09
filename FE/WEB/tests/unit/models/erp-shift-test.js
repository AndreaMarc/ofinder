import { module, test } from 'qunit';

import { setupTest } from 'poc-nuovo-fwk/tests/helpers';

module('Unit | Model | erp shift', function (hooks) {
  setupTest(hooks);

  // Replace this with your real tests.
  test('it exists', function (assert) {
    let store = this.owner.lookup('service:store');
    let model = store.createRecord('erp-shift', {});
    assert.ok(model);
  });
});
