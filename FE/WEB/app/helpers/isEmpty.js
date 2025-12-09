import { helper } from '@ember/component/helper';

export default helper(function isEmpty([fieldValue]) {
  return !fieldValue || fieldValue.length === 0;
});
