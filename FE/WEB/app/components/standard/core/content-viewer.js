/**
 * Mostra gli articoli del sito, nella lingua dell'utente.
 *
 *
 * @param {string} @guid Guid del template da mostrare
 * @param {function} @cb Opzionale. Callback da invocare quando l'utente cambia template premendo
 *                        i pulsanti di navigazione precedente/successivo
 */
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { htmlSafe } from '@ember/template';
import { action } from '@ember/object';

export default class StandardCoreContentViewerComponent extends Component {
  @service store;

  guid = '';
  cb = null;
  @tracked available = 'waiting';
  @tracked error = '';
  @tracked content = '';
  @tracked prev = null;
  @tracked next = null;

  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  async start() {
    this.available = 'waiting';
    this.guid = this.args.guid ? this.args.guid : '';

    this.cb =
      typeof this.args.cb !== 'undefined' && typeof this.args.cb === 'function'
        ? this.args.cb
        : null;

    if (!this.guid || this.guid === '') {
      this.available = 'unavailable';
      this.error = htmlSafe(
        `Configurazione errata. Contattare uno sviluppatore!`
      );
    } else {
      this.load();
    }
  }

  async load() {
    try {
      this.prev = null;
      this.next = null;

      let article = await this.store.findRecord('template', this.guid);
      this.content = htmlSafe(article.content);
      this.available = 'available';

      // ora verifico se esistono un template precedente ed uno successivo
      let others = await this.store.query('template', {
        filter: `and(equals(categoryId,'${article.categoryId}'),equals(erased,'false'))`,
        sort: 'order',
      });

      if (others.length > 1) {
        const ret = this.findSurroundingItemsById(article.id, others);
        this.prev = ret.previous;
        this.next = ret.next;
      }
    } catch (e) {
      console.error(e);
      this.available = 'unavailable';
      this.error = htmlSafe(`Errore nel recupero dell'articolo.`);
    }
  }

  findSurroundingItemsById(id, items) {
    const currentItem = items.find((item) => item.id === id);

    if (!currentItem) return { previous: null, next: null };

    let previous = null;
    let next = null;

    for (const item of items) {
      if (
        item.order < currentItem.order &&
        (!previous || item.order > previous.order)
      ) {
        previous = item;
      }

      if (
        item.order > currentItem.order &&
        (!next || item.order < next.order)
      ) {
        next = item;
      }
    }

    return { previous: previous, next: next };
  }

  @action
  changeGuid(id, name) {
    if (this.cb) this.cb(id, name);
  }
}
