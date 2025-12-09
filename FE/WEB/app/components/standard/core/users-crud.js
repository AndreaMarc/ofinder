/* eslint-disable ember/jquery-ember-run */
/* eslint-disable ember/no-jquery */
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

export default class StandardCoreUsersCrudComponent extends Component {
  @service('siteSetup') stp;
  @service permissions;
  @service jsUtility;
  @service jsonApi;
  @service session;
  @service dialogs;
  @service store;

  modelName = 'user-profile';
  @tracked listUserSize = 'fadeInLeft';
  @tracked editUserSize = 'd-none';
  @tracked available = 'waiting';
  @tracked records = [];
  @tracked tenants = [];
  @tracked editingRecord = null;
  @tracked currentTenant = null;
  @tracked filterText = '';
  @tracked registrationFields = null;
  @tracked nations = [];

  @tracked oldValue = null;
  @tracked newValue = null; // usato nella modifica del record

  @tracked seeAllTenantsUsers = false;
  methodUpdateVisibility = null;
  triggerRefreshFn = null;
  searchText = '';

  search = {
    page: {},
    filter: [
      {
        function: 'any',
        column: 'state',
        value: `'accepted','ownerCreated','selfCreated'`,
      },
    ], // { function: 'contains', column: 'tag', value: 'valoreX' }
    sort: `user.userProfile.lastName,user.userProfile.firstName,user.email,-createdAt`,
    include: 'tenant,user.userProfile,user.bannedUsers', //
  };
  @tracked currentPageNumber = 1;
  @tracked totalRecords = 0;
  @tracked recordsPerPage = '20';
  @tracked bannedIds = [];

  constructor(...attributes) {
    super(...attributes);
    this.registrationFields =
      this.stp.siteSetup.registrationFields.registration;
    this.currentTenant = this.session.get('data.tenantId');
    this.nations = this.jsUtility.nations();

    this.methodUpdateVisibility = this.args.methodUpdateVisibility || null;
    this.triggerRefreshFn = this.args.triggerRefreshFn || null;
    this.start.perform();
  }

