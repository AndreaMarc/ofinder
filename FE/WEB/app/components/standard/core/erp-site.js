import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { task } from 'ember-concurrency';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { TrackedObject } from 'tracked-built-ins';
import { v4 } from 'ember-uuid';

export default class StandardCoreErpSiteComponent extends Component {
  @service('siteSetup') stp;
  @service addonConfig;
  @service session;
  @service dialogs;
  @service store;

  @tracked serviceAvailable = 'waiting';
  @tracked window = 'list';
  @tracked sites = [];
  @tracked newRecord = null; // usato in creazione/modifica record
  currentTenant = null;

  constructor(...attributes) {
    super(...attributes);
    this.start.perform();
  }

  @action
  switchWindow(window) {
    if (window === 'insert') this.newRecord = this.initializeRecord('erp-site');
    this.window = window;
  }

  start = task({ drop: true }, async () => {
    try {
      this.serviceAvailable = 'waiting';

      if (!this.addonConfig.config.settings.addonErp) {
        this.serviceAvailable = 'unactive';
        return false;
      }

      this.newRecord = this.initializeRecord('erp-site');

      this.currentTenant = this.session.get('data.tenantId');

      let records = await this.store.query('erp-site', {
        filter: `equals(tenantId,'${this.currentTenant}')`,
        sort: 'name',
      });

      this.sites = records;
      this.serviceAvailable = 'available';
    } catch (e) {
      console.error(e);
      this.serviceAvailable = 'unavailable';
    }
  });

  @action
  changeValue(key, event) {
    let val = event.target && event.target.value ? event.target.value : event;

    if (
      [
        'administrativeHeadquarters',
        'registeredOffice',
        'operationalHeadquarters',
      ].includes(key)
    ) {
      val = !this.newRecord[key];
    }

    this.newRecord[key] = val;
  }

  @action
  edit(site) {
    this.newRecord = site;
    this.window = 'update';
  }

  @action
  save() {
    let uncomplete = false;
    let wanted = [
      'name',
      'address',
      'addressNumber',
      'phone',
      'city',
      'province',
      'zip',
      'state',
    ];
    wanted.forEach((item) => {
      if (!this.newRecord[item] || this.newRecord[item].toString() === '') {
        uncomplete = true;
      }
    });
    if (uncomplete) {
      this.dialogs.toast(
        `Tutti i campi sono obbligatori!`,
        'warning',
        'bottom-right',
        3
      );
      return false;
    }

    if (this.newRecord['zip'].length !== 5) {
      this.dialogs.toast(
        `Il C.A.P. deve contenere 5 caratteri!`,
        'warning',
        'bottom-right',
        3
      );
      return false;
    }

    this.dialogs.confirm(
      '<h6>Conferma</h6>',
      `<p>Confermi l'operazione?</p>`,
      () => {
        this.saveConfirmed.perform();
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  saveConfirmed = task({ drop: true }, async () => {
    let nr;

    try {
      this.newRecord['tenantId'] = this.currentTenant;
      if (this.newRecord.registeredOffice) await this.soleRegisteredOffice();
      if (this.newRecord.administrativeHeadquarters)
        await this.soleAdministrativeHeadquarters();

      if (
        this.window === 'insert' &&
        (!this.newRecord['id'] || this.newRecord['id'] === '')
      ) {
        // Inserimento nuovo record
        this.newRecord['id'] = v4();
        this.newRecord['createdAt'] = new Date().toISOString();

        let obj = JSON.parse(JSON.stringify(this.newRecord));

        nr = this.store.createRecord('erp-site', obj);
        await nr.save();

        this.start.perform();
        this.window = 'list';
      } else {
        // aggiornamento record esistente
        this.newRecord['updatedAt'] = new Date().toISOString();
        nr = this.store.peekRecord('erp-site', this.newRecord['id']);
        if (nr) {
          Object.entries(this.newRecord).forEach(([key, value]) => {
            if (nr.constructor.attributes.has(key)) {
              nr.set(key, value);
            }
          });

          if (nr.registeredOffice) await this.soleRegisteredOffice(nr.id);
          if (nr.administrativeHeadquarters)
            await this.soleAdministrativeHeadquarters(nr.id);

          await nr.save();
          this.start.perform();
          this.window = 'list';
        } else {
          this.dialogs.toast(
            `Si é verificato un errore. Riprovare!`,
            'error',
            'bottom-right',
            3
          );
        }
      }
    } catch (e) {
      console.error(e);
      if (this.window === 'insert') {
        nr.unloadRecord();
      }
      this.dialogs.toast(
        `Si è verificato un errore. Riprovare!`,
        'warning',
        'bottom-right',
        3
      );
    }
  });

  @action
  deleteSite(site) {
    this.dialogs.confirm(
      '<h6 class="text-danger">Conferma</h6>',
      `<p class="text-danger">Confermi l'eliminazione della sede <strong>${site.name}</strong>?</p>`,
      async () => {
        this.deleteConfirmed.perform(site);
      },
      null,
      ['Conferma', 'Annulla']
    );
  }

  deleteConfirmed = task({ drop: true }, async (site) => {
    try {
      await site.destroyRecord();
      this.start.perform();
    } catch (e) {
      console.error(e);
      this.dialogs.toast(
        `Si è verificato un errore. Riprova!`,
        'error',
        'bottom-right',
        3
      );
    }
  });

  async soleRegisteredOffice(currentId) {
    // Recupera tutte le sedi del tenant corrente
    let query = {
      filter: `and(equals(tenantId,'${this.currentTenant}'),equals(registeredOffice,'true'))`,
    };
    if (currentId) {
      query.filter = `and(equals(tenantId,'${this.currentTenant}'),equals(registeredOffice,'true'),not(equals(id,'${currentId}'))`;
    }

    // Recupera tutte le sedi legali del tenant corrente
    let sites = await this.store.query('erp-site', {
      filter: `and(equals(tenantId,'${this.currentTenant}'),equals(registeredOffice,'true'))`,
    });

    // Imposta come non sede legale
    for (let site of sites.slice()) {
      site.set('registeredOffice', false);
      await site.save();
    }
  }

  async soleAdministrativeHeadquarters(currentId) {
    let query = {
      filter: `and(equals(tenantId,'${this.currentTenant}'),equals(administrativeHeadquarters,'true'))`,
    };
    if (currentId) {
      query.filter = `and(equals(tenantId,'${this.currentTenant}'),equals(administrativeHeadquarters,'true'),not(equals(id,'${currentId}'))`;
    }

    // Recupera tutte le sedi amministrative del tenant corrente
    let sites = await this.store.query('erp-site', {
      filter: `and(equals(tenantId,'${this.currentTenant}'),equals(administrativeHeadquarters,'true'))`,
    });

    // Imposta come non sede amministrtiva
    for (let site of sites.slice()) {
      site.set('administrativeHeadquarters', false);
      await site.save();
    }
  }

  // recupera in automatico le proprietà dal model e crea un oggetto vuoto con tutte le sue proprietà tracked
  initializeRecord(modelName) {
    let modelInstance = this.store.createRecord(modelName); // Crea una nuova istanza del modello (ad esempio, 'user')
    // Crea un semplice oggetto JavaScript con tutte le proprietà del modello
    let modelData = {};
    modelInstance.eachAttribute((key) => {
      modelData[key] = modelInstance.get(key);
    });

    // Crea un nuovo TrackedObject da modelData
    let record = new TrackedObject(modelData);
    return record;
  }
}
