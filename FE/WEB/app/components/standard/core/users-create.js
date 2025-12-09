/**
 * Parametri :
 * @param {string} @roleId : Id del ruolo assegnato di default. se non definito viene usato user.
 * @param {int} @tenantDestinationId : Id del tenant di destinazione. Se vuoto, viene utlizzato
 *                                     il tenant corrente. Se valorizzato, deve essere un figlio di
 *                                     grado n-esimo del tenant corrente.
 * NOTA:
 * L'override lato BE nella post di user-profile prevede che il ruolo da assegnare all'utente sia
 * inviato in header. A questo provvede l'adapter personalizzato user-profile, che trasmette l'ID
 * del ruolo memorizzato nella variabile userRole del Servizio statusService.
 */

/* eslint-disable no-undef */
/* eslint-disable ember/jquery-ember-run */
/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { task } from 'ember-concurrency';
import $ from 'jquery';

export default class StandardCoreUsersCreateComponent extends Component {
  @service('siteSetup') stp;
  @service statusService;
  @service jsUtility;
  @service session;
  @service dialogs;
  @service store;
  @service fetch;

  modelName = 'user-profile';
  recordWeb = null;
  maeUsers = [];
  roleId = null;
  tenantDestinationId = '';
  triggerRefreshFn = null;

  @tracked s2Users = '';
  @tracked available = 'waiting';
  @tracked currentTenant = null;
  @tracked registrationFields = null;
  @tracked nations = [];
  @tracked newRecord = null; // usato in creazione nuovo record
  @tracked assignableRoles = [];
  @tracked roleName = '';

  constructor(...attributes) {
    super(...attributes);
    this.registrationFields =
      this.stp.siteSetup.registrationFields.registration;
    this.currentTenant = this.session.get('data.tenantId');
    this.newRecord = this.initializeRecord();
    this.nations = this.jsUtility.nations();
    this.triggerRefreshFn = this.args.triggerRefreshFn || null;

    this.start.perform();
  }

