/**
 * Servizio di gestione dei webSocket.
 * Può essere importato ovunque (es: un componente) ed utilizzato come segue:
 *
 *
 * export default class MyComponent extends Component {
  @service webSocket;

  @action
  sendMessage() {
    this.webSocket.sendMessage({ type: 'ping' });
    }
  }
 */

import Service from '@ember/service';
import { inject as service } from '@ember/service';
import { getAdvices } from 'poc-nuovo-fwk/utility/utils-get-advices';
import config from 'poc-nuovo-fwk/config/environment';

export default class WebSocketService extends Service {
  @service pushNotifications;
  @service pushCallback;
  @service translation;
  @service websockets;
  @service session;
  @service header;
  @service router;
  @service store;

  socketRef = null;

  connect() {
    let ws = config.apiHost.replace('https', 'wss').replace('http', 'ws');
    //ws = 'ws://192.168.200.103'; // TODO : cancella qui!
    console.log('AVVIO WEB-SOCKET: ' + ws);
    const socket = this.websockets.socketFor(`${ws}/websocket`);
    // Assegna i gestori degli eventi
    socket.on('open', this.openHandler, this);
    socket.on('message', this.messageHandler, this);
    socket.on('close', this.closeHandler, this);

    this.socketRef = socket;
  }

  // TODO : sistemare tutti gli eventi, se necessari
  openHandler(event) {
    console.log('WebSocket connected:', event);
    let firstMessage = {
      userId: this.session.get('data.id'),
      fingerprint: this.session.getFingerprint(),
    };
    //console.log(firstMessage);
    this.sendMessage(firstMessage);
  }

  messageHandler(event) {
    this._webSocketMessageReceived(event);
  }

  closeHandler(event) {
    console.error('WebSocket closed:', event);
  }

  sendMessage(message) {
    this.socketRef.send(JSON.stringify(message));
  }

  disconnect() {
    const socket = this.socketRef;

    // Rimuovi i gestori degli eventi
    try {
      socket.off('open', this.openHandler, this);
      socket.off('message', this.messageHandler, this);
      socket.off('close', this.closeHandler, this);

      socket.close();
    } catch (e) {
      // non sono neccesari log qui!
    }
  }

  // GESTISCE I MESSAGGI RICEVUTI DAL WEB-SOCKET
  async _webSocketMessageReceived(event) {
    let translation = this.translation.languageTranslation;
    //console.log('WebSocket message received:', event);
    try {
      let payload = JSON.parse(event.data);
      console.warn('WebSocket message received. data:', payload);

      // verifico se nel servizio pushCallback è presente la callback relativa al messaggio ricevuto
      try {
        payload.pushData = JSON.stringify(payload.pushData);
        // invoco la callback relativa al messaggio ricevuto
        this.pushCallback[payload.pushType](
          payload,
          translation,
          this.store,
          this.session,
          this.header,
          getAdvices,
          this.router
        );
      } catch (err) {
        console.error(
          `Non è stata definita la callback per il web-socket-type: ${payload.pushType}`,
          err
        );
      }
    } catch (e) {
      //console.error('Errore nel formato restituito dal WebSocket');
      if (!event.data.includes('Successfully connected')) {
        console.warn('WebSocket message received:', event.data);
      }
    }
  }
}
