/**
 * Da rinominare in ticket-tracker ?
 * Tiene traccia di quale opratore segue un ticket, aldilà di quelli proprietari del ticket
 */
import Model, { attr } from '@ember-data/model';

export default class TicketOperatorTrackerModel extends Model {
  @attr('string', { defaultValue: '' }) operatorId;
  @attr('string', { defaultValue: '' }) ticketId;
}
