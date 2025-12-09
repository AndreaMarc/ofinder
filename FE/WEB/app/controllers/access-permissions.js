/* eslint-disable no-undef */
import Controller from '@ember/controller';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

export default class AccessPermissionsController extends Controller {
  @service('siteSetup') stp;
  @service session;
  @service dialogs;
  @service router;
  @service store;
  @service fetch;

  @tracked available = 'waiting';
  @tracked record = null;
  @tracked error = '';
  @tracked resend = false;
  otp = '';
  tenantName = '';
  userTenantId = '';

  constructor(...attributes) {
    super(...attributes);
  }

  // invocato dal gestore di percordo quando il model è stato caricato
  // nota: this.model.otp ora è valorizzato e viene appunto ritornato dal metrodo model nel gestore di percorso
  async loadData() {
    try {
      this.otp = this.model.otp;

      let query = {
        filter: `and(equals(otpSended,'${
          this.otp
        }'),equals(userId,'${this.session.get('data.id')}'))`,
        include: 'tenant',
      };

      let record = await this.store.queryRecord('otp', query);

      // verifico che il record esista
      if (!record || record.length === 0) {
        this.error = 'Il link selezionato non è corretto oppure è scaduto.';
        this.available = 'unavailable';
      } else {
        // estraggo il record da user-tenant, che mi serve sia per il reinvio dell'email che per l'accettazione/rifiuto
        let userTenant = await this.store.queryRecord('user-tenant', {
          filter: `and(equals(userId,'${record.userId}'),equals(tenantId,'${record.tenantId}'))`,
        });
        this.userTenantId = userTenant.id;

        let creationDte = new Date(record.creationDate).getTime();
        let duration = this.stp.siteSetup.mailTokenExpiresIn;
        let now = new Date().getTime();

        if (!userTenant || userTenant.length === 0) {
          // l'associazione pendente in user-tenant non esiste
          this.error = 'Il link selezionato non è corretto oppure è scaduto.';
          this.available = 'unavailable';
        } else if (userTenant.state !== 'pending') {
          this.error =
            'Il link selezionato non è più attivo perchè hai già indicato la tua preferenza.';
          this.available = 'unavailable';
        } else if (!record.isValid) {
          // link già utilizzato
          this.error = 'Il link selezionato non è più attivo';
          this.available = 'unavailable';
        } else if (now > creationDte + duration * 60000) {
          // link scaduto
          this.error = 'Il link selezionato è scaduto';
          this.available = 'unavailable';
          this.resend = true;
        } else {
          // tutto ok
          this.record = record;
          this.tenantName = record.get('tenant.name');
          this.available = 'available';
        }
      }
    } catch (e) {
      console.error(e);
      this.error = 'Si è verificato un errore.';
      this.available = 'unavailable';
    }
  }

  // invio accettazione/rifiuto
  @action
  sendResult(operation) {
    let obj = {
      otp: this.otp,
      result: operation,
    };
    let self = this;
    this.fetch
      .call(`permission/result`, 'POST', obj, {}, true, self.session)
      .then(() => {
        // eslint-disable-next-line no-undef
        Swal.fire({
          icon: 'success',
          title: 'Operazione riuscita',
          text: `Ora sei un utente dell'azienda ${this.tenantName}`,
          confirmButtonText: 'Ok',
          allowOutsideClick: false,
          allowEscapeKey: false,
          allowEnterKey: false,
        }).then((result) => {
          if (result.isConfirmed) {
            this.router.transitionTo('authenticated');
          }
        });
      })
      .catch((e) => {
        console.error(e);
        if (e.status === 409) {
          Swal.fire({
            icon: 'error',
            title: 'Operazione annullata',
            html: `Hai già espresso la tua adesione o il tuo rifiuto.<br />Il link a questa pagina non è più utilizzabile.`,
            confirmButtonText: 'Ok',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
          }).then((result) => {
            if (result.isConfirmed) {
              this.router.transitionTo('authenticated');
            }
          });
        } else if (e.status === 406) {
          this.error = `Il link a questa pagina è ormai scaduto. Puoi richiedere l'invio di un nuovo link`;
          this.available = 'unavailable';
          this.resend = true;
          Swal.fire({
            icon: 'error',
            title: 'Link non più valido',
            text: `Il link a questa pagina è ormai scaduto. Puoi richiedere l'invio di un nuovo link`,
          });
        } else {
          this.dialogs.toast(
            'Si è verificato un errore. Riprova!',
            'error',
            'bottom-right',
            4
          );
        }
      });
  }

  // richiesta di reinvio dell'email con nuovo otp
  @action
  sendAgain() {
    let obj = {
      userTenantId: this.userTenantId,
    };

    this.fetch
      .call('permission/send-again', 'POST', obj, {}, true, this.session)
      .then(() => {
        // operazione riuscita
        // eslint-disable-next-line no-undef
        Swal.fire({
          icon: 'success',
          title: 'Operazione riuscita',
          text: `Riceverai un nuovo link via e-mail.<br /><small>(<em>il link attuale non è più valido, puoi cancellare l'e-mail precedente</em>)</small>`,
          confirmButtonText: 'Ok',
          allowOutsideClick: false,
          allowEscapeKey: false,
          allowEnterKey: false,
        }).then((result) => {
          if (result.isConfirmed) {
            this.router.transitionTo('authenticated');
          }
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
  }
}
