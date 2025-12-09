/**
 * Componente per impostare quali campi degli utenti mostrare in fase di
 * registrazione e nella pagina di modifica dati-profilo.
 * Le informazioni sono memorizzate in un oggetto serializzato nel
 * campo registrationFields della tabella Setup.
 * App e Web condividono gli stessi valori.
 *
 * Struttura del campo registrationFields
 *
 * {
 *  "registration":
 *  {
 *    firstName: '',
 *    lastName: '',
 *    email: '',
 *    contactEmail: '',
 *    nickName: '',
 *    sex: '',
 *    taxId: '',
 *    birthDate: '',
 *    birthCity: '',
 *    birthProvince: '',
 *    birthZIP: '',
 *    birthState: '',
 *    residenceCity: '',
 *    residenceProvince: '',
 *    residenceZIP: '',
 *    residenceState: '',
 *    residenceAddress: '',
 *    residenceHouseNumber: '',
 *    occupation: '',
 *    description: '',
 *    fixedPhone: '',
 *    mobilePhone: '',
 *  },
 *  "profile":
 *  {
 *    ... // come sopra
 *  }
 * }
 *
 * Ciascun campo può avere i seguenti valori (stringa):
 * '0': non richiesto,
 * '1': facoltativo,
 * '2': obbligatorio
 */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { task } from 'ember-concurrency';

export default class StandardCoreSetupGeneralComponent extends Component {
  @service siteSetup;
  @service dialogs;
  @service store;
  recordApp;
  recordWeb;

  @tracked serviceAvailable = 'waiting';
  @tracked savedChanges = true;

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    // recupero i record di setup
    this.recordWeb = this.args.recordWeb;
    this.recordApp = this.args.recordApp;
    this.serviceAvailable = this.args.serviceAvailable;

    this.registrationFields = this.recordWeb.registrationFields;

    if (typeof this.registrationFields['registration'] === 'undefined')
      this.registrationFields['registration'] = {};
    if (typeof this.registrationFields['profile'] === 'undefined')
      this.registrationFields['profile'] = {};

