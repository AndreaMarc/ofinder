/* eslint-disable prettier/prettier */
/**
 * COMPONENTE CHE ELENCA I TEMPLATE E NE CONSENTE LA MODIFICA
 *
 * Di default vengono mostrati i template di tutte le categorie associate al tenant corrente.
 * Puoi fornire un elenco di ID delle categorie da mostrare e in tal caso verranno visualizzate solo queste.
 * Puoi contemporaneamente fornire un elenco di ID di categorie da escludere.
 *
 * @param {string} title : titolo della lista dei template. Se omesso viene mostrato "GESTIONE TEMPLATE"
 * @param {string} @wantedCategory :  facoltativo. Stringa contenente i Code delle categorie di cui
 *                                    mostrare i template, separati da virgole. Se omesso, vengono
 *                                    mostrati i template di tutte le categorie.
 * @param {string} @excludedCategory : facoltativo. Stringa contenente gli Id/Guid delle categorie di
 *                                    cui non mostrare i template, separati da virgole.
 * @param {string} @showObject : facoltativo. Indica se mostrare il campo 'Oggetto dell'email'. Stringa vuota per non mostrare. Dafault false.
 * @param {string} @showTags : facoltativo. Indica se mostrare il campo 'Tags'. Stringa vuota per non mostrare. Dafault false.
 * ESEMPIO DI UTILIZZO
 * <Standard::Core::TemplatesCrud @wantedCategory='userGuide,userGuide' @excludedCategory='template'/>
 */

import Component from '@glimmer/component';
/* eslint-disable ember/no-jquery */
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { htmlSafe } from '@ember/template';
import $ from 'jquery';

export default class StandardCoreTemplatesCrudComponent extends Component {
  @service('siteSetup') stp;
  @service permissions;
  @service translation;
  @service jsUtility;
  @service session;
  @service dialogs;
  @service router;
  @service store;
  @service fetch;

  @tracked title = 'GESTIONE TEMPLATE';
  modelName = 'template';
  @tracked currentTenant = null;
  filteredCategories = null;

  @tracked available = 'waiting';
  @tracked textError = '';
  @tracked records = [];
  @tracked erasableCategory = '';
  @tracked category = null;
  @tracked realCategories = [];
  @tracked realCategoriesAdapted = [];
  @tracked filterText = '';
  @tracked reassignmentCategory = '0';

  @tracked showObject = false;
  @tracked showTags = false;
  @tracked showFeaturedImage = false;
  @tracked showRecycle = false;

  newValue = {}; // usato nella modifica del record
  @tracked newRecord = null; // usato in creazione nuovo record

  constructor(...attributes) {
    super(...attributes);
    this.currentTenant = this.session.get('data.tenantId');

    this.showObject =
      typeof this.args.showObject !== 'undefined'
        ? !!this.args.showObject
        : false;
    this.showTags =
      typeof this.args.showTags !== 'undefined' ? !!this.args.showTags : false;
    this.showFeaturedImage =
      typeof this.args.showFeaturedImage !== 'undefined'
        ? !!this.args.showFeaturedImage
        : false;

    this.newRecord = this.initializeRecord();

    if (this.args.title && this.args.title !== '') {
      this.title = this.args.title;
    }

    // filtro di ricerca
    $(document).on('change search', '.find-template', function (e) {
      if (e.type === 'change' && e.target.onsearch !== undefined) {
        return;
      }
      var n,
        tree = $.ui.fancytree.getTree(),
        match = $.trim($(this).val());
      n = tree.filterNodes(match, { mode: 'hide' });
      $('span.matches').text(n ? '(' + n + ' matches)' : '');
    });

    this.start();
  }

