/* eslint-disable no-undef */
/* eslint-disable ember/no-jquery */
/**
 * COMPONENTE PER CREARE I CAROLELLI/SLIDERS DEL TEMA
 *
 * Basato sul plugin https://kenwheeler.github.io/slick/
 *
 * ====== IMPORTANTE ======:
 * Per usare il componente occorre:
 *
 * 1) Aggiungere queste due righe nell'head dell'index.html:
 * <link rel="stylesheet" type="text/css" href="//cdn.jsdelivr.net/gh/kenwheeler/slick@1.8.1/slick/slick.css"/>
 * <link rel="stylesheet" type="text/css" href="//cdn.jsdelivr.net/gh/kenwheeler/slick@1.8.1/slick/slick-theme.css"/>
 *
 * 2) nell'array 'style-src' del file config/content-security-policy.js, aggiungere (se non presente):
 * 'https://cdn.jsdelivr.net',
 *
 * ---------------------------------------------------------------------------------------------------------
 *
 * ====== COME USARE IL COMPONENTE ======
 * MODO STATICO:
 * SE LE IMMAGINI SONO STATICHE E NON SONO DEVONO CAMBIARE
 *
 * 1) il contenuto (div, testi e immagini) va inserito nei file .hbs tra i tag del componente.
 * 2) fornire un oggetto Js di configurazione.
 *
 *
 *
 * MODO DINAMICO:
 * QUANDO LE IMMAGINI POSSONO CAMBIARE DINAMICAMENTE
 *
 *
 * @param {object} @configuration : oggetto di configurazione del carosello. Se in formato stringa, viene convertito in oggetto. Vedi documentazione https://kenwheeler.github.io/slick/
 * @param {array} @contentUrlBase64 : OPZIONALE
 *                            Oggetto per definire le immagini iniziali. Se viene fornito, l'eventuale html inserito tra
 *                            i tag del componente viene ignorato e cancellato. Ad ogni cambiamento, ricrea lo slider.
 *                            Può contenere solo immagini fornite via url o base64 (comprensivo di mime-type)
 *                            È così definito:
[
  {
    label: '', // stringa contenente l'eventuale testo html da mostrare. Altrimenti stringa vuota.
    image: '', // url o base64 dell'immagine da mostrare
  },
]

 *
 * @param {array} @contentGuid : OPZIONALE. Ignorato se viene fornito @contentUrlBase64
 *                            Oggetto per definire le immagini iniziali. Se viene fornito, l'eventuale html inserito tra
 *                            i tag del componente viene ignorato e cancellato. Ad ogni cambiamento, ricrea lo slider.
 *                            Può contenere solo immagini fornite via guid. È la soluzine più lenta xché richiede il
 *                            download dei base64 dal DB.
 *                            È così definito:
[
  {
    label: '', // stringa contenente l'eventuale testo html da mostrare. Altrimenti stringa vuota.
    imageGuid: '', // guid dell'immagine
    imageSize: 'md', // dimensione immagine (sm, md, lg), solo per imageGuid. Predefinito 'md'.
  },
]

 * @param {bool} @enlargement : consente l'ingrandimento delle immagini (solo se è usato @contentGuid)


 * ESEMPIO STATICO
 *
 *
 * Nell'HBS:
<Standard::SliderCarousel @configuration="...">
  <div>
    <div class="slider-item">1</div>
  </div>
 </Standard::SliderCarousel>
 *
 *
 * Nel Javascript:
@tracked configuration = {
  dots: true,
  slidesToShow: 1,
  slidesToScroll: 1,
}
 *
 */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import $ from 'jquery';

export default class StandardSliderCarouselComponent extends Component {
  @service store;

  @tracked available = 'waiting';
  @tracked error = '';
  @tracked id = v4();
  configuration = {};
  content = [];
  ref = null;
  exist = false;
  @tracked byGuid = false;

  constructor(...attributes) {
    super(...attributes);
  }

  @action
  async start() {
    this.available = 'waiting';
    let id = '#slider-carousel-' + this.id;
    this.ref = $(id);

    try {
      if (typeof this.args.configuration === 'undefined') {
        throw new Error('Configurazione del Carosello/Sliders non fornita.');
      } else if (
        typeof this.args.configuration === 'object' &&
        !Array.isArray(this.args.configuration)
      ) {
        this.configuration = this.args.configuration;
      } else if (typeof this.args.configuration === 'string') {
        try {
          this.configuration = JSON.parse(this.args.configuration);
        } catch (e) {
          throw new Error('Carousel/Sliders: configuration string not valid');
        }
      } else throw new Error('Carousel/Sliders: unknows configuration format');

      await this.getContent();

      this.available = 'available';
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
    }
  }

