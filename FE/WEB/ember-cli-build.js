'use strict';

const EmberApp = require('ember-cli/lib/broccoli/ember-app');

module.exports = function (defaults) {
  const app = new EmberApp(defaults, {
    // Add options here
    sassOptions: {
      includePaths: ['node_modules/bootstrap/scss'],
    },
    minifyCSS: {
      enabled: true,
    },

    'ember-simple-auth': {
      useSessionSetupMethod: true,
    },

    'ember-prism': {
      theme: 'okaidia',
      // for details: https://prismjs-com.translate.goog/?_x_tr_sl=it&_x_tr_tl=en&_x_tr_hl=it&_x_tr_pto=wapp#supported-languages
      components: ['css', 'scss', 'javascript', 'markup'], //needs to be an array, or undefined.
      plugins: [
        'line-numbers',
        'line-highlight',
        'toolbar',
        'copy-to-clipboard',
        'show-language',
      ],
    },

    autoImport: {
      alias: {
        fullcalendar: '@fullcalendar/core',
      },
      webpack: {
        node: {
          global: true,
        },
        // Ensure simple-icons ES module is properly resolved
        resolve: {
          fullySpecified: false,
        },
      },
    },
  });

  // Use `app.import` to add additional libraries to the generated
  // output files.
  //
  // If you need to use different assets in different
  // environments, specify an object as the first parameter. That
  // object's keys should be the environment name and the values
  // should be the asset to use in that environment.
  //
  // If the library that you are including contains AMD or ES6
  // modules that you would like to import into your application
  // please specify an object with the list of modules as keys
  // along with the exports of each module as its value.

  // bootstrap
  app.import('node_modules/popper.js/dist/umd/popper.js');
  app.import('node_modules/popper.js/dist/umd/popper-utils.js');
  app.import('node_modules/bootstrap/dist/js/bootstrap.js');

  // momentjs
  //app.import('node_modules/moment/min/moment-with-locales.min.js');
  /*app.import('node_modules/moment/moment.js');
  app.import('node_modules/timezone-js/src/date.js');
  app.import('node_modules/timezone-js/src/tz.js');
  app.import('node_modules/ember-cli-moment-shim/vendor/moment/moment.js');
  app.import('node_modules/ember-cli-moment-shim/vendor/timezone-js/date.js');
  app.import('node_modules/ember-cli-moment-shim/vendor/timezone-js/tz.js');
  */

  // alertify
  app.import('node_modules/alertifyjs/build/css/alertify.min.css');
  app.import('node_modules/alertifyjs/build/css/themes/bootstrap.min.css');
  app.import('node_modules/alertifyjs/build/alertify.min.js');

  // jszip, FileSaver, exceljs, jspdf
  // necessarie a DevExtrem per gli export excl e pdf.
  app.import('node_modules/jszip/dist/jszip.min.js');
  app.import('node_modules/file-saver/dist/FileSaver.min.js');
  app.import('node_modules/exceljs/dist/exceljs.min.js');
  app.import('node_modules/jspdf/dist/jspdf.umd.min.js');
  // a sè stante
  app.import('node_modules/html2pdf.js/dist/html2pdf.bundle.min.js'); // Importa html2pdf.js

  // dev-express
  app.import('node_modules/devextreme-quill/dist/dx-quill.min.js');
  app.import('vendor/Lib/css/dx.light.css');
  app.import('vendor/Lib/js/dx.all.js');
  app.import('vendor/Lib/css/icons/dxicons.woff2', {
    destDir: 'assets/icons',
  });
  app.import('vendor/Lib/css/icons/dxicons.woff', {
    destDir: 'assets/icons',
  });
  app.import('vendor/Lib/css/icons/dxicons.ttf', {
    destDir: 'assets/icons',
  });

  /**
   * COMPONENTI TEMA ARCHITECT
   */
  // metismenu
  app.import('node_modules/metismenu/dist/metisMenu.min.css');
  app.import('node_modules/metismenu/dist/metisMenu.min.js');
  // perfectScrollbar
  app.import('node_modules/perfect-scrollbar/dist/perfect-scrollbar.min.js');
  // animate.css
  app.import('node_modules/animate.compact.css');
  // popperjs
  app.import('node_modules/popper.js/dist/umd/popper.js');
  app.import('node_modules/popper.js/dist/umd/popper-utils.js');
  // slick-carousel
  app.import('node_modules/slick-carousel/slick/slick.min.js');
  // jquery-circle-progress
  app.import('node_modules/jquery-circle-progress/dist/circle-progress.min.js');
  // intro.js
  app.import('node_modules/intro.js/minified/introjs.min.css');
  app.import('node_modules/intro.js/minified/intro.min.js');
  // sweetalert2
  app.import('node_modules/sweetalert2/dist/sweetalert2.min.css');
  app.import('node_modules/sweetalert2/dist/sweetalert2.min.js');
  // toastr
  app.import('node_modules/toastr/build/toastr.min.css');
  app.import('node_modules/toastr/build/toastr.min.js');
  // jquery.fancytree
  app.import('node_modules/jquery.fancytree/dist/skin-lion/ui.fancytree.css');
  app.import(
    'node_modules/jquery.fancytree/dist/jquery.fancytree-all-deps.min.js'
  );
  app.import(
    'node_modules/jquery.fancytree/dist/modules/jquery.fancytree.filter.js'
  );
  app.import(
    'node_modules/jquery.fancytree/dist/modules/jquery.fancytree.table.js'
  );
  app.import(
    'node_modules/jquery.fancytree/dist/modules/jquery.fancytree.grid.js'
  );
  app.import(
    'node_modules/jquery.fancytree/dist/modules/jquery.fancytree.glyph.js'
  );
  app.import(
    'node_modules/jquery.fancytree/dist/modules/jquery.fancytree.edit.js'
  );
  app.import(
    'node_modules/jquery.fancytree/dist/modules/jquery.fancytree.childcounter.js'
  );
  app.import(
    'node_modules/jquery.fancytree/dist/modules/jquery.fancytree.columnview.js'
  );
  // clipboard
  app.import('node_modules/clipboard/dist/clipboard.min.js');
  // @chenfengyuan/datepicker
  app.import('node_modules/@chenfengyuan/datepicker/dist/datepicker.min.js'); // Carica prima il JS principale
  app.import('node_modules/@chenfengyuan/datepicker/i18n/datepicker.it-IT.js'); // Poi carica la lingua
  app.import('node_modules/@chenfengyuan/datepicker/dist/datepicker.min.css'); // Infine, carica il CSS
  // daterangepicker
  app.import('node_modules/daterangepicker/daterangepicker.css');
  app.import('node_modules/daterangepicker/daterangepicker.js');
  // jquery-validation
  app.import('node_modules/jquery-validation/dist/jquery.validate.min.js');
  // smartwizard
  app.import('node_modules/smartwizard/dist/css/smart_wizard_all.min.css');
  app.import('node_modules/smartwizard/dist/js/jquery.smartWizard.min.js');
  // @atomaras
  app.import(
    'node_modules/bootstrap-multiselect/dist/css/bootstrap-multiselect.css'
  );
  app.import(
    'node_modules/bootstrap-multiselect/dist/js/bootstrap-multiselect.js'
  );
  // select2
  app.import('node_modules/select2/dist/css/select2.css');
  app.import('node_modules/select2/dist/js/select2.min.js');
  app.import(
    'node_modules/@ttskch/select2-bootstrap4-theme/dist/select2-bootstrap4.min.css'
  );
  // nouislider
  app.import('node_modules/nouislider/dist/nouislider.min.css');
  app.import('node_modules/nouislider/dist/nouislider.min.js');
  // wnumb
  app.import('node_modules/wnumb/wNumb.min.js');
  // autosize
  app.import('node_modules/autosize/dist/autosize.min.js');
  // apexcharts
  app.import('node_modules/apexcharts/dist/apexcharts.css');
  app.import('node_modules/apexcharts/dist/apexcharts.min.js');
  // chart.js
  app.import('node_modules/chart.js/dist/chart.umd.js');
  // jquery-sparkline
  app.import('node_modules/jquery-sparkline/jquery.sparkline.min.js');
  // block-ui
  app.import('node_modules/block-ui/jquery.blockUI.js');
  // bootstrap4-toggle
  app.import('node_modules/bootstrap4-toggle/css/bootstrap4-toggle.min.css');
  app.import('node_modules/bootstrap4-toggle/js/bootstrap4-toggle.min.js');
  // cripto-js
  app.import('node_modules/crypto-js/crypto-js.js');

  // ckeditor5
  app.import('vendor/ckeditor5-38.1.0-d5emhsqgf60y/build/ckeditor.js');

  // DOMPurify
  app.import('node_modules/dompurify/dist/purify.min.js');

  return app.toTree();
};
