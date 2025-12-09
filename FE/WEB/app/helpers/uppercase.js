import { helper } from '@ember/component/helper';

export default helper(function uppercase(params /*, named*/) {
  // eslint-disable-next-line no-unused-vars
  const [value] = params;
  return value.toUpperCase();
});