  getImageBase64(imageGuid, size, label) {
    return new Promise((resolve) => {
      this.store
        .queryRecord('media-file', {
          filter: `equals(id,'${imageGuid}')`,
          size: size,
        })
        .then((record) => {
          let out = record.base64;
          resolve({ id: imageGuid, image: out, label: label });
        })
        .catch((error) => {
          console.log(error);
          //reject(error);
          resolve({ image: null, label: label });
        });
    });
  }

  @action
  async getContent() {
    let contentArray = null;

    if (this.exist) {
      this.ref.slick('unslick');
    }

    if (typeof this.args.contentUrlBase64 !== 'undefined') {
      if (Array.isArray(this.args.contentUrlBase64)) {
        if (this.args.contentUrlBase64.length > 0) {
          contentArray = this.args.contentUrlBase64;
          this.ref.empty();
        }
      } else {
        try {
          let x = JSON.parse(this.args.contentUrlBase64);

          if (x.length > 0) {
            contentArray = x;
            this.ref.empty();
          }
        } catch (e) {
          console.warn(
            'Errore: Il parametro @contentUrlBase64 del componente slider-carousel é una stringa che non rapresenta un array'
          );
        }
      }
    } else if (typeof this.args.contentGuid !== 'undefined') {
      if (Array.isArray(this.args.contentGuid)) {
        if (this.args.contentGuid.length > 0) {
          contentArray = this.args.contentGuid;
          this.ref.empty();
          this.byGuid = true;
        }
      } else {
        try {
          let x = JSON.parse(this.args.contentGuid);

          if (x.length > 0) {
            contentArray = x;
            this.ref.empty();
            this.byGuid = true;
          }
        } catch (e) {
          console.warn(
            'Errore: Il parametro @contentGuid del componente slider-carousel é una stringa che non rapresenta un array'
          );
        }
      }
      // ricavo i base 64 dal DB

      if (contentArray) {
        try {
          contentArray = contentArray.filter(
            (item) => item.imageGuid && item.imageGuid !== ''
          );
          if (contentArray.length > 0) {
            const promises = contentArray.map((item) =>
              this.getImageBase64(
                item.imageGuid,
                item.imageSize ? item.imageSize : 'md',
                item.label ? item.label : ''
              )
            );
            contentArray = await Promise.all(promises);
          }
        } catch (e) {
          console.error(
            'Slider-Carousel: errore nel recupero immagini da DB',
            e
          );
        }
      }
    }

    if (contentArray) {
      contentArray = contentArray.filter(
        (item) => item.image && item.image !== ''
      );

      if (contentArray.length > 0) {
        this.ref.empty();

        contentArray.forEach((element) => {
          let image = `<img width="100%" src="${
            element.image ? element.image : ''
          }" />`;

          let inner = `<div class="slider-item border-black p-1" data-id="${
            element.id
          }" data-label="${encodeURI(
            element.label && element.label !== '' ? element.label : ''
          )}">
            ${element.label && element.label !== '' ? element.label : ''}
            ${image}
          </div>`;

          this.ref.append(inner);
          setTimeout(() => {}, 50);
        });

        let self = this;
        $(document).on('click tap', '.slider-item', function (e) {
          e.preventDefault();
          let id = $(this).attr('data-id');
          let label = decodeURI($(this).data('label'));
          self.enlarge(id, label);
        });

        setTimeout(() => {
          this.ref.slick(this.configuration);
          this.exist = true;
        }, 50);
      }
    }
    return;
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);

    $(document).off('click tap', '.slider-item');
    this.ref.slick('unslick');
  }

  @action
  async enlarge(guid, label) {
    if (!guid || guid === '') return false;
    let pId = v4();

    try {
      Swal.fire({
        title: '',
        html: `
          <div class="swal2-content-custom" id="pop-${pId}">
            <p><i class="fa fa-spinner fa-spin mr-2"></i> Attendere prego...</p>
          </div>`,
        text: 'Questa è una modale a tutto schermo',
        icon: '',
        customClass: {
          container: 'swal2-fullscreen', // Classe personalizzata per il contenitore
        },
        showConfirmButton: true,
      });

      let b64 = await this.getImageBase64(guid, 'lg', '');

      let cont = `${label ? '<p class="swal2-text">' + label + '</p>' : ''}
        <img src="${b64.image}" class="swal2-image-custom" alt="My Image">`;
      $(`#pop-${pId}`).html(cont);
    } catch (e) {
      console.error(e);
      let cont = `<p class="text-danger">Si é verificato un errore</p>`;
      $(`#pop-${pId}`).html(cont);
    }
  }
}
