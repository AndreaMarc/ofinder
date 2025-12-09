/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { htmlSafe } from '@ember/template';
import $ from 'jquery';

export default class StandardCoreCategoryCrudComponent extends Component {
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  modelName = 'category';
  @tracked currentTenant = null;

  @tracked available = true;
  @tracked records = [];
  @tracked showUpdateCategory = false;
  @tracked erasableCategory = '';
  @tracked category = null;
  @tracked allCategories = [];
  @tracked allCategoriesForEdit = [];
  @tracked filterText = '';
  @tracked reassignmentCategory = '0';

  newValue = {}; // usato nella modifica del record
  @tracked newRecord = null; // usato in creazione nuovo record

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
    let self = this;
    this.available = true;
    await this.findRecords.perform(); // Avvia la task

    setTimeout(() => {
      $(`.slider-new-category`)
        .bootstrapToggle()
        .change(function () {
          let $this = $(this);
          let checked = $this.prop('checked');
          self.storeNewValue('enabled', false, {
            target: { value: checked },
          });
        });
    }, 60);
  }

  findRecords = task({ drop: true }, async () => {
    try {
      this.records = [];
      this.category = null;
      this.showUpdateCategory = false;
      let self = this;

      // recupero l'elenco di tutte le categorie per popolare la select di scelta della
      // parent-category (su inserimento nuova categoria)
      this.allCategories = await this.store.query(this.modelName, {
        filter: `equals(tenantId,'${this.currentTenant}')`,
        sort: `name`,
      });

      let tempRecords = await this.fetch.call(
        'categories/getTree',
        'GET',
        { tenantId: this.currentTenant },
        {},
        true,
        this.session
      );

      tempRecords = tempRecords.data;
      /*tempRecords = tempRecords.data.filter((item) => {
        return item.erasable;
      });*/

      if (tempRecords.length > 0) {
        this.records = tempRecords;
        let adaptedRecords = self.transformData(this.records); // adatto la risposta per fancyTree
        self.available = true;

        // renderizzo il componente fancyTree
        setTimeout(() => {
          $('#categoriesTree').fancytree({
            source: adaptedRecords,
            ...self.fancyTreeOptions,
            activate: function (event, data) {
              var node = data.node;

              self.category = null;
              self.erasableCategory = htmlSafe(
                `Categoria di sistema, cancellazione non disponibile!`
              );
              self.showUpdateCategory = false;

              if (!$.isEmptyObject(node.data)) {
                if (!node.unselectable) {
                  self.findRecord.perform(node.data.id);
                } else {
                  self.dialogs.toast(
                    'Categoria di sistema, non modificabile!',
                    'warning',
                    'bottom-right',
                    2
                  );
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
        }, 80);
      }
    } catch (e) {
      self.available = false;
    }
  });

  get recordsLength() {
    return this.records.length;
  }

  findCategoryById(categories, id) {
    for (let category of categories) {
      if (category.id === id) {
        return category;
      }

      if (category.child && category.child.length > 0) {
        let found = this.findCategoryById(category.child, id);
        if (found) {
          return found;
        }
      }
    }

    return null; // Restituisce null se non viene trovata nessuna categoria con l'ID specificato
  }

  // richiamo il record singolo da modificare
  findRecord = task({ drop: true }, async (id) => {
    this.category = null;
    this.erasableCategory = '';
    this.showUpdateCategory = true;
    // recupero l'elenco di tutte le categorie per popolare la select di scelta della parent-category
    this.allCategoriesForEdit = await this.store.query(this.modelName, {
      filter: `and(equals(tenantId,'${this.currentTenant}'),not(equals(id,'${id}')))`,
      sort: `name`,
    });

    let selected = this.findCategoryById(this.records, id);
    if (selected) {
      if (!selected.erasable) {
        // verifico se è una categoria di sistema
        this.erasableCategory = htmlSafe(
          `Categoria di sistema, cancellazione non disponibile!`
        );
      } else if (selected.child && selected.child.length > 0)
        // verifico se ha categorie figlie, che la renderebbero non cancellabile
        this.erasableCategory = htmlSafe(
          `Cancellazione non attiva poiché contiene sotto-categorie!`
        );
    }

    setTimeout(async () => {
      try {
        // recupero il record da modificare
        this.category = await this.store.findRecord('category', id);
      } catch (e) {
        console.error(e);
        this.category = null;
        this.showUpdateCategory = false;
        this.dialogs.toast(
          'Errore nel recupero della Categoria. Riprova!',
          'error',
          'bottom-right',
          4
        );
      }
    }, 10);
  });

  // annullo modifica di un record
  @action
  undoRecord() {
    this.category = null;
    this.showUpdateCategory = false;
    this.newValue = {};
  }
  // cattura il valore inserito dall'utente per la creazione/modifica di un record
  @action
  storeNewValue(field, isUpdate, event) {
    let value = event.target.value;
    if (['parentCategory'].includes(field)) {
      value = parseInt(value);
    }

    if (isUpdate) {
      this.newValue[field] = value;
    } else {
      this.newRecord[field] = value;
    }
  }

  // chiede conferma per salvataggio modifiche a un record
  @action
  saveVoice() {
    let self = this;
    this.dialogs.confirm(
      '<h6>Modifica record</h6>',
      `<p>Azione irreversibile. Confermi la modifica?</p>`,
      () => {
        self.saveVoiceConfirmed.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  saveVoiceConfirmed = task({ drop: true }, async () => {
    Object.keys(this.newValue).forEach((key) => {
      console.log(key, this.newValue[key]);
      this.category[key] = this.newValue[key];
    });

    if (this.category.id === this.category.parentTenant) {
      this.dialogs.toast(
        'La Categoria Superiore non può corrispondere a se stessa!',
        'error',
        'bottom-right',
        5
      );
      return false;
    }

    try {
      await this.category.save();
      this.category = null;
      this.showUpdateCategory = false;
      this.newValue = {};
      this.findRecords.perform();
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

  @action
  reassignment(event) {
    this.reassignmentCategory = event.target.value.toString();
  }

  @action
  delVoice(operation, event) {
    event.stopPropagation();
    let self = this;
    let info = '';
    if (operation === 'eraseAll') {
      info = `<h3>ATTENZIONE:</h3>
              <h5>Stai cancellando SIA la Categoria CHE i Template ad essa associati</h5>
              <br />
              <h6>Azione irreversibile.</h6>
              <h5>Confermi?</h5>`;
    } else {
      if (this.reassignmentCategory === '0') {
        // eslint-disable-next-line prettier/prettier
        this.dialogs.toast('Scegli la Categoria a cui verranno associati gli eventuali template esistenti', 'warning', 'bottom-right', 4);
        return false;
      } else {
        info = `<h4>ATTENZIONE:<br />Stai cancellando la Categoria. I Template verranno associati alla Categoria scelta.</h4>
              <h5>Confermi?</h5>`;
      }
    }
    this.dialogs.confirm(
      '<h3 class="text-danger">CANCELLAZIONE RECORD</h3>',
      `<div class="text-danger">${info}</div>`,
      () => {
        self.delVoiceConfirmed.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  delVoiceConfirmed = task({ drop: true }, async () => {
    try {
      let self = this;
      this.fetch
        .call(
          `categories/${this.category.id}?alternativeCategory=${self.reassignmentCategory}`,
          'DELETE',
          null,
          {},
          true,
          self.session
        )
        .then(() => {
          self.showUpdateCategory = false;
          self.newValue = {};
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

  // crea un nuovo record
  saveNewVoice = task({ drop: true }, async () => {
    if (!this.newRecord || !this.newRecord.name || this.newRecord.name === '') {
      this.dialogs.toast('Il nome è obbligatorio!', 'error', 'bottom-right', 4);
      return false;
    }

    try {
      let newR = this.store.createRecord(this.modelName, {
        name: this.newRecord.name.trim(),
        description: this.newRecord.description.trim() || '',
        type: this.newRecord.type.trim() || '',
        parentCategory: this.newRecord.parentCategory || 0,
        erasable: this.newRecord.erasable || true,
        tenantId: this.currentTenant,
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

  // adatto la risposta dell'API dei tenant annidati nella forma necessaria per il plugin fancyTree
  transformData(data) {
    return data.map((item) => {
      return {
        title: `${item.name} ${
          item.erasable ? '' : '<i class="ml-2 fa fa-ban text-warning"> </i>'
        }`,
        id: item.id,
        expanded: false,
        folder: item.child.length > 0,
        children: this.transformData(item.child),
        unselectable: !item.erasable,
      };
    });
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);

    // Rimuovi il listener quando il componente viene distrutto
    $(document).off('change search', '.find-category');
  }

  @action
  preventDropdownClose(event) {
    event.stopPropagation();
  }

  @action
  closeDropdown() {
    let dropdownElement = document.getElementById('deleteDropdown');
    let dropdown = new bootstrap.Dropdown(dropdownElement);
    dropdown.hide();
  }
}
