import Component from '@glimmer/component';
import { service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { task } from 'ember-concurrency';
import { htmlSafe } from '@ember/template';

export default class StandardCoreUserAuditComponent extends Component {
  @service permissions;
  @service session;
  @service dialogs;
  @service fetch;
  @service store;

  @tracked tenants = [];
  @tracked users = [];

  @tracked available = 'waiting';
  @tracked tenantId;
  @tracked method;
  @tracked startDate;
  @tracked endDate;
  @tracked userId;
  @tracked userEmail;
  @tracked partialRoute;
  @tracked pageSize = '';
  @tracked currentPageNumber = 1;

  @tracked results = [];
  @tracked pages = 1; // numero di pagine in cui sono divisi i risultati
  @tracked details = '';
  @tracked noResults = false;

  @tracked arrayPages = [1];
  @tracked previousPageNumber = 1;
  @tracked nextPageNumber = 2;

  constructor(...args) {
    super(...args);

    this.start();
  }

  start() {
    this.available = 'waiting';
    this.metod = '';
    this.startDate = '';
    this.endDate = '';
    this.userId = 0;
    this.userEmail = '';
    this.partialRoute = '';
    this.pageSize = '';
    this.currentPageNumber = 1;
    this.available = 'available';

    this.findTenants.perform();
  }

  // chiamata quando modifico i filtri di ricerca
  @action
  changeFilter(field, event) {
    let value = event.target.value.trim();
    this[field] = value;
    this.results = [];
    this.pages = 1;
    this.arrayPages = [1];

    if (field === 'tenantId') {
      this.findUsers.perform();
    }
    if (field === 'userId') {
      this.userEmail = '';
    }
    if (field === 'userEmail') {
      this.userId = '';
    }
  }

  @action
  changePageNumber(number) {
    this.currentPageNumber = number;
    if (number > 1) {
      this.previousPageNumber = number - 1;
    } else this.previousPageNumber = 1;
    if (number < this.pages) {
      this.nextPageNumber = number + 1;
    } else this.nextPageNumber = number;

    this.currentPageNumber = number;
    this.results = [];
    this.findResults.perform();
  }

  // ricerca elenco tenants, chiamata all'avvio
  findTenants = task({ drop: true }, async () => {
    try {
      this.tenantId = '';
      this.tenants = [];
      this.users = [];
      this.userId = '';
      this.userEmail = '';
      let tenants = [];

      if (this.permissions.hasPermissions(['canSeeAllTenants'])) {
        // l'utente può accedere a tutti i tenant
        tenants = await this.store.peekAll('tenant');
        if (tenants.length === 0) {
          tenants = await this.store.query('tenant', {
            sort: 'name',
          });
        }
      } else {
        // può accedere ai suoi soli tenant
        let tenantIds = [];
        let userTenants = this.session.get('data.associatedTenants');
        userTenants.forEach((element) => {
          tenantIds.push(`'${element.tenantId}'`);
        });

        tenants = await this.store.query('tenant', {
          filter: `any(id,${tenantIds.join()})`,
          sort: `name`,
        });
      }

      this.tenants = tenants.filter((item) => {
        return item && item.name && item.name.trim() !== '';
      });

      this.tenantId = this.tenants[0].id;

      await this.findUsers.perform();
    } catch (e) {
      console.error(e);
    }
  });

  // ricerca utenti per tenant
  findUsers = task({ drop: true }, async () => {
    try {
      this.users = [];
      this.userId = '';
      this.userEmail = '';

      let queryRecord = {
        include: 'user.userProfile',
        sort: `user.userProfile.lastName,user.userProfile.firstName`,
      };

      if (this.tenantId !== '') {
        queryRecord.filter = `equals(tenantId,'${this.tenantId}')`;
      }

      this.store.query('userTenant', queryRecord).then(async (userTenants) => {
        let users = [];

        if (userTenants.length > 0) {
          let userTenantArray = userTenants.slice(); // Utilizzo slice per ottenere un array nativo

          for (let userTenant of userTenantArray) {
            let user = await userTenant.get('user');
            let userProfile = await user.get('userProfile');

            // Creo un oggetto con le voci di interesse
            let userObj = {
              id: user.get('id'),
              email: user.get('email'),
              lastName: userProfile.get('lastName'),
              firstName: userProfile.get('firstName'),
            };

            // Aggiungo l'oggetto all'array
            users.push(userObj);
          }

          // Assegno l'array all'attributo this.users
          this.users = users;
        }
      });
    } catch (e) {
      console.error(e);
    }
  });

  createFilterObject() {
    let obj = null;
    if (this.userId !== '') {
      obj = obj || {};
      obj.UserId = this.userId;
    } else if (this.userEmail !== '') {
      obj = obj || {};
      obj.UserEmail = this.userEmail;
    }
    if (this.partialRoute !== '') {
      obj = obj || {};
      obj.PartialRoute = this.partialRoute;
    }
    if (this.startDate !== '') {
      obj = obj || {};
      obj.LogDateStart = new Date(this.startDate);
    }
    if (this.endDate !== '') {
      obj = obj || {};
      obj.logDateEnd = new Date(this.endDate);
    }
    if (this.metod !== '') {
      obj = obj || {};
      obj.Method = this.metod;
    }
    if (this.pageSize !== '') {
      obj = obj || {};
      obj.PageSize = parseInt(this.pageSize);
    }
    if (this.currentPageNumber !== 1) {
      obj = obj || {};
      obj.PageNumber = this.currentPageNumber;
    }

    return obj;
  }

  findResults = task({ drop: true }, async () => {
    try {
      this.noResults = false;
      let obj = this.createFilterObject();

      let res = await this.fetch.call(
        `log`,
        'POST',
        obj,
        {},
        true,
        this.session
      );
      this.results = res.Logs;
      this.pages = res.Pages;
      this.arrayPages = this.pageNumbers;

      if (this.results.length === 0) {
        this.noResults = true;
        this.pages = 1;
      }
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore. Riprovare!',
        'error',
        'bottom-right',
        3,
        null
      );
    }
  });

  get numberRecords() {
    return this.results.length;
  }

  // crea i pulsanti di paginazione
  get pageNumbers() {
    const numbers = [];
    const start = Math.max(this.currentPageNumber - 2, 1);
    const end = Math.min(this.currentPageNumber + 2, this.pages);

    for (let i = start; i <= end; i++) {
      numbers.push(i);
    }

    // Aggiungere numeri mancanti all'inizio o alla fine
    while (numbers.length < 5 && numbers[0] > 1) {
      numbers.unshift(numbers[0] - 1);
    }

    while (numbers.length < 5 && numbers[numbers.length - 1] < this.pages) {
      numbers.push(numbers[numbers.length - 1] + 1);
    }

    return numbers;
  }

  @action
  showDetails(log, type) {
    let out = '';
    switch (type) {
      case 'headers': {
        out += `<h5>Headers</h5>`;
        log.Headers.forEach((element) => {
          if (element.Key && element.Value && element.Value[0]) {
            out += `<p class="mb-1">
                      <strong class="text-primary">${element.Key}:</strong> ${element.Value[0]}
                    </p>`;
          }
        });
        break;
      }
      case 'payload': {
        out += `<h5>Payload</h5>`;
        let payload = log.Payload && log.Payload[0] ? log.Payload[0] : {};
        if (Object.keys(payload).length === 0) {
          out += `<p class="mb-1"><em>Non ci sono dati</em></p>`;
        } else {
          for (var key in payload) {
            if (Object.prototype.hasOwnProperty.call(payload, key)) {
              if (!['Method'].includes(key)) {
                out += `<p class="mb-1">
                          <strong class="text-primary">${key}:</strong> ${payload[key]}
                        </p>`;
              }
            }
          }
        }
        break;
      }
      case 'parameters': {
        out += `<h5>Parameters</h5>`;
        if (log.Parameters && log.Parameters !== '') {
          out += `<p class="mb-1">
                    ${log.Parameters}
                  </p>`;
        } else {
          out += `<p class="mb-1"><em>Non ci sono dati</em></p>`;
        }
        break;
      }
    }

    this.details = htmlSafe(out);
    const allDivs = document.querySelectorAll('.log-details-cont');
    allDivs.forEach((div) => div.classList.remove('active'));
    const targetDiv = document.querySelector(
      `.log-details-cont[data-id="${log.Id}"]`
    );
    if (targetDiv) {
      targetDiv.classList.add('active');
    }
  }
}
