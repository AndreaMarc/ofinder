/* eslint-disable prettier/prettier */
/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
//import { htmlSafe } from '@ember/template';
import $ from 'jquery';
import { v4 } from 'ember-uuid';
import { camelize } from '@ember/string';

export default class StandardCoreMediaCategoriesComponent extends Component {
  @service permissions;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  modelName = 'media-category';
  @tracked currentTenant = null;

  @tracked available = true;
  @tracked adaptedRecords = [];

  @tracked records = [];
  @tracked category = null;
  @tracked allCategories = [];

  @tracked newTypology = '';
  @tracked newCategory = '';

  @tracked newRecord = null; // usato in creazione nuovo record
  newValue = {}; // usato nella modifica del record

  constructor(...attributes) {
    super(...attributes);
    this.currentTenant = this.session.get('data.tenantId');

    this.newRecord = this.initializeRecord();
    this.start();

    // filtro di ricerca
    $(document).on('change search', '.find-category', function (e) {
      if (e.type === 'change' && e.target.onsearch !== undefined) {
        return;
      }
      var n,
        tree = $.ui.fancytree.getTree(),
        match = $.trim($(this).val());
      n = tree.filterNodes(match, { mode: 'hide' });
      $('span.matches').text(n ? '(' + n + ' matches)' : '');
    });
  }

  @action
  async start() {
    this.available = true;
    await this.findRecords.perform(); // Avvia la task
  }

