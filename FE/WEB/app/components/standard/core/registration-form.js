/**
 * COMPONENTE DI REGISTRAZIONE
 *
 * @param {string} tenantId guid del Tenant a cui deve essere associato l'utente che si registra
 */
/* eslint-disable no-undef */
/* eslint-disable ember/jquery-ember-run */
/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { htmlSafe } from '@ember/template';
import $ from 'jquery';
import { MD5 } from 'crypto-js';
import { TrackedObject } from 'tracked-built-ins'; // notare il plugin tracked-built-ins !
import { task } from 'ember-concurrency';
//import config from 'poc-nuovo-fwk/config/environment';

export default class StandardCoreRegistrationFormComponent extends Component {
  @service('siteSetup') stp;
  @service translation;
  @service jsUtility;
  @service session;
  @service dialogs;
  @service router;
  @service store;
  @service fetch;

  siteSetup = {};
  currentTenant = null;
  originalPassword = '';
  animationTimeout = null;

  @tracked currentLang = this.translation.currentLang.toUpperCase();
  @tracked available = false;
  @tracked registrationFields = null;
  @tracked newValue = {};
  @tracked nations = [];
  @tracked passwordType = 'password';
  // messaggi di errore
  @tracked passwordError = '';
  @tracked passwordConfirmError = '';
  @tracked taxIdError = '';
  @tracked isGoogle = false;
  otp = '';

  constructor(...attributes) {
    super(...attributes);

    this.siteSetup = this.stp.siteSetup;
    if (!this.siteSetup || !this.siteSetup.publicRegistration) {
      this.available = false;
    } else if (!this.stp.siteSetup.registrationFields.registration) {
      this.available = false;
    } else {
      this.available = true;
      this.currentTenant = this.args.tenantId || '1';
      this.nations = this.jsUtility.nations();

      let qp = this.queryParams;
      if (qp && qp.otp && qp.otp !== '') {
        this.isGoogle = true;
        this.otp = qp.otp;
      }

      this.start();
    }
  }

  start() {
    this.newValue = this.initializeRecord();
    this.registrationFields =
      this.stp.siteSetup.registrationFields.registration;

    if (!this.registrationFields) return false; // TODO mettere una condizione che non fa vedere il form perché setup incompleto

    setTimeout(() => {
      this.setupSelect2('#registrationBirthState');
      this.setupSelect2('#registrationResidenceState');
    }, 80);

    if (this.isGoogle) {
      this.newValue.email = this.otp;
    }
  }

  // helper locale per modificare il colore degli asterischi nell'interfaccia
  /*isEmpty(field) {
    if (!this.newValue[field] || this.newValue[field] === '') {
      return 'text-danger';
    } else return '';
  }
  */

  // cattura il valore inserito dall'utente per la creazione/modifica di un record
  @action
  storeNewValue(field, event) {
    let val;
    if (field === 'termsAccepted') {
      val = event.target.checked;
    } else if (field === 'birthDate') {
      try {
        val = new Date(event.target.value.trim()).toISOString();
      } catch (e) {
        val = null;
      }
    } else val = event.target.value.trim();

    this.newValue[field] = val;

    // verifica della password e del codice fiscale
    let regex = null;
    if (['password', 'confirmPassword'].includes(field)) {
      let info = htmlSafe(this.jsUtility.verifyPassword(val));
      if (field === 'password') {
        this.passwordError = info;
      } else this.passwordConfirmError = info;
    } else if (field === 'taxId') {
      regex = this.jsUtility.regex('taxId');
      if (!regex.test(val)) {
        this.taxIdError =
          this.translation.languageTranslation.component.registrationForm.uncorrectFormat; //'formato non corretto';
      }
    }
  }

