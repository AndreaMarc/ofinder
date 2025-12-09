import offLineConfig from 'poc-nuovo-fwk/_customs/offLineConfig';

// Scarica dal server le impostazioni di funzionamento del sito web
export async function setupGetData(store, custom) {
  // se custom = false, cerco nell'entità setup altrimenti in entità custom-setup
  return new Promise((resolve) => {
    console.log('SETUP: scarico impostazioni di funzionamento');

    let filter = `equals(environment,'${
      typeof window.cordova !== 'undefined' ? 'app' : 'web'
    }')`;

    let endpoint = custom ? 'custom-setup' : 'setup';

    store
      .queryRecord(endpoint, {
        filter: filter,
      })
      .then(function (data) {
        if (data) {
          //console.log('SETUP: ', data);
          console.log('SETUP: completato');
          resolve(data);
        } else {
          throw new Error('data not found');
        }
      })
      .catch((e) => {
        console.error(e);

        if (!custom) {
          let attributes = {
            environment: typeof window.cordova !== 'undefined' ? 'app' : 'web',
          };
          attributes = { ...attributes, ...offLineConfig };

          // eslint-disable-next-line no-unused-vars
          let stp = store.push({
            data: {
              id: 1,
              type: 'setup',
              attributes,
            },
          });

          console.log('SETUP: completato');
          resolve(stp);
        } else {
          resolve();
        }
      });
  });
}
