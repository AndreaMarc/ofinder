import Model, { attr, belongsTo } from '@ember-data/model';

export default class TicketTagMappingModel extends Model {
  @belongsTo('ticket', { async: true, inverse: 'ticketTagMappings' }) ticket;
  @belongsTo('ticket-tag', { async: true, inverse: 'ticketTagMappings' })
  ticketTag;

  @attr('string', { defaultValue: '' }) ticketId;
  @attr('string', { defaultValue: '' }) ticketTagId;
}
