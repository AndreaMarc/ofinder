'use strict';

module.exports = function (environment) {
  const ENV = {
    modulePrefix: 'poc-nuovo-fwk',
    environment,
    rootURL: '/',
    locationType: 'history',
    EmberENV: {
      EXTEND_PROTOTYPES: false,
      FEATURES: {
        // Here you can enable experimental features on an ember canary build
        // e.g. EMBER_NATIVE_DECORATOR_SUPPORT: true
      },
    },

    cordova: {
      enabled: process.env.CORDOVA_ENV === 'true', // Controlla la variabile d'ambiente
    },

    APP: {
      // Here you can pass flags/options to your application instance
      // when it is created
    },

    'ember-simple-auth': {
      routeAfterAuthentication: '',
      store: 'session-store:local-storage',
    },

    torii: {
      allowUnsafeRedirects: true,
      providers: {
        'facebook-oauth2': {
          apiKey: '631252926924840',
        },
      },
    },

    cordovaIosAppId: '', // appId dell'app iOS.

    // API HOST
    apiHostDev: 'https://ofinder.it',
    //apiHostDev: 'http://localhost:7002',
    apiHostTest: 'https://maefwk6-test.maestrale.it', //
    apiHostProd: 'https://maefwk6-prod.maestrale.it',
    // FRONT-END HOST
    feHostDev: 'https://fwk-dev.maestrale.it/',
    feHostTest: 'https://fwk-test.maestrale.it/',
    feHostProd: 'https://fwk-prod.maestrale.it/',

    namespaceHost: 'api/v6',
  };

  if (environment === 'development') {
    ENV.apiHost = ENV.apiHostDev;
    ENV.feHost = ENV.feHostDev;
    // ENV.APP.LOG_RESOLVER = true;
    // ENV.APP.LOG_ACTIVE_GENERATION = true;
    // ENV.APP.LOG_TRANSITIONS = true;
    // ENV.APP.LOG_TRANSITIONS_INTERNAL = true;
    // ENV.APP.LOG_VIEW_LOOKUPS = true;
  }

  if (environment === 'test-publish') {
    ENV.apiHost = ENV.apiHostTest;
    ENV.feHost = ENV.feHostTest;
    // ENV.APP.LOG_RESOLVER = true;
    // ENV.APP.LOG_ACTIVE_GENERATION = true;
    // ENV.APP.LOG_TRANSITIONS = true;
    // ENV.APP.LOG_TRANSITIONS_INTERNAL = true;
    // ENV.APP.LOG_VIEW_LOOKUPS = true;
  }

  if (environment === 'test') {
    // Testem prefers this...
    ENV.apiHost = ENV.apiHostTest;
    ENV.feHost = ENV.feHostTest;
    ENV.locationType = 'none';

    // keep test console output quieter
    ENV.APP.LOG_ACTIVE_GENERATION = false;
    ENV.APP.LOG_VIEW_LOOKUPS = false;

    ENV.APP.rootElement = '#ember-testing';
    ENV.APP.autoboot = false;
  }

  if (environment === 'production') {
    // here you can enable a production-specific feature
    ENV.apiHost = ENV.apiHostProd;
    ENV.feHost = ENV.feHostProd;
  }

  // Importa la configurazione CSP
  const CSPConfig = require('./content-security-policy')(environment);
  ENV.contentSecurityPolicy = CSPConfig;

  return ENV;
};
