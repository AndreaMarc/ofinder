import { helper } from '@ember/component/helper';

/**
 * to-string helper
 * Restituisce la stringa corrispondente al primo argomento passato.
 * Se passi null o undefined, restituisce stringa vuota.
 */
export default helper(function toString([value]) {
  // Se è già una stringa, la restituisco così com’è
  if (typeof value === 'string') {
    return value;
  }
  // Per null/undefined restituisco vuoto, altrimenti uso String()
  return value == null ? '' : String(value);
});
