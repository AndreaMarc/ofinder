import Model, { attr, belongsTo } from '@ember-data/model';

export default class TicketRelationModel extends Model {
  @belongsTo('tenant', { async: true, inverse: null }) tenantDestination;
  @belongsTo('ticket', { async: true, inverse: 'ticketRelationsAsFather' })
  fatherTicket;
  @belongsTo('ticket', { async: true, inverse: 'ticketRelationsAsChild' })
  childTicket;

  @attr('string', { defaultValue: '' }) fatherTicketId;
  @attr('string', { defaultValue: '' }) childTicketId;
  @attr('string', { defaultValue: '' }) typology;
  @attr('number', { defaultValue: 0 }) tenantDestinationId;
}
