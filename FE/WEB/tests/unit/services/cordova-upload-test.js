import { module, test } from 'qunit';
import { setupTest } from 'poc-nuovo-fwk/tests/helpers';

module('Unit | Service | cordova-upload', function (hooks) {
  setupTest(hooks);

  // TODO: Replace this with your real tests.
  QUnit.skip('it exists', function (assert) {
    let service = this.owner.lookup('service:cordova-upload');
    assert.ok(service);
  });
});
