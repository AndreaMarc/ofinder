import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
//import { htmlSafe } from '@ember/template';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';

export default class StandardCorePermissionsMasterComponent extends Component {
  @service permissions;
  @service session;
  @service store;

  @tracked serviceAvailable = 'waiting';
  @tracked tenantSelected = '';
  @tracked availableTenants = [];
  @tracked error = '';

  // seguono le tracked per aggiornamento dati tra i sotto-componenti
  @tracked lastUpdateRoles = 0;

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    this.tenantSelected = this.session.get('data.tenantId').toString();
    this.serviceAvailable = 'waiting';
    try {
      let userTenants = this.session.get('data.associatedTenants');
      if (userTenants.length === 0) {
        throw new Error(`Errore generale, impossibile accedere.`);
      }

      this.findRecords.perform();
    } catch (e) {
      this.error = e;
      this.serviceAvailable = 'unavailable';
    }
  }

  findRecords = task({ drop: true }, async () => {
    console.log('AGGIORNO TENANTS');
    try {
      if (this.permissions.hasPermissions(['canSeeAllTenants'])) {
        // l'utente può accedere a tutti i tenant
        await this.store
          .query('tenant', {
            sort: `name`,
          })
          .then((res) => {
            this.availableTenants = res;
            this.serviceAvailable = 'available';
          })
          .catch(() => {
            throw new Error(`Errore nell'estrazione delle aziende.`);
          });
      } else {
        // può accedere ai suoi soli tenant
        let tenantIds = [];
        let userTenants = this.session.get('data.associatedTenants');
        userTenants.forEach((element) => {
          tenantIds.push(`'${element.tenantId}'`);
        });

        await this.store
          .query('tenant', {
            filter: `any(id,${tenantIds.join()})`,
            sort: `name`,
          })
          .then((res) => {
            this.availableTenants = res;
            this.serviceAvailable = 'available';
          })
          .catch(() => {
            throw new Error(`Errore nell'estrazione delle aziende.`);
          });
      }
    } catch (e) {
      console.error(e);
      this.error = 'Si è verificato un errore.';
      this.serviceAvailable = 'unavailable';
    }
  });

  @action
  updateTenant(key) {
    this.tenantSelected = key;
  }

  // azione chiamata dai componenti figli per comunicare il cambiamento dei dati agli altri componenti
  @action
  updateRole() {
    this.lastUpdateRoles = new Date().getTime();
  }

  //array per selectTwo
  get selectTenantList() {
    let ct = this.session.get('data.tenantId').toString();
    return JSON.stringify(
      this.availableTenants.map((tenant) => {
        let x = {
          id: tenant.id,
          value: tenant.name,
        };

        if (x.id.toString() === ct) {
          x.selected = true;
          //this.updateTenant(x.id);
        }
        return x;
      })
    );
  }
}
