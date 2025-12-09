/**
 * NOTA: non esiste un end-point collegato a questo Model.
 * Viene utilizzato in fase di registrazione per generare
 * il trackedObject di verifica dei dati immessi.
 *
 * Deriva dall'unione dei campi di account con quelli di
 * userProfile.
 */
import Model, { attr } from '@ember-data/model';

export default class RegistrationModel extends Model {
  // Campi account
  @attr('string', { defaultValue: '' }) email;
  @attr('string', { defaultValue: '' }) password;
  @attr('string', { defaultValue: '' }) confirmPassword;
  @attr('string', { defaultValue: '' }) fingerPrint;
  @attr('string', { defaultValue: '' }) tenantId;
  @attr('boolean', { defaultValue: false }) termsAccepted;
  @attr('string', { defaultValue: '' }) cookieAccepted;
  @attr('string', { defaultValue: '' }) userLang;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  termsAcceptanceDate;

  // Campi userProfile
  @attr('string', { defaultValue: '' }) contactEmail;
  @attr('string', { defaultValue: '' }) firstName;
  @attr('string', { defaultValue: '' }) lastName;
  @attr('string', { defaultValue: '' }) nickName;
  @attr('string', { defaultValue: '' }) fixedPhone;
  @attr('string', { defaultValue: '' }) mobilePhone;
  @attr('string', { defaultValue: '' }) sex;
  @attr('string', { defaultValue: '' }) taxId;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  birthDate;
  @attr('string', { defaultValue: '' }) birthCity;
  @attr('string', { defaultValue: '' }) birthProvince;
  @attr('string', { defaultValue: '' }) birthZIP;
  @attr('string', { defaultValue: '' }) birthState;

  @attr('string', { defaultValue: '' }) residenceCity;
  @attr('string', { defaultValue: '' }) residenceProvince;
  @attr('string', { defaultValue: '' }) residenceZIP;
  @attr('string', { defaultValue: '' }) residenceState;
  @attr('string', { defaultValue: '' }) residenceAddress;
  @attr('string', { defaultValue: '' }) residenceHouseNumber;

  @attr('string', { defaultValue: '' }) occupation;
  @attr('string', { defaultValue: '' }) description;

  @attr('string', { defaultValue: '' }) profileImageId;

  @attr('string', { defaultValue: '' }) profileFreeFieldString1;
  @attr('string', { defaultValue: '' }) profileFreeFieldString2;
  @attr('string', { defaultValue: '' }) profileFreeFieldString3;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  profileFreeFieldDateTime;
  @attr('number', { defaultValue: 0 }) profileFreeFieldInt1;
  @attr('number', { defaultValue: 0 }) profileFreeFieldInt2;
  @attr('boolean', { defaultValue: false }) profileFreeFieldBoolean;
}
