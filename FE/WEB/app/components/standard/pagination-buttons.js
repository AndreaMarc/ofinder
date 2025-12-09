/**
 * @param {integer} currentPageNumber : pagina corrente. Tracked.
 * @param {integer} totalRecords : numero di record totali. Tracked.
 * @param {integer} recordsPerPage : numero di record visualizzati nella singola pagina. Tracked.
 * @param {integer} maxButtonsNumber : massimo numero di pulsanti da visualizzare (esclusi i pulsanti Inizio, Precedente, Successiva, Fine). Default 5.
 * @param {function} cb : callback da invocare per cambiare pagina. Le viene passato come argomento il numero della pagina richiesta.
 *
 * Esempio di utilizzo:
 * <Standard::PaginationButtons @currentPageNumber="" @totalRecords="" @recordsPerPage="" @maxButtonsNumber="3" @cb=""/>
 *
 */
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { action } from '@ember/object';

export default class StandardPaginationButtonsComponent extends Component {
  @tracked arrayPages = [1];
  @tracked currentPageNumber = 1;
  @tracked recordsPerPage = 1;
  @tracked totalRecords = 0;
  @tracked totalPagesNumber = 1;
  @tracked previousPageNumber = 1;
  @tracked nextPageNumber = 2;
  maxButtonsNumber = 5;
  cb = null;

  constructor(...args) {
    super(...args);

    if (this.args.cb && typeof this.args.cb === 'function') {
      this.cb = this.args.cb;
    }

    if (this.args.maxButtonsNumber) {
      this.maxButtonsNumber = parseInt(this.args.maxButtonsNumber);
    }

    this.start();
  }

  @action
  start() {
    if (this.args.totalRecords) {
      this.totalRecords = this.args.totalRecords;
    } else this.totalRecords = 0;

    if (
      typeof this.args.recordsPerPage !== 'undefined' &&
      !isNaN(this.args.recordsPerPage)
    ) {
      this.recordsPerPage = parseInt(this.args.recordsPerPage);
    } else this.recordsPerPage = this.totalRecords;

    if (this.args.currentPageNumber) {
      this.currentPageNumber = parseInt(this.args.currentPageNumber);
    } else this.currentPageNumber = 1;

    this.totalPagesNumber = this.specialRound(
      this.totalRecords / this.recordsPerPage
    );

    this.arrayPages = this.pageNumbers;
  }

  // crea i pulsanti di paginazione
  get pageNumbers() {
    const numbers = [];
    const start = Math.max(this.currentPageNumber - 2, 1);
    const end = Math.min(this.currentPageNumber + 2, this.totalPagesNumber);

    for (let i = start; i <= end; i++) {
      numbers.push(i);
    }

    // Aggiungere numeri mancanti all'inizio o alla fine
    while (numbers.length < this.maxButtonsNumber && numbers[0] > 1) {
      numbers.unshift(numbers[0] - 1);
    }

    while (
      numbers.length < this.maxButtonsNumber &&
      numbers[numbers.length - 1] < this.totalPagesNumber
    ) {
      numbers.push(numbers[numbers.length - 1] + 1);
    }

    return numbers;
  }

  @action
  changePageNumber(number) {
    this.currentPageNumber = number;
    if (number > 1) {
      this.previousPageNumber = number - 1;
    } else this.previousPageNumber = 1;
    if (number < this.totalPagesNumber) {
      this.nextPageNumber = number + 1;
    } else this.nextPageNumber = number;

    if (this.cb) {
      this.cb(number);
    }
  }

  // arrotonda all'intero superiore se il numero ha una qualsiasi parte decimale diversa da zero
  specialRound(number) {
    if (number % 1 === 0) {
      return number; // restituisce il numero se è già un intero
    } else {
      return Math.ceil(number); // restituisce l'intero superiore se il numero ha una parte decimale
    }
  }
}
