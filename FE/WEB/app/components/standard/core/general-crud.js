/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { task } from 'ember-concurrency';
import { action } from '@ember/object';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { htmlSafe } from '@ember/template';

export default class StandardCoreGeneralCrudComponent extends Component {
  @service('siteSetup') stp;
  @service translation;
  @service dialogs;
  @service jsonApi;
  @service session;
  @service fetch;
  @service store;

  @tracked available = 'waiting';
  @tracked visibleEntities = [];
  @tracked addEmptyOption = '';
  @tracked uniqueIdentitity = '';
  @tracked userEntitySelected = '';
  @tracked error = '';
  @tracked tableStructure = [];
  @tracked records = null;
  @tracked recordStructure = null;
  @tracked newRecord = null;
  entities = [];

  @tracked pageNumber = 1;
  @tracked pageSize = '10';
  @tracked filter = [];
  @tracked order = {};

  @tracked selectedRow = null;
  @tracked recordChanged = false;
  @tracked recordsLength = 0;

  // per espandibilità colonne
  isResizing = false;
  currentTh = null;

  constructor(...attributes) {
    super(...attributes);

    this.start();
  }

  @action
  async start() {
    this.available = 'waiting';
    // ricavo l'elenco delle entità Json:Api
    await this.findEntity.perform();

    try {
      // se nell'url viene passata un'entità, verifico che esista
      if (this.args.entity && this.args.entity !== '') {
        let exists = this.entities.filter((item) => {
          return item.entity === this.args.entity;
        });

        // mostro solo l'entità selezionata
        if (exists && exists.length > 0) {
          let unique = exists[0];
          unique.selected = true;
          this.visibleEntities = [unique];
          this.addEmptyOption = '';
          this.uniqueIdentitity = unique.entity;
          this.userEntitySelected = unique.entity;
        } else {
          throw new Error();
        }
      } else {
        throw new Error();
      }
    } catch (e) {
      // mostro tutte le entità
      this.addEmptyOption = '1';
      this.visibleEntities = this.entities;
      this.userEntitySelected = '';
      this.createForm();
    }

    this.available = 'available';
  }

  // estrae la lista delle entità formattate per il componente SelectTwo
  get select2Entities() {
    let newArr = [];
    this.visibleEntities.forEach((element) => {
      let x = {};
      x.id = element.entity;
      x.value =
        element.entity +
        (element.description ? ` - ${element.description}` : '');
      newArr.push(x);
    });
    return JSON.stringify(newArr);
  }

  // chiamata quando l'utente seleziona l'entità su cui operare
  @action
  setEntity(key) {
    this.userEntitySelected = key;

    this.records = null;
    this.recordsLength = 0;
    this.selectedRow = null;
    this.recordChanged = false;

    this.order = {};
    this.filter = [];
    this.createForm();
  }

  @action
  refresh() {
    this.records = null;
    this.recordsLength = 0;
    this.selectedRow = null;
    this.recordChanged = false;

    this.order = {};
    this.filter = [];
    this.createForm();
  }

  createForm() {
    this.records = null;
    this.selectedRow = null;
    this.recordChanged = false;

    // disegno l'header della tabella in base all'entità selezionata
    this.tableStructure = this.getModelArchitecture();

    // estraggo la struttura del record
    this.recordStructure = this.initializeRecord();

    // genero un record vuoto che sfrutterò per creare un nuovo record nel DB
    this.newRecord = this.initializeRecord();

    // carico i dati dell'entità delezionata
    this.findRecords.perform();
  }

