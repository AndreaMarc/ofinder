import Service from '@ember/service';

export default class StatusServiceService extends Service {
  isTemplateModified = false;
  isLegalModified = false;
  customerIdTimesheet = '';
  userRole = '';
  tenantDestinationId = '';
}
