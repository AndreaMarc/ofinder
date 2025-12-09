import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class TicketOperatorModel extends Model {
  @belongsTo('user', { async: true, inverse: null }) user;
  @belongsTo('tenant', { async: true, inverse: null }) tenantDestination;
  // eslint-disable-next-line prettier/prettier
  @belongsTo('area', { async: true, inverse: 'ticketOperator' }) area;
  @hasMany('ticket-operator-grant', { async: true, inverse: 'operator' })
  ticketOperatorGrants;

  @attr('string', { defaultValue: '' }) areaId;
  @attr('string', { defaultValue: '' }) userId;
  @attr('number', { defaultValue: 0 }) tenantDestinationId;
  @attr('boolean', { defaultValue: true }) masterTracker; // capo-area
}
