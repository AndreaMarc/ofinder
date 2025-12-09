import Model, { attr } from '@ember-data/model';

export default class TicketHistoryModel extends Model {
  @attr('string', { defaultValue: '' }) operation;
  @attr('string', { defaultValue: '' }) userId;
  @attr('string', { defaultValue: '' }) details;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  createdAt;
}
