/**
 * CREA IL COMPONENTE DROPDOWN DI CAMBIO LINGUA
 *
 * @param {string} @position : valori ammessi 'header' o 'footer' o 'unlogged' o 'other' in base a dove lo si vuole posizionare
 *                              Se 'header', avrà lo stile dei pulsanti del right-menu-header e come icona mostrerà la lingua corrente/di default, senza label.
 *                              Se 'footer', avrà lo stile dei pulsanti del left-footer e un'icona predefinita se non viene definita un'icona specifica
 *                              Se 'unlogged', è adatto alle pagine degli utenti non loggati (es: login). Mostrerà la lingua corrente/di default, senza label.
 *                              Se mancante o con qualunque altro valore, viene utilizzato come pulsante dropdwon.
 * @param {string} @icon : icona da utilizzare (tranne nel caso di posizionamento in header ove tale parametro viene ignorato)
 * @param {string} @label : label mostrata nel pulsante dropdown (caso di posizionamento generico)
 * @param {string} @buttonClass : classe di stile del pulsante dropdown (caso di posizionamento generico)
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::Core::ChooseLanguage @position="footer" @icon="lnr-earth"/>
 * <Standard::Core::ChooseLanguage @position="generic" @icon="fa fa-home" @label="Scegli lingua" @buttonClass="btn-primary"/>
 *
 */
import Component from '@glimmer/component';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';

export default class StandardCoreChooseLanguageComponent extends Component {
  // TODO: quando aggiungo/elimino una lingua da setup, aggironare l'header affinchè questo componente mostri le lingue aggiornate

  @service('siteSetup') stp;
  @service translation;
  @service siteLayout;

  headerArea = false;
  footerArea = false;
  genericArea = false;
  unloggedArea = false;
  icon = '';
  label = '';
  buttonClass = '';

  @tracked unloggedActive = false;

  constructor(...attributes) {
    super(...attributes);

    // definisco il posizionamento
    if (this.args.position) {
      if (this.args.position === 'header') {
        this.headerArea = true;
      } else if (this.args.position === 'footer') {
        this.footerArea = true;
      } else if (this.args.position === 'unlogged') {
        this.unloggedArea = true;
      } else this.genericArea = true;
    } else this.genericArea = true;

    // definisco l'icona
    if (this.genericArea || this.footerArea) {
      this.icon = this.args.icon && this.args.icon !== '' ? this.args.icon : '';
    }

    // definisco la label e la classe del pulsante
    if (this.genericArea) {
      this.label =
        this.args.label && this.args.label !== '' ? this.args.label : '';
      this.buttonClass =
        this.args.buttonClass && this.args.buttonClass !== ''
          ? this.args.buttonClass
          : '';
    }
  }

  get availableActiveLanguages() {
    return this.stp.availableActiveLanguages;
  }

  get showComponent() {
    return this.stp.availableActiveLanguages.length >= 2;
  }

  get currentLang() {
    return this.translation.currentLang; // lingua selezionata, o del browser o di default (impostata da app.js e dal service translation)
  }

  get getTextColor() {
    let out = this.siteLayout.headerLight === 'white' ? 'text-white' : '';
    return htmlSafe(out);
  }
  get getBgColor() {
    let out = this.siteLayout.headerBackground;
    return htmlSafe(out);
  }

  @action
  changeLang(code) {
    this.translation.changeLanguage(code);
    localStorage.setItem('poc-user-lang', code);
    this.unloggedActive = false;
  }

  @action
  toggleUnloggedArea() {
    this.unloggedActive = !this.unloggedActive;
  }
}
