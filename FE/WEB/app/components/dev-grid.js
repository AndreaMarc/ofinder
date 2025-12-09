/* eslint-disable ember/no-jquery */

/**
 * PARAMS
 * @id (string): id della tabella
 * @endpoint (string): endpoint per la chiamata API che strae i dati dal DB
 * @filterAttributes (array): elenco id delle tipologie di ricerca
 * @filterCategories (array): elenco id delle categorie di ricerca
 * @filterState (string): ricerca per stato
 */

/**
 * Riferimenti:
 * data-grid (https://js.devexpress.com/Documentation/Guide/UI_Components/DataGrid/Getting_Started_with_DataGrid/)
 */

import Component from '@glimmer/component';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import $ from 'jquery';
import { inject as service } from '@ember/service';
import config from 'poc-nuovo-fwk/config/environment';

export default class DevGridComponent extends Component {
  @service store;
  @service session;
  @service router;

  serverTokenEndpoint = `${config.apiHost}/${config.namespaceHost}/filtered-startup`;
  payload = {};
  method = 'GET'; // variabile che non servirebbe...!
  @tracked id = '';
  @tracked filterAttributes = [];
  @tracked filterCategories = [];
  @tracked filterState = '';
  @tracked error = '';
  @tracked summaryCategories = '';
  @tracked summaryTypes = '';
  @tracked summaryState = '';

  // costruttore
  constructor(...attributes) {
    super(...attributes);
    // verifico e adatto il parametro id
    this.id =
      typeof this.args.id === 'undefined' ? `id_${Date.now()}` : this.args.id;

    this.saveAttr();
  }

  // adatto e memorizzo gli attributi di interesse
  saveAttr() {
    this.filterAttributes = [];
    if (this.args.filterAttributes && this.args.filterAttributes !== '')
      this.filterAttributes = this.args.filterAttributes.split();

    this.filterCategories = [];
    if (this.args.filterCategories && this.args.filterCategories !== '')
      this.filterCategories = this.args.filterCategories.split();

    this.filterState = this.args.filterState
      ? this.args.filterState.toString()
      : '';
  }

  // creo il payload per il recupero delle startups
  @action
  setPayload() {
    return new Promise((resolve) => {
      this.payload = {};
      this.payload.listAttribute = this.filterAttributes;
      this.payload.listCategory = this.filterCategories;
      this.payload.state = this.filterState;

      // quanto sotto non servirebbe...!
      if (
        this.filterAttributes.length === 0 &&
        this.filterCategories.length === 0 &&
        this.filterState === ''
      )
        this.method = 'GET';
      else this.method = 'POST';
      resolve();
    });
  }

  // azione eseguita quando il componente è pronto (dom ready del componente)
  @action
  start() {
    this.setPayload()
      .then(() => {
        return this.getData();
      })
      .then((res) => {
        $(`#${this.id}`).dxDataGrid({
          dataSource: res,
          keyExpr: 'mc007Id',
          ...this.tableOptions,
        });
      })
      .catch((e) => {
        console.error(e);
        this.error = 'Errore inizializzazione DataGrid!';
      });
  }

  // azione eseguita quando cambiano gli attributi di ricerca
  @action change() {
    this.saveAttr();
    this.start();

    // Compilo il sommario dei filtri di ricerca
    // eslint-disable-next-line prettier/prettier
    this.summaryCategories = '<strong>Filtri categoria:</strong> ' + this.filterSummary(this.filterCategories, 'startup-category', 'mc029Title');
    // eslint-disable-next-line prettier/prettier
    this.summaryTypes = '<strong>Filtri tipologia:</strong> ' + this.filterSummary(this.filterAttributes, 'announcement-type', 'mc024Name');
    // eslint-disable-next-line prettier/prettier
    this.summaryState = '<strong>Filtri stato:</strong> ' + this.filterSummary(this.filterState, 'startup-type', 'mc022Name');
  }

  // recupero i dati dal server
  @action getData() {
    return new Promise((resolve, reject) => {
      this.error = '';
      $.ajax({
        url:
          this.serverTokenEndpoint +
          (this.method === 'GET' ? '?_=' + Date.now() : ''),
        type: this.method, // assurdo ma vero!
        contentType: 'application/json',
        headers: {
          authorization: this.session.get('data.access_token'),
        },
        data: JSON.stringify(this.payload),
      })
        .then((response) => {
          resolve(response.data);
        })
        .catch((e) => {
          this.error = 'Errore nel recupero dei dati delle StatUp!';
          reject(e);
        });
    });
  }

  // restituisce i filtri di ricerca in formato stringa
  filterSummary(arrayElements, modelType, typeElement) {
    let ret = '--<br />';
    if (arrayElements.length > 0) {
      let fa = arrayElements[0].split(',');
      let records = this.store.peekAll(modelType);
      let selecteds = [];
      records.forEach((element) => {
        if (fa.includes(element.id.toString())) {
          selecteds.push(element[typeElement].toLowerCase());
        }
      });
      if (selecteds.length > 0) ret = selecteds.toString() + '<br />';
    }
    return ret;
  }

  // json di configurazione di data-grid
  tableOptions = {
    columns: [
      {
        dataField: 'mc007Id',
        dataType: 'number',
        fixed: true,
        caption: '#ID',
      },
      {
        dataField: 'mc007Logo',
        fixed: true,
        caption: 'Logo',
        cellTemplate: function ($cellElement /*,cellInfo*/) {
          $cellElement.html(`<img src="no_img.png" width="40" alt="logo" />`);
        },
      },
      {
        dataField: 'mc007Title',
        fixed: true,
        caption: 'Rag. Soc.',
        width: 150,
        cellTemplate: function ($cellElement, cellInfo) {
          $cellElement.html(
            `<a href="startup/${cellInfo.data.mc007Id}" target="_self"><strong>${cellInfo.value}</strong></a>`
          );
        },
      },
      {
        dataField: 'categoriesString',
        width: 100,
        caption: 'Categorie',
      },
      {
        dataField: 'mc007Telephone',
        caption: 'Telefono',
      },
      {
        dataField: 'mc007Email',
        caption: 'Email',
      },
      {
        dataField: 'mc007Active',
        dataType: 'boolean',
        caption: 'Stato',
        cellTemplate: function ($cellElement, cellInfo) {
          if (cellInfo.value) {
            $cellElement.html(`<span style="color: green;">Attivo</span>`);
          } else {
            $cellElement.html(`<span style="color: red;">Disattivo</span>`);
          }
        },
      },
      {
        dataField: 'mc007Website',
        caption: 'Sito',
        cellTemplate: function ($cellElement, cellInfo) {
          $cellElement.html(
            `<a href="${cellInfo.value}" target="_blank">Visita</a>`
          );
        },
      },
    ],
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnAutoWidth: true,
    filterRow: { visible: true },
    searchPanel: { visible: true },
    paging: {
      pageSize: 10,
    },
    pager: {
      showPageSizeSelector: true,
      allowedPageSizes: [10, 25, 50],
    },
    summary: {
      totalItems: [
        {
          column: 'mc007Id',
          summaryType: 'count',
        },
      ],
    },
    selection: { mode: 'single' },
  };
}
