import { helper } from '@ember/component/helper';

/**
 * to-string helper
 * Restituisce il numero intero corrispondente al primo argomento passato.
 * Per null/undefined restituisce NaN
 */
export default helper(function parseInt([value]) {
  if (!value) {
    return NaN; // Se il valore è null o undefined, restituisce NaN
  }
  // Se è già una stringa, la restituisco così com’è
  if (typeof value === 'number') {
    return value;
  }
  // Per null/undefined restituisce NaN, altrimenti uso parseInt()
  return parseInt(value);
});
