/**
 * Converte una stringa data ISO nel formato data-ora Locale desiderato
 * (compreso fuso orario e traduzione).
 *
 * @param {string} value : data. es: '2023-12-22T03:31:45.867'
 * @param {object/null} format : null per il formato di default previsto da questo helper. Oppure: formato personalizzato come definito qui sotto.
 *                                Nota: è accettato anche il formato stringa, che viene parsato in oggetto.
 * @param {string} currentLang : lingua desiderata
 *
 *
 * Struttura dell'oggetto format:
 * { day: '2-digit', month: '2-digit', year: numeric, hour: '2-digit', minute: '2-digit', second: '2-digit' }
 *
 *
  Option          Values          Sample output
  ----------------------------------------------------
  weekday         'narrow'        'M'
                  'short'         'Mon'
                  'long'          'Monday'

  year            '2-digit'       '01'
                  'numeric'       '2001'

  month           '2-digit'       '01'
                  'numeric'       '1'
                  'narrow'        'J'
                  'short'         'Jan'
                  'long'          'January'

  day             '2-digit'       '01'
                  'numeric'       '1'

  hour            '2-digit'       '12 AM'
                  'numeric'       '12 AM'

  minute          '2-digit'       '0'
                  'numeric'       '0'

  second          '2-digit'       '0'
                  'numeric'       '0'

  timeZoneName    'short'         '1/1/2001 GMT+00:00'
                  'long'          '1/1/2001 GMT+00:00'



 * Esempio di utilizzo in file .hbs:
 * (convertdate2 value {year: 'numeric'} 'it')
 *
*/
import { helper } from '@ember/component/helper';

export default helper(function convertdate2(params /*, named*/) {
  try {
    let [value, format, currentLang] = params;

    if (!value) return '';
    if (!currentLang) currentLang = 'it';

    if (!format || typeof format === 'undefined' || format === '') {
      format = {
        day: '2-digit',
        month: 'long',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      };
    }

    if (typeof format === 'string') format = JSON.parse(format);

    let d = new Date(value);
    return d.toLocaleDateString(currentLang, format);
  } catch (e) {
    console.error(e);
    return '';
  }
});
