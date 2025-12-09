export function convertLangCode(code) {
  if (code) {
    //console.warn('CODICE: ', code);
    let ret = code;
    switch (code) {
      case 'en':
        ret = 'gb';
        break;
      case 'cs':
        ret = 'cz';
        break;
      case 'da':
        ret = 'dk';
        break;
      case 'el':
        ret = 'gr';
        break;
      case 'he':
        ret = 'il';
        break;
      case 'hi':
        ret = 'in';
        break;
      case 'ja':
        ret = 'jp';
        break;
      case 'ko':
        ret = 'kr';
        break;
      case 'zh':
        ret = 'cn';
        break;
    }
    return ret.toUpperCase();
  } else return '';
}
