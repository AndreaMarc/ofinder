/* eslint-disable prettier/prettier */
/* eslint-disable no-unused-labels */
/**
 * Estrae dal BE l'elenco delle configurazioni mancanti
 *
 * @param {*} store servizio nativo
 * @param {*} session servizio session
 */

//import { htmlSafe } from '@ember/template';

// Scarica dal server l'elenco delle configurazioni essenziali incomplete
export async function getAdvices(store, session, header) {
  let advices = {
    messages: [],
    notifications: [],
  };

  if (header.internalNotifications)
    advices.notifications = await _getNotifications(store, session);
  if (header.internalChat > 0)
    advices.messages = await _getMessages(store, session);
  return advices;
}

async function _getNotifications(store, session) {
  try {
    let userId = await session.get('data.id');
    let notifications = await store.query('notification', {
      filter: `and(equals(userId,'${userId}'),equals(read,'false'),equals(erased,'false'))`,
      sort: `-dateSent`,
    });

    return notifications;
  } catch (e) {
    console.error('Errore nel recupero delle notifiche non lette: ', e);
    return [];
  }
}

async function _getMessages(store, session) {
  // TODO : da implementare
  return [];
}