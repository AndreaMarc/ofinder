import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';

export default class StandardCoreHelpDeskAdminComponent extends Component {
  @service translation;
  @service siteLayout;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  @tracked filterType = '';
  @tracked importance = '';
  @tracked tickets;
  @tracked ticketsFiltered;
  @tracked filterSearch = '';

  @tracked numberAnonymous = 0;
  @tracked numberUnprocessed = 0;
  @tracked numberRunning = 0;
  @tracked currentLang = this.translation.currentLang;
  @tracked currentUserId = null;
  @tracked note = '';
  @tracked hourFormat = JSON.stringify({
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  });

  // per dettagli del ticket
  @tracked showDetails = false;
  @tracked ticketDetails = null;
  @tracked tenants = '';
  @tracked selectedTenantId = '';

  constructor(...attributes) {
    super(...attributes);
    this.currentUserId = this.session.get('data.id');
    this.start();
  }

  async start() {
    this.findAnonymous();
    this.findUnprocessed();
    this.findRunning();

    this.findTickets.perform();
  }

  async findAnonymous() {
    try {
      let an = await this.store.query('ticket', {
        filter: `and(equals(tenantId,'0'),equals(status,'unprocessed'))`,
      });
      this.numberAnonymous = an.meta.total;
    } catch (e) {
      console.error(e);
      this.numberAnonymous = 0;
    }
  }

  async findUnprocessed() {
    try {
      let wa = await this.store.query('ticket', {
        filter: `and(not(equals(tenantId,'0')),equals(status,'unprocessed'))`,
      });
      this.numberUnprocessed = wa.meta.total;
    } catch (e) {
      console.error(e);
      this.numberUnprocessed = 0;
    }
  }

  async findRunning() {
    try {
      let ru = await this.store.query('ticket', {
        filter: `and(not(equals(tenantId,'0')),equals(status,'running'))`,
      });
      this.numberRunning = ru.meta.total;
    } catch (e) {
      console.error(e);
      this.numberRunning = 0;
    }
  }

  @action
  setFilterType(type) {
    this.filterType = type;
    this.findTickets.perform();
  }

  @action
  setImportance(imp) {
    this.importance = imp;
  }

  @action
  setFilter(event) {
    this.filterSearch = event.target.value.toLowerCase();
  }

  @action
  clearFilter() {
    this.filterSearch = '';
  }

