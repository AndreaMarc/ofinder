/* eslint-disable ember/no-jquery */
import $ from 'jquery';

export function initialize() {
  // application
  setTimeout(() => {
    console.log('inizializzazione del tema!');

    // pulsante di cancellazione dei campi input
    $(document).on('click tap', '.clearButton', function () {
      let ref = $(this).attr('data-ref');
      $(ref).val('');
    });

    // Search wrapper trigger
    $(document).on('click', '.search-icon', function () {
      $(this).parent().parent().addClass('active');
    });

    $(document).on('click', '.search-wrapper .close', function () {
      $(this).parent().removeClass('active');
    });

    //
    setTimeout(() => {
      // BS4 Popover
      $('[data-toggle="popover-custom-content"]').each(function () {
        // i, obj
        $(this).popover({
          html: true,
          placement: 'auto',
          template: `<div class="popover popover-custom" role="tooltip">
              <div class="arrow">
            </div>
              <h3 class="popover-header"></h3>
              <div class="popover-body"></div>
            </div>`,
          content: () => {
            var id = $(this).attr('popover-id');
            return $('#popover-content-' + id).html();
          },
        });
      });

      $('[data-toggle="popover-custom-bg"]').each(function () {
        // i, obj
        var popClass = $(this).attr('data-bg-class');

        $(this).popover({
          trigger: 'focus',
          placement: 'top',
          template:
            '<div class="popover popover-bg ' +
            popClass +
            '" role="tooltip"><h3 class="popover-header"></h3><div class="popover-body"></div></div>',
        });
      });

      $('[data-toggle="popover"]').popover();

      $('[data-toggle="popover-custom"]').each(function () {
        // i, obj
        console.log('clicked');
        $(this).popover({
          html: true,
          container: $(this).parent().find('.rm-max-width'),
          content: function () {
            return $(this)
              .next('.rm-max-width')
              .find('.popover-custom-content')
              .html();
          },
        });
      });
    }, 100);

    $('body').on('click', function (e) {
      $('[rel="popover-focus"]').each(function () {
        if (
          !$(this).is(e.target) &&
          $(this).has(e.target).length === 0 &&
          $('.popover').has(e.target).length === 0
        ) {
          $(this).popover('hide');
        }
      });
    });

    $(document).on(
      'click',
      '.header-megamenu.nav > li > .nav-link',
      function (e) {
        $('[data-toggle="popover-custom"]').each(function () {
          if (
            !$(this).is(e.target) &&
            $(this).has(e.target).length === 0 &&
            $('.popover').has(e.target).length === 0
          ) {
            $(this).popover('hide');
          }
        });
      }
    );

    // Apertura menù laterale destro
    $(document).on('click', '.open-right-drawer', function () {
      $(this).addClass('is-active');
      $('.app-drawer-wrapper').addClass('drawer-open');
      $('.app-drawer-overlay').removeClass('d-none');
    });

    // Chiusura menù laterale destro
    $(document).on('click', '.drawer-nav-btn', function () {
      $('.app-drawer-wrapper').removeClass('drawer-open');
      $('.app-drawer-overlay').addClass('d-none');
      $('.open-right-drawer').removeClass('is-active');
    });

    // Chiusura menù laterale destro al click sulla pagina
    $(document).on('click', '.app-drawer-overlay', function () {
      $(this).addClass('d-none');
      $('.app-drawer-wrapper').removeClass('drawer-open');
      $('.open-right-drawer').removeClass('is-active');
    });

    // Apertura/Chiusura menù laterale sinistro su Mobile
    $(document).on('click', '.mobile-toggle-nav', function () {
      $(this).toggleClass('is-active');
      $('.app-container').toggleClass('sidebar-mobile-open');
    });

    // Apertura/Chiusura del popup "Profilo Utente" su Mobile
    $(document).on('click', '.mobile-toggle-header-nav', function () {
      $(this).toggleClass('active');
      $('.app-header__content').toggleClass('header-mobile-open');
    });

    // Chiusura menù laterale sinistro e del popup "Profilo Utente" su Mobile al click sulla pagina
    $(document).on('click', '.app-main__inner', function () {
      $('.mobile-toggle-nav').removeClass('is-active');
      $('.app-container').removeClass('sidebar-mobile-open');

      $('.mobile-toggle-header-nav').removeClass('active');
      $('.app-header__content').removeClass('header-mobile-open');
    });

    $(document).on('click', '.mobile-app-menu-btn', function () {
      $('.hamburger', this).toggleClass('is-active');
      $('.app-inner-layout').toggleClass('open-mobile-menu');
    });

    // Responsive
    var resizeClass = function () {
      var win = document.body.clientWidth;
      if (win < 1250) {
        $('.app-container').addClass('closed-sidebar-mobile closed-sidebar');
      } else {
        $('.app-container').removeClass('closed-sidebar-mobile closed-sidebar');
      }
    };

    $(window).on('resize', function () {
      resizeClass();
    });

    resizeClass();

    // FROM DEMO.JS

    $(document).on('click', '.btn-open-options', function () {
      $('.ui-theme-settings').toggleClass('settings-open');
    });

    // Toggle menù laterale sinistro su Desktop
    $(document).on('click', '.close-sidebar-btn', function () {
      var classToSwitch = $(this).attr('data-class');
      var containerElement = '.app-container';
      $(containerElement).toggleClass(classToSwitch);

      var closeBtn = $(this);

      if (closeBtn.hasClass('is-active')) {
        closeBtn.removeClass('is-active');
      } else {
        closeBtn.addClass('is-active');
      }
    });

    /*
    $(document).on('click', '.switch-container-class', function () {
      var classToSwitch = $(this).attr('data-class');
      var containerElement = '.app-container';
      $(containerElement).toggleClass(classToSwitch);

      $(this).parent().find('.switch-container-class').removeClass('active');
      $(this).addClass('active');
    });
    */

    $(document).on('click', '.switch-theme-class', function () {
      var classToSwitch = $(this).attr('data-class');
      var containerElement = '.app-container';

      if (classToSwitch == 'app-theme-white') {
        $(containerElement).removeClass('app-theme-gray');
        $(containerElement).addClass(classToSwitch);
      }

      if (classToSwitch == 'app-theme-gray') {
        $(containerElement).removeClass('app-theme-white');
        $(containerElement).addClass(classToSwitch);
      }

      if (classToSwitch == 'body-tabs-line') {
        $(containerElement).removeClass('body-tabs-shadow');
        $(containerElement).addClass(classToSwitch);
      }

      if (classToSwitch == 'body-tabs-shadow') {
        $(containerElement).removeClass('body-tabs-line');
        $(containerElement).addClass(classToSwitch);
      }

      $(this).parent().find('.switch-theme-class').removeClass('active');
      $(this).addClass('active');
    });

    // COMPONENTI

    // scrollbar.js
    setTimeout(function () {
      try {
        if ($('.scrollbar-container')[0]) {
          $('.scrollbar-container').each(function () {
            // eslint-disable-next-line no-undef, no-unused-vars
            const ps = new PerfectScrollbar($(this)[0], {
              wheelSpeed: 2,
              wheelPropagation: false,
              minScrollbarLength: 20,
            });
          });

          // eslint-disable-next-line no-undef, no-unused-vars
          /*
          const ps = new PerfectScrollbar('.scrollbar-sidebar', {
            wheelSpeed: 2,
            wheelPropagation: true,
            minScrollbarLength: 20,
          });
          */
        }
        // eslint-disable-next-line no-empty
      } catch (e) {}
    }, 1000);
  }, 100);
}

export default {
  initialize,
};
