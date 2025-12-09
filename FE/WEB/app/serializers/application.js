import JSONAPISerializer from '@ember-data/serializer/json-api'; // REST
//import { decamelize } from '@ember/string';

export default class ApplicationSerializer extends JSONAPISerializer {
  /*normalize(model, hash, prop) {
    //console.log(model, hash, prop);
    if (prop === 'comments') {
      hash.id = hash._id;
      delete hash._id;
    }

    return super.normalize(...arguments);
  }
  */

  // evito che Ember attui la camelizzazione dei nomi degli attributi
  keyForAttribute(attr) {
    return attr;
  }

  // evito che Ember attui la camelizzazione dei nomi delle relazioni
  keyForRelationship(key) {
    return key;
  }

  // converto i GUID degli ID in maiuscolo
  /*normalizeResponse(store, primaryModelClass, payload, id, requestType) {
    if (payload && payload.data && typeof payload.data.id === 'string') {
      payload.data.id = payload.data.id.toLowerCase();
    }
    return super.normalizeResponse(
      store,
      primaryModelClass,
      payload,
      id,
      requestType
    );
  }
  */

  normalizeQueryRecordResponse(
    store,
    primaryModelClass,
    payload,
    id,
    requestType
  ) {
    // Check if the payload is an array
    if (Array.isArray(payload.data)) {
      // Take the first item from the array as the primary data
      payload.data = payload.data[0];

      // Se il server risponde con un array vuoto, restituisco null come valore di payload.data (standard JSON:API)
      if (typeof payload.data === 'undefined') payload.data = null;
    }

    return super.normalizeQueryRecordResponse(
      store,
      primaryModelClass,
      payload,
      id,
      requestType
    );
  }

  /*async query(store, type, query) {
    let url = this.buildURL(type.modelName, null, null, 'query', query);
    let filters = [];

    if (query.filter) {
      for (let key in query.filter) {
        for (let operator in query.filter[key]) {
          let value = query.filter[key][operator];
          filters.push(`filter=${operator}(${key},'${value}')`);
        }
      }
    }

    let otherParams = [];
    for (let key in query) {
      if (key !== 'filter') {
        otherParams.push(`${key}=${query[key]}`);
      }
    }

    if (!isEmpty(filters) || !isEmpty(otherParams)) {
      url += '?' + filters.concat(otherParams).join('&');
    }

    return await this.ajax(url, 'GET');
  }*/
}
