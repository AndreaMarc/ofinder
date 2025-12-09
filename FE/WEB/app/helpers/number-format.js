/**
 * FORMATTA I NUMERI CON I SEPARATORI PREVISTI DALL'INTERNAZIONALIZZAZIONE
 *
 * Esempio di utilizzo:
 * {{number-format number}}
 */
import { helper } from '@ember/component/helper';

export default helper(function numberFormat(params /*, named*/) {
  const [value] = params;
  return new Intl.NumberFormat().format(value);
});
