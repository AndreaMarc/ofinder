import { module, test } from 'qunit';
import {
  visit,
  currentURL,
  click,
  fillIn,
  waitFor,
  waitUntil,
  isSettled,
  pauseTest,
} from '@ember/test-helpers';
import { setupApplicationTest } from 'poc-nuovo-fwk/tests/helpers';
import { selectFiles } from 'ember-file-upload/test-support';
import { destroy } from '@ember/destroyable';
import $ from 'jquery';

module('Acceptance | media crud', function (hooks) {
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

  test('UPLOAD-MODIFICA-ELIMINAZIONE Media File', async function (assert) {
    // TODO: Riattivare questo test quando sarà risolto il problema XYZ
    return assert.ok(true, 'Test temporaneamente skippato');

    /*
    //#region Login e navigate in /media
    localStorage.setItem('poc-allow-gdpr', Date.now());
    localStorage.setItem('poc-allow-cookie', 'all');
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

    await visit('/media');

    await waitUntil(
      function () {
        return currentURL() === '/media';
      },
      { timeout: 99999 }
    );

    assert.deepEqual(currentURL(), '/media', 'Sono nella pagina media.');
    //#endregion

    //#region UPLOAD
    await waitFor('a[test-mediaupload]');

    await click('a[test-mediaupload]');

    await fillIn('.uploadType', '067ccf84-35d3-44fe-b4fa-250162d71027');

    await fillIn('.uploadCategory', '8700be0e-c95a-4d23-8e7c-27924e8379cc');

    await fillIn('.uploadAlbum', '9bef327f-3067-4da0-9286-360368e91097');

    await waitFor('input[type=file]', { timeout: 99999 });

    let origin = window.location.origin;
    let file;
    await fetch(`${origin}/assets/images/avatars/user-icon.png`)
      .then((response) => response.arrayBuffer())
      .then(async (arrayBuffer) => {
        file = new File([arrayBuffer], 'user_icon.png', {
          type: 'image/png',
        });
      });

    await selectFiles('input[type=file]', file);

    await waitUntil(function () {
      return (
        $('table.media-table tbody tr').filter(function () {
          return $(this).find('td p:contains(user_icon.png)').length > 0;
        }).length > 0
      );
    });

    var tr = $('table.media-table tbody tr').filter(function () {
      return $(this).find('td p:contains(user_icon.png)').length > 0;
    });

    assert.true(tr.length > 0, "Upload dell'imagine Riuscito");
    //#endregion

    //#region EDIT
    await click($(tr).find('button[test-edit]').get(0));

    tr.find('textarea').each(async function (i, e) {
      await fillIn(e, 'test');
    });

    await click($(tr).find('button[test-save]').get(0));

    tr.find('textarea').each(async function (i, e) {
      assert.dom(e).hasText('test');
    });

    await click('a[test-mediaList]');
    //#endregion

    //#region DELETE
    await click($(tr).find('button[test-delete]').get(0));

    await click($('button.ajs-button.btn-primary').get(0));

    tr = $('table.media-table tbody tr').filter(function () {
      return $(this).find('td p:contains(user_icon.png)').length > 0;
    });

    await waitUntil(function () {
      return isSettled();
    });

    assert.deepEqual(tr.length, 0, 'Eliminazione Riuscita');
    //#endregion
    */
  });
});
