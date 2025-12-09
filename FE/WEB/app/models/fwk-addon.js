import Model, { attr } from '@ember-data/model';

export default class FwkAddonModel extends Model {
  @attr('number', { defaultValue: 0 }) addonsCode;
}
