import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';
import { debounce } from '@ember/runloop';
import { TrackedObject } from 'tracked-built-ins';

export default class StandardCoreTicketManagementComponent extends Component {
  @service translation;
  @service siteLayout;
  @service jsonApi;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  /**
   * Possibili valori di available:
   * 'preload': fase di caricamento iniziale
   * 'no-areas': non sono definite le aree
   * 'no-operators': nemmeno un operatore associato all'azienda corrente
   * 'no-pertinences': nessun ambito definito
   * 'noAreasForOperator: l'utente corrente ha il ruolo di accesso ma non è associato ad alcuna area
   * 'preload-error': errore nella fase di avvio
   * 'waiting': in attesa di download dei tickets
   * 'unavailable': errore nella get dei tickets
   * 'available': tutto ok
   */
  @tracked available = 'preload';
  myOperatorId = '';
  @tracked currentPage = 1;
  @tracked recordPerPage = '20';
  @tracked totalRecord = 0;
  @tracked sortType = 'date';
  onlyMine = false;

  @tracked filterType = '';
  @tracked filterSearch = new TrackedObject({
    customer: '',
    project: '',
    operator: '',
    message: '',
    date: '',
  });
  @tracked filterTag = '';
  @tracked importance = '';

  @tracked currentUserId = null;
  currentTenant = null;

  @tracked tags = [];
  @tracked tagsLength = 0;
  @tracked numberAnonymous = 0;
  @tracked numberUnprocessed = 0;
  @tracked numberRunning = 0;
  @tracked tickets = [];
  auth = null;

  @tracked hourFormat = JSON.stringify({
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  });

  updateSubComponent = null;
  changeTab = null;

  constructor(...attributes) {
    super(...attributes);
    this.currentUserId = this.session.get('data.id');
    this.currentTenant = this.session.get('data.tenantId');

    if (
      this.args.updateSubComponent &&
      typeof this.args.updateSubComponent === 'function'
    ) {
      this.updateSubComponent = this.args.updateSubComponent;
    }

    if (this.args.changeTab && typeof this.args.changeTab === 'function') {
      this.changeTab = this.args.changeTab;
    }

    this.start();
  }

  // verifiche preliminari
  async start() {
    if (this.args.auth) {
      this.auth = this.args.auth;
    }

    try {
      this.available = 'preload';

      // verifico se sono state impostate le aree
      this.areas = await this.store.query('area', {
        sort: 'name',
        filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
      });

      if (this.areas.length === 0) {
        this.available = 'no-areas';
        return false;
      }

      // verifico se sono stati definiti gli operatori
      let operators = await this.store.query('ticket-operator', {
        filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
      });

      if (operators.length === 0) {
        this.available = 'no-operators';
        return false;
      }

      // verifico se sono stati definiti gli ambiti
      let pertinences = await this.store.query('ticket-pertinence', {
        filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
      });
      if (pertinences.length === 0) {
        this.available = 'no-pertinences';
        return false;
      }

      // ricavo il mio OperatorId, per filtrare i soli miei ticket
      let myOperator = await this.store.queryRecord('ticket-operator', {
        filter: `equals(userId,'${this.currentUserId}')`,
      });
      if (myOperator) {
        this.myOperatorId = myOperator.id;
      } else {
        console.error('Error in find my operator!');
        this.available = 'noAreasForOperator';
        return false;
      }

      // estraggo l'elenco dei Tags
      this.findTags();

      // estraggo il numero di ticket anonimi, non-processati, in corso
      this.findAnonymous();
      this.findUnprocessed();
      this.findRunning();

      // estrae l'elenco dei record
      this.findTickets.perform();
    } catch (e) {
      console.error(e);
      this.available = 'preload-error';
    }
  }

  // estrae numero ticket anonimi
  async findTags() {
    try {
      let tags = await this.store.query('ticket-tag', {
        filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
      });

      this.tagsLength = tags.length;

      this.tags = JSON.stringify(
        tags.map((item) => {
          return {
            id: item.tag,
            value: item.name,
          };
        })
      );
    } catch (e) {
      console.error(e);
    }
  }

  // estrae numero ticket anonimi
  async findAnonymous() {
    try {
      let an = await this.store.query('ticket', {
        filter: `and(equals(tenantId,'0'),equals(status,'unprocessed'),equals(tenantDestinationId,'${this.currentTenant}'))`,
      });
      this.numberAnonymous = an.meta.total;
    } catch (e) {
      console.error(e);
      this.numberAnonymous = 0;
    }
  }

