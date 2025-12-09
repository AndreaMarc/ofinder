import Model, { attr, belongsTo } from '@ember-data/model';

export default class TicketAttachmentModel extends Model {
  @belongsTo('ticket-message', { async: true, inverse: 'ticketAttachment' })
  ticketMessage;

  @attr('string', { defaultValue: '' }) fileId;
  @attr('string', { defaultValue: '' }) messageId;
  @attr('string', { defaultValue: '' }) operatorId;
  @attr('string', { defaultValue: '' }) userId;
}
