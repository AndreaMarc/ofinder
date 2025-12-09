import { helper } from '@ember/component/helper';

export default helper(function length(params /*, named*/) {
  // eslint-disable-next-line no-unused-vars
  const [value] = params;
  if (value) {
    return value.length;
  } else return 0;
});