  // estrae numero ticket non-processati
  async findUnprocessed() {
    try {
      let wa = await this.store.query('ticket', {
        filter: `and(not(equals(tenantId,'0')),equals(status,'unprocessed'),equals(tenantDestinationId,'${this.currentTenant}'))`,
      });
      this.numberUnprocessed = wa.meta.total;
    } catch (e) {
      console.error(e);
      this.numberUnprocessed = 0;
    }
  }

  // estrae numero ticket in-corso
  async findRunning() {
    try {
      let ru = await this.store.query('ticket', {
        filter: `and(not(equals(tenantId,'0')),equals(status,'running'),equals(tenantDestinationId,'${this.currentTenant}'))`,
      });
      this.numberRunning = ru.meta.total;
    } catch (e) {
      console.error(e);
      this.numberRunning = 0;
    }
  }

  @action
  setRecordPerPage(event) {
    this.recordPerPage = event.target.value;
    this.currentPage = 1;
    this.findTickets.perform();
  }

  @action
  setCurrentPage(val) {
    this.currentPage = val;
    this.findTickets.perform();
  }

  @action
  setOnlyMine(val) {
    this.onlyMine = val;
    this.findTickets.perform();
  }

  @action
  setSort(event) {
    let val = event.target.value;
    this.sortType = val;
    this.findTickets.perform();
  }

