/**
 * COMPONENTE PER ACCETTAZIONE DEI COOKIE
 *
 * fa comparire un popup con il pulsante di accettazione dei cookie e il link alla privacy-policy
 *
 * ESEMPIO DI UTILIZZO:
 * <Standard::Core::Gdpr/>
 */

/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { action } from '@ember/object';
import { service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { htmlSafe } from '@ember/template';
import $ from 'jquery';

export default class StandardCoreGdprComponent extends Component {
  @service translation;
  @service dialogs;
  @service router;
  @service store;

  accepted = false;
  stp = null;
  userLang = 'it';
  @tracked message = '';

  constructor(...attributes) {
    super(...attributes);
    try {
      // eslint-disable-next-line prettier/prettier
      this.userLang = this.translation.currentLang || (navigator.language || navigator.userLanguage).substring(0, 2); // es: 'it-IT' => 'it'

      this.message =
        this.translation.languageTranslation.component.gdpr.waitPlease; //'Attendere prego...';
      this.accepted =
        localStorage.getItem('poc-allow-gdpr') &&
        localStorage.getItem('poc-allow-gdpr') !== '';
    } catch (e) {
      this.userLang = 'it';
      this.message = 'Attendere prego...';
      this.accepted =
        localStorage.getItem('poc-allow-gdpr') &&
        localStorage.getItem('poc-allow-gdpr') !== '';
    }

    this.start();
  }

  @action
  async start() {
    if (!this.accepted) {
      this.message = htmlSafe(
        `<i class="fa fa-spinner fa-spin mr-2"></i> Attendere prego...`
      );

      setTimeout(() => {
        $.blockUI.defaults = {
          fadeIn: 500,
          fadeOut: 500,
        };
        $.blockUI({ message: $('.gdpr-block') });
      }, 1500);

      this.message = await this.getTemplate('cookiesAcceptance');
    }
  }

  // TODO : chiamare la nuova API allow-anonymous che restituisce il template
  async getTemplate(code) {
    let content = '';
    await this.store
      .queryRecord('legal-term', {
        filter: `and(equals(language,'${
          this.userLang
        }'),equals(code,'${code}'),equals(active,'${true}'))`,
        sort: '-version',
      })
      .then((template) => {
        content = htmlSafe(template.content);
      })
      .catch(async () => {
        content = this.defaultMessage;
        if (this.userLang !== 'it') {
          // verifico se il template esiste nella lingua di default (italiano)
          await this.store
            .queryRecord('legal-term', {
              filter: `and(equals(language,'it'),equals(code,'${code}'),equals(active,'${true}'))`,
            })
            .then((template) => {
              content = template.content;
            });
        }
      });
    return content;
  }

  // accettazione/rifiuto
  @action
  accept(type) {
    if (type === 'none') {
      this.dialogs.confirm(
        '<h6>Conferma</h6>',
        `<p>Non potrai effettuare l'accesso al sito web.<br />Sei sicuro?</p>`,
        () => {
          $.unblockUI({ message: $('.gdpr-block') });
        },
        null,
        ['Conferma', 'Annulla']
      );
    } else if (type === 'limited') {
      localStorage.setItem('poc-allow-gdpr', Date.now());
      localStorage.setItem('poc-allow-cookie', type);
      localStorage.setItem('poc-user-lang', type);
      $.unblockUI({ message: $('.gdpr-block') });
    } else {
      localStorage.setItem('poc-allow-gdpr', Date.now());
      localStorage.setItem('poc-allow-cookie', type);
      $.unblockUI({ message: $('.gdpr-block') });
    }
  }

  // redirect alla pagina di privacy-policy
  @action
  read() {
    $.unblockUI({ message: $('.gdpr-block') });
    this.router.transitionTo('terms', 'privacyPolicy');
  }

  defaultMessage = htmlSafe(`<p><b>Apprezziamo la tua privacy</b></p>
    <p class="mb-0">Noi e i nostri partner utilizziamo tecnologie come i Cookies o il targeting ed elaboriamo dati personali come l'indirizzo IP o le informazioni del browser al fine di personalizzare la navigazione del sito. Queste tecnologie possono accedere al tuo dispositivo e aiutarci a mostrarti contenuti più pertinenti e migliorare la tua esperienza su Internet.
      Lo utilizziamo anche per misurare i risultati o allineare i contenuti del nostro sito web. Poiché apprezziamo la tua privacy, ti chiediamo il permesso di utilizzare le seguenti tecnologie.</p>
    <p>`);
}
