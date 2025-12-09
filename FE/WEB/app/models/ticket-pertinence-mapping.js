import Model, { attr, belongsTo } from '@ember-data/model';

export default class TicketPertinenceMappingModel extends Model {
  @belongsTo('ticket-pertinence', {
    async: true,
    inverse: 'ticketPertinenceMappings',
  })
  ticketPertinence;
  @belongsTo('area', { async: true, inverse: null }) area;

  @attr('string', { defaultValue: '' }) ticketPertinenceId;
  @attr('string', { defaultValue: '' }) areaId;
}