  @action
  async start() {
    this.available = 'waiting';

    // estraggo elenco categorie
    let categories = [];
    if (this.permissions.hasPermissions(['canSeeAllTenants'])) {
      // ho il permesso di vedere tutte le categorie
      categories = await this.store.findAll('category');
    } else {
      // vedo solo le categorie dei tenant a cui sono associato
      let tenantIds = [];
      let userTenants = this.session.get('data.associatedTenants');
      userTenants.forEach((element) => {
        tenantIds.push(`'${element.tenantId}'`);
      });
      categories = await this.store.query('category', {
        filter: `any(tenantId,${tenantIds.join()})`,
      });
    }

    // Convertiamo l'array di modelli Ember Data in un array di Code
    let categoryGuid = categories.map((model) => model.get('code'));

    // fin qui ho l'elenco delle categorie visibili dall'utente
    // se ha fornito un elenco di categorie da mostrare, verifico che siano comprese tra quelle visibili.
    if (this.args.wantedCategory && this.args.wantedCategory !== '') {
      let regex = this.jsUtility.regex('guidList');
      if (regex.test(this.args.wantedCategory)) {
        // trasformo 1,2,... in '1','2',...
        let t = this.args.wantedCategory.split(',');
        categoryGuid = t.filter((item) => categoryGuid.includes(item));
      } else {
        this.available = 'wrong';
        this.textError = htmlSafe(
          `Errore nella configurazione del componente Templates-Crud (verifica la struttura del parametro <em>wantedCategory</em>).`
        );
        return false;
      }
    }

    // se ha fornito un elenco di categorie da escludere, le elimino dall'elenco delle categorie
    if (this.args.excludedCategory && this.args.excludedCategory !== '') {
      let regex = this.jsUtility.regex('guidList');
      if (regex.test(this.args.excludedCategory)) {
        let t = this.args.excludedCategory.split(',');
        categoryGuid = categoryGuid.filter((item) => !t.includes(item));
      } else {
        this.available = 'wrong';
        this.textError = htmlSafe(
          `Errore nella configurazione del componente Templates-Crud (verifica la struttura del parametro <em>excludedCategory</em>).`
        );
        return false;
      }
    }

    let t = categoryGuid.map((element) => `'${element}'`);
    this.filteredCategories = t.join();

    await this.findRecords.perform(); // Avvia la task
  }

