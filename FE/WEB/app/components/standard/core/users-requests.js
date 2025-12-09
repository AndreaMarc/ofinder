/* eslint-disable no-undef */
/* eslint-disable ember/jquery-ember-run */
/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';
//import config from 'poc-nuovo-fwk/config/environment';
//import { v4 } from 'ember-uuid';
export default class StandardCoreUsersRequestsComponent extends Component {
  @service jsUtility;
  @service session;
  @service dialogs;
  @service jsonApi;
  @service store;
  @service fetch;

  modelName = 'user-tenant';
  @tracked available = 'waiting';
  @tracked records = [];
  @tracked filterText = '';
  @tracked state = 'pending';
  @tracked seeAllTenantsUsers = false;

  @tracked currentPageNumber = 1;
  @tracked totalRecords = 0;
  @tracked recordsPerPage = '10';
  methodUpdateVisibility = null;
  searchText = '';
  search = {
    page: { size: 5, number: 1 },
    filter: [], // { function: 'contains', column: 'tag', value: 'valoreX' }
    sort: `user.userProfile.lastName,user.userProfile.firstName,user.email`,
    include: 'user.userProfile',
  };

  constructor(...attributes) {
    super(...attributes);
    this.currentTenant = this.session.get('data.tenantId');
    this.methodUpdateVisibility = this.args.methodUpdateVisibility || null;
    this.start.perform();
  }

  @action
  updateSeeAllTenantsUsers(event) {
    let value = event.target.checked;
    if (this.methodUpdateVisibility) {
      this.methodUpdateVisibility(value);
    }
  }

  start = task({ drop: true }, async () => {
    try {
      this.seeAllTenantsUsers = this.args.seeAllTenantsUsers;

      // elimino l'eventuale filtro su TenantId
      let search = JSON.parse(JSON.stringify(this.search)); // copia profonda, senza riferimento
      search.page.size = this.recordsPerPage;
      search.page.number = this.currentPageNumber;

      let filters = this.search.filter;
      this.filters = filters.filter((obj) => obj.column !== 'tenantId');
      if (!this.seeAllTenantsUsers) {
        this.filters.push({
          function: 'equals',
          column: 'tenantId',
          value: this.session.get('data.tenantId'),
        });
        search.filter = this.filters;
      }

      // imposto il filtraggio per stato (pending, rifiutate ecc)
      if (this.state === '') {
        search.filter.push({
          function: 'any',
          column: 'state',
          value: `'accepted','pending','denied'`,
        });
      } else {
        search.filter.push({
          function: 'equals',
          column: 'state',
          value: this.state,
        });
      }

      // aggiungo eventuale filtro testuale
      if (this.searchText !== '') {
        search.filter.push({
          value: `or(contains(user.userProfile.lastName,'${this.searchText}'),contains(user.userProfile.firstName,'${this.searchText}'),contains(user.email,'${this.searchText}'))`,
        });
      }

      let query = this.jsonApi.queryBuilder(search);
      let userTenants = await this.store.query('user-tenant', query);

      if (!userTenants.length) {
        this.records = [];
        this.available = 'available';
        return;
      }

      // estraggo i dati degli utenti
      let usersPromises = userTenants.map((userTenant) =>
        userTenant.get('user')
      );
      await Promise.all(usersPromises);
      this.records = userTenants;
      this.totalRecords = userTenants.meta.total;
      this.available = 'available';
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
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

  get filteredRecords() {
    let newRecords = this.records;

    if (this.filterText) {
      newRecords = newRecords.filter((record) => {
        const user = record.get('user.content');
        const userProfile = user ? user.get('userProfile.content') : null;

        return (
          (userProfile &&
            userProfile.lastName &&
            userProfile.lastName
              .toLowerCase()
              .includes(this.filterText.toLowerCase())) ||
          (userProfile &&
            userProfile.firstName &&
            userProfile.firstName
              .toLowerCase()
              .includes(this.filterText.toLowerCase())) ||
          (user &&
            user.email &&
            user.email.toLowerCase().includes(this.filterText.toLowerCase()))
        );
      });
    }

    return newRecords;
  }

  get recordsLength() {
    return this.records.length;
  }

  @action
  insertFilter(event) {
    this.searchText = event.target.value.trim();
    this.records = [];
    this.start.perform();
  }

  @action
  changeState(event) {
    this.state = event.target.value.trim();
    this.start.perform();
  }

  sendAgain = task({ drop: true }, async (userTenantId) => {
    let obj = {
      userTenantId: userTenantId,
    };

    await this.fetch
      .call('permission/send-again', 'POST', obj, {}, true, this.session)
      .then(() => {
        // operazione riuscita
        this.start.perform();

        Swal.fire({
          title: 'Operazione riuscita',
          text: `L'utente riceverà una nuova e-mail con le istruzioni per autorizzarti ad accedere ai suoi dati`,
          icon: 'success',
          showCancelButton: false,
          confirmButtonText: 'Accedi',
        });
      })
      .catch((e) => {
        console.error(e);
        this.dialogs.toast(
          `Si è verificato un errore. Riprovare!`,
          'error',
          'bottom-right',
          4
        );
      });
  });

  // helper locale di visualizzazione data
  formatDate = (string) => {
    if (!string || string === '' || string === '1990-01-01T00:00:00') {
      return '--';
    }

    let format = {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    };
    return this.jsUtility.data(string, format);
  };
}
