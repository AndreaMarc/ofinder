/* eslint-disable prettier/prettier */
/* eslint-disable ember/no-jquery */
import sidebarContents from 'poc-nuovo-fwk/_customs/sidebarContents';
import ENV from 'poc-nuovo-fwk/config/environment';
import Component from '@glimmer/component';
import { inject as service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';
import { htmlSafe } from '@ember/template';
import $ from 'jquery';

export default class AppSidebarComponent extends Component {
  @service session;
  @service router;
  @service siteLayout;

  @tracked styleSidebarClass = '';
  @tracked menuItems = '';
  //@tracked currentRoute = null;

  constructor(...attributes) {
    super(...attributes);
    this.styleSidebarClass = this.styleSidebar;

    // Registra un listener per l'evento routeDidChange
    this.router.on('routeDidChange', this, this.routeDidChange);
  }

  // costruisce le classi di stile per la sidebar
  get styleSidebar(){
    let layoutStyle = this.siteLayout;
    let styleSidebarClass = '';
    if (this.session.isAuthenticated) {
      styleSidebarClass = layoutStyle.sidebarShadow ? 'sidebar-shadow ' : '';
      if(layoutStyle.sidebarLight === 'white'){
        styleSidebarClass += 'sidebar-text-light ';
      } else if(layoutStyle.sidebarLight === 'black'){
        styleSidebarClass += 'sidebar-text-dark ';
      }
      if (layoutStyle.sidebarBackground) styleSidebarClass += layoutStyle.sidebarBackground;
    }
    return styleSidebarClass;
  }

  // chiamata quando il componente è effettivamente disponibile nel DOM,
  // inietta le voci del menù laterale
  @action
  start() {
    this.menuItems = '';
    this.routeDidChange(); // Esegui l'azione quando il componente viene inserito nel DOM

    if (
      sidebarContents &&
      sidebarContents.menuJson &&
      sidebarContents.menuJson.length > 0
    ) {

      let permissions =
        typeof this.session.get('data.permissions') !== 'undefined'
          ? this.session.get('data.permissions')
          : [];

      let menuItems = '';
      sidebarContents.menuJson.forEach((element, index) => {
        // verifico se l'utente ha gli eventuali permessi richiesti
        let found = false;
        if (element.permissions && element.permissions.length > 0 && permissions.length > 0) {
          found = element.permissions.some((r) => permissions.includes(r));
        }

        let isCordova = typeof window.cordova !== 'undefined';
        // verifico se l'elemento può essere mostrato sull'app e sul web

        if((isCordova && element.devices.includes('app')) || (!isCordova && element.devices.includes('web'))) {

          // verifico se l'elemento è da mostrare nell'ambiente corrente
          if (element.environments.includes(ENV.environment)) {

            if (typeof element.permissions === 'undefined' || element.permissions.length === 0 || found) {
              // l'elemento può essere mostrato all'utente corrente

              if (element.divider) {
                // elemento divisore
                menuItems += `<li class="app-sidebar__heading">
                    ${element.icon && element.icon !== '' ? '<i class="' + element.icon + '"></i>' : ''}
                    ${element.label ? element.label : ''}
                  </li>`;

              } else {
                // elemento standard (voce di menù)
                menuItems += `<li class="maeItemSxSidebar" data-index="${index}">
                      <a href="#" data-url="${element.link && element.link !== '#' ? element.link : ''}" class="sidebarChangePage">
                        ${element.icon && element.icon !== '' ? '<i class="' + element.icon + '"></i>' : ''}
                        ${element.label ? element.label : ''}
                        ${element.childrens && element.childrens.length > 0 ? '<i class="metismenu-state-icon pe-7s-angle-down caret-left"></i>' : ''}
                      </a>`;

                if (element.childrens && element.childrens.length > 0) {
                  // voce principale con figli
                  menuItems += `<ul class="maeSubContSxSidebar" data-index="${index}">`;

                  element.childrens.forEach((subItem) => {

                    // verifico se l'utente ha gli eventuali permessi richiesti
                    let subFound = false;
                    if (subItem.permissions && subItem.permissions.length > 0 && permissions.length > 0) {
                      subFound = subItem.permissions.some((s) => permissions.includes(s));
                    }

                    // verifico se l'elemento è da mostrare nell'ambiente corrente
                    if (subItem.environments.includes(ENV.environment)){
                      if (typeof subItem.permissions === 'undefined' || subItem.permissions.length === 0 || subFound) {
                        menuItems += `<li class="maeSubItemSxSidebar">
                                              <a href="#" data-url="${subItem.link ? subItem.link : ''}" data-index="${index}" class="sidebarChangePage">
                                                  <i ${subItem.icon && subItem.icon !== '' ? 'class="' + subItem.icon + '"' : ''}></i>
                                                  ${subItem.label && subItem.label !== '' ? subItem.label : ''}
                                              </a>
                                          </li>`;
                      }
                    }

                  });
                  menuItems += `</ul>`;
                }

                menuItems += `</li>`;
                index++;
              }
            }
          }
        }

      });
      this.menuItems = htmlSafe(menuItems);

      setTimeout(() => {
        // Sidebar Menu
        $('.vertical-nav-menu').metisMenu();
      }, 40);

      // listener
      let self = this;
      $(document).off('click tap', '.sidebarChangePage');
      $(document).on('click tap', '.sidebarChangePage', function(e) {
        e.preventDefault();
        let $this = $(this);
        let url = $this.attr('data-url');
        if(url === '#' || url === '') return false;

        var win = document.body.clientWidth;
        if (typeof window.cordova !== 'undefined' && win < 1250) {
          $('.hamburger', this).toggleClass('is-active');
          $('.app-inner-layout').toggleClass('open-mobile-menu');

          $('.mobile-toggle-nav').removeClass('is-active');
          $('.app-container').removeClass('sidebar-mobile-open');
          // Chiudo l'eventuale popup dell'header
          $('.app-header__content').removeClass('header-mobile-open');
        }

        self.router.transitionTo(url);
      });
    }
  }

  @action
  willDestroy() {
    super.willDestroy(...arguments);

    // Rimuovi il listener quando il componente viene distrutto
    this.router.off('routeDidChange', this, this.routeDidChange);
    $(document).off('click tap', '.sidebarChangePage');
  }

  routeDidChange() {
    //utilizzo una variabile di appoggio per non mandare in errore il test del componente.
    let thisCurrentURL = this.router.currentURL;

    setTimeout(() => {
      // verifico se una voce secondaria è attiva
      let currentUrl = thisCurrentURL;

      $('.maeSubContSxSidebar').removeClass('mm-show');
      $('.maeItemSxSidebar, .sidebarChangePage').removeClass('mm-active');

      let $selected = $(`li.maeSubItemSxSidebar a[data-url="${currentUrl}"]`);

      if ($selected.length > 0) {
        let index = parseInt($selected.attr('data-index'));
        $selected.addClass('mm-active');
        $(`ul.maeSubContSxSidebar[data-index="${index}"]`).addClass('mm-show');
        $(`li.maeItemSxSidebar[data-index="${index}"]`).addClass('mm-active');
      } else {
        // verifico se una voce primaria è attiva
        $selected = $(`li.maeItemSxSidebar a[data-url="${currentUrl}"]`);
        if($selected.length > 0) {
          $selected.addClass('mm-active');
        }
      }
      $('.vertical-nav-menu').metisMenu();
    }, 20);
  }

  @action
  startupScrollbar() {
    // eslint-disable-next-line no-undef, no-unused-vars
    const ps = new PerfectScrollbar('.scrollbar-sidebar', {
      wheelSpeed: 2,
      wheelPropagation: true,
      minScrollbarLength: 20,
    });
  }
}