  // estraggo le struttura dell'entità scelta, prelevadola dai models
  getModelArchitecture() {
    if (
      this.userEntitySelected === '' ||
      typeof this.userEntitySelected !== 'string'
    )
      return false;

    let fieldsArray = [];
    try {
      const modelClass = this.store.modelFor(this.userEntitySelected);
      const attributes = (modelClass && modelClass.attributes) || {};

      attributes.forEach((meta, key) => {
        let element = { name: key };
        if (meta.options && meta.options.defaultValue)
          element.defaultValue = meta.options.defaultValue;

        switch (meta.type) {
          case 'string':
            element.inputType = 'string';
            element.width = htmlSafe(280);
            break;
          case 'number':
            element.inputType = 'number';
            element.width = htmlSafe(150);
            break;
          case 'date':
            element.inputType = 'date';
            element.width = htmlSafe(190);
            break;
          case 'boolean':
            element.inputType = 'boolean';
            element.width = htmlSafe(50);
            break;
          default:
            element.inputType = 'other';
            element.width = htmlSafe(280);
            break;
        }
        fieldsArray.push(element);
      });
    } catch (e) {
      console.error(e);
    }

    return fieldsArray;
  }

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord(entity) {
    let ent = null;
    if (entity) {
      ent = entity;
    } else if (
      this.userEntitySelected &&
      typeof this.userEntitySelected === 'string'
    ) {
      ent = this.userEntitySelected;
    }
    if (!ent) return false;

    let modelInstance = this.store.createRecord(ent); // Crea una nuova istanza del modello (ad esempio, 'user')
    // Crea un semplice oggetto JavaScript con tutte le proprietà del modello
    let modelData = {};
    modelInstance.eachAttribute((key) => {
      modelData[key] = modelInstance.get(key);
    });

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }

  findRecords = task({ restartable: true }, async () => {
    this.records = [];
    if (
      this.userEntitySelected === '' ||
      typeof this.userEntitySelected !== 'string'
    )
      return false;

    // compongo la query di ricerca
    let page = {
      size: this.pageSize,
      number: this.pageNumber,
    };

    let params = {
      page: page,
      filter: this.filter,
    };

    if (this.order && Object.keys(this.order).length > 0) {
      params.sort = Object.values(this.order).join(',');
    }
    let query = this.jsonApi.queryBuilder(params);
    this.records = await this.store.query(this.userEntitySelected, query);

    this.recordsLength = this.records.meta.total;
  });

  // cambia i filtri di ricerca
  @action
  changeFilter(column, filterType, value) {
    this.records = [];
    let index = this.filter.findIndex((obj) => obj.column === column);
    if (index !== -1) {
      this.filter.splice(index, 1);
    }

    if (value !== '' && value !== false) {
      let x = {};

      if (filterType === 'not-equals') {
        x.function = 'equals';
        x.negation = true;
      } else {
        x.function = filterType;
      }
      x.column = column;
      x.value = value;
      this.filter.push(x);
    }
    console.warn(this.filter);
    this.findRecords.perform();
  }

  // cambia l'ordine di visualizzazione
  @action
  changeOrder(column, order) {
    this.records = [];

    if (Object.prototype.hasOwnProperty.call(this.order, column)) {
      delete this.order[column];
    }

    if (order !== '') this.order[column] = order;
    this.findRecords.perform();
  }

  @action
  changePageSize(event) {
    this.pageSize = event.target.value;
    this.findRecords.perform();
  }

  @action
  changePageNumber(page) {
    this.pageNumber = page;
    this.findRecords.perform();
  }

  numberRecord(n, pageNumber, pageSize) {
    return (pageNumber - 1) * parseInt(pageSize) + n + 1;
  }

  // estrae l'elenco delle entità Json:Api
  findEntity = task({ drop: true }, async () => {
    this.entities = [];
    let self = this;

    // estraggo gli eventuali dati inseriti in setup/entità
    try {
      this.entitiesList = this.stp.siteSetup.entitiesList;
    } catch (e) {
      throw new Error();
    }

    await this.fetch
      .call('entity/getAll', 'GET', null, {}, true, this.session)
      .then((res) => {
        if (res.data && res.data.length > 0) {
          let x = [];
          res.data.forEach((element) => {
            let ent = element.split('.').at(-1);
            try {
              this.initializeRecord(ent); // se non esiste il model di tale entità, genera eccezione -> l'entità non viene messa nella select
              let objToAdd = { entity: ent };

              let details = this.entitiesList.filter((item) => {
                return item.entity == ent;
              });
              if (details.length > 0) {
                objToAdd.title = details[0].title;
                objToAdd.key = details[0].key;
                objToAdd.description = details[0].description;
                objToAdd.keyDescription = details[0].keyDescription;
              }

              x.push(objToAdd);
            } catch (e) {
              //console.error(e);
            }
          });
          self.entities = x;
        }
      })
      .catch((e) => {
        console.error(e);
        self.error =
          this.translation.languageTranslation.component.generalCrud.unableFetchRoutes;
        self.available = 'unavailable';
      });
  });

