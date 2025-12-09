import { helper } from '@ember/component/helper';

export default helper(function typeofHelper([value]) {
  return typeof value;
});
