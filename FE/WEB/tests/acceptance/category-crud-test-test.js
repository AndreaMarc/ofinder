import { module, test } from 'qunit';
import {
  visit,
  currentURL,
  click,
  fillIn,
  waitFor,
  waitUntil,
  pauseTest,
  isSettled,
} from '@ember/test-helpers';
import { setupApplicationTest } from 'poc-nuovo-fwk/tests/helpers';
import { destroy } from '@ember/destroyable';

module('Acceptance | category crud test', function (hooks) {
  setupApplicationTest(hooks);

  let session;
  hooks.beforeEach(async function () {
    session = this.owner.lookup('service:session');
    await session.authenticate(
      'authenticator:jwt',
      'unit.test@maestrale.it',
      '28ec245edc1a1d9813acb28441ae4313'
    );

    await waitUntil(() => {
      return new Promise((resolve) => {
        setTimeout(() => {
          resolve(true);
        }, 1500);
      });
    });
  });

  hooks.afterEach(async function () {
    destroy(session);
  });

  test('Create-Update-Remove category', async function (assert) {
    localStorage.setItem('poc-allow-gdpr', Date.now());
    localStorage.setItem('poc-allow-cookie', 'all');

    //#region Login e navigate in /categories
    // await visit('/login');

    // await fillIn('#identification', 'unit.test@maestrale.it');
    // await fillIn('#password', 'Maestrale2004!');

    // await click('#login-form-btn');

    // await waitUntil(
    //   function () {
    //     return currentURL() === '/';
    //   },
    //   { timeout: 99999 }
    // );

    await visit('/categories');

    assert.deepEqual(
      currentURL(),
      '/categories',
      'Sono nella pagina category.'
    );
    //#endregion

    //#region CREATE category
    await waitFor('input[test-input-name]');
    await fillIn('input[test-input-name]', 'Test');
    await fillIn(
      'textarea[test-input-description]',
      'UnitTest category non modificato'
    );
    await click('button[test-save]');

    await waitUntil(function () {
      return $('#categoriesTree').find("li:contains('Test ')").length > 0;
    });

    assert.true(
      $('#categoriesTree').find(" li:contains('Test')").length > 0,
      'categoria creata'
    );
    //#endregion

    //#region UPDATE category
    await click($('#categoriesTree').find(" li:contains('Test')").get(0));

    await waitFor('form[test-updateForm]', { timeout: 99999 });

    await fillIn('input[test-updateName]', 'MODIFICATO');
    await fillIn('textarea[test-updateDesc]', 'UnitTest category MODIFICATO');
    await click('button[test-updateSave]');

    await click($("button.ajs-button:contains('Conferma')").get(0));

    await waitUntil(function () {
      return $('#categoriesTree').find("li:contains('MODIFICATO ')").length > 0;
    });

    assert.true(
      $('#categoriesTree').find(" li:contains('MODIFICATO')").length > 0,
      'categoria modificata'
    );
    //#endregion

    //#region REMOVE category
    await click($('#categoriesTree').find(" li:contains('MODIFICATO')").get(0));

    await click('div[test-deleteDropdown]');
    await click('button[test-deleteAll]');

    await click($("button.ajs-button:contains('Conferma')").get(0));

    await waitUntil(function () {
      return isSettled();
    });

    assert.true(true, 'Categoria eliminata');
    //#endregion
  });
});
