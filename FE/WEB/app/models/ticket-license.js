import Model, { attr, belongsTo } from '@ember-data/model';

export default class TicketLicenseModel extends Model {
  @belongsTo('tenant', { async: true, inverse: null }) tenantDestination;

  @attr('number', { defaultValue: 0 }) tenantDestinationId;
  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  activationDate;
  @attr('boolean', { defaultValue: false }) unlimitedService;
  @attr('date-utc') expirationDate;
  @attr('boolean', { defaultValue: false }) canChoiseUnlogged; // ticket agli anonimi
  @attr('boolean', { defaultValue: false }) canManageProjects; // gestione dei Progetti (commesse) a cui assegnare i ticket

  @attr('boolean', { defaultValue: false }) canUseTag; // assegnare Tag ai Ticket
  @attr('boolean', { defaultValue: false }) multiArea; // dividere i ticket/todo per aree (reparti)
  @attr('boolean', { defaultValue: false }) canUseToDo; // toDo abilitati
  @attr('boolean', { defaultValue: false }) canUseLeader; // possibilità di definire capo-area
  @attr('boolean', { defaultValue: false }) canUseRoles; // possibilità di definire ruoli custom
  @attr('boolean', { defaultValue: false }) canUseTracker; // possibilità di seguire i ticket
  @attr('boolean', { defaultValue: false }) canRelateTicket; // può definire relazioni tra ticket
  @attr('boolean', { defaultValue: false }) canUseWorkflow; // può definire stati e flussi personalizzati
  @attr('boolean', { defaultValue: false }) canUseSla; // può definire stati e flussi personalizzati
  @attr('boolean', { defaultValue: false }) canUseScopes; // può definire gli ambiti
}
