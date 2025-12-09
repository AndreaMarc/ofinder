import Model, { attr } from '@ember-data/model';

export default class NotificationModel extends Model {
  @attr('string', { defaultValue: '' }) userId;
  @attr('string', { defaultValue: '' }) email; // ?
  @attr('string', { defaultValue: '' }) title;
  @attr('string', { defaultValue: '' }) body;
  @attr('string', { defaultValue: '' }) data; // dati allegati alla notifica push
  @attr('boolean', { defaultValue: false }) onlyData;
  @attr('string', { defaultValue: '' }) messageId; // popolata da firebase con risposta dell'invio
  @attr('date') dateSent;
  @attr('date') dateRead;
  @attr('boolean') read;
  @attr('boolean') erased;

  @attr('string', { defaultValue: '' }) templateCode; // codice del template email con cui inviare la mail. La lingua?
  //@attr('boolean', { defaultValue: true }) sendNotification;
  @attr('boolean', { defaultValue: true }) sendPushNotification;
  @attr('boolean', { defaultValue: true }) sendWebSocket;
  @attr('boolean', { defaultValue: false }) forceWebSocketApp;
  @attr('boolean', { defaultValue: false }) sendEmail;
  @attr('string', { defaultValue: '' }) pushType; // valore di push-type!

  @attr('string', { defaultValue: '' }) usersGuidList;
  @attr('string', { defaultValue: '' }) rolesGuidList;
  @attr('string', { defaultValue: '' }) tenantsIdList;
}