  // recupera l'alberatura ed impagina fancyTree
  findRecords = task({ drop: true }, async () => {
    let self = this;
    try {
      this.records = [];
      this.category = null;
      let self = this;

      // recupero l'elenco di tutte le categorie per popolare la select di scelta della
      // parent-category (su inserimento nuova categoria)
      this.allCategories = await this.store.query(this.modelName, {
        filter: `equals(tenantId,'${this.currentTenant}')`,
        sort: `name`,
      });

      let tempRecords = await this.fetch.call(
        'mediaCategories/getFullTree',
        'GET',
        { tenantId: this.currentTenant },
        {},
        true,
        this.session
      );

      tempRecords = tempRecords.data;

      if (tempRecords.length > 0) {
        this.records = tempRecords;
        this.adaptedRecords = self.transformData(this.records); // adatto la risposta per fancyTree
        self.available = true;

        // renderizzo il componente fancyTree
        setTimeout(() => {
          $('#categoriesTree').fancytree({
            source: this.adaptedRecords,
            ...self.fancyTreeOptions,
            extensions: ['table', 'filter', 'edit', 'childcounter'], //
            checkbox: false,
            table: {
              indentation: 20, // indent 20px per node level
              nodeColumnIdx: 0, // render the node title into the N column
            },
            childcounter: {
              deep: true,
              hideZeros: true,
              hideExpanded: false
            },
            edit: {
              triggerStart: ["click", "clickActive", "dblclick", "f2", "mac+enter", "shift+click"],
              save: function(event, data){

                let newVal = data.input.val();
                let type = data.node.type;
                let id = data.node.data.id;
                let color = 'success';
                if(type === 'category') {
                  color = 'warning';
                } else if (type === 'album') {
                  color = 'info';
                }

                if (self.permissions.hasPermissions(['MediaCategory.update'])) {
                  self.dialogs.confirm(
                    '<h6>Modifica</h6>',
                    `<p>Confermi la modifica del nome da '${data.orgTitle}' a '<span class="text-${color} font-weight-bold">${newVal}</span>'?</p>`,
                    () => {
                      self.updateName(id, newVal)
                        .then(() => {
                          data.node.setTitle(`<span class="text-${color} font-weight-bold">${newVal}</span>`);
                        })
                        .catch(() => {
                          data.node.setTitle(data.orgTitle);
                          self.dialogs.toast(
                            'Si è verificato un errore. Riprova!',
                            'error',
                            'bottom-right',
                            4
                          );
                        });
                    },
                    () => {
                      data.node.setTitle(data.orgTitle);
                    },
                    ['Conferma', 'Annulla']
                  );
                } else {
                  data.node.setTitle(data.orgTitle);
                  self.dialogs.toast(
                    `Non hai l'autorizzazione per la modifica del record!`,
                    'warning',
                    'bottom-right',
                    4
                  );
                }

                return true;
              },
              close: function(event, data){
                if( data.save ) {
                  $(data.node.span).addClass("pending");
                }
              }
            },
            renderColumns: function (event, data) {
              var node = data.node,
                $tdList = $(node.tr).find('>td');

              if (node.isFolder()) {
                //$tdList.eq(0).prop('colspan', 5).nextAll().remove();
                $tdList.eq(1).html(node.data.upDownButton);
                $tdList.eq(3).html(node.data.eraseInfo);
                $tdList.eq(2).html(node.data.eraseInfo2);
              }
            },
            activate: function (event) {
              // Prima controlla se il target è uno dei tuoi bottoni
              var target = $(event.originalEvent.target);
              if (
                target.hasClass('moveUpDown') ||
                target.hasClass('recycle') ||
                target.hasClass('recycle2') ||
                target.hasClass('changeRecycle') ||
                target.is('button') ||
                target.is('select')
              ) {
                event.stopImmediatePropagation();
                return false;  // Questo dovrebbe fermare ulteriori gestori di eventi
              }
            },
          });

          // TRIGGER
          // cancellazione di categoria e contenuto
          $('#categoriesTree').off('click', '.recycle');
          $('#categoriesTree').on('click', '.recycle', function (e) {
            e.stopPropagation();
            e.stopImmediatePropagation();
            e.preventDefault();
            let $this = $(this);
            let id = $this.attr('data-id');
            self.delRecord(id);
            return false;
          });

          // cancellazione categoria con spostamento del contenuto
          $('#categoriesTree').off('click', '.recycle2');
          $('#categoriesTree').on('click', '.recycle2', function (e) {
            e.stopPropagation();
            e.stopImmediatePropagation();
            e.preventDefault();
            let $this = $(this);
            let id = $this.attr('data-id');
            let alternative = $(`.changeRecycle[data-id="${id}"] option:selected`).val();
            if (alternative === '') {
              self.dialogs.toast('Scegli la destinazione del contenuto!', 'error', 'bottom-right', 4);
              return false;
            }
            self.delRecord(id, alternative);
            return false;
          });

          $('#categoriesTree').off('click', '.changeRecycle');
          $('#categoriesTree').on('click', '.changeRecycle', function (e) {
            e.stopPropagation();
            e.stopImmediatePropagation();
            e.preventDefault();
            return false;
          });

          // spostamento su/giù del template
          $('#categoriesTree').off('click', '.moveUpDown');
          $('#categoriesTree').on('click', '.moveUpDown', function (e) {
            e.stopPropagation();
            e.stopImmediatePropagation();
            e.preventDefault();
            let $this = $(this);
            let direction = $this.attr('data-direction'); // up/down
            let id = $this.attr('data-id');
            let order = parseInt($this.attr('data-order')); // posizione attuale
            let nextId = $this.attr('data-next-id'); // id del record da spostare
            let nextOrder = parseInt($this.attr('data-next-order')); // posizione del record successivo/precedente

            self.dialogs.confirm(
              '<h6>Modifica ordine</h6>',
              `<p>Confermi lo spostamento del record selezionato?</p>`,
              () => {
                self.changeOrder(direction, id, order, nextId, nextOrder);
              },
              null,
              ['Conferma', 'Annulla']
            );
          });
        }, 80);
      }
    } catch (e) {
      self.available = false;
    }
  });

  // modifica il nome della categoria
  async updateName(id, name) {
    try {
      let r = await this.store.findRecord('media-category', id);
      r.name = name;
      await r.save();
    } catch (e) {
      console.error(e);
      throw new Error(e);
    }
  }