  // estrae tutti i ticket
  findTickets = task({ drop: true }, async () => {
    try {
      this.available = 'waiting';
      let jsonApiFilter = {
        page: {
          number: this.currentPage,
          size: this.recordPerPage,
        },
        filter: [
          {
            function: 'equals',
            column: 'tenantDestinationId',
            value: this.currentTenant,
          },
        ],
        sort: '-creationDate',
        fields: [],
        include: `ticketTagMappings.ticketTag,user.userProfile,ticketRelationsAsFather,ticketRelationsAsChild`,
      };

      // imposto ordinamento
      if (this.sortType === 'date') {
        jsonApiFilter.sort = '-creationDate';
      } else if (this.sortType === 'customer') {
        jsonApiFilter.sort =
          'user.userProfile.lastName,user.userProfile.firstName';
      }

      // filtro i soli miei ticket
      // TODO
      if (this.onlyMine) {
        jsonApiFilter.include += `,ticketOperatorGrants`;
        jsonApiFilter.filter.push({
          function: 'has',
          column: '',
          value: `ticketOperatorGrants,equals(operatorId,'${this.myOperatorId}')`,
        });
      }

      // filtraggio per importanza
      if (this.importance !== '') {
        jsonApiFilter.filter.push({
          function: 'equals',
          column: 'priority',
          value: parseInt(this.importance),
        });
      }

      switch (this.filterType) {
        // ANONIMI
        case 'anonymous':
          jsonApiFilter.filter.push(
            {
              function: 'equals',
              column: 'tenantId',
              value: '0',
            },
            {
              function: 'equals',
              column: 'status',
              value: 'unprocessed',
            }
          );
          jsonApiFilter.include = `ticketTagMappings.ticketTag,user.userProfile,ticketRelationsAsFather,ticketRelationsAsChild`;
          break;
        // IN ATTESA
        case 'unprocessed':
          //queryParams.filter = `and(equals(status,'unprocessed'),equals(tenantDestinationId,'${this.currentTenant}'))`;
          jsonApiFilter.filter.push({
            function: 'equals',
            column: 'status',
            value: 'unprocessed',
          });
          jsonApiFilter.include = `ticketTagMappings.ticketTag,user.userProfile,ticketRelationsAsFather,ticketRelationsAsChild`;
          break;
        case 'running':
          //queryParams.filter = `and(equals(status,'running'),equals(tenantDestinationId,'${this.currentTenant}'))`;
          jsonApiFilter.filter.push({
            function: 'equals',
            column: 'status',
            value: 'running',
          });
          break;
        case 'complete':
          //queryParams.filter = `and(equals(status,'complete'),equals(tenantDestinationId,'${this.currentTenant}'))`;
          jsonApiFilter.filter.push({
            function: 'equals',
            column: 'status',
            value: 'complete',
          });
          break;
        case 'trashed':
          //queryParams.filter = `and(equals(status,'trashed'),equals(tenantDestinationId,'${this.currentTenant}'))`;
          //queryParams.include = null;
          jsonApiFilter.filter.push({
            function: 'equals',
            column: 'status',
            value: 'trashed',
          });
          jsonApiFilter.include = null;
          break;
        case '':
          //queryParams.filter = `and(not(equals(status,'trashed')),not(equals(status,'complete')),equals(tenantDestinationId,'${this.currentTenant}'))`;
          //queryParams.include = null;
          jsonApiFilter.filter.push(
            {
              function: 'equals',
              column: 'status',
              value: 'trashed',
              negation: true,
            },
            {
              function: 'equals',
              column: 'status',
              value: 'complete',
              negation: true,
            }
          );
          //jsonApiFilter.include = null;
          break;
      }

      let query = this.jsonApi.queryBuilder(jsonApiFilter);
      let tickets = await this.store.query('ticket', query);
      this.totalRecord = tickets.meta.total;

      console.log(tickets.length);
      /*
      // ora mi ricavo gli id degli operatori associati ai ticket
      let assignedIds = [
        ...new Set(
          tickets.map((ticket) => ticket.assignedId).filter((id) => id)
        ),
      ];
      if (assignedIds.length > 0) {
        // ricavo i dati degli Utenti assegnati e li associo ai ticket
        let assignedUsers = await this.store.query('user', {
          filter: `any(id,'${assignedIds.join("','")}')`, //{ id: assignedIds },
          include: `userProfile`,
        });

        tickets.forEach((ticket) => {
          let assignedUser = assignedUsers.find(
            (user) => user.id === ticket.assignedId
          );
          ticket.set('assignedUser', assignedUser);
        });
      }*/

      this.tickets = tickets;
      this.available = 'available';
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
  });

  getIcon(priority) {
    let icon = '';

    if (typeof priority === 'undefined') {
      icon = `<i class="fa fa-star text-secondary"></i>`;
    } else {
      switch (parseInt(priority)) {
        case 0:
          icon = `<i class="fa fa-star text-secondary"></i>`;
          break;
        case 1:
          icon = `<i class="fa fa-star text-success"></i>`;
          break;
        case 2:
          icon = `<i class="fa fa-star text-warning"></i>`;
          break;
        case 3:
          icon = `<i class="fa fa-star text-danger"></i>`;
          break;
        default:
          icon = `<i class="fa fa-star text-secondary"></i>`;
      }
    }
    return htmlSafe(icon);
  }

  // imposto un filtro di ricerca testuale
  @action
  setFilter(field, event) {
    this.filterSearch[field] = event.target.value;

    debounce(this, this.invokeFindTickets, null, 500);
  }
  invokeFindTickets() {
    // Qui invochi il task findTickets, passando searchTerm se necessario
    this.findTickets.perform();
  }
  // cancello il filtro di ricerca testuale
  @action
  clearFilter(field) {
    this.filterSearch[field] = '';
  }

  @action
  setFilterType(type) {
    this.filterType = type;
    this.findTickets.perform();
  }

  @action
  setImportance(imp) {
    this.importance = imp;
    this.findTickets.perform();
  }

  @action
  setFilterTag(tag) {
    this.filterTag = tag;

    if (this.filterTag === '') {
      this.tags = JSON.stringify(JSON.parse(this.tags));
    }
  }

  @action
  setOperatorTicket() {
    let self = this;
    this.dialogs.confirm(
      '<h4>Presa in carico</h4>',
      `<p>Stai prendendo in carico questa Richiesta di Assistenza.</p>
      <p>NOTA: il Cliente riceverà una notifica informativa.<br /><br />CONFERMI?</p>`,
      () => {
        self.setOperatorTicketConfirmed.perform();
      },
      null,
      ['Sì', 'No']
    );
  }

  setOperatorTicketConfirmed = task({ drop: true }, async () => {
    try {
      this.ticketDetails.status = 'running';
      this.ticketDetails.assignedId = this.session.get('data.id');
      await this.ticketDetails.save();

      //await this.ticketDetails.save();
      this.start();
      this.dialogs.toast(`Operazione riuscita`, 'success', 'bottom-right', 4);
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        4
      );
    }
  });

  /** AREA DETTAGLI DEL TICKET */

  @action
  toggleDetails(ticketId) {
    this.selectedTenantId = '';

    if (ticketId === 0) {
      this.showDetails = false;
    } else {
      this.showDetails = true;
      this.findTicketDetails.perform(ticketId);
    }
  }
}
