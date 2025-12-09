import Model, { attr, belongsTo, hasMany } from '@ember-data/model';

export default class TicketModel extends Model {
  @belongsTo('user', { async: true, inverse: null }) user;
  @belongsTo('tenant', { async: true, inverse: null }) tenant;
  @belongsTo('tenant', { async: true, inverse: null }) tenantDestination;
  //@belongsTo('ticketTag', { async: true, inverse: null }) ticketTag;
  @hasMany('ticket-tag-mapping', { async: true, inverse: 'ticket' })
  ticketTagMappings;
  @hasMany('ticket-message', { async: true, inverse: 'ticket' }) ticketMessages;
  @hasMany('ticket-to-do', { async: true, inverse: 'ticket' }) ticketToDos;
  @hasMany('ticket-operator-grant', { async: true, inverse: 'ticket' })
  ticketOperatorGrants;
  @hasMany('ticket-relation', { async: true, inverse: 'fatherTicket' })
  ticketRelationsAsFather;
  @hasMany('ticket-relation', { async: true, inverse: 'childTicket' })
  ticketRelationsAsChild;
  @hasMany('ticket-to-do', { async: true, inverse: 'ticket' }) ticketToDos;
  @hasMany('ticket-project-mapping', { async: true, inverse: 'ticket' })
  ticketProjectMappings;

  @attr('string', { defaultValue: '' }) email;
  @attr('string', { defaultValue: '' }) phone;
  @attr('string', { defaultValue: '' }) organization;
  @attr('string', { defaultValue: '' }) vat;

  @attr('string', { defaultValue: '0' }) assignedId; // id dell'utente operatore che gestisce la richiesta
  @attr('string', { defaultValue: '' }) userId; // id dell'utente che ha creato la richiesta
  @attr('number', { defaultValue: 0 }) tenantId; // tenant dell'azienda con cui è stata creata la richiesta
  @attr('number', { defaultValue: 0 }) tenantDestinationId; // tenant dell'azienda a cui è destinata la richiesta di assistenza

  @attr('string', { defaultValue: '' }) message;
  @attr('string', { defaultValue: 'unprocessed' }) status;

  @attr('number', { defaultValue: 0 }) priority; // importanza della richiesta, per chi la gestisce. 0 = non specificata, 3 = massima.
  @attr('number', { defaultValue: 0 }) priorityOwner; // importanza della richiesta, per chi l'ha creata. 0 = non specificata, 3 = massima.
  @attr('string', { defaultValue: '' }) closedById; // id dell'utente che ha impostato il ticket come completato
  @attr('date') closedDate; // data di chiusura
  @attr('string', { defaultValue: '' }) note;
  @attr('string', { defaultValue: '' }) answer;

  @attr('string', { defaultValue: '' }) organizationToBeConfirmed; // ragione sociale inserita dagli utenti anonimi
  @attr('string', { defaultValue: '' }) firstName;
  @attr('string', { defaultValue: '' }) lastName;

  @attr('date', {
    defaultValue() {
      return new Date();
    },
  })
  creationDate;

  @attr('number', { defaultValue: 0 }) number;
  //@attr('string', { defaultValue: '' }) ticketTagId;
}
