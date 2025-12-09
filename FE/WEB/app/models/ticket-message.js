import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class TicketMessageModel extends Model {
  @belongsTo('ticket', { async: true, inverse: 'ticketMessage' }) ticket;
  @hasMany('ticket-attachment', { async: true, inverse: 'ticketMessage' })
  ticketAttachment;

  @attr('string', { defaultValue: '' }) ticketId;
  @attr('string', { defaultValue: '' }) message;
  @attr('string', { defaultValue: '' }) operatorId;
  @attr('string', { defaultValue: '' }) userId;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  createdAt;
}
