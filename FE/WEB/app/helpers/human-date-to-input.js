/**
 * Prende una data in formato umano:
 * (es: Mon Jun 10 2024 02:00:00 GMT+0200 (Ora legale dell’Europa centrale))
 *
 * e la converte nel formato adatto a campi di input di tipo
 * "date" e "datetime-local"
 *
 * Esempio di utilizzo:
 * human-date-to-input "Mon Jun 10 2024 02:00:00 GMT+0200 (Ora legale dell’Europa centrale)" "date"
 */

import { helper } from '@ember/component/helper';

export default helper(function humanDateToInput([dateString, type = 'date']) {
  let date = new Date(dateString);

  // Ottieni le parti dell'oggetto Date necessarie per il formato YYYY-MM-DD
  let year = date.getUTCFullYear();
  let month = String(date.getUTCMonth() + 1).padStart(2, '0');
  let day = String(date.getUTCDate()).padStart(2, '0');

  // Formatta la data in base al tipo di input
  if (type === 'datetime-local') {
    let hours = String(date.getUTCHours()).padStart(2, '0');
    let minutes = String(date.getUTCMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  } else {
    return `${year}-${month}-${day}`;
  }
});
