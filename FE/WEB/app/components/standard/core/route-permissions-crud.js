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

export default class StandardCoreRoutePermissionsCrudComponent extends Component {
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
  @tracked routesList = [];
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
    //this.findTenants.perform(); // Avvia la task.
    this.findRoles.perform();
  }

  findRoles = task({ drop: true }, async () => {
    try {
      if (!this.currentTenant.id) return;
      this.roles = [];
      this.currentRole = '';

      this.roles = await this.store.query('role', {
        filter: `equals(tenantId,'${parseInt(this.currentTenant.id)}')`,
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
      this.routesList = [];
      this.records = [];
      if (!this.currentRole) return;

      $(`.route-perms`).bootstrapToggle('destroy');

      this.records = await this.store.query(this.modelName, {
        filter: `and(equals(roleId,'${this.currentRole}'),equals(claimType,'route'))`,
      });

      try {
        this.routesList = this.stp.siteSetup.routesList;
      } catch (e) {
        throw new Error();
      }

      this.routesList.map((obj) => {
        let associatedClaims = this.records.filter((item) => {
          return item.claimValue.includes(`r-p-${obj.route}`);
        });
        obj.associatedClaim = associatedClaims.length > 0;
      });
      // console.log(this.routesList);
    } catch (e) {
      this.available = false;
    }
  });

  get filteredRoutes() {
    if (this.filterText) {
      return this.routesList.filter((route) =>
        route.title.toLowerCase().includes(this.filterText.toLowerCase())
      );
    } else {
      return this.routesList;
    }
  }

  get routesLength() {
    return this.routesList.length;
  }

  @action
  bootstrapToggle(rowIndex) {
    if (rowIndex === this.routesList.length - 1) {
      $(`.route-perms`).bootstrapToggle('destroy');

      setTimeout(() => {
        $(`.route-perms`).bootstrapToggle();
      }, 20);
    }
  }

  @action
  save() {
    let self = this;
    this.dialogs.confirm(
      '<h6>Aggiornamento dei permessi di ROTTA</h6>',
      `<h5 class="text-danger">ATTENZIONE!</h5>
      <h6>Controlla scrupolosamente le impostazioni selezionate<br /><br />Confermi?</h6>`,
      () => {
        self.saveConfirmed.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  // crea un nuovo record
  saveConfirmed = task({ drop: true }, async () => {
    // costruisco il json da passare al BE
    let out = {
      roleId: this.currentRole,
      claimsType: 'route',
      claims: [],
    };
    let $elements = $('.route-perms:checked');
    if ($elements.length > 0) {
      $.each($elements, function () {
        let $this = $(this);
        let permValue = $this.attr('data-route');
        out.claims.push(permValue);
      });
    }

    this.entities = [];
    let self = this;

    await this.fetch
      .call('roleClaims/route', 'POST', out, {}, true, this.session)
      .then(() => {
        self.dialogs.toast('Operazione riuscita', 'success', 'bottom-right', 2);
      })
      .catch(() => {
        self.dialogs.toast(
          'Si è verificato un errore. Riprova!',
          'error',
          'bottom-right',
          5
        );
      });
  });

  //delete

  @action
  insertFilter(event) {
    this.filterText = event.target.value;
  }

  @action
  encode(value) {
    return encodeURI(value);
  }
}
