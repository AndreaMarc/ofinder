import Inflector from 'ember-inflector';

export function initialize(/*application*/) {
  const inflector = Inflector.inflector;

  // Tell the inflector that the plural of "campus" is "campuses"
  inflector.irregular('ticket-tag', 'ticketTags');
  inflector.irregular('ticket-operator', 'ticketOperators');
  inflector.uncountable('ticket-history');
  inflector.irregular('ticket-message', 'ticketMessages');
  inflector.irregular('ticket-attachment', 'ticketAttachments');
  inflector.irregular('ticket-to-do', 'ticketToDos');
  inflector.irregular('ticket-operator-grant', 'ticketOperatorsGrants');
  inflector.irregular('ticket-tag-mapping', 'ticketTagMappings');
  inflector.irregular('ticket-custom-setup', 'ticketCustomSetups');
  inflector.irregular('ticket-license', 'ticketLicenses');
  inflector.irregular('ticket-pertinence', 'ticketPertinences');
  inflector.irregular('ticket-pertinence-mapping', 'ticketPertinenceMappings');
  inflector.irregular('ticket-project-mapping', 'ticketProjectMappings');

  inflector.irregular('to-do', 'toDos');
  inflector.irregular('to-do-relation', 'toDoRelations');

  inflector.irregular('project', 'projects');

  inflector.irregular('geo-country', 'geoCountries');
  inflector.irregular('geo-region', 'geoRegions');
  inflector.irregular('geo-subregion', 'geoSubregions');
  inflector.irregular('geo-city', 'geoCities');
  inflector.irregular('geo-first-division', 'geoFirstDivisions');
  inflector.irregular('geo-second-division', 'geoSecondDivisions');
  inflector.irregular('geo-third-division', 'geoThirdDivisions');
  inflector.irregular('geo-mapping', 'geoMappings');

  inflector.irregular('fwk-addon', 'fwkAddons');

  inflector.irregular('media-file', 'mediaFiles');
  inflector.irregular('role-claim', 'roleClaims');
  inflector.irregular('user-role', 'userRoles');
  inflector.irregular('user-tenant', 'userTenants');
  inflector.irregular('user-profile', 'userProfiles');
  inflector.irregular('user-device', 'userDevices');
  inflector.irregular('user-preference', 'userPreferences');
  inflector.irregular('legal-term', 'legalTerms');
  inflector.irregular('banned-user', 'bannedUsers');
  inflector.irregular('media-category', 'mediaCategories');
  inflector.irregular('custom-setup', 'customSetups');
  inflector.irregular('fwk-addon', 'fwkAddons');
  inflector.irregular('third-parts-token', 'thirdPartsTokens');
  // ERP
  inflector.irregular('erp-employee', 'erpEmployees');
  inflector.irregular('erp-employee-role', 'erpEmployeeRoles');
  inflector.irregular('erp-role', 'erpRoles');
  inflector.irregular('erp-external-worker-detail', 'erpExternalWorkerDetails');
  inflector.irregular('erp-site', 'erpSites');
  inflector.irregular('erp-shift', 'erpShifts');
  inflector.irregular('erp-site-user-mapping', 'erpSiteUserMappings');
  inflector.irregular('erp-site-working-time', 'erpSiteWorkingTimes');
  inflector.irregular('erp-employee-working-hour', 'erpEmployeeWorkingHours');

  // Poichè le API non sono standard, istruisco Ember di non pluralizzare queste rotte!
  //inflector.uncountable('terms');
  //Inflector.inflector.irregular('person', 'people');
  //Inflector.inflector.uncountable('sheep');
}

export default {
  initialize,
};
