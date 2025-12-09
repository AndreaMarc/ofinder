import { helper } from '@ember/component/helper';

export default helper(function include(params /*, named*/) {
  const [item, value] = params;
  return typeof item === 'undefined' || item.toString() === value.toString()
    ? true
    : false;
});
