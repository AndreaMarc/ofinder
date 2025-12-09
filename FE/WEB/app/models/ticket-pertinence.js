import Model, { attr, hasMany } from '@ember-data/model';

export default class TicketPertinenceModel extends Model {
  @hasMany('ticket-pertinence-mapping', {
    async: true,
    inverse: 'ticketPertinence',
  })
  ticketPertinenceMappings;

  @attr('number', { defaultValue: 0 }) tenantDestinationId;
  @attr('string', { defaultValue: '' }) name;
  @attr('string', { defaultValue: '' }) note;
  @attr('number', { defaultValue: 1 }) availableFor; // 0 = non loggati, 1 = loggati, 2 = entrambi
}
