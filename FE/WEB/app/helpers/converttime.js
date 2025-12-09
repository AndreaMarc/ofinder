/**
 * Converte una stringa data ISO nel formato orario Locale desiderato
 * (compreso fuso orario).
 *
 * @param {string} value : data. es: '2023-12-22T03:31:45.867'
 * @param {string} format : null per il formato di default previsto da questo helper. Oppure: vedi metodo data del servizio js-utility.js
 * @param {string} currentLang : lingua desiderata
 *
 * Esempio di utilizzo in file .hbs:
 * (converttime value {minute: '2-digit'} 'it')
 */
import { helper } from '@ember/component/helper';

export default helper(function converttime(params /*, named*/) {
  try {
    let [value, format, currentLang] = params;
    if (!value) return '';
    if (!currentLang) currentLang = 'it';

    if (!format || typeof format === 'undefined' || format.toString() === '') {
      format = {
        hour: '2-digit',
        minute: '2-digit',
      };
    }

    if (typeof format === 'string') format = JSON.parse(format);

    let d = new Date(value);
    return d.toLocaleTimeString(currentLang, format);
  } catch (e) {
    return '';
  }
});
