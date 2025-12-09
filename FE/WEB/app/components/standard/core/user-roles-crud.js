/**
 * Consente di accociare gli utenti ai ruoli
 */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !

export default class StandardCoreUserRolesCrudComponent extends Component {
  @service permissions;
  @service jsonApi;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  @tracked available = true;
  @tracked serviceUpperAvailable = 'waiting';
  @tracked currentTenant = null;
  @tracked myRoles = [];
  @tracked usersTenant1 = [];
  @tracked findUser = new TrackedObject({
    currentUserPageNumber: 1,
    usersPerPage: 20,
    totalUsers: 0,
    searchedWord: '',
  });
  @tracked userToAdd = null;
  @tracked rolesToAdd = [];
  userRoleChoice = '';
  @tracked currentRole = '';
  @tracked records = [];
  @tracked listUserToAddRole = '';

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  changeUserFilter(key, event) {
    let obj = this.findUser;
    obj[key] = event.target.value.trim();
    this.findUser = new TrackedObject(obj);
    this.findUsers.perform();
  }

  @action
  async start() {
    this.serviceUpperAvailable = 'waiting';
    let currentTenant = this.args.currentTenant ? this.args.currentTenant : '';
    if (currentTenant === '') return false;

    this.currentTenant = await this.store.peekRecord('tenant', currentTenant);

    // per tutti gli utenti che non sono super-admin, estraggo elenco dei propri ruoli e
    // potranno gestire (assegnare, eliminare) solo quelli che possiedono
    if (!this.permissions.hasPermissions(['isSuperAdmin'])) {
      let roles = await this.fetch.call(
        'roleClaims/getAssignableClaims',
        'GET',
        null,
        {},
        true,
        this.session
      );
      this.myRoles = roles.roles.split(',');
    }

    this.available = true;
    this.findRoles.perform();
    this.findUsers.perform();
  }

  // ricava l'elenco di tutti i ruoli del tenant corrente
  findRoles = task({ drop: true }, async () => {
    try {
      if (!this.currentTenant || !this.currentTenant.id) return;
      this.currentRole = '';

      this.roles = await this.store.query('role', {
        filter: `equals(tenantId,'${parseInt(this.currentTenant.id)}')`,
        sort: `name`,
      });

      // chi non è superadmin può aggiungere i soli ruoli di sua competenza
      if (!this.permissions.hasPermissions(['isSuperAdmin'])) {
        this.roles = this.roles.filter((item) => {
          return this.myRoles.indexOf(item.id) > -1;
        });
      }

      this.serviceUpperAvailable = 'available';
    } catch (e) {
      console.error('Error in findRoles:', e);
      this.available = false;
    }
  });

  // ricava l'elenco degli utenti con tutti i ruoli a cui appartengono
  findUsers = task({ drop: true }, async () => {
    try {
      this.usersTenant1 = [];
      if (!this.currentTenant || !this.currentTenant.id) return;

      let query = {
        filter: [
          {
            function: 'equals',
            column: 'tenantId',
            value: this.currentTenant.id,
          },
          {
            function: 'equals',
            column: 'user.deleted',
            value: 'false',
          },
        ],
        page: {
          size: this.findUser.usersPerPage,
          number: this.findUser.currentUserPageNumber,
        },
        sort: 'user.userProfile.lastName,user.userProfile.firstName',
        include: `user.userProfile,user.userRoles.role,user`,
      };

      if (this.findUser.searchedWord !== '') {
        query.filter.push({
          function: null,
          value: `or(contains(user.userProfile.lastName,'${this.findUser.searchedWord}'),contains(user.userProfile.firstName,'${this.findUser.searchedWord}'),contains(user.email,'${this.findUser.searchedWord}'))`,
        });
      }

      this.usersTenant1 = await this.store.query(
        'userTenant',
        this.jsonApi.queryBuilder(query)
      );

      this.findUser.totalUsers = this.usersTenant1.meta.total;
      this.serviceUpperAvailable = 'available';
    } catch (e) {
      console.error('Error in findUsers:', e);
      this.available = false;
    }
  });

  // area "cerca per utente" - aggiunge ruolo ad utente
  // crea la lista di ruoli da mostrare nella select
  @action
  async addRole(user) {
    if (user) {
      let userId = user.get('id');
      let haveRoles = await this.store.query('user-role', {
        filter: `and(equals(tenantId,'${this.currentTenant.id}'),equals(userId,'${userId}'))`,
      });

      if (haveRoles.length > 0) {
        let rta = this.roles.filter(
          (obj1) => !haveRoles.some((obj2) => obj2.roleId === obj1.id)
        );

        rta.sort((a, b) => a.name.localeCompare(b.name));
        this.rolesToAdd = rta;
      } else {
        this.rolesToAdd = this.roles;
      }
      this.userToAdd = user;
    } else {
      this.userToAdd = null;
    }
  }

