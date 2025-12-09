/**
 * CREA IL COMPONENTE ShowImage
 * è un div con un'immagine di background.
 * Tale immagine può essere fornita via link o guid, in questo ultimo caso viene
 * recuperata dal database come Base64.
 *
 * @param {string} @image : guid della foto da scaricare dal database oppure
 *                          url della foto (assoluto o relativo alla cartella public della root principale)

 * @param {bool} @byUrl : indica se il parametro @image è un url (stringa true) o un guid (stringa vuota)
 * @param {string} @size : valevole quando @byUrl = false, indica la dimensione della foto da scaricare dal
 *                          server. Possibili valori 'sm', 'md', 'lg'. Default 'sm'.
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::ShowImage @image="http://...." @size="" @byUrl="true"/>
 * <Standard::ShowImage @image="a123-b4f9-...." @size="md" @byUrl=""/>
 *
 */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { htmlSafe } from '@ember/template';
import { action } from '@ember/object';

export default class StandardShowImageComponent extends Component {
  @service store;

  @tracked backgroundUrl = htmlSafe(
    `background-image: url(assets/images/sliders/loading_image.png)`
  );
  image = '';
  size = 'sm';
  byUrl = false;

  constructor(...attributes) {
    super(...attributes);

    this.byUrl =
      this.args.byUrl && this.args.byUrl.toString() === 'true' ? true : false;
    this.image =
      this.args.image && this.args.image !== '' ? this.args.image : '';

    if (
      this.args.size &&
      this.args.size !== '' &&
      ['sm', 'md', 'lg'].includes(this.args.size)
    ) {
      this.size = this.args.size;
    }

    this.saveAttr();
  }

  // adatto e memorizzo gli attributi di interesse
  @action
  saveAttr() {
    this.byUrl =
      this.args.byUrl && this.args.byUrl.toString() === 'true' ? true : false;
    this.image =
      this.args.image && this.args.image !== '' ? this.args.image : '';

    if (
      this.args.size &&
      this.args.size !== '' &&
      ['sm', 'md', 'lg'].includes(this.args.size)
    ) {
      this.size = this.args.size;
    }

    this.show();
  }

  async show() {
    let self = this;
    if (this.image === '') {
      this.byUrl = true;
      this.updateImage('assets/images/sliders/undefined_image.png');
    } else {
      if (this.byUrl) {
        // il parametro 'image' contiene l'url della foto
        this.updateImage(this.image);
      } else {
        // il parametro 'image' contiene il guid della foto
        this.store
          .queryRecord('media-file', {
            filter: `equals(id,'${this.image}')`,
            size: this.size,
          })
          .then(function (record) {
            //console.log(record);
            self.updateImage(record.base64);
          })
          .catch(() => {
            console.error(`L'immagine '${self.image}' non è stata trovata`);
            this.byUrl = true;
            self.updateImage('assets/images/sliders/404_image.png');
          });
      }
    }
  }

  // imposta l'immagine di background
  updateImage(imageUrl) {
    let absoluteUrl = imageUrl;
    if (!imageUrl.includes('base64')) {
      absoluteUrl =
        imageUrl.startsWith('/') || imageUrl.startsWith('http')
          ? imageUrl
          : '/' + imageUrl;
    }
    this.backgroundUrl = htmlSafe(`background-image: url(${absoluteUrl})`);
  }
}
