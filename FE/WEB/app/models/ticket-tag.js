import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class TicketTagModel extends Model {
  @belongsTo('tenant', { async: true, inverse: null }) tenantDestination;
  @hasMany('ticket-tag-mapping', { async: true, inverse: 'ticketTag' })
  ticketTagMappings;

  @attr('string', { defaultValue: '' }) name;
  @attr('string', { defaultValue: '' }) tag;
  @attr('string', { defaultValue: '' }) note;
  @attr('number', { defaultValue: 0 }) tenantDestinationId;
}
