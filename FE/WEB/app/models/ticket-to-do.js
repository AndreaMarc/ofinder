import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class TicketToDoModel extends Model {
  @belongsTo('ticket', { async: true, inverse: 'ticketToDos' }) ticket;
  @belongsTo('to-do', { async: true, inverse: null }) toDo;

  @attr('string', { defaultValue: '' }) ticketId;
  @attr('string', { defaultValue: '' }) toDoId;
}
