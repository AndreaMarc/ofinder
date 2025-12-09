import Model, { attr } from '@ember-data/model';

export default class CustomSetupModel extends Model {
  @attr('string', { defaultValue: 'app' }) environment;
  @attr('boolean', { defaultValue: false }) maintenanceAdmin;
  @attr('objects', { defaultValue: {} }) generic;
}
