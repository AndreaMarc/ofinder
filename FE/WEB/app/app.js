/* eslint-disable no-unused-vars */
import Application from '@ember/application';
import Resolver from 'ember-resolver';
import loadInitializers from 'ember-load-initializers';
import config from 'poc-nuovo-fwk/config/environment';
//import { schedule } from '@ember/runloop';

// Definisco gli alias globali delle seguenti librerie necessarie a Dev Extreme per
// esportare in excel e pdf.
import { saveAs } from 'file-saver';
import jsPDF from 'jspdf';
import ExcelJS from 'exceljs';
window.saveAs = saveAs;
window.ExcelJS = ExcelJS;
window.jsPDF = jsPDF;

export default class App extends Application {
  modulePrefix = config.modulePrefix;
  podModulePrefix = config.podModulePrefix;
  Resolver = Resolver;
  siteSetup = {};
  currentLang = 'it';

  constructor() {
    super(...arguments);
    // definisco le attività che devono essere eseguite prima dell'avvio dell'applicazione.
    // ATTENZIONE: eventuali errori non gestiti impediranno l'avvio dell'applicazione!
  }
}

loadInitializers(App, config.modulePrefix);
