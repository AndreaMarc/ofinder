import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
//import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import $ from 'jquery';
import { v4 } from 'ember-uuid';
import 'moment-timezone';

export default class StandardCoreCustomerUsersCrudComponent extends Component {
  @service('siteSetup') stp;
  @service permissions;
  @service jsUtility;
  @service jsonApi;
  @service session;
  @service dialogs;
  @service store;

  @tracked listUserSize = 'fadeInLeft';
  @tracked editUserSize = 'd-none';
  @tracked available = 'waiting';
  @tracked records = [];
  @tracked tenants = [];
  @tracked editingRecord = null;
  @tracked currentTenant = null;
  @tracked currentUserId = null;
  @tracked filterText = '';
  @tracked registrationFields = null;
  @tracked nations = [];

  @tracked oldValue = null;
  @tracked newValue = null; // usato nella modifica del record
  @tracked newRole = null; // usato nella modifica del ruolo

  @tracked currentPageNumber = 1;
  @tracked totalRecords = 0;
  @tracked recordsPerPage = '20';

  @tracked tenantRoles;

  searchText = '';
  search = {
    page: {},
    filter: [
      {
        function: 'any',
        column: 'state',
        value: `'accepted','ownerCreated','selfCreated'`,
      },
      {
        function: 'equals',
        column: 'user.deleted',
        value: 'false',
      },
    ], // { function: 'contains', column: 'tag', value: 'valoreX' }
    sort: `user.userProfile.lastName,user.userProfile.firstName,user.email,-createdAt`,
    include: 'tenant,user.userProfile,user.bannedUsers', //
  };

  constructor(...attributes) {
    super(...attributes);
    this.registrationFields =
      this.stp.siteSetup.registrationFields.registration;
    this.currentTenant = this.session.get('data.tenantId');
    this.currentUserId = this.session.get('data.id');
    this.nations = this.jsUtility.nations();

    this.start.perform();
  }

  start = task({ drop: true }, async () => {
    // poichè l'utente non può vedere gli utenti di tutti i tenant, mostro solo gli utenti che appartengono ai ruoli previsti in
    // setup->rolesForEditUsers e ad altri eventuali ruoli custom di questo tenant.
    // Quindi intanto mi ricavo eventuali ruoli custom di questo tenant (ovvero ruoli che non sono presenti nel tenant Master):
    let roleIds = [];
    let masterRoles = await this.store.query('role', {
      filter: `equals(tenantId,'1')`,
    });
    masterRoles = masterRoles.map((item) => item.name);

    let myRoles = await this.store.query('role', {
      filter: `equals(tenantId,'${this.currentTenant}')`,
    });
    myRoles = myRoles.map((item) => {
      return { id: item.id, name: item.name };
    });

    myRoles = myRoles.filter((item) => {
      return !masterRoles.includes(item.name);
    });
    if (myRoles && myRoles.length > 0) {
      roleIds = [...myRoles.map((item) => item.id)];
    }
    // ora ho gli id dei ruoli custom di questo tenant, a cui aggiungo quelli in rolesForEditUsers
    let userIds = [];
    try {
      let rolesForEditUsers = this.stp.siteSetup.rolesForEditUsers
        .split(',')
        .filter((item) => {
          return item && item !== null && item !== undefined && item !== '';
        })
        .map((item) => `${item.trim()}`);
      let qp = '';
      if (rolesForEditUsers.length > 0) {
        if (rolesForEditUsers.length === 1) {
          qp = `and(equals(tenantId,'1'),equals(name,'${rolesForEditUsers[0]}'))`;
        } else {
          let sq = rolesForEditUsers.map((item) => `equals(name,'${item}')`);
          qp = `and(equals(tenantId,'1'),or(${sq.join(',')}))`;
        }
        let setupRole = await this.store.query('role', {
          filter: qp,
        });
        if (setupRole && setupRole.length > 0) {
          roleIds = [
            ...new Set([...roleIds, ...setupRole.map((item) => item.id)]),
          ];
        }
      }

      // roleIds contiene i ruoli del tenant corrente di cui visualizzare gli utenti
      if (roleIds.length === 1) {
        qp = `and(equals(tenantId,'${this.currentTenant}'),equals(roleId,'${roleIds[0]}'))`;
      } else {
        let sq = roleIds.map((item) => `equals(roleId,'${item}')`);
        qp = `and(equals(tenantId,'${this.currentTenant}'),or(${sq.join(
          ','
        )}))`;
      }
      userIds = await this.store.query('user-role', {
        filter: qp,
      });
      userIds = userIds.map((item) => item.userId);

      // nella select di scelta ruolo ho bisogno dei nomi e degli id dei ruoli roleIds
      let tenantRoles = await this.store.query('role', {
        filter: `any(id,'${roleIds.join("','")}')`,
      });
      tenantRoles = tenantRoles.map((item) => {
        return { id: item.id, name: item.name };
      });
      this.tenantRoles = tenantRoles;
      // eslint-disable-next-line no-empty
    } catch (e) {}

    // ora estraggo gli utenti
    this.records = [];

    let search = JSON.parse(JSON.stringify(this.search)); // copia profonda, senza riferimento
    if (this.recordsPerPage !== '') {
      search.page.size = this.recordsPerPage;
      search.page.number = this.currentPageNumber;
    }

    let filters = search.filter;

    // aggiungo il filtro sugli id degli utenti trovati in precedenza
    filters.push({
      value: `any(userId,'${userIds.join("','")}')`,
    });

    // elimino e reinserisco il filtro su TenantId
    filters = filters.filter((obj) => obj.column !== 'tenantId');
    filters.push({
      function: 'equals',
      column: 'tenantId',
      value: this.currentTenant,
    });

    // aggiungo eventuale filtro testuale
    if (this.searchText !== '') {
      filters.push({
        value: `or(contains(user.userProfile.lastName,'${this.searchText}'),contains(user.userProfile.firstName,'${this.searchText}'),contains(user.email,'${this.searchText}'),contains(tenant.name,'${this.searchText}'))`,
      });
    }
    search.filter = filters;

    let query = this.jsonApi.queryBuilder(search);
    let userTenants = await this.store.query('user-tenant', query);

    if (!userTenants.length) {
      this.records = [];
      this.available = 'available';
      return;
    }

    // Aggiungi l'informazione su user e tenant a ciascun oggetto utente
    let users = await Promise.all(
      userTenants.map(async (userTenant) => {
        let user = await userTenant.get('user');
        let tenant = userTenant.get('tenant');
        return { user, tenant };
      })
    );

    // Aggiungi l'informazione user-role a ciascun oggetto utente
    let urs = await this.store.query('user-role', {
      filter: `and(equals(tenantId,'${
        this.currentTenant
      }'),any(roleId,'${roleIds.join("','")}'))`,
    });
    users.map((user) => {
      let hisRole = urs.filter((us) => {
        return us.userId === user.user.id;
      });
      hisRole = hisRole && hisRole.length > 0 ? hisRole[0] : [];
      return (user.userRole = hisRole);
    });

    this.records = users;
    this.totalRecords = userTenants.meta.total;
    this.available = 'available';
  });