  findRecords = task({ drop: true }, async () => {
    let self = this;
    try {
      this.realCategories = [];
      let newWantedCategory = '';

      this.realCategories = await this.store.query('category', {
        filter: `and(equals(tenantId,'${this.currentTenant}'),any(code,${this.filteredCategories}))`,
        sort: `name`,
      });

      if (this.realCategories.length === 0) {
        this.available = 'wrong';
        this.textError = htmlSafe(
          //`Nessuna Categoria corrisponde agli ID forniti negli attributi <em>wantedCategory</em> e <em>excludedCategory</em> del componente Templates-Crud.`
          `Non sono presenti Categorie!`
        );
        return false;
      }

      // ricostruisco l'elenco this.wantedCategory in base alle categorie realmente esistenti
      newWantedCategory = [];
      this.realCategories.forEach((element) => {
        newWantedCategory.push(element.id);
      });
      newWantedCategory = newWantedCategory.join();

      // estraggo i dati (categorie e template) dal DB
      let tempRecords = await this.fetch.call(
        'categories/getFullTree',
        'GET',
        {
          tenantId: this.currentTenant,
          categoryIds: newWantedCategory,
          language: self.stp.siteSetup.defaultLanguage,
        },
        {},
        true,
        this.session
      );
      this.records = tempRecords.data;

      let adaptedRecords = this.transformData(this.records); // adatto la risposta per fancyTree
      this.realCategoriesAdapted = this.transformForSelect(this.records, 0);
      this.available = 'available';

      // renderizzo il componente fancyTree
      //let self = this;
      setTimeout(() => {
        $('#templateTree').fancytree({
          source: adaptedRecords,
          ...self.fancyTreeOptions,
          extensions: ['table', 'filter'],
          checkbox: false,
          table: {
            indentation: 20, // indent 20px per node level
            nodeColumnIdx: 0, // render the node title into the N column
          },
          renderColumns: function (event, data) {
            var node = data.node,
              $tdList = $(node.tr).find('>td');

            if (node.isFolder()) {
              $tdList.eq(0).prop('colspan', 5).nextAll().remove();
            } else {
              $tdList.eq(1).html(node.data.activeState);
              $tdList.eq(2).html(node.data.eraseInfo);

              if (!node.data.erased) {
                if ($tdList.eq(3).find('select').length === 0) {
                  // inserisco la select nella 4° colonna (solo se la select non è già stata aggiunta).
                  var $select = $(
                    `<select class="changeCategory form-control form-control-sm" data-item-id="${node.data.id}">`
                  ).append($('<option>', { value: '', text: '--' }));
                  let other = self.otherCategory(node.data.categoryId);
                  other.forEach((element) => {
                    $select.append(
                      $('<option>', {
                        value: element.id,
                        text: self.htmlToText(element.name),
                      })
                    );
                  });

                  $tdList.eq(3).append($select);
                }
              }


              $tdList.eq(4).html(node.data.upDownButton);
            }
          },
          activate: function (event, data) {
            // evito l'attivazione se viene cliccata la select o i bottoni
            var target = $(event.originalEvent.target);
            if (
              target.hasClass('changeCategory') ||
              target.hasClass('moveUpDown') ||
              target.hasClass('recycle')
            ) {
              return;
            }

            if (
              $(event.originalEvent.target).is('select') ||
              $(event.originalEvent.target).is('button')
            ) {
              return;
            }

            var node = data.node;

            if (!$.isEmptyObject(node.data)) {
              if (node.type === 'category') {
                return false;
              } else {
                self.openDetail(node.data.id);
              }
            } else {
              self.dialogs.toast(
                'Si è verificato un errore. Riprova!',
                'error',
                'bottom-right',
                3
              );
            }
          },
        });

        // spostamento su/giù del template
        $(document).off('click', '.moveUpDown');
        $(document).on('click', '.moveUpDown', function (e) {
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

        // cambio categoria
        $(document).off('change click', '.changeCategory');
        $(document).on('change click', '.changeCategory', function (e) {
          e.stopPropagation();
          e.stopImmediatePropagation();
          e.preventDefault();
          let $this = $(this);
          let newCategoryId = $this.val();
          if (newCategoryId === '') return false;
          let recordId = $this.attr('data-item-id');

          self.dialogs.confirm(
            '<h6>Spostamento in altra categoria</h6>',
            `<p>Confermi lo spostamento?</p>`,
            () => {
              self.changeCategory(recordId, newCategoryId);
            },
            null,
            ['Conferma', 'Annulla']
          );
        });

        // operazioni su elementi cestinati
        $(document).off('click', '.recycle');
        $(document).on('click', '.recycle', function (e) {
          e.stopPropagation();
          e.stopImmediatePropagation();
          e.preventDefault();
          let $this = $(this);
          let id = $this.attr('data-id');
          let operation = $this.attr('data-operation');

          if (operation === 'restore') {
            self.dialogs.confirm(
              '<h6>Ripristino</h6>',
              `<p>Confermi il ripristino?</p>`,
              () => {
                self.restoreTemplate(id);
              },
              null,
              ['Conferma', 'Annulla']
            );
          } else {
            self.dialogs.confirm(
              '<h6 class="text-danger">Cancellazione definitiva</h6>',
              `<p class="text-danger">Il Template verrà cancellato in modo permanente. Azione irreversibile.<br />Confermi?</p>`,
              () => {
                self.destroyTemplate(id);
              },
              null,
              ['Conferma', 'Annulla']
            );
          }


        });
      }, 80);
    } catch (e) {
      self.available = 'unavailable';
    }
  });

  htmlToText(html) {
    var txt = document.createElement('textarea');
    txt.innerHTML = html;
    return txt.value;
  }

  // adatto la risposta dell'API dei template nella forma necessaria per il plugin fancyTree
  transformData(data, categoryId) {
    let res = [],
    expanded = false;

    data.forEach((item, index) => {
      let title = '',
        activeState = '',
        eraseInfo = '',
        newCategoryId = 0,
        upDownButton = '';
      if (item.elementType === 'category') {
        // operazioni per categorie
        title = `<span class="text-success font-weight-bold">${item.name}</span>`;
        newCategoryId = item.id;
      } else {
        // operazioni per template
        newCategoryId = categoryId ? categoryId : item.id;
        title = `<span class="title-box">${item.name}</span>
          <p class="description-box">${item.description}</p>`;
        if (item.erased) {
          activeState = `<div class="ml-2 badge bg-danger">Cancellato</div>`;
        } else {
          if (item.active) {
            activeState = `<div class="ml-2 badge bg-success">Attivo</div>`;
          } else {
            activeState = `<div class="ml-2 badge bg-danger">Non attivo</div>`;
          }
        }

        if (!item.erasable) {
          eraseInfo = `<div class="ml-2 badge bg-warning">Non cancellabile</div>`;
        }

        if (!item.erased) {
          // pulsanti spostamento up/down
          if (index === 0) {
            if (data[index + 1])
              upDownButton += `
                <button class="moveUpDown btn btn-sm btn-success btn-icon-only" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index + 1].id}" data-next-order="${data[index + 1].order}" data-direction="down">
                  <i class="moveUpDown fa fa-angle-down" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index + 1].id}" data-next-order="${data[index + 1].order}" data-direction="down"></i>
                </button>`;
          } else if (index < data.length - 1) {
            upDownButton += `
              <button class="moveUpDown btn btn-sm btn-success btn-icon-only" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index - 1].id}" data-next-order="${data[index - 1].order}" data-direction="up">
                <i class="moveUpDown fa fa-angle-up" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index - 1].id}" data-next-order="${data[index - 1].order}" data-direction="up"></i>
              </button>
              <button class="moveUpDown btn btn-sm btn-success btn-icon-only" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index + 1].id}" data-next-order="${data[index + 1].order}" data-direction="down">
                <i class="moveUpDown fa fa-angle-down" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index + 1].id}" data-next-order="${data[index + 1].order}" data-direction="down"></i>
              </button>`;
          } else {
            upDownButton += `
              <button class="moveUpDown btn btn-sm btn-success btn-icon-only" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index - 1].id}" data-next-order="${data[index - 1].order}" data-direction="up">
                <i class="moveUpDown fa fa-angle-up" data-order="${item.order}" data-id="${item.id}" data-next-id="${data[index - 1].id}" data-next-order="${data[index - 1].order}" data-direction="up"></i>
              </button>`;
          }
        } else {
          // pulsante di cancellazione definitiva
          upDownButton += `
              <button class="recycle btn btn-sm btn-warning btn-icon-only" data-id="${item.id}" data-operation="restore">
                <i class="recycle pe-7s-repeat" data-id="${item.id}" data-operation="restore"></i>
              </button>
              <button class="recycle btn btn-sm btn-danger btn-icon-only" data-id="${item.id}" data-operation="deletePermenently">
                <i class="recycle fa fa-trash" data-id="${item.id}" data-operation="deletePermenently"></i>
              </button>`;
        }

      }

      let thisExpanded = false;
      if (item.child.length > 0 && !expanded) {
        expanded = true;
        thisExpanded = true;
      }

      if (item.elementType === 'category' || item.elementType !== 'category' && (item.erased === true && this.showRecycle || item.erased !== true && !this.showRecycle)) {
        res.push({
          title: title,
          id: item.id,
          expanded:  thisExpanded,
          folder: item.elementType === 'category',
          children: this.transformData(item.child, item.id),
          type: item.elementType,
          activeState: activeState,
          eraseInfo: eraseInfo,
          categoryId: newCategoryId,
          upDownButton: upDownButton,
          erased: item.erased,
        });
      }

    });
    return res;
  }

  transformForSelect(data, level) {
    let res = [];
    level = typeof level === 'undefined' ? 0 : parseInt(level);
    data = data.filter((item) => item.elementType === 'category');

    let name = '';
    data.forEach((element) => {
      name = '';
      for (let i = 0; i < level; i++) {
        name += `&nbsp;`;
      }
      name += level === 0 ? `${element.name}` : `- ${element.name}`;
      res.push({ id: element.id, name: htmlSafe(name) });

      if (element.child.length > 0) {
        let sub = this.transformForSelect(element.child, level + 1);
        if (sub) res = [...res, ...sub];
      }
    });
    return res;
  }

  otherCategory(currentCategory) {
    return this.realCategoriesAdapted.filter((item) => {
      return item.id !== currentCategory;
    });
  }

  async changeCategory(recordId, newCategoryId) {
    let template = await this.store.findRecord('template', recordId);
    template.categoryId = newCategoryId;
    await template.save();
    this.findRecords.perform();
  }

  async changeOrder(direction, id, order, nextId, nextOrder) {
    try {
      if (order === nextOrder) {
        if (direction === 'down') {
          order++;
        } else nextOrder++;
      }
      let currentRecord = await this.store.findRecord('template', id);
      let nextRecord = await this.store.findRecord('template', nextId);
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

  async restoreTemplate(id) {
    try {
      let template = await this.store.findRecord('template', id);
      template.erased = false;
      await template.save();
      this.findRecords.perform();
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

  async destroyTemplate(id) {
    try {
      let template = await this.store.findRecord('template', id);
      await template.destroyRecord();
      this.findRecords.perform();
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

  get recordsLength() {
    return this.records.length;
  }

  // cattura il valore inserito dall'utente per la creazione/modifica di un record
  @action
  async storeNewValue(field, event) {
    let value = event.target.value;
    this.newRecord[field] = value;

    if (field === 'categoryId') {
      this.newRecord.order = 0;
      // estraggo il massimo valore di order dei template in questa categoria
      let max = await this.store.query('template', {
        sort: '-order',
        'page[limit]': 1,
        filter: `equals(categoryId,'${value}')`,
      });
      if (max && max.length > 0) this.newRecord.order = parseInt(max[0].order) + 1;
    }
  }

  // crea un nuovo record
  saveNewVoice = task({ drop: true }, async () => {
    if (!this.newRecord || !this.newRecord.name || this.newRecord.name === '') {
      this.dialogs.toast('Il nome è obbligatorio!', 'error', 'bottom-right', 4);
      return false;
    }
    if (
      !this.newRecord ||
      !this.newRecord.categoryId ||
      this.newRecord.categoryId === '0'
    ) {
      this.dialogs.toast(
        'La categoria è obbligatoria!',
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    try {
      let newR = this.store.createRecord('template', {
        id: v4(),
        name: this.newRecord.name.trim(),
        categoryId: this.newRecord.categoryId,
        description: '',
        content: '',
        contentNoHtml: '',
        active: false,
        tags: '',
        code: v4(),
        language: this.translation.currentLang,
        erasable: true,
        copyInNewTenants: false,
        erased: false,
        order: this.newRecord.order,
        featuredImage: '',
      });

      await newR.save();
      this.findRecords.perform();

      // resetto l'oggetto newRecord per svuotare i campi input collegati
      this.newRecord = this.initializeRecord();
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

  // apre la pagina di modifica del template
  openDetail(id) {
    // qui indicare se mostrare oggetto e tags!
    this.router.transitionTo('template-details', id, {
      queryParams: {
        showObject: this.showObject,
        showTags: this.showTags,
        showFeaturedImage: this.showFeaturedImage,
      },
    });
  }

  @action
  insertFilter(event) {
    this.filterText = event.target.value.trim();
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
  toggleRecycle() {
    this.showRecycle = !this.showRecycle;
    this.findRecords.perform();
  }

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

  @action
  willDestroy() {
    super.willDestroy(...arguments);

    // Rimuovi il listener quando il componente viene distrutto
    $(document).off('change search', '.find-template');
    $(document).off('change', '.changeCategory');
    $(document).off('click', '.changeCategory');
    $(document).off('click', '.moveUpDown');
  }
}