  // operazioni preliminari al salvataggio
  @action
  save() {
    let errors = [];
    // verifica campi obbligatori
    Object.keys(this.registrationFields).forEach((key) => {
      let required = this.registrationFields[key] === '2';
      if (required) {
        if (!this.newValue[key] || this.newValue[key].toString() === '') {
          errors.push('');
        }
      }
    });

    if (errors.length > 0) {
      this.dialogs.toast(
        this.translation.languageTranslation.component.registrationForm
          .mandatoryFields, //`Tutti i campi contrassegnati dall'asterisco sono obbligatori`,
        'error',
        'bottom-right',
        4
      );

      // animazione degli asterischi
      clearTimeout(this.animationTimeout);
      $('.registration-mandatory').removeClass('registration-animations');
      setTimeout(() => {
        $('.registration-mandatory.text-danger').addClass(
          'registration-animations'
        );
        this.animationTimeout = setTimeout(() => {
          $('.registration-mandatory').removeClass('registration-animations');
        }, 2000);
      }, 100);

      return false;
    }

    // verifica accettazione dei termini
    if (!this.newValue.termsAccepted) {
      this.dialogs.toast(
        this.translation.languageTranslation.component.registrationForm
          .needAccept, //`Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy!`,
        'error',
        'bottom-right',
        4
      );
      return false;
    }

    // verifica delle e-mail
    let regex;
    errors = [];
    regex = this.jsUtility.regex('email');
    if (!regex.test(this.newValue.email) && !this.isGoogle) {
      errors.push(
        this.translation.languageTranslation.component.registrationForm
          .uncorrectEmail
      ); // `Il formato del campo 'E-mail' non è corretto`
    }

    if (
      this.newValue.contactEmail !== '' &&
      !regex.test(this.newValue.contactEmail)
    ) {
      errors.push(
        this.translation.languageTranslation.component.registrationForm
          .uncorrectContactEmail
      ); // `Il formato del campo 'E-mail di Contatto' non è corretto`
    }

    // verifica delle password
    regex = this.jsUtility.regex('validPassword');
    if (!regex.test(this.newValue.password)) {
      errors.push(
        this.translation.languageTranslation.component.registrationForm
          .uncorrectPassword
      ); // `Il formato del campo 'Password' non è corretto`
    }
    if (!regex.test(this.newValue.confirmPassword)) {
      errors.push(
        this.translation.languageTranslation.component.registrationForm
          .uncorrectConfirmPassword
      ); // `Il formato del campo 'Conferma Password' non è corretto`
    }
    if (this.newValue.confirmPassword !== this.newValue.password) {
      errors.push(
        this.translation.languageTranslation.component.registrationForm
          .mismatchPassword
      ); // `Le due Password non corrispondono`
    }

    // verifica lunghezza nome e cognome
    if (
      (this.newValue.lastName !== '' && this.newValue.lastName.length < 2) ||
      (this.newValue.firstName !== '' && this.newValue.firstName.length < 2)
    ) {
      errors.push(
        this.translation.languageTranslation.component.registrationForm
          .minLengthNames
      ); // Inserisci almeno due caratteri nei campi Nome e Cognome
    }

    // verifica del codice fiscale
    regex = this.jsUtility.regex('taxId');
    if (this.newValue.taxId !== '' && !regex.test(this.newValue.taxId)) {
      errors.push(
        this.translation.languageTranslation.component.registrationForm
          .uncorrectTaxId
      ); // Il formato del campo 'Codice Fiscale' non è corretto
    }

    // TODO : aggiungere verifiche e blocchi sull'eventuale campo nick-name

    if (errors.length > 0) {
      this.dialogs.toast(errors[0], 'error', 'bottom-right', 4);
      return false;
    }

    this.originalPassword = this.newValue.password;
    this.newValue.password = MD5(this.newValue.password).toString();
    this.newValue.confirmPassword = MD5(
      this.newValue.confirmPassword
    ).toString();
    this.newValue.tenantId = this.currentTenant;
    this.newValue.fingerPrint = this.session.getFingerprint();
    this.newValue.birthDate = new Date(this.newValue.birthDate).toISOString();

    this.newValue.cookieAccepted = localStorage.getItem('poc-allow-cookie');
    this.newValue.userLang = this.translation.currentLang;
    let d = new Date().toISOString();
    this.newValue.termsAcceptanceDate = d;
    this.newValue.registrationDate = d;

    this.createUser.perform(this.newValue);
  }