  @action
  changePageNumber(currentPageNumber) {
    this.currentPageNumber = parseInt(currentPageNumber);
    this.start.perform();
  }
  @action
  changePageSize(event) {
    this.recordsPerPage = event.target.value;
    this.start.perform();
  }

  // numero di utenti
  get recordsLength() {
    return this.records.length;
  }

  // abilito modifica di un record
  editRecord = task({ drop: true }, async (record, roleId) => {
    this.newRole = await this.store.queryRecord('user-role', {
      filter: `and(equals(tenantId,'${this.currentTenant}'),equals(roleId,'${roleId}'),equals(userId,'${record.id}'))`,
    });
    this.editingRecord = record; // faccio rimanere il solo pulsante relativo al record selezionato
    await this.store
      .queryRecord('user', {
        include: 'userProfile',
        filter: `equals(id,'${record.id}')`,
      })
      .then((rec) => {
        rec.get('userProfile').then((userProfile) => {
          let userProfileData = userProfile.serialize();

          this.newValue = this.initializeRecord('user-profile');
          this.oldValue = this.initializeRecord('user-profile');
          Object.keys(userProfileData.data.attributes).forEach((key) => {
            if (key === 'birthDate') {
              let val = null;
              if (userProfileData.data.attributes[key]) {
                let offset = new Date().getTimezoneOffset(); // sottraggo offset del fuso orario
                val = new Date(
                  new Date(userProfileData.data.attributes[key]).getTime() -
                    offset * 60 * 1000
                );
              }
              this.newValue[key] = val;
              this.oldValue[key] = val;
            } else {
              this.newValue[key] = userProfileData.data.attributes[key];
              this.oldValue[key] = userProfileData.data.attributes[key];
            }
          });
          this.newValue.id = userProfile.id;
          this.newValue.email = record.email;
        });
      })
      .catch((e) => {
        console.error(e);
        this.editingRecord = null;
        this.dialogs.toast(
          'Si è verificato un errore. Riprova!',
          'error',
          'bottom-right',
          3
        );
      });

    this.listUserSize = 'd-none';
    this.editUserSize = 'fadeInRight';
  });

  // annullo modifica di un record
  @action
  undoRecord() {
    this.store.findRecord('user', this.editingRecord.id, {
      reload: true,
    });
    this.editingRecord = null;
    this.newValue = this.initializeRecord('user-profile');
    this.oldValue = this.initializeRecord('user-profile');
    this.newRole = null;
    this.listUserSize = 'fadeInLeft';
    this.editUserSize = 'd-none';
  }

  // cattura il valore inserito dall'utente per la creazione/modifica di un record
  @action
  storeNewValue(field, event) {
    let val = '';
    if (field === 'birthDate') {
      try {
        val = new Date(event.target.value + 'T00:00:00Z'); // Crea un oggetto data con orario impostato a mezzanotte
      } catch (e) {
        console.error(e);
        val = null;
      }
    } else val = event.target.value.trim();

    this.newValue[field] = val;
  }