  start = task({ restartable: true }, async () => {
    try {
      let roleIds = await this.fetch.call(
        `roleClaims/getAssignableClaims`,
        'GET',
        {},
        {
          accept: 'application/json',
        },
        true,
        this.session
      );

      let arrIds = JSON.parse(roleIds).roles.split(',');

      arrIds = arrIds.map((item) => {
        return `'${item}'`;
      });

      /*
      this.assignableRoles = await this.store.query('role', {
        filter: `and(any(id,${arrIds}),equals(tenantId,'${this.session.get(
          'data.tenantId'
        )}'))`,
      });
      */

      // TODO
      // migliorare così:
      /**
       * estrarre i ruoli di questo Tenant, come faccio in customer-user-crud, poi
       * filtrarli sulla base dei miei claims (assignableRoles) quindi inserire una
       * select in fase di creazione, mostrando il risultato di tale filtraggio.
       * Così un customer-admin potrà assegnare il ruolo suo e "inferiori".
       */
      this.assignableRoles = await this.store.query('role', {
        filter: `any(id,${arrIds})`,
      });

      if (typeof this.args.roleId !== 'undefined' && this.args.roleId !== '') {
        this.roleId = this.args.roleId;
        try {
          let role = await this.store.findRecord('role', this.roleId);
          this.roleName = role.name;
          // eslint-disable-next-line no-empty
        } catch (e) {}
      } else {
        let userRole = this.assignableRoles.find((x) => x.name === 'User');
        this.roleId = userRole.id;
        this.roleName = userRole.name;
      }

      if (
        typeof this.args.tenantDestinationId !== 'undefined' &&
        this.args.tenantDestinationId !== ''
      ) {
        this.tenantDestinationId = this.args.tenantDestinationId;
      }

      /*
      console.warn('3 ' + this.roleName + ' ' + this.roleId);
      alert('3 ' + this.roleName + ' ' + this.roleId);
      */

      this.available = 'available';
      this.recordWeb = await this.getRecord('web');

      // estraggo la lista degli operatori Maestrale per select2
      this.maeUsers = await this.recordWeb.maeUsers;
      let s2Users = [];
      this.maeUsers.forEach((element, index) => {
        s2Users.push({
          id: `${index}`,
          value: `${element.lastName} ${element.firstName}`,
        });
      });
      this.s2Users = JSON.stringify(s2Users);

      setTimeout(() => {
        this.setupSelect2('#usersCrudBirthState');
        this.setupSelect2('#usersCrudResidenceState');
        this.available = 'available';
      }, 80);
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
  });

  // annullo modifica di un record
  @action
  clean() {
    this.newRecord = this.initializeRecord();
  }

  // viene selezionato un operatore
  @action
  async setOperator(key) {
    if (key !== '') {
      let selected = this.maeUsers[key];

      this.storeNewValue('lastName', selected.lastName);
      this.storeNewValue('firstName', selected.firstName);
      this.storeNewValue('contactEmail', selected.email);
      this.storeNewValue('email', selected.email);

      let select = `<select id="users-create-select" class="form-control">
                    <option value="">--</option>`;

      if (this.assignableRoles) {
        this.assignableRoles.forEach((element) => {
          select += `<option value="${element.id}">${element.name}</option>`;
        });

        select += '</select>';
      }

      let self = this;
      this.dialogs.confirm(
        '<h6>Inserimento operatore</h6>',
        `<p>Seleziona un ruolo per l'utente ${selected.lastName} ${selected.firstName}:</p> ${select}`,
        () => {
          if ($('#users-create-select').val() !== '') {
            this.statusService.userRole = $('#users-create-select').val();
            self.saveVoice(true);
          } else {
            this.dialogs.toast(
              `Seleziona un ruolo`,
              'error',
              'bottom-right',
              4
            );
            return false;
          }
        },
        null,
        ['Crea', 'Annulla']
      );
    } else {
      this.storeNewValue('lastName', '');
      this.storeNewValue('firstName', '');
      this.storeNewValue('contactEmail', '');
      this.storeNewValue('email', '');
    }
  }

  // cattura il valore inserito dall'utente per la creazione/modifica di un record
  @action
  storeNewValue(field, event) {
    let val = '';
    try {
      val = event.target.value;
    } catch (e) {
      val = event;
    }
    this.newRecord[field] = val.trim();
  }

  // Applica select2 e ne gestisce la variazione
  @action
  setupSelect2(element) {
    $(element).select2({
      theme: 'bootstrap4',
      width: '100%',
    });

    // Make sure to sync changes made by Select2 with Ember
    $(element).on('change', () => {
      let field = $(element).attr('data-field');
      let selectedValue = $(element).val();
      this.newRecord[field] = selectedValue;
    });
  }

  @action
  teardownSelect2(element) {
    $(element).off('change');
    $(element).select2('destroy');
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);
    // Rimuovi il listener quando il componente viene distrutto
    this.teardownSelect2('#usersCrudBirthState');
    this.teardownSelect2('#usersCrudResidenceState');
  }

