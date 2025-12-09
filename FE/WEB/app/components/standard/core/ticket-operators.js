/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { task } from 'ember-concurrency';
import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins';
import $ from 'jquery';

export default class StandardCoreTicketOperatorsComponent extends Component {
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  @tracked available = 'waiting';
  @tracked available2 = 'waiting';
  @tracked activeBox = 0;
  @tracked areas = [];
  @tracked users = '[]';
  @tracked areaUsers = '[]';
  @tracked selectedArea = null;
  @tracked operators = [];
  @tracked toggleId = '';
  @tracked onlyLeader = false;
  ticketManagementRoleId = null;
  tenantOperators = [];
  currentTenant = null;
  @tracked deletingId = null;
  @tracked changeLeaderId = null;
  auth = {};

  constructor(...args) {
    super(...args);
    if (
      this.args.updateSubComponent &&
      typeof this.args.updateSubComponent === 'function'
    ) {
      this.updateSubComponent = this.args.updateSubComponent;
    }

    if (this.args.auth) {
      this.auth = this.args.auth;
    }

    this.currentTenant = this.session.get('data.tenantId');

    this.editRecord = this.initializeRecord();
    this.preload.perform();
  }

  async getAreas() {
    let areas = await this.store.query('area', {
      sort: 'name',
      filter: `equals(tenantDestinationId,'${this.currentTenant}')`,
      include: 'ticketOperator',
    });

    let areasWithOperatorCount = await Promise.all(
      areas.map(async (area) => {
        let operators = await area.get('ticketOperator');
        let operatorCount = operators ? operators.length : 0;

        return {
          id: area.id,
          name: area.get('name'),
          note: area.get('note'),
          erasable: area.get('erasable'),
          tenantDestinationId: area.get('tenantDestinationId'),
          operatorCount, // Aggiungo il conteggio degli operatori
        };
      })
    );
    this.areas = areasWithOperatorCount;
  }

