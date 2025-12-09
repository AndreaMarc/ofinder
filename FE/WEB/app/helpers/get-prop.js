/** Estrae e restituisce il valore di una proprietà di un oggetto, come si farebbe in javascript.
 *
 * @param {object} obj : oggetto da cui estrarre il valore
 * @param {string} key : chiave di cui leggere il valore
 *
 * {{get-prop obj key}}
 *
 * Esempio:
 * Se abbiamo un oggetto this.newRecord e una proprietà contenuta in column.name,
 * useremo:
 *
 * {{get-prop this.newRecord column.name}}
 *
 */
import { helper } from '@ember/component/helper';

export function getProp([object, propName]) {
  return object[propName];
}

export default helper(getProp);
