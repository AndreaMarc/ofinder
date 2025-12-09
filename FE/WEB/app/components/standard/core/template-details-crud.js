/**
 * Consente la modifica dei Template.
 * @param {ember-object} @model : è il modello ember contenente i dati del template da modificare
 * @param {string} @showObject : indica se mostrare il campo 'oggetto dell'email'. Stringa vuota per non mostrarlo. Default false.
 * @param {string} @showTags : indica se mostrare il campo Tags. Stringa vuota per non mostrarlo. Default false.
 * @param {string} @showFeaturedImage : indica se mostrare il campo di inserimento dell'immagine-in-evidenza. Stringa vuota per non mostrarlo. Default false.
 */
/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import $ from 'jquery';
import { convertLangCode } from '../../../utility/convert-lang-code';
import { getIncomplete } from 'poc-nuovo-fwk/utility/utils-incomplete-config';

export default class StandardCoreTemplateDetailsCrudComponent extends Component {
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
  @tracked showFeaturedImage = false;
  @tracked showObject = false;
  @tracked showTags = false;

  handleModificationStatus = null;

  constructor(...attributes) {
    super(...attributes);

    //this.start();
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

  get erasable() {
    let er = false;
    if (this.template && this.template.erasable) {
      er = true;
    } else if (
      this.template &&
      this.template.language !== this.stp.siteSetup.defaultLanguage
    ) {
      er = true;
    }
    return er;
  }

  @action
  async start() {
    if (this.args.model) {
      this.template = this.args.model;
      this.handleModificationStatus = this.args.handleModificationStatus;

      this.showFeaturedImage =
        typeof this.args.showFeaturedImage !== 'undefined'
          ? !!this.args.showFeaturedImage
          : false;
      this.showObject =
        typeof this.args.showObject !== 'undefined'
          ? !!this.args.showObject
          : false;
      this.showTags =
        typeof this.args.showTags !== 'undefined'
          ? !!this.args.showTags
          : false;
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
    let templateTranslation = await this.store.query('template', {
      filter: `and(equals(code,'${this.template.code}'),any(language,${codes}))`,
    });

    // creo l'array che mostra lo stato delle traduzioni
    let langToShow = [];
    this.availableActiveLanguages.forEach((element) => {
      let lang = {
        templateCode: this.template.code,
        templateExists: false,
        templateName: this.template.name,
        langCode: element.code,
        flagCode: convertLangCode(element.code),
        complete: false,
        categoryId: this.template.categoryId,
      };
      let finded = templateTranslation.filter((item) => {
        return item.language === element.code;
      });
      if (finded && finded.length > 0) {
        lang.templateExists = true;
        finded = finded[0];
        if (finded.active && finded.content && finded.content !== '') {
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
  createOpenTemplate(templateExists, code, language, categoryId, name) {
    if (!this.template || language === this.template.language) return false;
    let self = this;
    console.log(
      `templateExists: ${templateExists}, code: ${code}, language: ${language}, name: ${this.template.name}, categoryId: ${categoryId}, name: ${name}`
    );

    let obj = {
      code: code,
      language: language,
      categoryId: categoryId,
      name: name,
    };
    this.fetch
      .call('templates/getOrCreate', 'POST', obj, {}, true, this.session)
      .then(async (res) => {
        console.log(res);
        if (res && res.id) {
          this.template = null;
          if (
            this.permissions.hasPermissions(['canSeeIncompleteConfigurations'])
          ) {
            this.header.incomplete = await getIncomplete(
              this.fetch,
              this.session
            );
          }

          this.ckReload = new Date().getTime();
          self.router.transitionTo('template-details', res.id, {
            queryParams: {
              showObject: this.showObject,
              showTags: this.showTags,
              showFeaturedImage: this.showFeaturedImage,
            },
          });
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
    return false;
  }

  // cattura il valore inserito dall'utente per la creazione/modifica di un record
  @action
  storeNewValue(field, event) {
    let val = '';
    try {
      val = event.target.value;
    } catch (e) {
      val = event.trim();
    }
    if (typeof val === 'string') val = val.trim();
    this.template[field] = val;

    if (this.template.hasDirtyAttributes) {
      this.savedChanges = false;
    } else {
      this.savedChanges = true;
    }
    this.statusService.isTemplateModified = !this.savedChanges;

    // verifiche conformità dei dati
    if (field === 'tags') {
      let regex = this.jsUtility.regex('validTag');
      if (!regex.test(this.template[field])) {
        this.dialogs.toast(
          'I Tag devono contenere solo lettere, numeri e underscore. Non sono ammessi spazi né caratteri speciali.',
          'warning',
          'bottom-right',
          5,
          null
        );
      }
    }

    // verifica dello Stato
    this.setLocalState();
  }

  setLocalState() {
    if (
      this.template['active'] &&
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
      this.statusService.isTemplateModified = false;
      this.paginateTranslationArea();
      if (this.permissions.hasPermissions(['canSeeIncompleteConfigurations'])) {
        this.header.incomplete = await getIncomplete(this.fetch, this.session); // aggiorna elenco configurazioni incomplete
      }

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

  @action
  delTemplate() {
    this.dialogs.confirm(
      `<h6>Cancellazione dell'articolo</h6>`,
      `<p>L'articolo verrà spostato nel cestino e non sarà più visualizzabile dagli utenti.<br /><br />Confermi?</p>`,
      this.executeDelete,
      null,
      ['Conferma', 'Annulla']
    );
  }

  @action
  async executeDelete() {
    let self = this;
    try {
      this.template.erased = true;
      await this.template.save();
      //await this.template.destroyRecord();
      if (this.permissions.hasPermissions(['canSeeIncompleteConfigurations'])) {
        this.header.incomplete = await getIncomplete(this.fetch, this.session); // aggiorna elenco configurazioni incomplete
      }

      //this.router.transitionTo('templates');
      window.history.back();
    } catch (e) {
      console.error(e);
      self.dialogs.toast(
        'Si è verificato un errore.<br />Cancellazione non riuscita.',
        'error',
        'bottom-right',
        4,
        null
      );
    }
  }
}
