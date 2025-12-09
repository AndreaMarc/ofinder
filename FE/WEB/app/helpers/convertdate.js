import { helper } from '@ember/component/helper';

export default helper(function convertdate(params /*, named*/) {
  let [value, format] = params;
  if (!format || typeof format === 'undefined' || format === '')
    format = 'YYYY-MM-DD';
  //return moment(value, 'YYYY-MM-DDTHH:mm:ss').format(format);
  //console.log(value);
  //console.log(moment.utc(value, 'YYYY-MM-DDTHH:mm:ss').local().format(format));
  //console.log(moment.utc(value, 'YYYY-MM-DDTHH:mm:ss').local().format(format));
  //console.log(moment.parseZone(value).local().format(format));
  return moment.parseZone(value).local().format(format);
});
