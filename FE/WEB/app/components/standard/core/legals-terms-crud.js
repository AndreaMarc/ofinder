/* eslint-disable no-undef */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';
import { getIncomplete } from 'poc-nuovo-fwk/utility/utils-incomplete-config';

export default class StandardCoreLegalsTermsCrudComponent extends Component {
  @service('siteSetup') stp;
  @service permissions;
  @service translation;
  @service dialogs;
  @service session;
  @service header;
  @service router;
  @service store;
  @service fetch;

  @tracked available = 'waiting';
  @tracked currentLang = 'it';
  @tracked privacy = [];
  @tracked terms = [];
  @tracked cookie = [];
  @tracked ok_privacy = 'waiting';
  @tracked ok_terms = 'waiting';
  @tracked ok_cookie = 'waiting';
  @tracked error_privacy = '';
  @tracked error_terms = '';
  @tracked error_cookie = '';
  @tracked newVersion_privacy = 0;
  @tracked newVersion_terms = 0;
  @tracked newVersion_cookie = 0;
  @tracked saving_privacy = false;
  @tracked saving_terms = false;
  @tracked saving_cookie = false;

  constructor(...attributes) {
    super(...attributes);
    this.currentLang = this.translation.currentLang;
    this.start();
  }

  @action
  async start() {
    if (!this.permissions.hasPermissions(['LegalTerm.read'])) {
      this.available = 'unavailable';
      return false;
    }
    this.newVersion_privacy = 0;
    this.newVersion_terms = 0;
    this.newVersion_cookie = 0;

    this.available = 'available';
    this.getData('privacy');
    this.getData('terms');
    this.getData('cookie');
  }

  get availableActiveLanguages() {
    return this.stp.availableActiveLanguages;
  }

  @action
  changeLang(event) {
    this.currentLang = event.target.value;
    this.start();
  }

  @action
  async getData(type) {
    let code = '';
    let property = '';
    if (type === 'privacy') {
      code = 'privacyPolicy';
      property = 'privacy';
    } else if (type === 'terms') {
      code = 'termsEndConditions';
      property = 'terms';
    } else {
      code = 'cookiesAcceptance';
      property = 'cookie';
    }

    try {
      this[`ok_${property}`] = 'waiting';
      this[`error_${property}`] = '';
      this[property] = await this.store.query('legal-term', {
        filter: `and(equals(language,'${this.currentLang}'),equals(code,'${code}'))`,
        sort: `-version`,
      });
      this[`ok_${property}`] = 'available';

      if (this[property].length > 0) {
        // ricavo la versione più alta esistente nel DB così da proporne una maggiore
        let maxVersionElement = this[property].reduce((prev, current) => {
          return prev.version > current.version ? prev : current;
        });

        this[`newVersion_${property}`] = parseFloat(
          parseFloat(maxVersionElement.version) + 0.1
        ).toFixed(1);

        // verifico se c'è almeno un documento attivo
        let active = this[property].filter((element) => element.active);
        if (active.length === 0) {
          this[`error_${property}`] = 'Un documento deve essere sempre attivo!';
        } else {
          if (active.length > 1) {
            this[`error_${property}`] = 'Solo un documento può essere attivo!';
          }
        }
      } else {
        this[`newVersion_${property}`] = 1.1;
        this[`error_${property}`] = 'Un documento deve essere sempre presente';
      }
    } catch (e) {
      console.error(e);
      this[`ok_${property}`] = 'unavailable';
    }
  }

  @action
  edit(id) {
    this.router.transitionTo('legals-details', id);
  }

  @action
  newValue(type, event) {
    this[`newVersion_${type}`] = parseFloat(event.target.value).toFixed(1);
  }

