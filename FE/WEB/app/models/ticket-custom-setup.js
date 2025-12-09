import Model, { attr } from '@ember-data/model';

export default class TicketCustomSetupModel extends Model {
  @attr('number', { defaultValue: 0 }) tenantDestinationId; // tenant dell'azienda a cui è destinata la richiesta di assistenza
  @attr('objects', { defaultValue: () => {} }) ticketSetup;
}
