/**
 * RIPRODUCE FILE MUSICALI - PER APP E WEB
 */
import Service from '@ember/service';

export default class AudioService extends Service {
  device = null;
  /**
   * RIPRODUCE UN SUONO
   * @param {string} fileName : nome del file senza estensione.
   *
   * NOTA:
   * - per iOS e Web verrà applicata in automatico l'estensione m4a
   * - per Android verrà applicata in automatico l'estensione aac
   */
  async play(fileName, device) {
    try {
      this.device = typeof device !== 'undefined' ? device : null;

      if (typeof window.cordova !== 'undefined') {
        // ambiente Cordova
        let my_media = this._getSound(fileName);
        if (my_media) my_media.play();
      } else {
        // ambiente Web
        let audioContext = new AudioContext();

        // Carica il file audio (sostituisci con il tuo percorso audio)
        const response = await fetch(this._getSound(fileName));
        const audioData = await response.arrayBuffer();
        const audioBuffer = await audioContext.decodeAudioData(audioData);

        // Crea un nodo di sorgente audio
        const source = audioContext.createBufferSource();
        source.buffer = audioBuffer;

        // Connetti il nodo di sorgente all'output dell'AudioContext
        source.connect(audioContext.destination);

        // Riproduci il file audio
        source.start(0);
      }
    } catch (e) {
      console.error('Unable play sound:', e);
    }
  }

  // utility per i file musicali
  _getMediaURL(s) {
    if (typeof this.device === 'undefined') return false;
    // eslint-disable-next-line no-undef
    if (this.device.platform.toLowerCase() === 'android')
      return '/android_asset/www/' + s;
    return s;
  }

  _getSound(file_name) {
    if (typeof window.cordova === 'undefined') {
      return `assets/sounds/${file_name}.m4a`;
    } else {
      if (typeof this.device === 'undefined') return false;
      let media;
      // eslint-disable-next-line no-undef
      if (this.device.platform.toUpperCase() === 'IOS') {
        let fileUrl = this._getMediaURL(`assets/sounds/${file_name}.m4a`);
        // eslint-disable-next-line no-undef
        media = new Media(fileUrl, null, (err) => {
          console.warn('Audio Error: ' + err.code);
        });
      } else {
        let fileUrl = this._getMediaURL(`assets/sounds/${file_name}.aac`);
        // eslint-disable-next-line no-undef
        media = new Media(fileUrl, null, (err) => {
          console.warn('Audio Error: ' + err.code);
        });
      }

      return media;
    }
  }
}
