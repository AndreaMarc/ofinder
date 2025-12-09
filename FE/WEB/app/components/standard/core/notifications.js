/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { getAdvices } from 'poc-nuovo-fwk/utility/utils-get-advices';
import { task } from 'ember-concurrency';
//import { htmlSafe } from '@ember/template';
import { action } from '@ember/object';
import $ from 'jquery';

export default class StandardCoreNotificationsComponent extends Component {
  @service pushCallback;
  @service translation;
  @service session;
  @service dialogs;
  @service header;
  @service router;
  @service store;

  @tracked serviceAvailable = 'waiting';
  @tracked notifications = [];
  @tracked pageNumber = 1;
  @tracked pageSize = 10;
  @tracked error = '';
  @tracked totalRecords = 0;
  @tracked typeCallbackExists = [];

  constructor(...attributes) {
    super(...attributes);
    this.typeCallbackExists = []; // inserire i tipi di push-type per i quali si vuole far comparire il pulsante di azione
    this.start();
  }

  // ricava le notifiche (anche lette)
  @action
  async start() {
    this.serviceAvailable = 'waiting';
    try {
      let userId = await this.session.get('data.id');
      let notifications = await this.store.query('notification', {
        filter: `and(equals(userId,'${userId}'),equals(erased,'false'))`,
        sort: `-dateSent`,
        page: {
          size: this.pageSize,
          number: this.pageNumber,
        },
      });

      this.notifications = notifications;
      this.totalRecords = this.notifications.meta.total;
      this.serviceAvailable = 'available';
    } catch (e) {
      this.serviceAvailable = 'unavailable';
      console.error('Errore nel recupero delle notifiche: ', e);
      this.error = `Errore nel recupero delle notifiche.`;
    }
  }

  // callback per cambio pagina
  @action
  changePage(number) {
    this.pageNumber = parseInt(number);
    this.start();
  }

  @action
  async showExpanded(index, notify) {
    $(`.notifications-preview[data-id="${index}"]`).toggleClass('expanded');
    $(`.show-btn[data-id="${index}"]`).toggle();

    if (!notify.read) {
      notify.read = true;
      notify.dateRead = new Date();
      try {
        await notify.save();
        this.refreshHeaderAdvices();
      } catch (error) {
        console.error('Error saving the notification:', error);
      }
    }
  }

  @action
  async deleteSingleNotification(notify) {
    notify.erased = true;
    await notify.save();
    this.refreshHeaderAdvices();
  }

  // aggiorna le notifiche in headers
  async refreshHeaderAdvices() {
    this.header.updatingAdvices = true;
    this.header.advicesList = await getAdvices(
      this.store,
      this.session,
      this.header
    );
    this.header.notifications = this.header.advicesList.notifications.length;
    this.header.messages = this.header.advicesList.messages.length;
    this.header.updatingAdvices = false;
  }

  // per cancellare o impostare come lette tutte le notifiche mostrate
  @action
  async processAll(operation) {
    // definisco un array di promesse contenente le operazioni di delete/patch dei singoli record
    let promises = this.notifications.map((notify) => {
      if (operation === 'delete') {
        return this.deleteNotification.perform(notify);
      } else if (operation === 'setReaded' && !notify.read) {
        return this.setReaded.perform(notify);
      }
      // Se non c'è una condizione che corrisponde, risolvi la promessa direttamente.
      return Promise.resolve();
    });

    // Attendo che tutte le promesse siano state risolte o rigettate.
    await Promise.allSettled(promises);

    // aggiorno le notifiche nell'header
    this.refreshHeaderAdvices();
    this.changePage(1);
    this.start();
  }

  deleteNotification = task({ enqueue: true }, async (notify) => {
    try {
      notify.erased = true;
      await notify.save();
      this.refreshHeaderAdvices();
    } catch (e) {
      console.error('Impossibile cancellare la notifica: ', e);
    }
  });

  setReaded = task({ enqueue: true }, async (notify) => {
    try {
      notify.read = true;
      notify.dateRead = new Date();
      await notify.save();
    } catch (e) {
      console.error('Impossibile impostare la notifica come letta: ', e);
    }
  });

  @action
  openCallBack(notify) {
    try {
      let translation = this.translation.languageTranslation;
      if (!notify.read) {
        this.setReaded.perform();
      }

      // eslint-disable-next-line prettier/prettier
      this.pushCallback[notify.pushType]({ pushData: notify.data }, null, translation, this.store, this.session, this.header, getAdvices, this.router);
    } catch (e) {
      console.error(
        'Callback non definita per notifica di tipo: ' + notify.type
      );
    }
  }
}
