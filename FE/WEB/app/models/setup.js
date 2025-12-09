import Model, { attr } from '@ember-data/model';

export default class SetupModel extends Model {
  // IMPOSTAZIONI GENERALI
  @attr('string', { defaultValue: '' }) applicationName;
  @attr('string', { defaultValue: 'app' }) environment;
  @attr('string', { defaultValue: '' }) minAppVersion;
  // //@attr('boolean', { defaultValue: false }) maintenanceAdmin;
  @attr('boolean', { defaultValue: false }) maintenance;
  //@attr('boolean', { defaultValue: false }) useRemoteFiles;
  @attr('boolean', { defaultValue: false }) disableLog;
  @attr('boolean', { defaultValue: true }) publicRegistration;
  @attr('objects', { defaultValue: '{}' }) registrationFields;
  @attr('boolean', { defaultValue: false }) canSearch;
  @attr('string', { defaultValue: 'app' }) beLanguage; // linguaggio del backend

  // SICUREZZA
  @attr('number', { defaultValue: 60 }) passwordExpirationPeriod;
  @attr('number', { defaultValue: 2 }) previousPasswordsStored;
  @attr('number', { defaultValue: 10 }) failedLoginAttempts;
  @attr('number', { defaultValue: 10 }) blockingPeriodDuration;
  @attr('string', { defaultValue: 'Maestrale2004!' }) defaultUserPassword;
  @attr('number', { defaultValue: 24 }) accessTokenExpiresIn;
  @attr('number', { defaultValue: 8 }) refreshTokenExpiresIn;
  @attr('arrays', { defaultValue: () => [] }) routesList;
  @attr('arrays', { defaultValue: () => [] }) entitiesList;
  @attr('boolean', { defaultValue: false }) useUrlStaticFiles;
  @attr('boolean', { defaultValue: true }) canChangeTenants;
  @attr('number', { defaultValue: 24 }) mailTokenExpiresIn; // per le richieste otp inviate via email
  @attr('boolean', { defaultValue: false }) mailerUsesAltText;
  @attr('boolean', { defaultValue: true }) forceLoginRedirect;
  @attr('arrays', { defaultValue: () => [] }) defaultClaims;
  @attr('boolean', { defaultValue: true }) useMD5;
  @attr('boolean', { defaultValue: false }) logicDelete;
  @attr('string', { defaultValue: '' }) rolesForEditUsers;

  // SOCIAL
  @attr('objects', { defaultValue: '{}' }) thirdPartsAccesses;
  @attr('objects', { defaultValue: '{}' }) googleCredentials;

  // COMUNICAZIONI
  @attr('string', { defaultValue: '0' }) internalChat;
  @attr('boolean', { defaultValue: true }) internalNotifications;
  @attr('boolean', { defaultValue: false }) pushNotifications;

  // ASPETTO
  @attr('string', { defaultValue: 'left' }) sliderPosition; // 'left' or 'right'
  @attr('string', { defaultValue: 'left' }) sliderRegistrationPosition; // 'left' or 'right'
  @attr('string', { defaultValue: 'left' }) sliderTermsPosition; // 'left' or 'right'
  @attr('boolean', { defaultValue: true }) fixedHeader;
  @attr('boolean', { defaultValue: true }) fixedFooter;
  @attr('boolean', { defaultValue: true }) fixedSidebar;
  //@attr('boolean', { defaultValue: true }) bodyTabsShadow;
  @attr('boolean', { defaultValue: true }) bodyTabsLine;
  @attr('boolean', { defaultValue: true }) appThemeWhite;
  @attr('boolean', { defaultValue: true }) headerShadow;
  @attr('boolean', { defaultValue: true }) sidebarShadow;
  @attr('string', { defaultValue: 'white' }) headerLight; // white/black
  @attr('string', { defaultValue: 'white' }) sidebarLight; // white/black
  @attr('string', { defaultValue: 'bg-info' }) headerBackground; // maxLength 25
  @attr('string', { defaultValue: 'bg-dark' }) sidebarBackground; // maxLength 25

  @attr('arrays', {
    defaultValue: () =>
      `[{"page":"login","title":"Title 1","description":"Description for login and welcome-page","url":"assets/images/originals/city.jpg"},{"page":"registration","title":"Title 2","description":"Description 2 for registration page","url":"assets/images/originals/city.jpg"},{"page":"terms","title":"Title 1","description":"Description for terms page","url":"assets/images/originals/city.jpg"}]`,
  })
  sliderPics; // serialized array. 'arrays' is a trasformation custom defined (transforms/arrays.js)

  @attr('arrays', {
    defaultValue: () => '[{"code":"it","label":"Italiano","active":true}]',
  })
  availableLanguages;
  @attr('string', { defaultValue: 'it' }) defaultLanguage;
  @attr('arrays', { defaultValue: () => {} }) languageSetup;
  @attr('arrays', { defaultValue: () => [] }) maeUsers;
  @attr('objects', { defaultValue: () => {} }) ticketService;
}
