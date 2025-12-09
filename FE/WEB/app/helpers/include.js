/**
 * VERIFICA L'ESISTENZA DI UN VALORE IN UN ARRAY.
 *
 * Esempio di utilizzo:
 * (include this.array value)
 */
import { helper } from '@ember/component/helper';

export default helper(function include(params /*, named*/) {
  const [items, value] = params;
  return items.indexOf(value) > -1;
});
