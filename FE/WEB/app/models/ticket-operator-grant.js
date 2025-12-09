import Model, { attr, belongsTo } from '@ember-data/model';

export default class TicketOperatorGrantModel extends Model {
  @belongsTo('ticket', { async: true, inverse: 'ticketOperatorGrants' }) ticket;
  @belongsTo('ticket-to-do', { async: true, inverse: 'ticketOperatorGrants' })
  ticketToDo;
  @belongsTo('ticket-operator', {
    async: true,
    inverse: 'ticketOperatorGrants',
  })
  operator;
  @belongsTo('user', { async: true, inverse: null }) createdBy;

  @attr('string', { defaultValue: '' }) ticketId;
  @attr('string', { defaultValue: '' }) ticketToDoId;
  @attr('string', { defaultValue: '' }) operatorId;
  @attr('string', { defaultValue: '' }) createdById;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  createdAt;
}
