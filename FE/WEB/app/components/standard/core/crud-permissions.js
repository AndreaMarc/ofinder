/**
 * Consente la gestione dei permessi di Crud
 *
 * @param {string}  currentTenant guid del tenant corrente
 */
/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
//import { v4 } from 'ember-uuid';
//import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import config from 'poc-nuovo-fwk/config/environment';
import $ from 'jquery';

export default class StandardCoreCrudPermissionsComponent extends Component {
  @service('siteSetup') stp;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  modelName = 'role-claim';
  @tracked available = true;
  @tracked serviceUpperAvailable = 'waiting';
  @tracked records = [];
  @tracked tenants = [];
  @tracked entities = [];
  @tracked roles = [];
  @tracked editingRecordId = null;
  @tracked currentTenant = null;
  @tracked currentRole = '';
  @tracked filterText = '';
  @tracked changed = false;

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    this.serviceUpperAvailable = 'waiting';
    let currentTenant = this.args.currentTenant ? this.args.currentTenant : '';
    if (currentTenant === '') return false;
    this.currentTenant = await this.store.peekRecord('tenant', currentTenant);
    this.available = true;
    this.findRoles.perform();
  }

  findRoles = task({ drop: true }, async () => {
    try {
      if (!this.currentTenant.id) return;
      this.currentRole = '';

      this.roles = await this.store.query('role', {
        filter: `equals(tenantId,'${this.currentTenant.id}')`,
        sort: `name`,
      });
      this.serviceUpperAvailable = 'available';
    } catch (e) {
      this.available = false;
    }
  });

  // consente il cambio ruolo
  @action
  async updateRole(event) {
    this.currentRole = event.target.value;
    this.findRecords.perform();
  }

  findRecords = task({ restartable: true }, async () => {
    try {
      $(`.crud-perms`).bootstrapToggle('destroy');
      this.entities = [];

      if (!this.currentRole) return;
      this.entities = await this.findEntity.perform();

      this.records = [];
      this.records = await this.store.query(this.modelName, {
        filter: `and(equals(roleId,'${this.currentRole}'),equals(claimType,'crud'))`,
        sort: `claimValue`,
      });
    } catch (e) {
      this.available = false;
    }
  });

  @action
  isChecked(entity, operation) {
    let foundRecord = this.records.find(
      (record) => record.claimValue === `${entity}.${operation}`
    );
    if (foundRecord) {
      return true;
    } else {
      return false;
    }
  }

  @action
  isDisabled(entity) {
    let foundRecord = this.records.find(
      (record) => record.claimValue === `${entity}.read`
    );
    if (foundRecord) {
      return false;
    } else {
      return true;
    }
  }

  // estrae l'elenco delle entità
  findEntity = task({ drop: true }, async () => {
    this.entities = [];
    let self = this;

    // estraggo gli eventuali dati inseriti in setup/entità
    try {
      this.entitiesList = this.stp.siteSetup.entitiesList;
    } catch (e) {
      throw new Error();
    }

    this.fetch
      .call('entity/getAll', 'GET', null, {}, true, this.session)
      .then((res) => {
        if (res.data && res.data.length > 0) {
          let x = [];
          res.data.forEach((element) => {
            let ent = element.split('.').at(-1);
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
          });
          self.entities = x;
        }
      })
      .catch(() => {
        self.available = false;
      });
  });

  get filteredEntities() {
    if (this.filterText && this.filterText !== '') {
      return this.entities.filter((record) => {
        return (
          (record.entity &&
            record.entity
              .toLowerCase()
              .includes(this.filterText.toLowerCase())) ||
          (record.description &&
            record.description
              .toLowerCase()
              .includes(this.filterText.toLowerCase())) ||
          (record.title &&
            record.title.toLowerCase().includes(this.filterText.toLowerCase()))
        );
      });
    } else {
      return this.entities;
    }
  }

  get entitiesLength() {
    return this.entities && this.entities.length;
  }

  @action
  bootstrapToggle(rowIndex) {
    let self = this;
    if (rowIndex === this.entities.length - 1) {
      $(`.crud-perms`).bootstrapToggle('destroy');

      setTimeout(() => {
        $(`.crud-perms`).bootstrapToggle().change(self.bootstrapToggleFunction);
      }, 20);
    }
  }

  bootstrapToggleFunction() {
    let $this = $(this);
    let entity = $this.attr('data-entity');
    let operation = $this.attr('data-operation');
    let checked = $this.prop('checked');

    if (operation === 'read' && !checked) {
      // se non ha il permesso di lettura, non può avere quelli di creazione, aggiornamento, cancellazione
      $(`.crud-perms[data-entity="${entity}"]:not([data-operation="read"])`)
        .bootstrapToggle('destroy')
        .prop('checked', false)
        .prop('disabled', true)
        .bootstrapToggle()
        .change(this.bootstrapToggleFunction);
    } else {
      $(`.crud-perms[data-entity="${entity}"]:not([data-operation="read"])`)
        .bootstrapToggle('destroy')
        .prop('disabled', false)
        .bootstrapToggle()
        .change(this.bootstrapToggleFunction);
    }
  }

  @action
  save() {
    let self = this;
    this.filterText = '';
    this.dialogs.confirm(
      '<h6>Aggiornamento dei permessi di CRUD</h6>',
      `<h5 class="text-danger">ATTENZIONE!</h5>
      <h6>Controlla scrupolosamente le impostazioni selezionate<br /><br />Confermi?</h6>`,
      () => {
        setTimeout(() => {
          self.saveConfirmed.perform();
        }, 200);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  // crea un nuovo record
  saveConfirmed = task({ drop: true }, async () => {
    try {
      // costruisco l'oggetto dal passare al BE
      let out = {
        roleId: this.currentRole,
        claims: [],
      };

      let $rows = $('#role-permissions-crud-table > tbody > tr');
      $.each($rows, function () {
        let $this = $(this);
        let entity = $this.attr('data-entity');

        let temp = {};
        temp['read'] = $(
          `.crud-perms[data-entity="${entity}"][data-operation="read"]`
        ).is(':checked');
        temp['create'] = $(
          `.crud-perms[data-entity="${entity}"][data-operation="create"]`
        ).is(':checked');
        temp['update'] = $(
          `.crud-perms[data-entity="${entity}"][data-operation="update"]`
        ).is(':checked');
        temp['delete'] = $(
          `.crud-perms[data-entity="${entity}"][data-operation="delete"]`
        ).is(':checked');

        let toSave = {};
        toSave[entity] = temp;
        out.claims.push(toSave);
      });

      let self = this;

      this.fetch
        .call('roleClaims/crud', 'POST', out, {}, true, this.session)
        .then((res) => {
          if (res.success) {
            self.dialogs.toast(
              'Operazione riuscita',
              'success',
              'bottom-right',
              3
            );
            self.findRecords.perform();
          } else {
            throw new Error();
          }
        })
        .catch((e) => {
          console.error(e);
          throw new Error();
        });
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

  @action
  encode(value) {
    return encodeURI(value);
  }
}
