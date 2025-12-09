import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

export default class StandardCoreUsersBannedComponent extends Component {
  @service jsonApi;
  @service session;
  @service dialogs;
  @service store;

  @tracked available = 'waiting';
  @tracked records = [];
  @tracked seeAllTenantsUsers = false;
  @tracked showHistory = false;
  methodUpdateVisibility = null;
  triggerRefreshFn = null;
  searchText = '';

  search = {
    page: { size: 5, number: 1 },
    filter: [], // { function: 'contains', column: 'tag', value: 'valoreX' }
    sort: `user.userProfile.lastName,user.userProfile.firstName,user.email`,
    include: 'user,user.userProfile',
  };
  @tracked currentPageNumber = 1;
  @tracked totalRecords = 0;
  @tracked recordsPerPage = '20';

  constructor(...attributes) {
    super(...attributes);
    this.currentTenant = this.session.get('data.tenantId');
    this.methodUpdateVisibility = this.args.methodUpdateVisibility || null;
    this.triggerRefreshFn = this.args.triggerRefreshFn || null;
    this.start.perform();
  }

  start = task({ drop: true }, async () => {
    try {
      this.seeAllTenantsUsers = this.args.seeAllTenantsUsers;
      this.records = [];

      let search = JSON.parse(JSON.stringify(this.search)); // copia profonda, senza riferimento
      search.page.size = this.recordsPerPage;
      search.page.number = this.currentPageNumber;

      if (!this.showHistory) {
        search.filter = [
          {
            value: `and(equals(lockActive,'true'),greaterThan(lockEnd,'${new Date().toISOString()}'))`,
          },
        ];
      } else {
        search.filter = [];
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
          value: `or(contains(user.userProfile.lastName,'${this.searchText}'),contains(user.userProfile.firstName,'${this.searchText}'),contains(user.email,'${this.searchText}'))`,
        });
      }
      search.filter = filters;

      let query = this.jsonApi.queryBuilder(search);
      let banneds = await this.store.query('banned-user', query);

      if (!banneds.length) {
        this.records = [];
        this.available = 'available';
        return;
      }

      this.records = banneds;
      this.totalRecords = 0;
      this.available = 'available';
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
  });

  isActive(record) {
    if (record.lockActive && new Date(record.lockEnd) > new Date()) {
      return 'active';
    } else return 'unactive';
  }

  @action
  updatehistory() {
    this.showHistory = !this.showHistory;
    this.start.perform();
  }

  @action
  updateSeeAllTenantsUsers(event) {
    let value = event.target.checked;
    if (this.methodUpdateVisibility) {
      this.methodUpdateVisibility(value);
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
  @action
  insertFilter(event) {
    this.searchText = event.target.value.trim();
    this.records = [];
    this.start.perform();
  }

  // rimuove il Ban
  @action
  removeBan(id) {
    this.dialogs.confirm(
      '<h4 class="text-danger">ATTENZIONE</h4>',
      `<h5 class="text-danger">STAI RIABILITANDO L'UTENTE</h5>
       <h6 class="text-danger">CONFERMI?</h6>`,
      () => {
        this.removeBanConfirmed.perform(id);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  removeBanConfirmed = task({ drop: true }, async (id) => {
    await this.store
      .findRecord('banned-user', id)
      .then(async (record) => {
        record.lockActive = false;
        await record.save();
        this.start.perform();
        if (this.triggerRefreshFn) this.triggerRefreshFn();
      })
      .catch((e) => {
        console.error(e);
        this.start.perform();
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
  removeUser(id) {
    this.dialogs.confirm(
      '<h4 class="text-danger">ATTENZIONE</h4>',
      `<h5 class="text-danger">CANCELLAZIONE DEFINITIVA DELL'UTENTE</h5>
       <h6 class="text-danger">Azione irreversibile.<br /><br />CONFERMI?</h6>`,
      () => {
        this.removeUserConfirmed.perform(id);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  removeUserConfirmed = task({ drop: true }, async (id) => {
    await this.store
      .queryRecord('user', {
        filter: `equals(id,'${id}')`,
      })
      .then(async (user) => {
        await user.destroyRecord();
        this.start.perform();
        if (this.triggerRefreshFn) this.triggerRefreshFn();
      })
      .catch((e) => {
        console.error(e);
        this.start.perform();
        this.dialogs.toast(
          'Si è verificato un errore. Riprova!',
          'error',
          'bottom-right',
          3
        );
      });
  });

  // numero di utenti
  get recordsLength() {
    return this.records.length;
  }

  get curTenant() {
    return parseInt(this.session.get('data.tenantId'));
  }
}