  start = task({ drop: true }, async () => {
    try {
      this.seeAllTenantsUsers = this.args.seeAllTenantsUsers;
      this.records = [];

      let search = JSON.parse(JSON.stringify(this.search)); // copia profonda, senza riferimento
      if (this.recordsPerPage !== '') {
        search.page.size = this.recordsPerPage;
        search.page.number = this.currentPageNumber;
      }

      let filters = search.filter;
      // elimino l'eventuale filtro su TenantId
      filters = filters.filter((obj) => obj.column !== 'tenantId');

      if (!this.seeAllTenantsUsers) {
        filters.push({
          function: 'equals',
          column: 'tenantId',
          value: this.currentTenant,
        });
      }
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

      // estraggo anche l'elenco dei bannati
      let search2;
      if (!this.seeAllTenantsUsers) {
        search2 = {
          filter: [
            {
              value: `and(equals(lockActive,'true'),greaterThan(lockEnd,'${new Date().toISOString()}'))`,
            },
            {
              function: 'equals',
              column: 'tenantId',
              value: this.currentTenant,
            },
          ],
        };
      } else {
        search2 = {
          filter: [
            {
              value: `and(equals(lockActive,'true'),greaterThan(lockEnd,'${new Date().toISOString()}'))`,
            },
          ],
        };
      }
      let query2 = this.jsonApi.queryBuilder(search2);
      let banned = await this.store.query('banned-user', query2);
      this.bannedIds = banned.map((record) => record.get('userId'));
      // fine estrazione utenti bannati
      this.records = users;
      this.totalRecords = userTenants.meta.total;
      this.available = 'available';

      setTimeout(() => {
        this.setupSelect2('#usersCrudBirthState');
        this.setupSelect2('#usersCrudResidenceState');
      }, 80);
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
  });

  @action
  updateSeeAllTenantsUsers(event) {
    let value = event.target.checked;
    if (this.methodUpdateVisibility) {
      this.methodUpdateVisibility(value);
      this.currentPageNumber = 1;
    }
  }

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
  editRecord = task({ drop: true }, async (record) => {
    this.editingRecord = record; // faccio rimanere il solo pulsante relativo al record selezionato
    await this.store
      .queryRecord('user', {
        include: 'userProfile',
        filter: `equals(id,'${record.id}')`,
      })
      .then((rec) => {
        rec.get('userProfile').then((userProfile) => {
          let userProfileData = userProfile.serialize();

          this.newValue = this.initializeRecord();
          this.oldValue = this.initializeRecord();
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
    this.newValue = this.initializeRecord();
    this.oldValue = this.initializeRecord();

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

  // cancella associazione utente-tenant
  @action
  removeAssociation() {
    this.dialogs.confirm(
      '<h4 class="text-danger">ATTENZIONE</h4>',
      `<h5 class="text-danger">STAI ELIMINANDO L'UTENTE</h5>
      <h6 class="text-danger">Azione irreversibile.<br /><br />CONFERMI?</h6>`,
      () => {
        this.removeAssociationConfirmed.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  removeAssociationConfirmed = task({ drop: true }, async () => {
    await this.store
      .queryRecord('user-tenant', {
        filter: `and(equals(userId,'${this.newValue.userId}'),equals(tenantId,'${this.currentTenant}'))`,
      })
      .then(async (userTenant) => {
        await userTenant.destroyRecord();
        this.start.perform();

        this.editingRecord = null;
        this.newValue = this.initializeRecord();
        this.oldValue = this.initializeRecord();

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
        this.newValue = this.initializeRecord();
        this.oldValue = this.initializeRecord();

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

  // bannaggio utente
  @action
  ban(days) {
    // eslint-disable-next-line no-undef
    Swal.fire({
      title: 'ATTENZIONE',
      html: `Stai bannando l'utente per ${days} giorni.<br />Non potrà più accedere all'applicativo fino alla scadenza del blocco.<br />Sei sicuro?`,
      icon: 'warning',
      showDenyButton:
        this.permissions.hasPermissions(['isOwner']) ||
        this.permissions.hasPermissions(['isDeveloper']) ||
        this.permissions.hasPermissions(['isSuperAdmin'])
          ? true
          : false,
      showCancelButton: true,
      confirmButtonText: 'Banna',
      denyButtonText: `Banna su tutte le aziende`,
      cancelButtonText: 'Annulla',
      denyButtonColor: '#dc3741',
      cancelButtonColor: '#6c757d',
      customClass: {
        confirmButton: 'btn btn-hover-shine',
        denyButton: 'btn btn-hover-shine',
        cancelButton: 'btn btn-hover-shine',
      },
    }).then((result) => {
      /* Read more about isConfirmed, isDenied below */
      if (result.isConfirmed) {
        this.banConfirmed.perform(days, false);
      } else if (result.isDenied) {
        this.banConfirmed.perform(days, true);
      }
    });
  }

  banConfirmed = task({ drop: true }, async (days, crossTenants) => {
    try {
      let now = new Date();
      let expiration = new Date(now.getTime() + days * 24 * 60 * 60 * 1000);

      let record = this.store.createRecord('banned-user', {
        id: v4(),
        userId: this.editingRecord.id,
        supervisorId: this.session.get('data.id'),
        lockStart: now, //.toISOString(),
        lockEnd: expiration, //.toISOString(),
        lockActive: true,
        crossTenantBanned: crossTenants,
        tenantId: parseInt(this.session.get('data.tenantId')),
        lockDays: days,
      });
      await record.save();

      this.editingRecord = null;
      this.newValue = this.initializeRecord();
      this.oldValue = this.initializeRecord();

      this.listUserSize = 'fadeInLeft';
      this.editUserSize = 'd-none';
      this.start.perform();
      if (this.triggerRefreshFn) this.triggerRefreshFn();
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        3
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
}
