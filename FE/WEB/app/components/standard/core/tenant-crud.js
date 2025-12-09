/**
 * CONSENTE LA GESTIONE DEI TENANT
 *
 * @param {string} reloadMaster : la Task da invocare per il reload dei tenant nel componente superiore (permissions-master). Eventualmente nulla.
 */

/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
//import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { htmlSafe } from '@ember/template';
//import config from 'poc-nuovo-fwk/config/environment';
import $ from 'jquery';

export default class StandardCoreTenantCrudComponent extends Component {
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  modelName = 'tenant';
  @tracked available = true;
  @tracked records = [];
  @tracked showUpdateTenant = false;
  @tracked erasableTenant = false;
  @tracked notErasableReason = '';
  @tracked tenant = null;
  @tracked allTenants = [];
  @tracked filterText = '';
  @tracked displayBilling = 'd-none';

  @tracked reloadMaster = null;

  newValue = {}; // usato nella modifica del record
  @tracked newRecord = null; // usato in creazione nuovo record

  constructor(...attributes) {
    super(...attributes);
    this.reloadMaster = this.args.reloadMaster ? this.args.reloadMaster : null;
    this.newRecord = this.initializeRecord();
    this.start();

    // filtro di ricerca
    $(document).on('change search', '.find-tenant', function (e) {
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
      $(`.slider-new-tenant`)
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
    this.records = [];

    this.tenant = null;
    this.showUpdateTenant = false;
    let self = this;

    // recupero l'elenco di tutti i tenant per popolare la select di scelta del parent-tenant
    this.allTenants = await this.store.findAll('tenant');

    this.fetch
      .call('tenants/getTree', 'GET', null, {}, true, this.session)
      .then((res) => {
        if (res.success && res.data) {
          self.records = res.data;
          let adaptedRecords = self.transformData(res.data); // adatto la risposta per fancyTree
          self.available = true;

          // renderizzo il componente fancyTree
          setTimeout(() => {
            $('#tenantsTree').fancytree({
              source: adaptedRecords,
              ...self.fancyTreeOptions,
              activate: function (event, data) {
                var node = data.node;
                if (!$.isEmptyObject(node.data)) {
                  self.findRecord.perform(node.data.id);
                } else {
                  this.dialogs.toast(
                    'Si è verificato un errore. Riprova!',
                    'error',
                    'bottom-right',
                    3
                  );
                }
              },
            });
          }, 80);
        } else {
          throw new Error();
        }
      })
      .catch(() => {
        self.available = false;
      });
  });

  get recordsLength() {
    return this.records.length;
  }

  findTenantById(tenants, id) {
    for (let tenant of tenants) {
      if (tenant.id === id) {
        return tenant;
      }

      if (tenant.child && tenant.child.length > 0) {
        let found = this.findTenantById(tenant.child, id);
        if (found) {
          return found;
        }
      }
    }

    return null; // Restituisce null se non viene trovato nessun tenant con l'ID specificato
  }

  // richiamo il record singolo da modificare
  findRecord = task({ drop: true }, async (id) => {
    let self = this;
    this.tenant = null;
    this.erasableTenant = false;
    this.showUpdateTenant = true;
    // recupero l'elenco di tutti i tenant per popolare la select di scelta del parent-tenant
    this.allTenants = await this.store.findAll('tenant');

    // verifico se è eliminabile (cioè se non ha tenant-figli)
    let selected = this.findTenantById(this.records, id);
    if (selected) {
      if (selected.isRecovery) {
        this.notErasableReason = htmlSafe(
          `Cancellazione non attiva poiché è un Tenant di sistema`
        );
      } else if (!selected.isErasable) {
        this.notErasableReason = htmlSafe(
          `Cancellazione non attiva poiché è un Tenant amministrativo`
        );
      } else if (selected.child && selected.child.length !== 0) {
        this.notErasableReason = htmlSafe(
          `Cancellazione non attiva poiché contiene sub-tenant!`
        );
      } else {
        this.notErasableReason = '';
        this.erasableTenant = true;
      }
    }

    setTimeout(async () => {
      try {
        $(`.slider-tenant`).bootstrapToggle('destroy');
        // recupero il tenant da modificare
        this.tenant = await this.store.findRecord('tenant', id);
        //if (this.tenant.)

        setTimeout(() => {
          // eslint-disable-next-line no-self-assign
          this.tenant = this.tenant;

          $(`.slider-tenant`)
            .bootstrapToggle()
            .change(function () {
              let $this = $(this);
              let field = $this.attr('data-field');
              let checked = $this.prop('checked');
              self.storeNewValue(field, true, {
                target: { value: checked },
              });
            });
        }, 100);
      } catch (e) {
        console.error(e);
        this.tenant = null;
        this.showUpdateTenant = false;
        this.dialogs.toast(
          'Errore nel recupero del Tenant. Riprova!',
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
    this.tenant = null;
    this.showUpdateTenant = false;
    this.newValue = {};
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

    if (field === 'matchBillingAddress') {
      if (value) {
        // non mostro campi relativi alla sede di fatturazione
        this.displayBilling = 'd-none';
        // nei campi della sede di fatturazione memorizzo gli stessi valori inseriti per la sede legale
        switch (field) {
          case 'registeredOfficeAddress':
            field = 'billingAddressAddress';
            break;
          case 'registeredOfficeCity':
            field = 'billingAddressCity';
            break;
          case 'registeredOfficeProvince':
            field = 'billingAddressProvince';
            break;
          case 'registeredOfficeState':
            field = 'billingAddressState';
            break;
          case 'registeredOfficeRegion':
            field = 'billingAddressRegion';
            break;
          case 'registeredOfficeZIP':
            field = 'billingAddressZIP';
            break;
        }
        this.newValue[field] = value;
      } else {
        // mostro campi relativi alla sede di fatturazione
        this.displayBilling = 'd-block';
      }
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
      //console.log(key, this.newValue[key]);
      this.tenant[key] = this.newValue[key];
    });

    if (this.tenant.name === '') {
      this.dialogs.toast(
        'Il Nome del Tenanst è obbligatorio!',
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    if (this.tenant.id === this.tenant.parentTenant) {
      this.dialogs.toast(
        'Il Tenanst Superiore non può corrispondere al tenant stesso!',
        'error',
        'bottom-right',
        5
      );
      return false;
    }

    try {
      await this.tenant.save();
      this.tenant = null;
      this.showUpdateTenant = false;
      this.newValue = {};
      this.findRecords.perform();
      if (this.reloadMaster) this.reloadMaster.perform();
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
  delVoice() {
    let self = this;
    this.dialogs.confirm(
      '<h6>Cancellazione record</h6>',
      `<p class="text-danger">Azione irreversibile. Confermi la cancellazione?</p>`,
      () => {
        self.delVoiceConfirmed.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  delVoiceConfirmed = task({ drop: true }, async () => {
    try {
      await this.tenant.destroyRecord();
      this.showUpdateTenant = false;
      this.newValue = {};
      this.findRecords.perform();
      if (this.reloadMaster) this.reloadMaster.perform();
    } catch (e) {
      console.error(e);
    }
  });

  // crea un nuovo record
  saveNewVoice = task({ drop: true }, async () => {
    if (
      !this.newRecord ||
      !this.newRecord.name ||
      this.newRecord.name === '' ||
      !this.newRecord.organization ||
      this.newRecord.organization === '' ||
      typeof this.newRecord.parentTenant === 'undefined'
    ) {
      this.dialogs.toast(
        'Tutti i campi sono obbligatori!',
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    try {
      let newR = this.store.createRecord(this.modelName, {
        name: this.newRecord.name,
        organization: this.newRecord.organization,
        parentTenant: this.newRecord.parentTenant,
        enabled: this.newRecord.enabled,
      });

      await newR.save();
      this.findRecords.perform();

      // resetto l'oggetto newRecord per svuotare i campi input collegati
      this.newRecord = this.initializeRecord();
      if (this.reloadMaster) this.reloadMaster.perform();
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
    this.filterText = event.target.value;
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
          item.enabled
            ? ''
            : '<div class="mb-0 badge badge-dot badge-dot-lg bg-danger"> </div>'
        }`,
        id: item.id,
        expanded: false,
        folder: item.child.length > 0,
        children: this.transformData(item.child),
      };
    });
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);

    // Rimuovi il listener quando il componente viene distrutto
    $(document).off('change search', '.find-tenant');
  }
}
