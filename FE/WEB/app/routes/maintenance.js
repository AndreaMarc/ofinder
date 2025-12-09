import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';

export default class MaintenanceRoute extends Route {
  @service session;
  @service router;

  async beforeModel() {
    await this.session.setup();
    if (this.session.isAuthenticated) {
      let permissions = await this.session.get('data.permissions');
      if (permissions.includes('canBypassMaintenance')) {
        this.router.transitionTo('authenticated');
      }
    }
  }
}
