/**
 * Sebbene tutte le istanze di Ember siano sincronizzate, genero un BroadcastChannel
 * per sincronizzare tutte le schede aperte in occasione di eventi custom.
 *
 * Ad esempio utilizziamo questo servizio al cambio del Tenant, per forzare il
 * refresh di tutte le istanze
 *
 * Per importare il servizio (ad es. in un componente):
 * @service('broadcast-channel') channel;
 *
 * e per utilizzarlo:
 * this.channel.postMessage('refresh-tenant');
 */
export function initialize(appInstance) {
  console.log('REGISTRAZIONE SERVIZIO BROADCAST');
  // Creazione del BroadcastChannel
  const channel = new BroadcastChannel('maefwk_reserved_channel');

  // Impostazione del gestore di messaggi
  channel.addEventListener('message', (event) => {
    console.warn(`Broadcast channel message: ${event.data}`);
    switch (event.data) {
      case 'refresh-tenant':
        // richiesto reload per cambio tenant
        window.location.replace('/');
        break;
    }
  });

  // Registro il canale nell'istanza dell'applicazione per
  // utilizzarlo in altri parti dell'app (es. servizi, componenti, ecc.)
  appInstance.register('service:broadcast-channel', channel, {
    instantiate: false,
  });
}

export default {
  initialize,
};
