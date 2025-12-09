/**
 * COMPONENTE PER IL TITOLO DELLE PAGINE
 *
 * Esempio di utilizzo:
 *
 * <ArchitectUi::AppMain::AppPageTitle @title="Media" @titleKey="template.media.title"
 *    @description="Gestisci i media dell'applicativo" @descriptionKey="template.media.titleDesc" @icon="pe-7s-album icon-gradient bg-night-fade"/>
 *
 *
 * @param {string} @title  è il valore di default del titolo della pagina
 * @param {string} @titleKey  se usi le traduzioni, è la chiave che si riferisce alla traduzione del titolo della pagina, altrimenti stringa vuota
 * @param {string} @description è il valore di default del sotto-titolo
 * @param {string} @descriptionKey se usi le traduzioni, è la chiave che si riferisce alla traduzione del sotto-titolo, altrimenti stringa vuota
 * @param {string} @icon è il codice dell'icona da mostrare a sinistra del titolo. Stringa vuota per non mostrare l'icona.
 * @param {string} @iconBg è il codice per il background dell'icona. Se omesso o vuoto viene utilizzato il colore dell'header.
 */

import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { htmlSafe } from '@ember/template';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

export default class ArchitectUIAppMainAppPageTitleComponent extends Component {
  @service siteLayout;

  @tracked title = '';
  @tracked titleKey = '';
  @tracked description = '';
  @tracked descriptionKey = '';
  @tracked icon = '';
  showIcon = false;

  // costruttore
  constructor(...attributes) {
    super(...attributes);
    this.start();
  }

  @action
  start() {
    this.title = typeof this.args.title !== 'undefined' ? this.args.title : '';
    this.titleKey =
      typeof this.args.titleKey !== 'undefined' ? this.args.titleKey : '';
    this.description =
      typeof this.args.description !== 'undefined' ? this.args.description : '';
    this.descriptionKey =
      typeof this.args.descriptionKey !== 'undefined'
        ? this.args.descriptionKey
        : '';

    this.icon = '';
    if (typeof this.args.icon !== 'undefined' && this.args.icon !== '') {
      this.icon = this.args.icon;
      this.showIcon = true;
    }
  }

  /*get iconStyle() {
    let icon = '';
    if (this.args.iconBg && this.args.iconBg !== '') {
      icon = `${this.args.icon} icon-gradient ${this.args.iconBg}`;
    } else {
      icon = `${this.args.icon} icon-gradient ${this.getBgColor}`;
    }
    return htmlSafe(icon);
  }*/

  get getBgColor() {
    let iconBg = '';
    if (this.args.iconBg && this.args.iconBg !== '') {
      iconBg = `${this.args.icon} icon-gradient ${this.args.iconBg}`;
    } else {
      iconBg = `${this.args.icon} icon-gradient ${this.siteLayout.headerBackground}`;
    }
    return htmlSafe(iconBg);
  }
}