    await this.setDefaultValue();
  }

  @action
  changeValue(field, forRegistration, event) {
    if (forRegistration) {
      this.registrationFields.registration[field] = event.target.value;
    } else {
      this.registrationFields.profile[field] = event.target.value;
    }
    console.log(field, event.target.value, this.registrationFields);

    this.recordWeb.registrationFields = this.registrationFields;
    this.recordApp.registrationFields = this.registrationFields;
  }

  /**
   *
   * @param {string} field : nome del campo. Es: profileFreeFieldString1
   * @param {string} fieldType : field/label. Se field, stabilisce la visibilità del campo custom. Se label, imposta la label del campo
   * @param {string} forRegistration 0/1/2. 0 = solo per registrazione. 1 = solo per profilo. 2 = per registrazione e profilo
   * @param {*} event
   */
  @action
  changeCustom(field, fieldType, forRegistration, varType, event) {
    let val = event.target.value;

    // verifico se il campo esiste nel json
    if (typeof this.registrationFields.registration[field] === 'undefined') {
      this.registrationFields.registration[field] = {};
    }
    if (typeof this.registrationFields.profile[field] === 'undefined') {
      this.registrationFields.profile[field] = {};
    }

    if (forRegistration === 0) {
      this.registrationFields.registration[field][fieldType] = val;
      this.registrationFields.registration[field]['type'] = varType;
    } else if (forRegistration === 1) {
      this.registrationFields.profile[field][fieldType] = val;
      this.registrationFields.profile[field]['type'] = varType;
    } else {
      this.registrationFields.registration[field][fieldType] = val;
      this.registrationFields.profile[field][fieldType] = val;

      this.registrationFields.registration[field]['type'] = varType;
      this.registrationFields.profile[field]['type'] = varType;
    }

    //console.log(field, event.target.value, this.registrationFields);

    this.recordWeb.registrationFields = this.registrationFields;
    this.recordApp.registrationFields = this.registrationFields;
  }

  save = task({ drop: true }, async () => {
    try {
      await this.recordWeb.save();
      await this.recordApp.save();

      this.siteSetup.setSetup('registrationFields', this.registrationFields);

      this.dialogs.toast('Operazione riuscita', 'success', 'bottom-right', 3);
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

  async setDefaultValue() {
    this.registrationFields.registration.firstName = '2';
    this.registrationFields.registration.lastName = '2';
    this.registrationFields.registration.email = '2';
    this.registrationFields.registration.contactEmail = '0';

    this.registrationFields.profile.firstName = '2';
    this.registrationFields.profile.lastName = '2';
    this.registrationFields.profile.email = '2';

    if (typeof this.registrationFields.profile.contactEmail === 'undefined')
      this.registrationFields.profile.contactEmail = '0';

    if (typeof this.registrationFields.registration.nickName === 'undefined')
      this.registrationFields.registration.nickName = '0';
    if (typeof this.registrationFields.profile.nickName === 'undefined')
      this.registrationFields.profile.nickName = '0';

    if (typeof this.registrationFields.registration.sex === 'undefined')
      this.registrationFields.registration.sex = '0';
    if (typeof this.registrationFields.profile.sex === 'undefined')
      this.registrationFields.profile.sex = '0';

    if (typeof this.registrationFields.registration.taxId === 'undefined')
      this.registrationFields.registration.taxId = '0';
    if (typeof this.registrationFields.profile.taxId === 'undefined')
      this.registrationFields.profile.taxId = '0';

    if (typeof this.registrationFields.registration.fixedPhone === 'undefined')
      this.registrationFields.registration.fixedPhone = '0';
    if (typeof this.registrationFields.profile.fixedPhone === 'undefined')
      this.registrationFields.profile.fixedPhone = '0';

    if (typeof this.registrationFields.registration.mobilePhone === 'undefined')
      this.registrationFields.registration.mobilePhone = '0';
    if (typeof this.registrationFields.profile.mobilePhone === 'undefined')
      this.registrationFields.profile.mobilePhone = '0';

    if (typeof this.registrationFields.registration.birthDate === 'undefined')
      this.registrationFields.registration.birthDate = '0';
    if (typeof this.registrationFields.profile.birthDate === 'undefined')
      this.registrationFields.profile.birthDate = '0';

    if (
      typeof this.registrationFields.registration.birthProvince === 'undefined'
    )
      this.registrationFields.registration.birthProvince = '0';
    if (typeof this.registrationFields.profile.birthProvince === 'undefined')
      this.registrationFields.profile.birthProvince = '0';

    if (typeof this.registrationFields.registration.birthZIP === 'undefined')
      this.registrationFields.registration.birthZIP = '0';
    if (typeof this.registrationFields.profile.birthZIP === 'undefined')
      this.registrationFields.profile.birthZIP = '0';

    if (typeof this.registrationFields.registration.birthState === 'undefined')
      this.registrationFields.registration.birthState = '0';
    if (typeof this.registrationFields.profile.birthState === 'undefined')
      this.registrationFields.profile.birthState = '0';

    if (
      typeof this.registrationFields.registration.residenceCity === 'undefined'
    )
      this.registrationFields.registration.residenceCity = '0';
    if (typeof this.registrationFields.profile.residenceCity === 'undefined')
      this.registrationFields.profile.residenceCity = '0';

    if (
      typeof this.registrationFields.registration.residenceProvince ===
      'undefined'
    )
      this.registrationFields.registration.residenceProvince = '0';
    if (
      typeof this.registrationFields.profile.residenceProvince === 'undefined'
    )
      this.registrationFields.profile.residenceProvince = '0';

    if (
      typeof this.registrationFields.registration.residenceZIP === 'undefined'
    )
      this.registrationFields.registration.residenceZIP = '0';
    if (typeof this.registrationFields.profile.residenceZIP === 'undefined')
      this.registrationFields.profile.residenceZIP = '0';

    if (
      typeof this.registrationFields.registration.residenceState === 'undefined'
    )
      this.registrationFields.registration.residenceState = '0';
    if (typeof this.registrationFields.profile.residenceState === 'undefined')
      this.registrationFields.profile.residenceState = '0';

    if (
      typeof this.registrationFields.registration.residenceAddress ===
      'undefined'
    )
      this.registrationFields.registration.residenceAddress = '0';
    if (typeof this.registrationFields.profile.residenceAddress === 'undefined')
      this.registrationFields.profile.residenceAddress = '0';

    if (
      typeof this.registrationFields.registration.residenceHouseNumber ===
      'undefined'
    )
      this.registrationFields.registration.residenceHouseNumber = '0';
    if (
      typeof this.registrationFields.profile.residenceHouseNumber ===
      'undefined'
    )
      this.registrationFields.profile.residenceHouseNumber = '0';

    if (typeof this.registrationFields.registration.occupation === 'undefined')
      this.registrationFields.registration.occupation = '0';
    if (typeof this.registrationFields.profile.occupation === 'undefined')
      this.registrationFields.profile.occupation = '0';

    if (typeof this.registrationFields.registration.description === 'undefined')
      this.registrationFields.registration.description = '0';
    if (typeof this.registrationFields.profile.description === 'undefined')
      this.registrationFields.profile.description = '0';

    return;
  }
}
