/**
 * this.header.notifications : numero notifiche non lette
 * this.header.messages : numero messaggi non letti
 * this.header.updatingAdvices : diventa true mentre è in corso il refresh delle notifiche e dei messaggi
 * this.showAlert : true se ci sono messaggi non letti o notifiche non lette
 */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { htmlSafe } from '@ember/template';
import { getAdvices } from 'poc-nuovo-fwk/utility/utils-get-advices';

export default class StandardCoreMessagesNotificationsComponent extends Component {
  @service translation;
  @service siteLayout;
  @service webSocket;
  @service header;
  @service session;
  @service store;
  @tracked activeTabs = 0;

  constructor(...attributes) {
    super(...attributes);
  }

  @action
  startWebSocket() {
    this.webSocket.connect();
  }

  get showAlert() {
    return this.header.getMessagesNotifications();
  }

  get getTextColor() {
    let out =
      this.siteLayout.headerLight === 'white' ? 'text-white' : 'text-dark';
    return htmlSafe(out);
  }
  get getBgColor() {
    let out = this.siteLayout.headerBackground;
    return htmlSafe(out);
  }

  // attivo se da setup sono attive le notifiche e/o la messaggeria (estraggo il dato da servizio header)
  get active() {
    let active =
      this.header.internalChat > 0 || this.header.internalNotifications;
    return active;
  }
  // verifico se la messaggeria è attiva
  get activeMessages() {
    return this.header.internalChat > 0;
  }
  // verifico se le notifiche sono attive
  get activeNotifications() {
    return this.header.internalNotifications;
  }

  // numero messaggi non letti
  get messages() {
    return this.header.messages;
  }
  // numero notifiche non lette
  get notifications() {
    return this.header.notifications;
  }

  // indica se la lista di messaggi/notifiche è in fase di caricamento (cioè chiamata API in corso)
  get updatingAdvices() {
    return this.header.updatingAdvices;
  }

  // testo che indica se e quante notifiche/messaggi ci sono
  get info() {
    // compongo il messaggio
    let out = {
      msg: '',
      activeNotifications: false,
      active1: false,
      activeMessages: false,
      active2: false,
    };
    let arr = [];

    if (this.messages > 0) {
      arr.push(
        `${this.messages} ${this.translation.languageTranslation.component.messagesNotifications.unreadedMessages}`
      );
    }
    if (this.notifications > 0) {
      arr.push(
        `${this.notifications} ${this.translation.languageTranslation.component.messagesNotifications.unreadedNotifications}`
      );
    }

    if (arr.length === 0) {
      out.msg = `${this.translation.languageTranslation.component.messagesNotifications.noNotices}`;
    } else {
      out.msg =
        `${this.translation.languageTranslation.component.messagesNotifications.youHave} ` +
        arr.join(' e ');
    }
    // compongo lo stato di attivazione e visibilità
    out.activeNotifications = this.activeNotifications;
    out.activeMessages = this.activeMessages;

    if (this.activeTabs === 0) {
      // non ho ancora mai premuto i pulsanti di cambio tab
      if (this.activeNotifications && this.activeMessages) {
        out.active1 = true;
        out.active2 = false;
      } else if (this.activeNotifications && !this.activeMessages) {
        out.active1 = true;
        out.active2 = false;
      } else if (!this.activeNotifications && this.activeMessages) {
        out.active1 = false;
        out.active2 = true;
      }
    } else {
      out.active1 = this.activeTabs === 1 ? true : false;
      out.active2 = this.activeTabs === 2 ? true : false;
    }
    return out;
  }

  get advicesList() {
    return this.header.advicesList;
  }
  get notificationsList() {
    return this.header.advicesList.notifications;
  }

  // selezione della tab da visualizzare (notifiche o messaggi)
  @action
  clickTab(tabNumber, event) {
    event.stopPropagation();
    this.activeTabs = parseInt(tabNumber);
  }

  willDestroy() {
    super.willDestroy(...arguments);
    this.webSocket.disconnect();
  }
}
