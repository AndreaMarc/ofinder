import { helper } from '@ember/component/helper';
import { convertLangCode } from '../utility/convert-lang-code';

// adatta i codici di lingua BCP47 => ISO 639.2

export default helper(function ([code]) {
  return convertLangCode(code);
});
