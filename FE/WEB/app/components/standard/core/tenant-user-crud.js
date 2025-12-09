/**
 * @param {string} currentTenant guid del tenant corrente
 * @param {function} updateRole funzione da chiamare per comunicare agli altri sotto-componenti che i ruoli sono cambiati
 */
/* eslint-disable no-undef */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
export default class StandardCoreTenantUserCrudComponent extends Component {
  @service dialogs;
  @service store;

  modelName = 'user-tenant';
  updateRole = null;
  @tracked serviceAvailable = 'waiting';
  @tracked records = [];
  @tracked tenants = [];
  @tracked users = [];
  @tracked currentTenant = null;
  @tracked filterText = '';
  @tracked newRecordId = '';

  constructor(...attributes) {
    super(...attributes);
    this.updateRole = this.args.updateRole ? this.args.updateRole : null;
    this.start();
  }

  @action
  async start() {
    let currentTenant = this.args.currentTenant ? this.args.currentTenant : '';
    if (currentTenant === '') return false;
    this.currentTenant = await this.store.peekRecord('tenant', currentTenant);
    this.serviceAvailable = 'waiting';
    this.initializeBase.perform(); // Avvia la task.
  }

  initializeBase = task({ drop: true }, async () => {
    try {
      // ricavo lista completa degli utenti
      this.users = await this.store.query('userProfile', {
        sort: `lastName,firstName`,
        include: 'user',
        filter: `equals(user.deleted,'false')`,
      });

      await this.findRecords.perform();
    } catch (e) {
      this.serviceAvailable = 'unavailable';
    }
  });

  findRecords = task({ drop: true }, async () => {
    try {
      if (!this.currentTenant) return false;

      this.records = [];

      const userTenants = await this.store.query(this.modelName, {
        include: 'user.userProfile',
        filter: `equals(tenantId,'${this.currentTenant.id}')`,
        sort: `user.userProfile.lastName,user.userProfile.firstName,user.email`,
      });

      if (userTenants && userTenants.length > 0) {
        let users = [];
        for (const userTenant of userTenants) {
          let user = await userTenant.user;
          if (user) {
            let userProfile = await user.get('userProfile');
            let temp = {
              id: userTenant.id,
              lastName: userProfile ? userProfile.get('lastName') : '',
              firstName: userProfile ? userProfile.get('firstName') : '',
              email: user.email,
            };
            users.push(temp);
          }
        }
        this.records = users;
      }
      this.serviceAvailable = 'available';
    } catch (e) {
      console.error(e);
      this.serviceAvailable = 'unavailable';
      throw new Error();
    }
  });

  get filteredRecords() {
    if (this.filterText) {
      return this.records.filter((record) => {
        return (
          (record.lastName &&
            record.lastName
              .toLowerCase()
              .includes(this.filterText.toLowerCase())) ||
          (record.firstName &&
            record.firstName
              .toLowerCase()
              .includes(this.filterText.toLowerCase())) ||
          (record.email &&
            record.email.toLowerCase().includes(this.filterText.toLowerCase()))
        );
      });
    } else {
      return this.records;
    }
  }

  get recordsLength() {
    return this.records.length;
  }

  get freeUsers() {
    let self = this;
    let x = this.users.filter((user) => {
      return !self.records.some((obj2) => obj2.id === user.id);
    });
    let arr = [];
    x.forEach((user) => {
      arr.push({
        id: user.user.get('id'),
        value: `${user.get('lastName') || ''} ${
          user.get('firstName') || ''
        } - ${user.user.get('email')}`,
      });
    });
    return JSON.stringify(arr);
  }

  // consente il cambio tenant
  @action
  async updateTenant(event) {
    if (event.target.value !== '') {
      this.currentTenant = await this.store.peekRecord(
        'tenant',
        event.target.value
      );
      this.findRecords.perform();
    } else {
      this.currentTenant = null;
      this.records = [];
    }
  }

  // cattura il valore selezionato dall'utente per la creazione di un record
  @action
  storeNewValue(val) {
    this.newRecordId = val;
  }

  // crea un nuovo record
  @action
  saveNewVoice() {
    // verifica coerenza dei dati
    if (!this.newRecordId) {
      this.dialogs.toast(`Seleziona l'utente`, 'error', 'bottom-right', 3);
      return false;
    }

    this.dialogs.confirm(
      '<h6>Associazione utente</h6>',
      `<p class="text-danger">ATTENZIONE: stai associando l'Utente al Tenant. Confermi?</p>`,
      () => {
        this.saveNewVoiceConfirmed.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  saveNewVoiceConfirmed = task({ drop: true }, async () => {
    try {
      let newR = this.store.createRecord(this.modelName, {
        id: v4(),
        userId: this.newRecordId,
        tenantId: this.currentTenant.id,
        state: 'accepted',
        ip: '0.0.0.0',
      });

      await newR.save();
      this.findRecords.perform();

      // resetto l'oggetto newRecord per svuotare i campi input collegati
      this.newRecordId = '';
      if (this.updateRole) this.updateRole();
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Si è verificato un errore, riprovare!',
        'error',
        'bottom-right',
        4
      );
    }
  });

  @action
  errorDelatingVoice() {
    this.dialogs.toast(
      'Si è verificato un errore, riprovare!',
      'error',
      'bottom-right',
      4
    );
  }

  @action
  delVoice(record) {
    let self = this;
    this.dialogs.confirm(
      '<h6>Cancellazione associazione</h6>',
      `<p class="text-danger">ATTENZIONE: l'utente non avrà più accesso al Tenant.<br />Confermi la cancellazione?</p>`,
      async () => {
        try {
          let association = await self.store.findRecord(
            self.modelName,
            record.id
          );
          if (association) {
            await association.destroyRecord();
          }

          // eslint-disable-next-line prettier/prettier
          self.dialogs.toast('Operazione riuscita', 'success', 'bottom-right', 3);
          self.findRecords.perform();
          if (self.updateRole) self.updateRole();
        } catch (e) {
          if (e instanceof Promise) {
            // mi viene restituita una Promise
            e.then((resolvedError) => {
              console.error(resolvedError);
              if (resolvedError.errors && resolvedError.errors.length > 0) {
                let error = resolvedError.errors[0];
                if (error.status && parseInt(error.status) === 406) {
                  Swal.fire({
                    icon: 'error',
                    title: 'Non cancellabile',
                    text: `Non puoi dissociare l'utente dal Tenant di Recupero senza averlo prima associato ad un altro Tenant.`,
                  });
                } else {
                  self.errorDelatingVoice();
                }
              } else {
                self.errorDelatingVoice();
              }
            }).catch(() => {
              self.errorDelatingVoice();
            });
          } else {
            console.error(e);
            self.errorDelatingVoice();
          }
        }
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  @action
  insertFilter(event) {
    this.filterText = event.target.value;
  }
}
