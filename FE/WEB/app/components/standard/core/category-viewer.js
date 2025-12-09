/**
 * Mostra gli articoli della Categoria indicata.
 * Se la Categoria contiene sottocategorie, può mostrare anche le sotto-categorie con struttura accordion.
 *
 * @param {string} categoryCode       Obbligatorio. E' il campo Code della categoria padre
 * @param {string} showSubCategory    stringa vuota per non mostrare le sotto-categorie. "true" altrimenti.
 * @param {string} showFirstCategory  se showSubCategory è true, indica se l'accordion deve mostrare anche la
 *                                    categoria padre e i suoi template. Stringa vuota esclude la categoria
 *                                    padre. "true" altrimenti.
 * @param {string} seePreview se seePreview è "true", indica che all'interno dell'accordion non viene mostrato contentViewer ma direttamente il contenuto del template con immagine allegata.
 * @param {string} enableSearch "true" per abilitare la barra di ricerca.
 * @param {string} tenantId Se i template devono essere prelevati da uno specifico tenant, inserirne qui l'id. Altrimenti lasciare vuoto per
 *                          prelevare i template dal tenant corrente dell'utente.
 *
 * NOTA: per ora lavora solo su sotto-categorie di primo livello.
 * TODO : aggiungere ricorsività per lavorare a più livelli di sotto-categorie
 *
 * Esempio di utilizzo:
 * <Standard::Core::CategoryViewer @categoryCode="maeProduct" @showSubCategory="true" @enableSearch="true" @seePreview="true" @tenantId="1" />
 */

import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { htmlSafe } from '@ember/template';
import { task } from 'ember-concurrency';
import { action } from '@ember/object';
import { v4 } from 'ember-uuid';

export default class StandardCoreCategoryViewerComponent extends Component {
  @service translation;
  @service session;
  @service store;

  categoryCode = '';
  showSubCategory = false;
  showFirstCategory = false;
  enableSearch = false;
  seePreview = false;
  defaultTenant = null;
  @tracked filter = '';

  @tracked accordionId = '';
  @tracked available = 'waiting';
  @tracked error = '';

  @tracked categories = [];
  @tracked step = 1;

  @tracked templateTitle = '';
  @tracked templateId = '';

  constructor(...attributes) {
    super(...attributes);

    if (this.args.tenantId && this.args.tenantId !== '') {
      this.defaultTenant = this.args.tenantId;
    }

    this.accordionId = v4();
    this.categoryCode = this.args.categoryCode ? this.args.categoryCode : '';
    this.showSubCategory = this.args.showSubCategory
      ? !!this.args.showSubCategory
      : false;
    this.showFirstCategory = this.args.showFirstCategory
      ? !!this.args.showFirstCategory
      : false;

    this.enableSearch = this.args.enableSearch
      ? !!this.args.enableSearch
      : false;

    this.seePreview = this.args.seePreview ? !!this.args.seePreview : false;
    this.start.perform();
  }

  start = task({ restartable: true }, async () => {
    this.categories = [];
    let cats = [];
    // console.warn('TEST');
    try {
      if (this.categoryCode === '') {
        this.available = 'unavailable';
        this.error = htmlSafe(
          'Errore di configurazione. Contattare uno sviluppatore'
        );
        throw new Error();
      }

      // verifico che la categoria principale esista, che sia attiva e ne ricavo i template associati
      let tenantId = this.session.get('data.tenantId');
      if (this.defaultTenant) {
        tenantId = this.defaultTenant;
      }
      let catId = 0;

      let templates = [];

      if (this.filter !== '') {
        templates = await this.store.query('template', {
          filter: `and(or(contains(content,'${this.filter}'),contains(name,'${this.filter}')),equals(language,'${this.translation.currentLang}'),equals(active,'true'))`,
        });
      } else {
        templates = await this.store.query('template', {
          filter: `and(equals(language,'${this.translation.currentLang}'),equals(active,'true'))`,
        });
      }

      let categoryIds = [...new Set(templates.map((x) => `'${x.categoryId}'`))];
      let templateIds = [...new Set(templates.map((x) => `'${x.id}'`))];

      try {
        let c = await this.store.queryRecord('category', {
          filter: `and(equals(code,'${this.categoryCode}'),equals(tenantId,'${tenantId}'))`,
          include: `template`,
        });

        catId = c.id;
        if (this.showFirstCategory) cats = [c];
      } catch (e) {
        console.error(e);
        this.available = 'unavailable';
        this.error = htmlSafe(
          'Errore di configurazione. Contattare uno sviluppatore'
        );
        throw new Error();
      }

      // se richiesto, estraggo le sotto-categorie
      if (this.showSubCategory && categoryIds.length !== 0) {
        try {
          let res = await this.store.query('category', {
            filter: `and(any(parentCategory,'${catId}'),any(id,${categoryIds}))`,
            include: `template`,
            sort: `name`,
          });

          if (res && res.length > 0) {
            cats = [...cats, ...res];
          }
        } catch (e) {
          console.error('Impossibile estrarre le sotto-categorie: ', e);
        }
      }

      templateIds = [...new Set(templates.map((x) => `${x.id}`))];

      if (cats && cats.length > 0) {
        cats.forEach(async (category) => {
          let templates = await category.get('template'); // await category.template;
          let temps = [];
          templates.forEach((x) => {
            if (templateIds.includes(x.id)) {
              temps.push(x);
            }
          });
          category.set('template', temps);
        });
      }

      if (cats && cats.length > 0) this.categories = cats;
      this.available = 'available';
    } catch (e) {
      console.error(e);
    }
  });

  htmlText(text) {
    return htmlSafe(text);
  }

  get sortedCategories() {
    let sc = this.categories;

    if (sc && sc.length > 0) {
      sc.forEach(async (category) => {
        let templates = await category.template;

        if (templates && templates.length > 0) {
          templates = templates.filter((item) => item.erased === false);
          // ordinamento alfabetico dei template
          let sortedTemplates = templates.slice().sort((a, b) => {
            if (a.order < b.order) return -1;
            if (a.order > b.order) return 1;
            return 0;
          });
          category.set('template', sortedTemplates);
        }
      });
    }
    return sc;
  }

  get filteredCategories() {
    let sc = this.sortedCategories;
    if (this.filter !== '') {
      if (this.seePreview) {
        if (sc && sc.length > 0) {
          sc.forEach(async (category) => {
            let templates = await category.get('template'); // await category.template;

            let temp = [];
            templates.forEach((template) => {
              if (
                template.content
                  .toLowerCase()
                  .includes(this.filter.toLowerCase()) ||
                template.name.toLowerCase().includes(this.filter.toLowerCase())
              ) {
                temp.push(template);
              }
            });

            category.set('template', temp);
          });
        }
      } else {
        if (sc && sc.length > 0) {
          sc = sc.filter((item) =>
            item.name.toLowerCase().includes(this.filter.toLowerCase())
          );
        }
      }
    }

    return sc;
  }

  @action
  openTemplate(id, name) {
    this.templateId = id;
    this.templateTitle = htmlSafe(name);
    this.step = 2;
  }

  @action
  closeTemplate() {
    this.templateTitle = '';
    this.step = 1;
  }

  // helper locale
  templateLength(templates) {
    return templates ? templates.length : 0;
  }

  @action
  changeFilter(event) {
    this.filter = event.target.value.trim();
    this.start.perform();
  }
}
