import Model, { attr, belongsTo } from '@ember-data/model';

export default class TicketProjectMappingModel extends Model {
  @belongsTo('ticket', { async: true, inverse: 'ticketProjectMappings' })
  ticket;
  @belongsTo('project', { async: true, inverse: 'ticketProjectMappings' })
  project;

  @attr('string', { defaultValue: '' }) ticketId;
  @attr('string', { defaultValue: '' }) projectId;
}
