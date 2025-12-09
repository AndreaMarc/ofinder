/**
 * Esegue la sanificazione del contenuto HTML e poi lo marca come sicuro per essere utilizzato in un template.
 * Da usare quando si vuole stampare a schermo il contenuto proveniente da fonti non sicure (es: un form di inserimento dati).
 *
 * Esempio di utilizzo:
 * {{sanitize-content "<script>alert('Hello');</script>Test <b>bold</b> text"}}
 */
import { helper } from '@ember/component/helper';
import { htmlSafe } from '@ember/template';
import DOMPurify from 'dompurify';

export default helper(function sanitizeContent(params) {
  let [content] = params;
  let sanitizedContent = DOMPurify.sanitize(content);
  return htmlSafe(sanitizedContent);
});