  // salvataggio dati nel DB
  createUser = task({ drop: true }, async (record) => {
    let self = this;

    let endpoint = this.isGoogle ? 'account/registerOtp' : 'account/register';

    await this.fetch
      .call(endpoint, 'POST', record, {}, false, this.session)
      .then(() => {
        // operazione riuscita
        if (this.isGoogle) {
          // registrazione google avvenuta
          Swal.fire({
            title:
              this.translation.languageTranslation.component.registrationForm
                .missionComplete, //'Operazione riuscita',
            text: this.translation.languageTranslation.component
              .registrationForm.loginGoogleNow, // 'Ora puoi effettuare l'accesso con Google',
            icon: 'success',
            showCancelButton: false,
            confirmButtonText:
              this.translation.languageTranslation.component.registrationForm
                .sigIn, //'Accedi',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
          }).then((result) => {
            if (result.isConfirmed) {
              self.router.transitionTo('login');
            }
          });
        } else {
          // registrazione standard avvenuta
          Swal.fire({
            title:
              this.translation.languageTranslation.component.registrationForm
                .missionComplete, //'Operazione riuscita',
            text: this.translation.languageTranslation.component
              .registrationForm.loginToComplete, // 'Effettua il login per confermare il tuo profilo',
            icon: 'success',
            showCancelButton: false,
            confirmButtonText:
              this.translation.languageTranslation.component.registrationForm
                .sigIn, //'Accedi',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
          }).then((result) => {
            if (result.isConfirmed) {
              self.router.transitionTo('login', {
                queryParams: {
                  email: self.newValue.email,
                },
              });
              self.newValue = this.initializeRecord();
            }
          });
        }
      })
      .catch((error) => {
        this.newValue.password = this.originalPassword;
        this.newValue.confirmPassword = this.originalPassword;

        // TODO : gestire i messaggi di errore noti
        switch (error.status) {
          case 409:
            Swal.fire(
              this.translation.languageTranslation.component.registrationForm
                .alreadyExixts, //'Già registrato',
              this.translation.languageTranslation.component.registrationForm
                .alreadyExixtsInfo, //`La tua e-mail risulta già registrata. Prova a recuperare la password`,
              'warning'
            );
            break;
          case 406:
            Swal.fire(
              this.translation.languageTranslation.component.registrationForm
                .missingData, // Dati mancanti
              this.translation.languageTranslation.component.registrationForm
                .allRequired, // Tutti i campi contrassegnati dall'asterisco sono obbligatori
              'warning'
            );
            break;
          case 411:
            Swal.fire(
              this.translation.languageTranslation.component.registrationForm
                .uncorrectData, // Dati incorretti
              this.translation.languageTranslation.component.registrationForm
                .minLenghtName, // I campi Nome e Cognome devono contenere almeno 2 caratteri
              'warning'
            );
            break;
          case 412:
            Swal.fire(
              this.translation.languageTranslation.component.registrationForm
                .failureToAccept, // Mancata accettazione
              this.translation.languageTranslation.component.registrationForm
                .acceptanceRequired, // Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy
              'warning'
            );
            break;
          case 416:
            Swal.fire(
              this.translation.languageTranslation.component.registrationForm
                .uncorrectData, // Dati incorretti
              this.translation.languageTranslation.component.registrationForm
                .uncorrectTaxId, // Il formato del campo 'Codice Fiscale' non è corretto
              'warning'
            );
            break;
          case 417:
            Swal.fire(
              this.translation.languageTranslation.component.registrationForm
                .uncorrectData, // Dati incorretti
              this.translation.languageTranslation.component.registrationForm
                .uncorrectEmail, // Il formato del campo 'Email' non è corretto
              'warning'
            );
            break;
          case 423:
            Swal.fire(
              this.translation.languageTranslation.component.registrationForm
                .prohibitedOperation, // Operazione non consentita
              this.translation.languageTranslation.component.registrationForm
                .recordingDisabled, // La registrazione a questo portale non è consentita
              'warning'
            );
            break;
          default:
            this.dialogs.toast(
              this.translation.languageTranslation.component.registrationForm
                .error, // `Si è verificato un errore. Riprovare!`,
              'error',
              'bottom-right',
              4
            );
        }
      });
  });

  // mostra/nasconde le password
  @action
  showPassword() {
    this.passwordType = this.passwordType === 'password' ? 'text' : 'password';
  }

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord() {
    let modelInstance = this.store.createRecord('registration'); // Crea una nuova istanza del modello
    // Crea un semplice oggetto JavaScript con tutte le proprietà del modello
    let modelData = {};
    modelInstance.eachAttribute((key) => {
      modelData[key] = modelInstance.get(key);
    });

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }

  // Applica select2 e ne gestisce la variazione
  @action
  setupSelect2(element) {
    let self = this;
    $(element).select2({
      theme: 'bootstrap4',
      width: '100%',
    });

    // Make sure to sync changes made by Select2 with Ember
    $(element).on('change', () => {
      let field = $(element).attr('data-field');
      let selectedValue = $(element).val();
      self.newValue[field] = selectedValue;
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
    this.teardownSelect2('#registrationBirthState');
    this.teardownSelect2('#registrationResidenceState');
  }

  get queryParams() {
    try {
      const currentURL = this.router.currentURL;
      const queryString = currentURL.split('?')[1];

      if (!queryString) {
        return {};
      }

      return queryString.split('&').reduce((acc, pair) => {
        const [key, value] = pair.split('=');
        acc[decodeURIComponent(key)] = decodeURIComponent(value);
        return acc;
      }, {});
    } catch (e) {
      return {};
    }
  }
}
