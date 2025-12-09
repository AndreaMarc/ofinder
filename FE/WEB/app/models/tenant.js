import Model, { attr, hasMany } from '@ember-data/model';

export default class TenantModel extends Model {
  @hasMany('user-tenant', { async: true, inverse: 'tenant' }) userTenant;
  @hasMany('category', { async: true, inverse: 'tenant' }) category;
  @hasMany('media-category', { async: true, inverse: 'tenant' }) mediaCategory;

  @attr('string', { defaultValue: '' }) name;
  @attr('number', { defaultValue: 0 }) parentTenant;
  @attr('boolean', { defaultValue: false }) enabled;
  @attr('boolean', { defaultValue: true }) isErasable;
  @attr('boolean', { defaultValue: false }) isRecovery;
  @attr('string', { defaultValue: '' }) description;

  @attr('string', { defaultValue: '' }) organization;

  @attr('string', { defaultValue: '' }) tenantVAT;
  @attr('string', { defaultValue: '' }) taxId;
  @attr('string', { defaultValue: '' }) email;
  @attr('string', { defaultValue: '' }) tenantPEC;
  @attr('string', { defaultValue: '' }) phoneNumber;
  @attr('string', { defaultValue: '' }) webSite;
  @attr('string', { defaultValue: '' }) tenantSDI;
  @attr('string', { defaultValue: '' }) tenantIBAN;
  @attr('string', { defaultValue: '' }) owner;
  @attr('string', { defaultValue: '' }) commercial;
  @attr('number', { defaultValue: 0 }) shareCapital;
  @attr('string', { defaultValue: '' }) registeredOfficeAddress; // indirizzo della sede legale
  @attr('string', { defaultValue: '' }) registeredOfficeCity;
  @attr('string', { defaultValue: '' }) registeredOfficeProvince;
  @attr('string', { defaultValue: '' }) registeredOfficeState;
  @attr('string', { defaultValue: '' }) registeredOfficeRegion;
  @attr('string', { defaultValue: '' }) registeredOfficeZIP;

  @attr('boolean', { defaultValue: true }) matchBillingAddress;
  @attr('string', { defaultValue: '' }) billingAddressAddress;
  @attr('string', { defaultValue: '' }) billingAddressCity;
  @attr('string', { defaultValue: '' }) billingAddressProvince;
  @attr('string', { defaultValue: '' }) billingAddressState;
  @attr('string', { defaultValue: '' }) billingAddressRegion;
  @attr('string', { defaultValue: '' }) billingAddressZIP;

  // free-field
  @attr('string', { defaultValue: '' }) freeFieldString1;
  @attr('string', { defaultValue: '' }) freeFieldString2;
  @attr('string', { defaultValue: '' }) freeFieldString3;
  @attr('number', { defaultValue: 0 }) freeFieldInt1;
  @attr('number', { defaultValue: 0 }) freeFieldInt2;
  @attr('number', { defaultValue: 0 }) freeFieldFloat; // dimensione: (10,3)
  @attr('boolean', { defaultValue: false }) freeFieldBoolean1;
  @attr('boolean', { defaultValue: false }) freeFieldBoolean2;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  freeFieldDateTime1;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  freeFieldDateTime2;
}