  findTickets = task({ drop: true }, async () => {
    try {
      let queryParams = {};
      queryParams.sort = `-creationDate`;
      queryParams.include = `user,user.userProfile,tenant`;

      switch (this.filterType) {
        case 'anonymous':
          queryParams.filter = `and(equals(tenantId,'0'),equals(status,'unprocessed'))`;
          queryParams.include = null;
          break;
        case 'unprocessed':
          queryParams.filter = `equals(status,'unprocessed')`;
          break;
        case 'running':
          queryParams.filter = `equals(status,'running')`;
          break;
        case 'complete':
          queryParams.filter = `equals(status,'complete')`;
          break;
        case 'trashed':
          queryParams.filter = `equals(status,'trashed')`;
          queryParams.include = null;
          break;
        case '':
          queryParams.filter = `and(not(equals(status,'trashed')),not(equals(status,'complete')))`;
          queryParams.include = null;
          break;
      }

      let tickets = await this.store.query('ticket', queryParams);

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
      }
      this.tickets = tickets;
    } catch (e) {
      console.error(e);
    }
  });

  get filteredTickets() {
    let filtered;
    if (this.filterSearch === '') {
      filtered = this.tickets;
    } else {
      filtered = this.tickets.filter((item) => {
        return (
          item.message.toLowerCase().includes(this.filterSearch) ||
          // eslint-disable-next-line prettier/prettier
          item.get('tenant') && item.get('tenant').get('name') && item.get('tenant').get('name').toLowerCase().includes(this.filterSearch) ||
          // eslint-disable-next-line prettier/prettier
          item.get('user') && item.get('user').get('userProfile') && item.get('user').get('userProfile').get('lastName').toLowerCase().includes(this.filterSearch) ||
          // eslint-disable-next-line prettier/prettier
          item.get('user') && item.get('user').get('userProfile') && item.get('user').get('userProfile').get('firstName').toLowerCase().includes(this.filterSearch) ||
          // eslint-disable-next-line prettier/prettier
          item.get('user') && item.get('user').get('email')  && item.get('user').get('email').toLowerCase().includes(this.filterSearch)
        );
      });
    }

    if (this.importance !== '') {
      filtered = filtered.filter((item) => {
        return item.priority === this.importance;
      });
    }
    return filtered;
  }

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

  findTenants = task({ drop: true }, async () => {
    await this.store
      .query('tenant', {
        filter: `equals(enabled,'true')`,
        sort: `name`,
      })
      .then((tenants) => {
        let tenantsArray = [];
        tenants.forEach((tenant) => {
          tenantsArray.push({
            id: tenant.id,
            value: `${tenant.name ? tenant.name : ''} ${
              tenant.phone ? '- ' + 'Tel:' + tenant.phone : ''
            } ${tenant.tenantVAT ? '- ' + 'P.Iva:' + tenant.tenantVAT : ''} ${
              tenant.email ? '- ' + tenant.email.split(';').join(' | ') : ''
            }`,
          });
        });

        this.tenants = JSON.stringify(tenantsArray);
      });
  });

  findTicketDetails = task({ drop: true }, async (ticketId) => {
    try {
      this.findTenants.perform();
      let tkt = await this.store.findRecord('ticket', ticketId);

      // ricavo dettagli operatore associato
      if (tkt.assignedId) {
        let op = await this.store.queryRecord('user', {
          filter: `equals(id,'${tkt.assignedId}')`,
          include: `userProfile`,
        });

        if (op && op.length > 0) {
          tkt.set('assignedUser', op);
        }
      }
      this.ticketDetails = tkt;
    } catch (e) {
      console.error(e);
    }
  });

  @action
  setAssignedTenant(tenantId) {
    this.selectedTenantId = tenantId;
  }

  @action
  trashTicket() {
    let self = this;
    this.dialogs.confirm(
      '<h4 class="text-danger">Cancellazione della Richiesta</h4>',
      `<p class="text-danger">Stai spostando la Richiesta nel cestino. Sei sicuro?</p>`,
      () => {
        self.trashTicketConfirmed.perform();
      },
      null,
      ['Sì, cancella la richiesta', 'Annulla']
    );
  }

  trashTicketConfirmed = task({ drop: true }, async () => {
    try {
      this.ticketDetails.status = 'trashed';
      await this.ticketDetails.save();

      this.selectedTenantId = '';
      this.showDetails = false;

      this.start();
      this.dialogs.toast(`Richiesta cancellata!`, 'success', 'bottom-right', 4);
    } catch (e) {
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        4
      );
    }
  });

  @action
  assignTicket() {
    let self = this;
    this.dialogs.confirm(
      '<h4>Assegnazione della Richiesta</h4>',
      `<p class="text-danger">ATTENZIONE</p>
      <p>Stai assegnando il ticket al Cliente selezionato. Questo significa che il Cliente vedrà tale ticket nell'elenco delle sue 'Richieste inviate'.</p>
      <p>SEI DAVVERO SICURO CHE LA RICHIESTA APPARTENGA A QUESTO CLIENTE?</p>`,
      () => {
        self.assignTicketConfirmed.perform();
      },
      null,
      ['Sì, sono sicuro', 'No']
    );
  }

  assignTicketConfirmed = task({ drop: true }, async () => {
    try {
      let tkt = await this.store.queryRecord('ticket', {
        filter: `equals(id,'${this.ticketDetails.id}')`,
      });

      tkt.status = 'unprocessed';
      tkt.tenantId = this.selectedTenantId;
      await tkt.save();
      //await this.findTicketDetails.perform(tkt.id);
      let tenant = await this.store.findRecord('tenant', this.selectedTenantId);
      this.ticketDetails.set('tenant', tenant);

      this.start();
      this.dialogs.toast(`Assegnazione riuscita`, 'success', 'bottom-right', 4);
    } catch (e) {
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        4
      );
    }
  });

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
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        4
      );
    }
  });

  @action
  removeTenant() {
    let self = this;
    this.dialogs.confirm(
      '<h4 class="text-danger">Rimozione dell\'assegnazione all\'azienda</h4>',
      `<h5 class="text-danger">ATTENZIONE: questa opzione non andrebbe mai usata!</h5>
      <p class="text-danger mb-3">Procedere solo se la richiesta era anonima ed è stata assegnata all'azienda sbagliata</p>
      <p>NOTA: il Cliente non vedrà più la segnalazione nello storico delle richieste inviate.<br /><br />CONFERMI?</p>`,
      () => {
        self.removeTenantConfirmed.perform();
      },
      null,
      ['Sì', 'No']
    );
  }

  removeTenantConfirmed = task({ drop: true }, async () => {
    try {
      this.ticketDetails.status = 'unprocessed';
      this.ticketDetails.assignedId = 0;
      this.ticketDetails.tenantId = 0;
      await this.ticketDetails.save();

      //this.ticketDetails.set('assignedId', ticket.assignedId);
      //await this.ticketDetails.save();
      this.start();
      this.dialogs.toast(`Operazione riuscita`, 'success', 'bottom-right', 4);
    } catch (e) {
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        4
      );
    }
  });

  @action
  setCompleted() {
    let self = this;
    let resp = document.getElementById('help-desk-admin-answer').value.trim();

    if (resp === '') {
      this.dialogs.toast(
        `Inserisci il messaggio di risposta!`,
        'warning',
        'bottom-right',
        4
      );
      return false;
    }

    let msg = `<p>Non potrai più apportare modifiche.<br />Il Cliente riceverà una notifica informativa contenente il messaggio di risposta.</p>
                <p>CONFERMI IL COMPLETAMENTO DEL TASK?</p>`;

    this.dialogs.confirm(
      '<h4>Chiusura del Ticket</h4>',
      msg,
      () => {
        self.setCompletedConfirmed.perform(resp);
      },
      null,
      ['Sì', 'No']
    );
  }

  setCompletedConfirmed = task({ drop: true }, async (resp) => {
    try {
      this.ticketDetails.status = 'complete';
      this.ticketDetails.answer = resp;
      this.ticketDetails.closedDate = new Date();
      this.ticketDetails.closedById = this.session.get('data.id');
      await this.ticketDetails.save();
      // eslint-disable-next-line prettier/prettier
      this.sendNotifications(this.ticketDetails.id, this.ticketDetails.userId, false);

      this.start();
      this.dialogs.toast(`Operazione riuscita`, 'success', 'bottom-right', 4);
    } catch (e) {
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        4
      );
    }
  });

  @action
  deleteTicket() {
    let self = this;
    this.dialogs.confirm(
      '<h4 class="text-danger">Cancellazione definitiva</h4>',
      `<p class="text-danger">Il ticket verrà cancellato definitivamente. Azione irreversibile.<br />Confermi?</p>`,
      () => {
        self.deleteTicketConfirmed.perform();
      },
      null,
      ['Sì', 'No']
    );
  }

  deleteTicketConfirmed = task({ drop: true }, async () => {
    try {
      this.ticketDetails.destroyRecord();
      this.selectedTenantId = '';
      this.showDetails = false;

      this.start();
      this.dialogs.toast(
        `Ticket eliminato definitivamente!`,
        'success',
        'bottom-right',
        4
      );
    } catch (e) {
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'error',
        'bottom-right',
        4
      );
    }
  });

  @action
  async setPriority(priority) {
    try {
      this.ticketDetails.set('priority', priority);
      await this.ticketDetails.save();
      this.start();
    } catch (e) {
      console.error(e);
    }
  }

  @action
  writeNote(value) {
    this.note = value;
  }

  saveNote = task({ drop: true }, async () => {
    this.ticketDetails.set('note', this.note.trim());
    await this.ticketDetails.save();
  });

  // per inviare notifiche push ed email ai destinatari
  /**
   * @param {string} ticketId : guid del record ticket
   * @param {string} toUserId : guid dell'utente destinatario della notifica. Se è una notifica collettiva, sarà ''.
   * @param {bool} toTicketManagement : true se la notifica va inviata agli utenti che gestiscono il Servizio di Assistenza Clienti, false altrimenti.
   */
  async sendNotifications(ticketId, toUserId, toTicketManagement) {
    await this.fetch.call(
      'tickets/helpDeskNotifications',
      'POST',
      {
        ticketId: ticketId,
        userId: toUserId,
        toTicketManagement,
      },
      {},
      true,
      this.session
    );
  }

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

  get getBgColor() {
    let iconBg = '';
    if (this.args.iconBg && this.args.iconBg !== '') {
      iconBg = `${this.args.icon} icon-gradient ${this.args.iconBg}`;
    } else {
      iconBg = `${this.args.icon} icon-gradient ${this.siteLayout.headerBackground}`;
    }
    return htmlSafe(iconBg);
  }
}
