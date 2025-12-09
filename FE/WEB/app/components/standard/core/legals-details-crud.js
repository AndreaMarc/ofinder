/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import $ from 'jquery';
import { convertLangCode } from '../../../utility/convert-lang-code';
import { getIncomplete } from 'poc-nuovo-fwk/utility/utils-incomplete-config';

export default class StandardCoreLegalsDetailsCrudComponent extends Component {
  @service statusService;
  @service('siteSetup') stp;
  @service translation;
  @service permissions;
  @service store;
  @service router;
  @service session;
  @service dialogs;
  @service fetch;
  @service jsUtility;
  @service header;

  @tracked available = 'waiting';
  @tracked template = null;
  @tracked langToShow = [];
  @tracked savedChanges = true;
  @tracked localState = '';
  @tracked ckReload = '';
  handleModificationStatus = null;

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  get availableActiveLanguages() {
    return this.stp.availableActiveLanguages;
  }

  get showLangArea() {
    return this.stp.availableActiveLanguages.length >= 2;
  }

  get currentLang() {
    return this.translation.currentLang; // lingua selezionata, o del browser o di default (impostata da app.js e dal service translation)
  }

  @action
  async start() {
    if (this.args.model) {
      this.template = this.args.model;
      this.handleModificationStatus = this.args.handleModificationStatus;
    } else {
      this.available = 'unavailable';
      return;
    }

    let self = this;
    this.available = 'available';

    setTimeout(() => {
      $(`.bootstrapToggle`)
        .bootstrapToggle()
        .change(function () {
          let $this = $(this);
          let checked = $this.prop('checked');
          self.storeNewValue('active', {
            target: { value: checked },
          });
        });
    }, 60);

    this.setLocalState();

    if (this.showLangArea) {
      // mostro la sezione relativa alle traduzioni
      this.paginateTranslationArea();
    }
  }

  async paginateTranslationArea() {
    // verifico se esistono le traduzioni del template nelle varie lingue attive del sito
    let codes = this.getLangCode();
    let templateTranslation = await this.store.query('legal-term', {
      filter: `and(equals(code,'${this.template.code}'),any(language,${codes}),equals(version,'${this.template.version}'))`,
    });

    // creo l'array che mostra lo stato delle traduzioni
    let langToShow = [];
    this.availableActiveLanguages.forEach((element) => {
      let lang = {
        templateCode: this.template.code,
        templateExists: false,
        templateName: this.template.title,
        templateVersion: this.template.version,
        langCode: element.code,
        flagCode: convertLangCode(element.code),
        complete: false,
      };

      let finded = templateTranslation.filter((item) => {
        return item.language === element.code;
      });
      if (finded && finded.length > 0) {
        lang.templateExists = true;
        finded = finded[0];
        if (finded.content && finded.content !== '') {
          // finded.active &&
          lang.complete = true;
        }
      }
      langToShow.push(lang);
    });
    this.langToShow = langToShow;
  }

  // recupera i codici di lingua delle lingue attive
  getLangCode() {
    let codes = [];
    this.availableActiveLanguages.forEach((element) => {
      codes.push(`'${element.code}'`);
    });
    return codes.join();
  }

  // chiamata al click sui pulsanti delle traduzioni
  @action
  createOpenTemplate(templateExists, code, language, name, version) {
    if (!this.template || language === this.template.language) return false;
    let self = this;

    let obj = {
      title: name,
      note: this.template.note,
      code: code,
      language: language,
      content: '',
      active: false,
      version: version,
    };
    this.fetch
      .call('legalTerms/getOrCreate', 'POST', obj, {}, true, this.session)
      .then(async (res) => {
        if (res && res.id) {
          // TODO : svuotare i campi di inserimento
          this.template = null;
          if (
            this.permissions.hasPermissions(['canSeeIncompleteConfigurations'])
          ) {
            this.header.incomplete = await getIncomplete(
              this.fetch,
              this.session
            );
          }

          this.header.incomplete = getIncomplete(this.fetch, this.session); // aggiorna elenco configurazioni incomplete
          this.ckReload = new Date().getTime();
          self.router.transitionTo('legals-details', res.id);
        } else {
          throw new Error();
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
  }

  @action
  changeHtml(html) {
    let par = { target: { value: html } };
    this.storeNewValue('content', par);
  }

  // cattura il valore inserito dall'utente per la creazione/modifica di un record
  @action
  storeNewValue(field, event) {
    let val = event.target.value;
    if (typeof val === 'string') val = val.trim();
    this.template[field] = event.target.value;

    if (this.template.hasDirtyAttributes) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }
    this.statusService.isLegalModified = !this.savedChanges;

    // verifica dello Stato
    this.setLocalState();
  }

  setLocalState() {
    if (
      //this.template['active'] &&
      this.template['content'] &&
      this.template['content'] !== ''
    ) {
      this.localState = true;
    } else {
      this.localState = false;
    }
  }

  save = task({ drop: true }, async () => {
    try {
      await this.template.save();
      this.savedChanges = true;
      this.statusService.isLegalModified = false;
      this.paginateTranslationArea();
      if (this.permissions.hasPermissions(['canSeeIncompleteConfigurations'])) {
        this.header.incomplete = await getIncomplete(this.fetch, this.session); // aggiorna elenco configurazioni incomplete
      }

      //console.warn(this.header.incomplete);
      this.dialogs.toast(
        'Salvataggio completato',
        'success',
        'bottom-right',
        3,
        null
      );
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        'Errore nel salvataggio. Riprovare!',
        'error',
        'bottom-right',
        4,
        null
      );
    }
  });
}
