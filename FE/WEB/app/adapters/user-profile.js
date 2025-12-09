import ApplicationAdapter from './application';
import { inject as service } from '@ember/service';

export default class UserProfileAdapter extends ApplicationAdapter {
  @service statusService;

  ajaxOptions(url, type, options) {
    let userRole = this.statusService.userRole;
    let tenantDestinationId = this.statusService.tenantDestinationId;

    let hash = super.ajaxOptions(url, type, options);

    if (type === 'POST' && userRole && userRole !== '') {
      hash.headers = hash.headers || {};
      hash.headers['roles'] = userRole;
      hash.headers['tenantDestinationId'] = tenantDestinationId;
    }

    return hash;
  }
}