  get recordsLength() {
    return this.records.length;
  }

  // estrae lista delle tipologie
  get getTypology() {
    return this.allCategories.filter(item => item.type === 'typology');
  }

  get getCategory() {
    return this.allCategories.filter(item => item.parentMediaCategory === this.newTypology);
  }

  @action
  setTypology(event) {
    this.newTypology = event.target.value.trim();
  }

  @action
  setCategory(event) {
    this.newCategory = event.target.value.trim();
  }
  // cattura il valore inserito dall'utente per la creazione/modifica di un record
  @action
  storeNewValue(field, isUpdate, event) {
    let value = event.target.value;

    if (isUpdate) {
      this.newValue[field] = value;
    } else {
      this.newRecord[field] = value;
    }
  }

  // crea un nuovo record
  saveNewVoice = task({ drop: true }, async (typology) => {
    if (!this.newRecord || !this.newRecord.name || this.newRecord.name === '') {
      this.dialogs.toast('Il nome è obbligatorio!', 'error', 'bottom-right', 4);
      return false;
    }

    let obj = {
      id: v4(),
      name: this.newRecord.name.trim(),
      code: camelize(this.newRecord.name.trim()), // v4(),
      description: '',
      type: typology,
      tenantId: this.currentTenant,
      erasable: true,
      parentMediaCategory: '',
      order: 0,
    }

    let query = { sort: '-order' }

    if (typology === 'typology') {
       obj.parentMediaCategory = '';
       query.filter = `and(equals(type,'typology'),equals(parentMediaCategory,''))`;
    } else if (typology === 'category') {
      if (this.newTypology === '') {
        this.dialogs.toast('Scegli la Tipologia!', 'error', 'bottom-right', 4);
        return false;
      }
      obj.parentMediaCategory = this.newTypology;
      query.filter = `and(equals(type,'category'),equals(parentMediaCategory,'${this.newTypology}'))`;
    } else if (typology === 'album') {
      if (this.newTypology === '') {
        this.dialogs.toast('Scegli la Tipologia!', 'error', 'bottom-right', 4);
        return false;
      }
      if (this.newCategory === '') {
        this.dialogs.toast('Scegli la Categoria!', 'error', 'bottom-right', 4);
        return false;
      }
      obj.parentMediaCategory = this.newCategory;
      query.filter = `and(equals(type,'album'),equals(parentMediaCategory,'${this.newCategory}'))`;
    }

    // estraggo l'ordine massimo presente nel DB
    let max = await this.store.query(this.modelName, query);
    if (max && max.length > 0) {
      obj.order = parseInt(max[0].order) + 1;
    } else obj.order = 0;

    try {
      let newR = this.store.createRecord(this.modelName, obj);

      await newR.save();
      this.findRecords.perform();

      // resetto l'oggetto newRecord per svuotare i campi input collegati
      this.newRecord = this.initializeRecord();
      this.newTypology = '';
      this.newCategory = '';
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore, riprovare!',
        'error',
        'bottom-right',
        4
      );
    }
  });

  fancyTreeOptions = {
    activeVisible: true, // Make sure, active nodes are visible (expanded)
    aria: false, // Enable WAI-ARIA support
    autoActivate: true, // Automatically activate a node when it is focused using keyboard
    autoCollapse: true, // Automatically collapse all siblings, when a node is expanded
    autoScroll: true, // Automatically scroll nodes into visible area
    clickFolderMode: 3, // 1:activate, 2:expand, 3:activate and expand, 4:activate (dblclick expands)
    checkbox: false, // Show check boxes
    checkboxAutoHide: undefined, // Display check boxes on hover only
    debugLevel: 0, // 0:quiet, 1:errors, 2:warnings, 3:infos, 4:debug
    disabled: false, // Disable control
    focusOnSelect: false, // Set focus when node is checked by a mouse click
    escapeTitles: false, // Escape `node.title` content for display
    generateIds: false, // Generate id attributes like <span id='fancytree-id-KEY'>
    idPrefix: 'ft_', // Used to generate node idÂ´s like <span id='fancytree-id-<key>'>
    icon: true, // Display node icons
    keyboard: true, // Support keyboard navigation
    keyPathSeparator: '/', // Used by node.getKeyPath() and tree.loadKeyPath()
    minExpandLevel: 1, // 1: root node is not collapsible
    quicksearch: true, // Navigate to next node by typing the first letters
    rtl: false, // Enable RTL (right-to-left) mode
    selectMode: 1, // 1:single, 2:multi, 3:multi-hier
    tabindex: '0', // Whole tree behaves as one single control
    titlesTabbable: false, // Node titles can receive keyboard focus
    tooltip: false, // Use title as tooltip (also a callback could be specified)
    extensions: ['filter'],
    filter: {
      autoExpand: true,
    },
  };

  // adatto la risposta dell'API delle categorie annidate nella forma necessaria per il plugin fancyTree
  transformData(data) {
    let res = [],
    expanded = false;

    data.forEach((item, index) => {
      let title = '',
        eraseInfo = '',
        eraseInfo2 = '',
        newCategoryId = 0,
        upDownButton = '',
        changeRecycle = '';
      if (['typology', 'category', 'album'].includes(item.elementType)) {
        // operazioni tipologie, categorie ed album
        if (item.elementType === 'typology') {
          title = `<span class="text-success font-weight-bold">${item.name}</span>`;
        } else if (item.elementType === 'category') {
          title = `<span class="text-warning font-weight-bold">${item.name}</span>`;
        } else if (item.elementType === 'album') {
          title = `<span class="text-info font-weight-bold">${item.name}</span>`;
        }

        newCategoryId = item.id;

        if (!item.erasable) {
          eraseInfo = `<div class="badge bg-warning mr-1">Non cancellabile</div>`;
        } else {
          //
          if (this.permissions.hasPermissions(['MediaCategory.delete'])) {
            eraseInfo = `<button class="recycle btn btn-sm btn-outline-danger btn-shine-hover mr-1" data-id="${item.id}">ELIMINA (anche il contenuto)</button>`
            changeRecycle = this.getOptions(data, item.elementType, item.id);
            eraseInfo2 = `
              <div class="input-group">
                <select data-id="${item.id}" class="changeRecycle form-control form-control-sm">
                  <option value="">--</option>
                  ${changeRecycle}
                </select>
                <div class="input-group-append">
                  <button class="recycle2 btn btn-sm btn-outline-warning btn-shine-hover mr-1" data-id="${item.id}">ELIMINA e sposta il contenuto</button>
                </div>
              </div>`;
          }
        }

        // pulsanti spostamento up/down
        if (this.permissions.hasPermissions(['MediaCategory.update'])) {
          let leftClass = 'ml-1', btnColor = 'success';
          if (item.elementType === 'category') {
            leftClass = 'ml-3';
            btnColor = 'warning';
          } else if (item.elementType === 'album') {
            leftClass = 'ml-5';
            btnColor = 'info';
          }
          upDownButton = `<div class="${leftClass}">`;
          if (index === 0) {
            if (data[index + 1])
              upDownButton += `
                <button class="moveUpDown btn btn-sm btn-${btnColor} btn-icon-only" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index + 1].id}" data-next-order="${data[index + 1].order}" data-direction="down">
                  <i class="moveUpDown fa fa-angle-down" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index + 1].id}" data-next-order="${data[index + 1].order}" data-direction="down"></i>
                </button>`;
          } else if (index < data.length - 1) {
            upDownButton += `
              <button class="moveUpDown btn btn-sm btn-${btnColor} btn-icon-only" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index - 1].id}" data-next-order="${data[index - 1].order}" data-direction="up">
                <i class="moveUpDown fa fa-angle-up" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index - 1].id}" data-next-order="${data[index - 1].order}" data-direction="up"></i>
              </button>
              <button class="moveUpDown btn btn-sm btn-${btnColor} btn-icon-only" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index + 1].id}" data-next-order="${data[index + 1].order}" data-direction="down">
                <i class="moveUpDown fa fa-angle-down" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index + 1].id}" data-next-order="${data[index + 1].order}" data-direction="down"></i>
              </button>`;
          } else {
            upDownButton += `
              <button class="moveUpDown btn btn-sm btn-${btnColor} btn-icon-only" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index - 1].id}" data-next-order="${data[index - 1].order}" data-direction="up">
                <i class="moveUpDown fa fa-angle-up" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index - 1].id}" data-next-order="${data[index - 1].order}" data-direction="up"></i>
              </button>`;
          }
          upDownButton += '</div>';
        }
      }

      let thisExpanded = false;
      if (item.child.length > 0 && !expanded) {
        expanded = true;
        thisExpanded = true;
      }

      res.push({
        title: title,
        id: item.id,
        expanded:  thisExpanded,
        folder: ['typology', 'category', 'album'].includes(item.elementType),
        children: this.transformData(item.child, item.id),
        type: item.elementType,
        eraseInfo: eraseInfo,
        eraseInfo2: eraseInfo2,
        categoryId: newCategoryId,
        upDownButton: upDownButton,
      });

    });
    return res;
  }

  // ricava le categorie per le select di cancellazione con spostamento
  getOptions(data, type, id) {
    let res = '';
    let filtered = this.allCategories.filter((item) => {
      return item.type === type && item.id !== id
    });
    filtered.forEach(element => {
      res += `<option value="${element.id}">${element.name}</option>`;
    });
    return res;
  }

  // eliminazione record
  @action
  delRecord(id, alternative) {
    let self = this;
    this.dialogs.confirm(
      '<h6 class="text-danger">Cancellazione</h6>',
      `<p class="text-danger">Azione irreversibile. Confermi l'operazione?</p>`,
      () => {
        self.delVoiceConfirmed.perform(id, alternative);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  delVoiceConfirmed = task({ drop: true }, async (id, alternative) => {
    try {
      let self = this;
      let endpoint = `mediaCategories/${id}?alternativeCategory=`;
      if (alternative && alternative !== '') {
        endpoint += alternative;
      }
      this.fetch
        .call(
          endpoint,
          'DELETE',
          null,
          {},
          true,
          self.session
        )
        .then(() => {
          self.findRecords.perform();
        })
        .catch((e) => {
          throw new Error(e);
        });
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        5
      );
    }
  });

  // spostamento del record
  async changeOrder(direction, id, order, nextId, nextOrder) {
    try {
      if (order === nextOrder) {
        if (direction === 'down') {
          order++;
        } else nextOrder++;
      }
      let currentRecord = await this.store.findRecord('media-category', id);
      let nextRecord = await this.store.findRecord('media-category', nextId);
      currentRecord.order = nextOrder;
      nextRecord.order = order;
      await currentRecord.save();
      await nextRecord.save();
      this.findRecords.perform();
      this.dialogs.toast('Operazione riuscita!', 'success', 'bottom-right', 4);
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprovare!',
        'error',
        'bottom-right',
        4
      );
    }
  }

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord(this.modelName); // Crea una nuova istanza del modello (ad esempio, 'user')
    // Crea un semplice oggetto JavaScript con tutte le proprietà del modello
    let modelData = {};
    modelInstance.eachAttribute((key) => {
      modelData[key] = modelInstance.get(key);
    });

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);

    // Rimuovi il listener quando il componente viene distrutto
    $(document).off('change search', '.find-category');
    $('#categoriesTree').off('click', '.recycle');
    $('#categoriesTree').off('click', '.recycle2');
    $('#categoriesTree').off('click', '.changeRecycle');
    $('#categoriesTree').off('click', '.moveUpDown');
  }

}
