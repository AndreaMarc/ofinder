/**
 * COMPONENTE PER GLI SLIDER
 * Da utilizzare nella pagine di login, registrazione, termini&condizioni,
 * mostra le foto e i testi impostati nella apposita pagina di setup.
 *
 * @param {string} @page : pagina in cui è posizionato. Possibili valori: 'login', 'registration', 'terms'.
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::Core::LoginSlider @page="login"/>
 *
 */

/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { action } from '@ember/object';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import $ from 'jquery';

export default class StandardCoreLoginSliderComponent extends Component {
  @service session;
  @service('siteSetup') stp;

  page = 'login';
  @tracked sliderId = '';
  @tracked sliderPics = [];
  @tracked numberPics = 0;

  siteSetup = {};

  // costruttore
  constructor(...attributes) {
    super(...attributes);
    this.sliderId = `slick-slider_${Date.now()}`;

    this.siteSetup = this.stp.siteSetup;

    if (
      typeof this.args.page !== 'undefined' &&
      ['login', 'terms', 'registration'].includes(this.args.page)
    ) {
      this.page = this.args.page;
    }

    // impagino le foto fornite dal setup
    if (
      this.siteSetup &&
      this.siteSetup.sliderPosition &&
      this.siteSetup.sliderPosition !== ''
    ) {
      if (
        this.siteSetup.sliderPics &&
        this.siteSetup.sliderPics !== '' &&
        this.siteSetup.sliderPics !== '""'
      ) {
        let sliderPics =
          typeof this.siteSetup.sliderPics === 'string'
            ? JSON.parse(this.siteSetup.sliderPics)
            : this.siteSetup.sliderPics;
        let filteredPics = sliderPics.filter((item) => {
          return item.page === this.page;
        });

        if (filteredPics) {
          this.numberPics = filteredPics.length;

          if (filteredPics && filteredPics.length > 0) {
            let secureCssPics = [];
            filteredPics.forEach((element) => {
              let pic = element;
              secureCssPics.push(pic);
            });
            this.sliderPics = secureCssPics;
          }
        }
      }
    }
  }

  @action
  startSlider(element) {
    //console.log('start', element);
    let $slider = $(`#${element.id} .slick-slider`);
    let numberPics = element.attributes['data-numberpics'].value;

    if (parseInt(numberPics) > 1) {
      $slider.slick({
        dots: true,
        slidesToShow: 1,
        slidesToScroll: 1,
      });
    }
  }
}
