import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class AreaModel extends Model {
  @belongsTo('tenant', { async: true, inverse: null }) tenantDestination;
  @hasMany('ticket-operator', { async: true, inverse: 'area' })
  ticketOperator;

  @attr('string', { defaultValue: '' }) name;
  @attr('string', { defaultValue: '' }) note;
  @attr('boolean', { defaultValue: true }) erasable;
  @attr('number', { defaultValue: 0 }) tenantDestinationId;
}