  // estrae elenco record
  preload = task({ drop: true }, async () => {
    try {
      this.available = 'waiting';
      this.available2 = 'waiting';

      await this.getAreas();

      let tm = await this.store.queryRecord('role', {
        filter: `and(equals(name,'TicketManagement'),equals(tenantId,'${this.currentTenant}'))`,
      });
      if (!tm || tm.length === 0) {
        throw new Error(
          'Il Tenant corrente non ha il ruolo "TicketManagement"'
        );
      }
      this.ticketManagementRoleId = tm.id;

      if (this.areas.length > 0) {
        this.selectedArea = this.areas[0].id;

        // ESTRAGGO ELENCO UTENTI DA RENDERE OPERATORI
        let users = await this.store.query('user', {
          sort: 'userProfile.lastName,userProfile.firstName',
          include: 'userProfile',
        });
        users = users.filter((item) => item.tenantId === this.currentTenant); // solo di questo tenant

        // estraggo dal DB l'elenco degli utenti con il ruolo di gestione ticket
        let tenantsOperatorsWithRole = await this.store.query('user-role', {
          include: 'role',
          filter: `and(equals(role.name,'TicketManagement'),equals(tenantId,'${this.currentTenant}'))`,
        });
        tenantsOperatorsWithRole = tenantsOperatorsWithRole.map(
          (x) => x.userId
        );

        // OPERATORI TOTALI
        // dall'eleco utenti estraggo solo quelli con ruolo di userTicket
        this.tenantOperators = users
          //.filter((item) => item.tenantId === this.currentTenant)
          .filter((item) => {
            return tenantsOperatorsWithRole.includes(item.id);
          });

        // UTENTI DA CONVERTIRE IN OPERATORI (attraverso la select)
        // dall'elenco utenti elimino quelli che sono già opertori
        let usersFree = users.filter((item) => {
          return !tenantsOperatorsWithRole.includes(item.id);
        });

        this.users = JSON.stringify(
          usersFree.map((item) => {
            let lastName = item.userProfile.get('lastName');
            let firstName = item.userProfile.get('firstName');
            let email = item.email;

            let name = `${lastName && lastName !== '' ? lastName : ''} ${
              firstName && firstName !== '' ? firstName : ''
            }`.trim();

            let full = '';
            if (email && name && name != '') {
              full = (name + ' - ' + email).trim();
            } else full = email.trim();

            return {
              id: item.id,
              value: full,
            };
          })
        );

        this.start.perform();
      }
      this.available = 'available';
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
  });

  // estrae elenco operatori per il reparto selezionato
  start = task({ drop: true }, async () => {
    try {
      this.available2 = 'waiting';

      this.operators = [];
      this.operators = await this.store.query('ticket-operator', {
        sort: 'user.userProfile.lastName,user.userProfile.firstName',
        include: 'user,user.userProfile',
        filter: this.onlyLeader
          ? `and(equals(areaId,'${this.selectedArea}'),equals(tenantDestinationId,'${this.currentTenant}'),equals(masterTracker,'true'))`
          : `and(equals(areaId,'${this.selectedArea}'),equals(tenantDestinationId,'${this.currentTenant}'))`,
      });

      // elimino dalla select "Aggiungi operatore" quelli già presenti nell'Area corrente
      let arrUserId = this.operators.map((x) => x.userId);

      let areaUsers = this.tenantOperators.filter((item) => {
        return !arrUserId.includes(item.id);
      });

      this.areaUsers = JSON.stringify(
        areaUsers.map((item) => {
          return {
            id: item.id,
            value: `${item.userProfile.get('lastName')} ${item.userProfile.get(
              'firstName'
            )}`,
          };
        })
      );

      this.available2 = 'available';
    } catch (e) {
      console.error(e);
      this.available2 = 'unavailable';
    }
  });

  @action
  setTab(number) {
    this.activeBox = number;
  }

  // #region TAB 1 - CANCELLO OPERATORE
  @action
  deleteUserOperator(operator) {
    let self = this;
    this.dialogs.confirm(
      '<h6 class="text-danger">Eliminazione Operatore</h6>',
      `<p class="text-danger">
        Vuoi davvero togliere l'utente <strong><em>${
          operator.userProfile.get('lastName')
            ? operator.userProfile.get('lastName')
            : ''
        } ${
        operator.userProfile.get('firstName')
          ? operator.userProfile.get('firstName')
          : ''
      }</em></strong> dall'elenco dei tuoi Operatori?<br />
        NOTA: verrà eliminato anche dagli eventuli Reparti a cui è associato.<br /><br />
        Confermi?
      </p>`,
      () => {
        self.deleteUserOperatorConfirmed.perform(operator);
      },
      null,
      ['Sì', 'No']
    );
  }

  deleteUserOperatorConfirmed = task({ drop: true }, async (operator) => {
    try {
      this.deletingId = operator.id;
      await this.fetch
        .call(
          'ticket/deleteOperatorAuth',
          'DELETE',
          { id: operator.id },
          {},
          true,
          this.session
        )
        .then(() => {
          this.updateSubComponent();
        })
        .catch((e) => {
          throw new Error(e);
        });
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
  // #endregion

  // #region TAB 1 - TRASFORMO UN UTENTE IN OPERATORE
  @action
  addOperator(id) {
    let operator = JSON.parse(this.users).filter((item) => item.id === id);
    operator = operator[0];
    let self = this;
    this.dialogs.confirm(
      '<h6">Nuovo Operatore</h6>',
      `<p>
        Vuoi davvero abilitare l'utente <strong class="text-primary"><em>${operator.value}</em></strong> alla gestione dei Ticket?<br />
      </p>`,
      () => {
        self.addOperatorConfirmed.perform(operator.id);
      },
      null,
      ['Sì', 'No']
    );
  }

  addOperatorConfirmed = task({ drop: true }, async (id) => {
    try {
      let newR = this.store.createRecord('user-role', {
        id: v4(),
        roleId: this.ticketManagementRoleId,
        userId: id,
        tenantId: this.currentTenant,
      });
      await newR.save();

      this.updateSubComponent();
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
  // #endregion

  // TAB 2 - SELEZIONO REPARTO
  @action
  changeArea(event) {
    this.selectedArea = event.target.value;
    this.start.perform();
  }

  // #region ASSOCIO OPERATORE AD UN'AREA
  @action
  selectOperatorToAdd(id) {
    if (!id) return false;

    let sel = this.tenantOperators.filter((item) => item.id === id);
    if (!sel || !sel[0] || !sel[0].id) {
      return false;
    }
    let firstName = sel[0].userProfile.get('firstName');
    let lastName = sel[0].userProfile.get('lastName');

    let area = this.areas.filter((item) => item.id === this.selectedArea);
    if (area && area[0]) {
      area = area[0].name;
    }

    let self = this;

    this.dialogs.confirm(
      `<h6>Associazione dell'Operatore al Reparto</h6>`,
      `<p>Stai associando l'utente <strong class="text-primary">${
        lastName ? lastName : ''
      } ${
        firstName ? firstName : ''
      }</strong> al reparto <strong class="text-success">${area}</strong>.<br /><br />Confermi?</p>`,
      () => {
        if (this.auth.license.canUseLeader && this.auth.custom.canUseLeader) {
          setTimeout(() => {
            self.selectOperatorToAdd2(id, lastName, firstName, area);
          }, 50);
        } else {
          self.selectOperatorToAddConfirmed.perform(id, false);
        }
      },
      null,
      ['Sì', 'No']
    );
  }
  selectOperatorToAdd2(id, lastName, firstName, area) {
    let self = this;
    this.dialogs.confirm(
      `<h6>Conferma</h6>`,
      `<p>L'utente <span class="text-primary">${lastName ? lastName : ''} ${
        firstName ? firstName : ''
      }</span> deve essere <strong><u>Capo-Area</u></strong> del reparto <span class="text-success">${area}</span>?</p>`,
      () => {
        self.selectOperatorToAddConfirmed.perform(id, false);
      },
      () => {
        self.selectOperatorToAddConfirmed.perform(id, true);
      },
      ['È un operatore normale', 'È un Capo-Area']
    );
  }
  selectOperatorToAddConfirmed = task({ drop: true }, async (id, isLeader) => {
    if (!id) return false;
    try {
      let newR = this.store.createRecord('ticket-operator', {
        id: v4(),
        areaId: this.selectedArea,
        userId: id,
        tenantDestinationId: this.currentTenant,
        masterTracker: isLeader,
      });
      await newR.save();

      //this.updateSubComponent();
      this.start.perform();
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
  // #endregion

  // switch "mostra solo capi-area"
  @action
  toggleOnlyLeader() {
    if (!this.auth.license.canUseLeader) {
      // eslint-disable-next-line no-undef
      Swal.fire({
        title: 'Opzione non disponibile!',
        html: `La tua licenza non consente di definire i Capi-Area`,
        icon: 'warning',
        showCancelButton: true,
        showConfirmButton: true,
        cancelButtonText: `Ignora`,
        confirmButtonText: `ESPANDI LA LICENZA`,
        customClass: {
          confirmButton: 'btn btn-outline-primary btn-hover-shine ml-2',
          cancelButton: 'btn btn-outline-secondary btn-hover-shine',
        },
        buttonsStyling: false,
        reverseButtons: true,
      }).then((result) => {
        if (result.isConfirmed) {
          this.router.transitionTo('ticket-license');
        }
      });

      this.onlyLeader = false;
      return false;
    }

    this.onlyLeader = !this.onlyLeader;
    this.start.perform();
  }

  // Trasformo operatore in capoarea e viceversa
  changeLeaderStatus = task({ drop: true }, async (operator) => {
    let oldStatus = operator.masterTracker;
    try {
      this.changeLeaderId = operator.id;

      operator.masterTracker = !oldStatus;
      await operator.save();
    } catch (e) {
      console.error(e);
      operator.masterTracker = oldStatus;
    }
  });

  // #region Dissocio operatore da reparto
  @action
  deleteAssociation(record) {
    let self = this;
    this.deletingId = record.id;
    let lastName = '',
      firstName = '';

    try {
      lastName = record.user.get('userProfile').get('lastName');
      firstName = record.user.get('userProfile').get('firstName');
    } catch (e) {
      console.error(e);
    }

    let area = this.areas.filter((item) => item.id === this.selectedArea);
    if (area && area[0]) {
      area = area[0].name;
    }

    this.dialogs.confirm(
      `<h6 class="text-danger">Dissociazione dell'Operatore dal Reparto</h6>`,
      `<p class="text-danger">Vuoi davvero eliminare l'operatore <strong>${
        lastName ? lastName : ''
      } ${
        firstName ? firstName : ''
      }</strong> dal reparto "<em>${area}</em>"?</p>`,
      () => {
        self.deleteAssociationConfirmed.perform(record);
      },
      null,
      ['Sì', 'No']
    );
  }
  deleteAssociationConfirmed = task({ drop: true }, async (record) => {
    try {
      await record.destroyRecord();
      await this.getAreas();
      this.start.perform();
    } catch (e) {
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        3
      );
    }
  });
  // #endregion

  get masterCount() {
    return this.operators.filter((item) => item.masterTracker).length;
  }

  willDestroy() {
    super.willDestroy(...arguments);
    if (this.updateSubComponent) {
      this.updateSubComponent();
    }
  }

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord('area'); // Crea una nuova istanza del modello (ad esempio, 'user')
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