  @action
  newRecord(type) {
    this.dialogs.confirm(
      '<h6>Creazione documento legale</h6>',
      `<p>Stai creando una nuova versione del documento. Ricorda che:<br />
      <ul>
        <li>il nuovo documento verrà creato in bozza (non attivo)</li>
        <li>il documento attualmente attivo non verrà né modificato né disattivato</li>
        <li>non potrai eliminare il nuovo documento (i documenti non sono cancellabili)</li>
      </ul>
      <br />Confermi?</p>`,
      () => {
        this.newRecordConfirmed.perform(type);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  newRecordConfirmed = task({ drop: true }, async (type) => {
    try {
      let version = this[`newVersion_${type}`];
      if (version === 0) {
        this.dialogs.toast(
          'Scegli una versione maggiore di zero!',
          'error',
          'bottom-right',
          3
        );
        return false;
      }
      let code = '';
      if (type === 'privacy') {
        code = 'privacyPolicy';
        this.saving_privacy = true;
      } else if (type === 'terms') {
        code = 'termsEndConditions';
        this.saving_terms = true;
      } else {
        code = 'cookiesAcceptance';
        this.saving_cookie = true;
      }

      // verifico se la versione scelta esiste già
      let exists = await this.store.query('legal-term', {
        filter: `and(equals(version,'${version}'),equals(code,'${code}'))`,
      });

      if (exists && exists.length !== 0) {
        Swal.fire(
          'Versione già esistente',
          'Scegli una versione maggiore di quelle già esistenti',
          'warning'
        );
        this.closeLoading();
        return false;
      }

      // estraggo il primo record già esistente per copiarne i dati (es: titolo, codice ecc)
      let existing = await this.store.query('legal-term', {
        filter: `and(equals(language,'${this.currentLang}'),equals(code,'${code}'))`,
        sort: `version`,
      });

      if (!existing || existing.length === 0) {
        this.dialogs.toast(
          'Si è verificato un errore. Riprova!',
          'error',
          'bottom-right',
          3
        );
        this.closeLoading();
        return false;
      }
      existing = existing[0];
      let legal = this.store.createRecord('legal-term', {
        id: v4(),
        title: existing.title,
        note: existing.note,
        code: existing.code,
        language: existing.language,
        content: '',
        active: false,
        version: version,
      });

      await legal.save();
      this.closeLoading();
      this.getData(type);
      if (this.permissions.hasPermissions(['canSeeIncompleteConfigurations'])) {
        this.header.incomplete = await getIncomplete(this.fetch, this.session); // aggiorna elenco configurazioni incomplete
      }
    } catch (e) {
      this.closeLoading();
      this.dialogs.toast(
        'Si è verificato un errore. Riprova!',
        'error',
        'bottom-right',
        3
      );
    }
  });

  closeLoading() {
    this.saving_privacy = false;
    this.saving_terms = false;
    this.saving_cookie = false;
  }

  @action
  activation(id) {
    this.dialogs.confirm(
      '<h6>Attivazione del documento</h6>',
      `<p><bold>ATTENZIONE:</bold><br />
      <ul>
        <li>questo documento sostituisce il precedente e sarà immediatamente visibile</li>
        <li>tutti gli utenti registrati che accedono al sito/app dovranno accettare il nuovo documento</li>
        <li>assicurati che il testo di questa versione sia completo</li>
      </ul>
      <br />Confermi?</p>`,
      () => {
        this.activationConfirmed.perform(id);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  activationConfirmed = task({ drop: true }, async (id) => {
    let obj = {
      id: id,
      dataActivation: new Date().toISOString(),
    };
    this.fetch
      .call('legalTerms/activation', 'PATCH', obj, {}, true, this.session)
      .then(async () => {
        this.start();
        if (
          this.permissions.hasPermissions(['canSeeIncompleteConfigurations'])
        ) {
          this.header.incomplete = await getIncomplete(
            this.fetch,
            this.session
          ); // aggiorna elenco configurazioni incomplete
        }
      })
      .catch((e) => {
        console.error(e);
        this.dialogs.toast(
          'Si è verificato un errore, riprovare!',
          'error',
          'bottom-right',
          4
        );
      });
  });
}
