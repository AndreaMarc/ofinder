import Service from '@ember/service';

export default class JsonApiService extends Service {
  /**
   * METODO PER LA COMPOSIZIONE ATOMATICA DELLE QUERY IN FORMATO JSON:API
   * NOTA: lo sviluppo attale non è completo né esaustivo. Allo stato attuale è idoneo per buona parte delle applicazioni.
   *
   * @param {object} params l'oggetto contenente i parametri di ricerca. Tale oggetto è
   * definito come mostrato sotto. Ogni voce è opzionale.
   *
   * {
      page: {},
      filter: [],
      sort: [],
      fields: [],
      include: [],
   * }
   *
   * DETTAGLI DELLE SINGOLE VOCI
   *
   * @param {object} page : è un oggetto contenente due voci:
   *    @param {number} size : numero di record da estrarre,
   *    @param {number} number : numero di pagina (paginazione)
   *
   * @param {array} filter : array di oggetti. Ciascun oggetto è un filtro di ricerca ed ha questa struttura
   *    @param {object}
   *        @param {string} function : funzione di filtraggio (es: equals), secondo lo standard JSON:API, come definite qui: https://www.jsonapi.net/usage/reading/filtering.html
   *        @param {string} column : nome dell'attributo su cui applicare la funzione di filtraggio (es: id)
   *        @param {string} value : valore del filtro. Un valore "null" verrà convertito in un valore nullo del campo di ricerca.
   *        @param {string} value2 : facoltativo, defalt null. Usato per le function che prevedono de parametri (and/or)
   *        @param {bool} negation : facoltativo, default false. Usato per negare la condizione (not)
   *
   *        NOTA:
   *          per funzioni and/or : se sono previsti 2 soli valori in and/or, il parametro column è ignorato e dentro value e value2 inserire le condizioni (eventualmente contenenti le colonne)
   *                                se sono presenti più elementi, vedi il caso funzione 'extra'
   *          per funzione any    : in value inserire i valori separati da virgole e contenuti tra apici singoli. Es: 'a','b','c'
   *          per funzione has    : il parametro column viene ignorato. In value inserire l'entità cercata.
   *          funzione extra      : quando le casistiche qui previste non soddisfano le esigenze, porre function a null e in value scrivere la condizione formattata secondo lo standard richiesto dal BE. il campo column è ignorato.
   *
   * Un esempio:
   * filter: [
   *    {
   *      function: 'contains',
   *      column: 'tag',
   *      value: 'valoreX',
   *    }
   * ]
   *
   * @param {string} sort : ordinamento. Come definito dallo standard JSON:API, dettagli qui: https://www.jsonapi.net/usage/reading/sorting.html
   *
   * @param {string} fields : TODO
   * @param {string} include : TODO
   *
   *
   *
   */

  queryBuilder(params) {
    // imposto la query di ricerca
    if (params.filter && params.filter.length > 0) {
      if (params.filter.length === 1) {
        let element = params.filter[0];
        params.filter = this._subFilter(element); // `${element.function}(${element.column},'${element.value}')`;
      } else {
        let subFilterStrings = params.filter.map((element) => {
          return this._subFilter(element); // `${element.function}(${element.column},'${element.value}')`;
        });
        let filterStrings = `and(${subFilterStrings.join()})`;

        params.filter = filterStrings;
      }
    }

    return params;
  }

  _subFilter(element) {
    let filterStrings = '';

    if (!element.function) {
      filterStrings = `${element.value}`;
    } else if (['and', 'or'].includes(element.function)) {
      filterStrings = `${element.function}(${element.value},'${element.value2}')`;
    } else if (element.function === 'any') {
      filterStrings = `${element.function}(${element.column},${element.value})`;
    } else if (element.function === 'has') {
      filterStrings = `${element.function}(${element.value})`;
    } else if (element.value === 'null') {
      filterStrings = `${element.function}(${element.column},null)`;
    } else {
      filterStrings = `${element.function}(${element.column},'${element.value}')`;
    }

    if (element.negation) {
      filterStrings = `not(${filterStrings})`;
    }
    return filterStrings;
  }
}
