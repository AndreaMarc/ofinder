/**
 * MODO CORRETTO PER EFFETTUARE CHIAMATE FETCH
 * (Quando possibile, usare ember-data anzichè chiamate fetch)
 */

@service fetch;
// ...
this.fetch
  .call('entity/getAll', 'GET', null, {}, true, this.session)
  .then((res) => {
    // ...
  })
  .catch((e) => {
    // ...
  });