  // cattura variazioni per la creazione di un nuovo record
  @action
  updateNewRecord(field, isBoolean, event) {
    let val = '';
    try {
      val = isBoolean ? event.target.checked : event.target.value;
    } catch (e) {
      val = isBoolean ? !!event : event;
    }
    this.newRecord[field] = val;
    //console.warn(field, this.newRecord[field]);
  }

  @action
  saveNewRecord() {
    this.dialogs.confirm(
      '<h6>Creazione record</h6>',
      `<p>Confermi la creazione del record?</p>`,
      () => {
        this.saveNew.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  saveNew = task({ drop: true }, async () => {
    let temp = await this.store.createRecord(
      this.userEntitySelected,
      this.newRecord
    );
    await temp
      .save()
      .then((res) => {
        console.warn(res);
        this.findRecords.perform();
      })
      .catch(async (e) => {
        let errorInstance = await e;
        console.error(errorInstance);
        if (errorInstance.errors && errorInstance.errors.length > 0) {
          let err = 'Si sono verificati i seguenti errori:<br /><br />';
          errorInstance.errors.forEach((element) => {
            err += `<p>
                    <span class="text-danger">
                      ${element.status}: ${element.title}
                      <br />
                      (<span class="text-warning">
                        <small><em>${element.detail}</em></small>
                      </span>)
                    </span>
                    </p>`;
          });

          // eslint-disable-next-line no-undef
          Swal.fire({
            icon: 'error',
            title: 'Errori di salvataggio',
            html: err,
            width: '80%',
          });
        }
      });
  });

  @action
  selectRow(index) {
    console.log('INDEX: ', index);
    this.selectedRow = index;
  }

  @action
  changeExisting(record, column, isBoolean, event) {
    let val = '';
    try {
      val = isBoolean ? event.target.checked : event.target.value;
    } catch (e) {
      val = isBoolean ? !!event : event;
    }

    record[column] = val;
    this.recordChanged = record.hasDirtyAttributes;
  }

  @action
  async undoChange(record) {
    await record.rollbackAttributes();
    this.recordChanged = record.hasDirtyAttributes;
  }

  @action
  saveChange(record) {
    this.dialogs.confirm(
      '<h6 class="text-danger">Modifica record</h6>',
      `<p class="text-danger">Stai salvando le modifiche apportate. Confermi?</p>`,
      () => {
        this.saveChangeConfirmed.perform(record);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  saveChangeConfirmed = task({ drop: true }, async (record) => {
    await record.save();
    this.recordChanged = record.hasDirtyAttributes;
    record = this.store.findRecord(this.userEntitySelected, record.id);
  });

  @action
  deleteRecord(record) {
    this.dialogs.confirm(
      '<h6 class="text-danger">Cancellazione record</h6>',
      `<p class="text-danger">
        <strong>ATTENZIONE:</strong><br />
        Stai eliminando il record. Azione irreversibile<br /><br />Confermi?
      </p>`,
      () => {
        this.deleteRecordConfirmed.perform(record);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  deleteRecordConfirmed = task({ drop: true }, async (record) => {
    await record.destroyRecord();
    this.recordChanged = record.hasDirtyAttributes;
    this.selectedRow = null;
  });

  // #region Espandibilità colonne
  @action
  setupResizableColumns(element) {
    let thElements = element.querySelectorAll('.resizable');

    thElements.forEach((th) => {
      th.addEventListener('mousedown', () => {
        this.isResizing = true;
        this.currentTh = th;
        document.addEventListener('mousemove', this.handleMouseMove);
        document.addEventListener('mouseup', this.stopResizing);
      });
    });
  }

  @action
  handleMouseMove(e) {
    if (this.isResizing && this.currentTh) {
      let previousWidth = this.currentTh.offsetWidth;
      let newWidth = e.clientX - this.currentTh.getBoundingClientRect().left;

      // Calcola la differenza di larghezza
      let difference = newWidth - previousWidth;

      this.currentTh.style.width = `${newWidth}px`;

      // Ajust the table's width
      let table = this.currentTh.closest('table');
      table.style.width = `${table.offsetWidth + difference}px`;
    }
  }

  @action
  stopResizing() {
    this.isResizing = false;
    document.removeEventListener('mousemove', this.handleMouseMove);
    document.removeEventListener('mouseup', this.stopResizing);
  }
  // #endregion

  willDestroy() {
    super.willDestroy(...arguments);
    document.removeEventListener('mousemove', this.handleMouseMove);
    document.removeEventListener('mouseup', this.stopResizing);
  }
}
