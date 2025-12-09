/* eslint-disable ember/no-jquery */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { task } from 'ember-concurrency';
import $ from 'jquery';

export default class StandardCoreSetupMultiLanguageComponent extends Component {
  @service('siteSetup') stp;
  @service dialogs;
  @service store;

  siteSetup = [];
  recordWeb = null;
  recordApp = null;
  @tracked serviceAvailable = 'waiting';
  @tracked savedChanges = true;
  @tracked availableLanguages = [
    { code: 'it', label: 'Italiano', active: false },
  ];
  @tracked remainingLanguages = [];

  // costruttore
  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    this.recordWeb = this.args.recordWeb;
    this.recordApp = this.args.recordApp;
    this.serviceAvailable = this.args.serviceAvailable;

    if (this.serviceAvailable === 'available') {
      // estraggo il dato da recordWeb (recordApp contiene sempre lo stesso valore)
      let availables = this.recordWeb.availableLanguages;

      // verifico che l'italiano (lingua di default) sia presente
      let it = availables.filter((item) => {
        return item.code === 'it';
      });
      if (!it || it.length === 0) {
        availables.push({
          code: 'it',
          label: 'Italiano',
          active: false,
        });
      }

      this.updAvailableLanguages(availables);
    }
  }

  updAvailableLanguages(availableLanguages) {
    this.availableLanguages = availableLanguages;
    this.remainingLanguages = this.getClonableLang();

    let self = this;
    // abilito i toggle
    setTimeout(() => {
      $(`.lang-active`)
        .bootstrapToggle()
        .change(function () {
          self.savedChanges = false;

          // estraggo elenco delle lingue
          let al = [],
            $l = $('.lang-active');
          $.each($l, function () {
            let $this = $(this);
            let x = {
              active: $this.prop('checked'),
              code: $this.attr('data-code'),
              label: $this.attr('data-label'),
            };
            al.push(x);
          });
          self.updAvailableLanguages(al);
        });
    }, 10);
  }

  get numberActive() {
    return this.stp.availableActiveLanguages.length >= 2;
  }

  // estrae un array contenente le lingue non ancora in uso
  getClonableLang() {
    let origins = [
      {
        code: 'ar',
        label: 'عربي',
        selectLabel: 'Arabic',
        selectNations: 'Saudi Arabia',
      },
      {
        code: 'cs',
        label: 'čeština',
        selectLabel: 'Czech',
        selectNations: 'Czech Republic',
      },
      {
        code: 'da',
        label: 'Dansk',
        selectLabel: 'Danish',
        selectNations: 'Denmark',
      },
      {
        code: 'de',
        label: 'Deutsch',
        selectLabel: 'German',
        selectNations: 'Germany',
      },
      {
        code: 'el',
        label: 'Ελληνικά',
        selectLabel: 'Modern Greek',
        selectNations: 'Greece',
      },
      {
        code: 'en',
        label: 'English',
        selectLabel: 'English',
        selectNations:
          'Australia, United Kingdom, Ireland, United States, South Africa',
      },
      {
        code: 'es',
        label: 'Español',
        selectLabel: 'Spanish',
        selectNations: 'Spain, Mexico',
      },
      {
        code: 'fi',
        label: 'Suomalainen',
        selectLabel: 'Finnish',
        selectNations: 'Finland',
      },
      {
        code: 'fr',
        label: 'Français',
        selectLabel: 'French',
        selectNations: 'Canada, France',
      },
      {
        code: 'he',
        label: 'Hébreu',
        selectLabel: 'Hebrew',
        selectNations: 'Israel',
      },
      {
        code: 'hi',
        label: 'हिन्दी',
        selectLabel: 'Hindi',
        selectNations: 'India',
      },
      {
        code: 'hu',
        label: 'Magyar',
        selectLabel: 'Hungarian',
        selectNations: 'Hungary',
      },
      {
        code: 'id',
        label: 'Bahasa Indonesia',
        selectLabel: 'Indonesian',
        selectNations: 'Indonesia',
      },
      {
        code: 'it',
        label: 'Italiano',
        selectLabel: 'Italian',
        selectNations: 'Italy',
      },
      {
        code: 'ja',
        label: '日本',
        selectLabel: 'Japanese',
        selectNations: 'Japan',
      },
      {
        code: 'ko',
        label: '한국인',
        selectLabel: 'Korean',
        selectNations: 'Republic of Korea',
      },
      {
        code: 'nl',
        label: 'Nederlands',
        selectLabel: 'Dutch',
        selectNations: 'Belgium, Netherlands',
      },
      {
        code: 'no',
        label: 'Norsk',
        selectLabel: 'Norwegian',
        selectNations: 'Norway',
      },
      {
        code: 'pl',
        label: 'Polski',
        selectLabel: 'Polish',
        selectNations: 'Poland',
      },
      {
        code: 'pt',
        label: 'Português',
        selectLabel: 'Portuguese',
        selectNations: 'Brazil, Portugal',
      },
      {
        code: 'ro',
        label: 'Română',
        selectLabel: 'Romanian',
        selectNations: 'Romania',
      },
      {
        code: 'ru',
        label: 'Русский',
        selectLabel: 'Russian',
        selectNations: 'Russian Federation',
      },
      {
        code: 'sk',
        label: 'Slovaški',
        selectLabel: 'Slovak',
        selectNations: 'Slovakia',
      },
      {
        code: 'sv',
        label: 'Svenska',
        selectLabel: 'Swedish',
        selectNations: 'Sweden',
      },
      {
        code: 'th',
        label: 'ไทย',
        selectLabel: 'Thai',
        selectNations: 'Thailand',
      },
      {
        code: 'tr',
        label: 'Türk',
        selectLabel: 'Turkish',
        selectNations: 'Turkey',
      },
      {
        code: 'zh',
        label: '中国人',
        selectLabel: 'Chinese',
        selectNations: 'China, Hong Kong, Taiwan',
      },
    ];

    let self = this;
    let notUsed = origins.filter((item) => {
      let isUsed = self.availableLanguages.filter((item2) => {
        return item2.code === item.code;
      });

      let res = isUsed.length === 0 ? true : false;
      return res;
    });

    return notUsed;
  }

  @action
  clone(code, label, umanLanguage) {
    let self = this;
    this.dialogs.confirm(
      'CLONAZIONE LINGUA',
      `<h6>Confermi la creazione della lingua '${umanLanguage}'?</h6>`,
      async () => {
        self.cloneConfirmed.perform(code, label);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  cloneConfirmed = task({ drop: true }, async (code, label) => {
    let self = this;
    this.savedChanges = false;
    try {
      // recupero la struttura dalla lingua di default
      await this.store
        .queryRecord('translation', {
          filter: `equals(languageCode,'it')`,
        })
        .then(async function (record) {
          // creo la lingua clonata
          let newRecord = await self.store.createRecord('translation', {
            languageCode: code,
            translationWeb: record.translationWeb,
            translationApp: record.translationApp,
          });
          await newRecord.save();

          // aggiorno la lista delle lingue disponibili
          let al = self.availableLanguages;
          al.push({
            code: code,
            label: label,
            active: false,
          });
          self.updAvailableLanguages(al);
          await self.saveLangs.perform();
        })
        .catch((e) => {
          console.error(e);
          throw new Error();
        });
    } catch (e) {
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
  deleteLang(code, label) {
    let self = this;
    this.dialogs.confirm(
      'CANCELLAZIONE LINGUA',
      `<h5 class="text-danger">ATTENZIONE: azione irreversibile, verranno cancellate tutte le eventuali traduzioni già apportate
      <br /><br />
      Confermi la cancellazione della lingua '${label}'?
      </h5>`,
      async () => {
        self.deleteLangConfirmed.perform(code);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }
  deleteLangConfirmed = task({ drop: true }, async (code) => {
    let self = this;
    try {
      // recupero la struttura dalla lingua di default
      let record = await self.store.queryRecord('translation', {
        filter: `equals(languageCode,'${code}')`,
      });
      if (record) {
        await record.destroyRecord();
      }

      let new_al = this.availableLanguages.filter((item) => {
        return item.code !== code;
      });
      self.updAvailableLanguages(new_al);
      await self.saveLangs.perform();
    } catch (e) {
      this.dialogs.toast(
        'Errore nella cancellazione. Riprovare!',
        'error',
        'bottom-right',
        4,
        null
      );
    }
  });

  saveLangs = task({ drop: true }, async () => {
    this.recordApp['availableLanguages'] = this.availableLanguages;
    this.recordWeb['availableLanguages'] = this.availableLanguages;
    try {
      await this.recordWeb.save();
      await this.recordApp.save();
      this.savedChanges = true;

      // aggiorno il '@service translation' così da propagare la modifica al pulsante di traduzione
      this.stp.setSetup('availableLanguages', this.availableLanguages);

      this.dialogs.toast(
        'Salvataggio completato',
        'success',
        'bottom-right',
        3,
        null
      );
    } catch (e) {
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