  // cattura variazioni al ruolo di un utente
  @action
  storeNewRole(event) {
    let val = event.target.value.trim();
    this.newRole.roleId = val;
  }

  // Applica select2 e ne gestisce la variazione
  @action
  setupSelect2(element) {
    $(element).select2({
      theme: 'bootstrap4',
      width: '100%',
    });

    // Make sure to sync changes made by Select2 with Ember
    $(element).on('change', () => {
      let field = $(element).attr('data-field');
      let selectedValue = $(element).val();
      this.newValue[field] = selectedValue;
    });
  }

  @action
  teardownSelect2(element) {
    $(element).off('change');
    $(element).select2('destroy');
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);
    // Rimuovi il listener quando il componente viene distrutto
    this.teardownSelect2('#usersCrudBirthState');
    this.teardownSelect2('#usersCrudResidenceState');
  }

  // chiede conferma per salvataggio modifiche a un record
  @action
  saveVoice() {
    let errors = 0;
    Object.keys(this.registrationFields).forEach((key) => {
      let required = this.registrationFields[key] === '2';
      if (required) {
        if (!this.newValue[key] || this.newValue[key].toString() === '') {
          errors++;
        }
      }
    });

    if (errors > 0) {
      this.dialogs.toast(
        `Tutti i campi contrassegnati dall'asterisco sono obbligatori`,
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    let regex;
    errors = [];

    // verifica lunghezza nome e cognome
    if (
      (this.newValue.lastName !== '' && this.newValue.lastName.length < 2) ||
      (this.newValue.firstName !== '' && this.newValue.firstName.length < 2)
    ) {
      errors.push(`Inserisci almeno due caratteri nei campi Nome e Cognome`);
    }

    // verifica del codice fiscale
    regex = this.jsUtility.regex('taxId');
    if (
      this.registrationFields.taxId == '2' &&
      this.newValue.taxId !== '' &&
      !regex.test(this.newValue.taxId)
    ) {
      errors.push(`Il formato del campo 'Codice Fiscale' non è corretto`);
    }

    // TODO : aggiungere verifiche e blocchi sull'eventuale campo nick-name

    if (errors.length > 0) {
      this.dialogs.toast(errors[0], 'error', 'bottom-right', 4);
      return false;
    }

    let self = this;
    this.dialogs.confirm(
      '<h6>Modifica utente</h6>',
      `<p>Azione irreversibile. Confermi la modifica?</p>`,
      () => {
        self.updateUser.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  // aggiorna i dati dell'utente nel DB
  updateUser = task({ drop: true }, async () => {
    let self = this;
    try {
      let existingUser = await this.store.findRecord(
        'user-profile',
        this.newValue.id
      );

      Object.keys(self.newValue).forEach((key) => {
        if (key !== 'id') {
          existingUser[key] = self.newValue[key];
        }
      });

      await existingUser.save();
      await this.newRole.save();
      this.start.perform();
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprova!`,
        'error',
        'bottom-right',
        4
      );
    }
  });

  @action
  insertFilter(event) {
    this.searchText = event.target.value.trim();
    this.records = [];
    this.start.perform();
  }

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord(entity) {
    let modelInstance = this.store.createRecord(entity); // Crea una nuova istanza del modello (ad esempio, 'user')
    // Crea un semplice oggetto JavaScript con tutte le proprietà del modello
    let modelData = {};
    modelInstance.eachAttribute((key) => {
      modelData[key] = modelInstance.get(key);
    });

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }

  // cancella utente dal DB
  @action
  removeUser() {
    this.dialogs.confirm(
      '<h4 class="text-danger">ATTENZIONE</h4>',
      `<h5 class="text-danger">CANCELLAZIONE DEFINITIVA DELL'UTENTE</h5>
       <h6 class="text-danger">Azione irreversibile.<br /><br />CONFERMI?</h6>`,
      () => {
        this.removeUserConfirmed.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  removeUserConfirmed = task({ drop: true }, async () => {
    await this.store
      .queryRecord('user', {
        filter: `equals(id,'${this.editingRecord.id}')`,
      })
      .then(async (user) => {
        await user.destroyRecord();
        this.start.perform();

        this.editingRecord = null;
        this.newValue = this.initializeRecord('user-profile');
        this.oldValue = this.initializeRecord('user-profile');

        this.listUserSize = 'fadeInLeft';
        this.editUserSize = 'd-none';
        if (this.triggerRefreshFn) this.triggerRefreshFn();
      })
      .catch((e) => {
        console.error(e);
        this.dialogs.toast(
          'Si è verificato un errore. Riprova!',
          'error',
          'bottom-right',
          3
        );
      });
  });

  @action
  updateSeeAllTenantsUsers(event) {
    // TODO
  }

  // bannaggio utente
  @action
  ban(days) {}
}