  // chiede conferma per salvataggio modifiche a un record
  @action
  saveVoice(bypassConfirm) {
    let errors = 0;
    this.newRecord.id = v4();

    //console.warn(this.registrationFields);
    Object.keys(this.registrationFields).forEach((key) => {
      if (key !== 'email') {
        // nota: in questa registrazione (da admin) il BE non accetta il campo e-mail, quindi usiamo contactEmail
        let required = this.registrationFields[key] === '2';
        if (required && this.newRecord[key].toString() === '') {
          errors++;
        }
      }
    });

    if (errors > 0) {
      this.dialogs.toast(
        `Tutti i campi contrassegnati dall'asterisco sono obbligatori`,
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    if (!bypassConfirm) {
      let self = this;
      this.dialogs.confirm(
        '<h6>Creazione nuovo utente</h6>',
        `<p>Confermi?</p>`,
        () => {
          this.statusService.userRole = this.roleId;
          this.statusService.tenantDestinationId = this.tenantDestinationId;
          self.createUser.perform(this.newRecord);
        },
        null,
        ['Conferma', 'Annulla']
      );
    } else {
      if (this.statusService.userRole === '') {
        this.statusService.userRole = this.roleId;
        this.statusService.tenantDestinationId = this.tenantDestinationId;
      }
      this.createUser.perform(this.newRecord);
    }
  }

  createUser = task({ drop: true }, async (newUser) => {
    try {
      let user = this.store.createRecord('user-profile', newUser);
      await user.save();
      this.statusService.userRole = '';
      this.statusService.tenantDestinationId = '';
      this.start.perform();
      Swal.fire({
        title: 'Utente creato',
        html: `L'utente è stato creato correttamente,${
          this.roleName !== ''
            ? ' con ruolo <em>' + this.roleName + '</em>'
            : ''
        }<br /><br />
              <small>Puoi modificarne il ruolo nel menù<br /><em>Impostazioni > Autorizzazioni</em></small>`,
        icon: 'success',
        showCancelButton: false,
      });
      if (this.triggerRefreshFn) this.triggerRefreshFn();
    } catch (error) {
      error.then((e) => {
        if (e.errors && e.errors[0] && e.errors[0].status) {
          let status = parseInt(e.errors[0].status);

          // utente creato correttamente ma invio e-mail non riuscito
          if (status === 428) {
            this.statusService.userRole = '';
            this.statusService.tenantDestinationId = '';
            this.newRecord = this.initializeRecord();
            this.start.perform();

            Swal.fire({
              title: 'Operazione parzialmente riuscita',
              html: `L'utente è stato creato correttamente.<br />L'invio dell'e-mail informativa non è riuscito.<br /><br />
                    <small>L'utente potrà effettuare un recupero password per accedere.</small>`,
              icon: 'warning',
              showCancelButton: false,
            });
            if (this.triggerRefreshFn) this.triggerRefreshFn();
          } else if (status === 451) {
            // utente già esistente ed associato ad altro tenant
            this.newRecord = this.initializeRecord();
            Swal.fire({
              title: 'Richiesta di autorizzazione',
              html: `L'utente è già registrato presso un'altra azienda di questo portale.
                  Deve quindi autorizzarti ad accedere alla sua anagrafica.<br /><br />
                  <small>Gli abbiamo inviato un'email contenente il link per l'autorizzazione<br /><br />
                  Maggiori dettagli nella sezione<br />"Richieste di autorizzazione"</small>`,
              icon: 'warning',
              showCancelButton: false,
            });
          } else if (status === 409) {
            // utente già registrato a questo tenant
            this.newRecord = this.initializeRecord();
            Swal.fire({
              title: 'Utente esistente',
              html: `L'utente con e-mail <em>${newUser.contactEmail}</em> è già registrato!`,
              icon: 'error',
              showCancelButton: false,
            });
          } else if (status === 412) {
            // utente già registrato a questo tenant
            this.newRecord = this.initializeRecord();
            Swal.fire({
              title: `In attesa dell'Utente!`,
              html: `È già stata inviata una richiesta di associazione all'utente con e-mail <em>${newUser.contactEmail}</em>.<br /><br />
                L'utente dovrà accettare o rifiutare la richiesta tramite il link contenuto nell'e-mail che ha ricevuto.<br /><br />
                <small>Puoi eventualmente rimandare l'e-mail di autorizzazione dall'apposita tab</small>`,
              icon: 'error',
              showCancelButton: false,
            });
          } else if (status === 406) {
            // il tentantDestinationId non è figlio n-esismo del tenant corrente
            Swal.fire({
              title: 'Operazione negata',
              html: `Non hai i permessi necessari per eseguire questa azione!`,
              icon: 'error',
              showCancelButton: false,
            });
          } else {
            this.dialogs.toast(
              `Si è verificato un errore. Riprova!`,
              'error',
              'bottom-right',
              4
            );
          }
        } else {
          // errore generico
          this.dialogs.toast(
            `Si è verificato un errore. Riprova!`,
            'error',
            'bottom-right',
            4
          );
        }
      });
      this.statusService.userRole = '';
      this.statusService.tenantDestinationId = '';
    }
  });

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord(this.modelName); // Crea una nuova istanza del modello (ad esempio, 'user')
    // Crea un semplice oggetto JavaScript con tutte le proprietà del modello
    let modelData = {};
    modelInstance.eachAttribute((key) => {
      modelData[key] = modelInstance.get(key);
    });

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }

  async getRecord(environment) {
    let self = this;
    return new Promise((resolve) => {
      // recupero i record di setup
      try {
        self.store
          .queryRecord('setup', {
            filter: `equals(environment,'${environment}')`,
          })
          .then(function (record) {
            resolve(record);
          });
      } catch (e) {
        console.error(e);
        resolve(null);
      }
    });
  }
}