  // cancella associazione utente-ruolo
  @action
  delUserRole(userRoleRecord) {
    let self = this;
    this.dialogs.confirm(
      '<h6>Rimozione ruolo</h6>',
      `<p class="text-danger">Confermi la rimozione del ruolo per l'utente?</p>`,
      () => {
        self.delUserRoleConfirmed.perform(userRoleRecord);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  delUserRoleConfirmed = task({ drop: true }, async (userRoleRecord) => {
    this.userRoleChoice = '';
    await userRoleRecord.destroyRecord();
    this.findUsers.perform();
  });

  // selezione del ruolo da aggiungere all'utente
  @action
  newUserRoleChoice(event) {
    let value = event && event.target ? event.target.value : event;
    this.userRoleChoice = value;
  }

  // salva il ruolo aggiunto all'utente
  @action
  saveUserRole() {
    let self = this;
    this.dialogs.confirm(
      '<h6>Aggiungi ruolo</h6>',
      `<p>Confermi l'associazione del nuovo ruolo all'utente?</p>`,
      () => {
        self.saveUserRoleConfirmed.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  saveUserRoleConfirmed = task({ drop: true }, async () => {
    let self = this;
    if (this.userRoleChoice === '') {
      this.dialogs.toast('Scegli il ruolo!', 'error', 'bottom-right', 4);
      return;
    }

    try {
      let newR = await this.store.createRecord('user-role', {
        id: v4(),
        roleId: this.userRoleChoice,
        userId: this.userToAdd.get('id'),
        tenantId: this.currentTenant.id,
      });
      newR.save();
      this.userToAdd = null;
      this.rolesToAdd = [];

      setTimeout(() => {
        self.findUsers.perform();
        self.findRecords.perform();
      }, 200);
    } catch (e) {
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        4
      );
    }
  });

  /**
   * ***********************
   * AREA 2: CERCA PER RUOLO
   * ***********************
   */

  // consente il cambio ruolo
  @action
  async updateRole(event) {
    this.currentRole = event.target.value;
    this.findRecords.perform();
  }

  // ricava il ruolo specificato con relative associazioni di utenze
  findRecords = task({ drop: true }, async () => {
    try {
      if (this.currentRole === '') return;
      this.records = [];
      /*
      this.records = await this.store.query('role', {
        filter: `and(equals(id,'${this.currentRole}'),equals(tenantId,'${this.currentTenant.id}'))`,
        include: 'userRoles.user',
      });
      */

      this.records = await this.store.query('user-role', {
        filter: `and(equals(tenantId,'${this.currentTenant.id}'),equals(roleId,'${this.currentRole}'))`,
        include: 'user.userProfile',
      });

      this.freeUsersToAddRole();
    } catch (e) {
      console.error('Error in findRecords:', e);
      this.available = false;
    }
  });

  // dato il ruolo scelto, ricavo gli utenti liberi (cioè non ancora associati)
  async freeUsersToAddRole() {
    let userTenants = await this.store.query('user-tenant', {
      filter: `and(equals(tenantId,'${this.currentTenant.id}'),any(state,'accepted','ownerCreated','selfCreated'))`,
      include: `user.userProfile`,
      sort: `user.userProfile.lastName,user.userProfile.firstName`,
    });

    // Estrai gli id degli user già assegnati al ruolo corrente
    let assignedUserIds = new Set(
      this.records.map((ur) => ur.belongsTo('user').id())
    );
    console.log('assignedUserIds', assignedUserIds);

    // ricavo gli utenti liberi
    let candidates = [];
    for (let ut of userTenants.slice()) {
      let user = await ut.user;
      if (!assignedUserIds.has(user.id)) {
        candidates.push(user);
      } else {
        console.log(
          `Non aggiungo ${user.get('userProfile').get('lastName')} ${user
            .get('userProfile')
            .get('firstName')} con id '${user.id}'`
        );
      }
    }

    this.listUserToAddRole = JSON.stringify(
      candidates.map((item) => {
        return {
          id: item.id,
          value: `${item.get('userProfile').get('lastName')} ${item
            .get('userProfile')
            .get('firstName')} - ${item.email}`,
        };
      })
    );
  }

  // aggiunge il ruolo all'utente
  @action
  saveUserRole2() {
    if (this.userRoleChoice === '') {
      this.dialogs.toast(`Scegli l'utente!`, 'error', 'bottom-right', 4);
      return;
    }
    let self = this;
    this.dialogs.confirm(
      '<h6>Aggiungi ruolo</h6>',
      `<p>Confermi l'associazione del nuovo ruolo all'utente?</p>`,
      () => {
        self.saveUserRoleConfirmed2.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  saveUserRoleConfirmed2 = task({ drop: true }, async () => {
    let self = this;
    try {
      let newR = await this.store.createRecord('user-role', {
        id: v4(),
        roleId: this.currentRole,
        userId: this.userRoleChoice,
        tenantId: this.currentTenant.id,
      });
      await newR.save();

      self.findUsers.perform();
      self.findRecords.perform();
    } catch (e) {
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        4
      );
    }
  });
}
