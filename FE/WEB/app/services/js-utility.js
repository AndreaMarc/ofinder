import Service from '@ember/service';
import { inject as service } from '@ember/service';

export default class JsUtilityService extends Service {
  @service translation;

  // #region REGEX
  /**
   * REGULAR EXPRESSION
   * @param {string} type : tipologia di espressione regolare desiderata (es: 'upperCase')
   *
   * How to use:
   * let regex = this.jsUtility.regex('email');
   * if (regex.test('aa.bb@callback.it')) ...
   *
   */
  regex(type) {
    var reg;
    switch (type) {
      case 'upperCase': // almeno una maiuscola
        reg = new RegExp('^(?=.*?[A-Z])');
        break;
      case 'lowerCase': // almeno una minuscola
        reg = new RegExp('^(?=.*?[a-z])');
        break;
      case 'number': // almeno un numero
        reg = new RegExp('^(?=.*?[0-9])');
        break;
      case 'specialChar': // almeno un carattere speciale
        reg = new RegExp('^(?=.*?[#?!@$%^&*-])');
        break;
      case 'email': // formato email
        // eslint-disable-next-line prettier/prettier, no-useless-escape
        //reg = new RegExp(/^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/);
        reg = new RegExp(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/);
        break;
      case 'validTag': // solo lettere, numeri, underscore
        reg = new RegExp('^[A-Za-z0-9?,_-]+$');
        break;
      case 'validPassword': // una maiuscola, una minuscola, un carattere speciale, minimo 8 caratteri
        reg = new RegExp(
          '^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$'
        );
        break;
      case 'twoLetters': // esattamente due lettere minuscole
        reg = new RegExp(/^[a-z]{2}$/);
        break;
      case 'appVersion': // 1.0.0
        reg = new RegExp(/^[0-9]+\.[0-9]+\.[0-9]+$/);
        break;
      case 'lnu': // lettere, numeri, underscore
        reg = new RegExp(/^[a-zA-Z0-9_]+$/);
        break;
      case 'lndd': // lettere, numeri, trattini, dot
        // eslint-disable-next-line no-useless-escape
        reg = new RegExp(/^[a-zA-Z0-9\.-]+$/);
        break;
      case 'taxId': // codice fiscale italiano
        reg = new RegExp(
          /^[A-Za-z]{6}[0-9]{2}[A-Za-z]{1}[0-9]{2}[A-Za-z]{1}[0-9]{3}[A-Za-z]{1}$/
        );
        break;
      case 'vat': // partita iva
        reg = new RegExp(/^[0-9]{11}$/);
        break;
      case 'guid': // lista di guid separati da virgole
        reg = new RegExp(
          /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/
        );
        break;
      case 'guidList': // lista di guid separati da virgole
        reg = new RegExp(/^([a-zA-Z0-9]+,)*[a-zA-Z0-9]+$/);
        break;
      case 'time': // orario in formato 8:00 (12 e 24 ore)
        reg = new RegExp(/^([01]?\d|2[0-3]):[0-5]\d$/);
        break;
      case 'dateEn': // data in formato inglese es: 2025-05-22
        reg = new RegExp(/^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$/);
        break;
      case 'datetimeEn': // data e ora in formato inglese es: 2025-05-22T12:00:00
        reg = new RegExp(
          /^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])T([01]\d|2[0-3]):([0-5]\d)$/
        );
        break;
      default:
        reg = null;
    }
    return reg;
  }
  // #endregion

  // #region getUTC
  /**
   * Prende una data locale e la converte nel formato UTC richiesto dal trasformatore date-utc
   * @param {string} localDate : stringa della data locale
   */
  getUTC(localDate) {
    let localDateObject = new Date(localDate); // Creiamo un oggetto Date a partire dalla data locale
    var now_utc = Date.UTC(
      localDateObject.getUTCFullYear(),
      localDateObject.getUTCMonth(),
      localDateObject.getUTCDate(),
      localDateObject.getUTCHours(),
      localDateObject.getUTCMinutes(),
      localDateObject.getUTCSeconds()
    );
    return new Date(now_utc).toISOString();
  }
  // #endregion

  // #region DATA
  /**
   * // Trasformo il timeStamp o un oggetto data in una stringa testuale formattata nella lingua dell'utente
    * @param {int/dataObject}data : se intero rappresenta un timestamp. Altrimenti deve essere un dataObject
    * @param {object}options : oggetto per stabilire i campi da mostrare (come sotto indicato)
    *
    * Esempio di options:
    * { day: '2-digit', month: '2-digit', year: numeric, hour: '2-digit', minute: '2-digit', second: '2-digit' }
    *
    Option          Values          Sample output
    ----------------------------------------------------
    weekday         'narrow'        'M'
                    'short'         'Mon'
                    'long'          'Monday'

    year            '2-digit'       '01'
                    'numeric'       '2001'

    month           '2-digit'       '01'
                    'numeric'       '1'
                    'narrow'        'J'
                    'short'         'Jan'
                    'long'          'January'

    day             '2-digit'       '01'
                    'numeric'       '1'

    hour            '2-digit'       '12 AM'
                    'numeric'       '12 AM'

    minute          '2-digit'       '0'
                    'numeric'       '0'

    second          '2-digit'       '0'
                    'numeric'       '0'

    timeZoneName    'short'         '1/1/2001 GMT+00:00'
                    'long'          '1/1/2001 GMT+00:00'
    */
  data(data, options) {
    let lang = this.translation.currentLang;
    let dataObject;

    if (Object.prototype.toString.call(data) === '[object Date]') {
      dataObject = data;
    } else if (this._isNumber(data)) {
      dataObject = new Date(parseInt(data) * 1000);
    } else {
      return '';
    }

    return dataObject.toLocaleDateString(lang, options);
  }
  // #endregion

  // #region TIME
  // come sopra, ma restituisce solo l'orario
  time(data, options) {
    let lang = 'it-IT';
    let dataObject;

    if (Object.prototype.toString.call(data) === '[object Date]') {
      dataObject = data;
    } else if (this._isNumber(data)) {
      dataObject = new Date(parseInt(data) * 1000);
    } else {
      return '';
    }

    return dataObject.toLocaleTimeString(lang, options);
  }
  // #endregion

  // #region SLEEP
  // introduce una pausa (ms in millisecondi)
  sleep(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
  // #endregion

  // Verifica se una variabile è un numero
  // combina i seguenti controlli: (typeof value === 'number') && value === Number(value) && Number.isFinite(value)
  _isNumber(value) {
    if (typeof value !== 'number') {
      return false;
    }
    if (value !== Number(value)) {
      return false;
    }
    if (Number.isFinite(value) === false) {
      return false;
    }
    return true;
  }

  // estrae informazioni sul sistema operativo e sul browser
  getDeviceInfos() {
    if (typeof window.cordova !== 'undefined') {
      // ambiente Cordova
      let out = 'App';
      return out;
    } else {
      // ambiente Web
      let userAgent = navigator.userAgent;

      let os = 'Sistema operativo sconosciuto';
      if (userAgent.indexOf('Win') > -1) os = 'Windows';
      else if (userAgent.indexOf('Mac') > -1) os = 'MacOS';
      else if (userAgent.indexOf('Linux') > -1) os = 'Linux';
      else if (userAgent.indexOf('Android') > -1) os = 'Android';
      else if (userAgent.indexOf('like Mac') > -1) os = 'iOS';

      let browserName = 'Browser sconosciuto';

      if (userAgent.indexOf('Opera') > -1 || userAgent.indexOf('OPR') > -1)
        browserName = 'Opera';
      else if (userAgent.indexOf('Edg') > -1) browserName = 'Microsoft Edge';
      else if (
        userAgent.indexOf('MSIE') > -1 ||
        userAgent.indexOf('Trident') > -1
      )
        browserName = 'Microsoft Internet Explorer';
      else if (userAgent.indexOf('Firefox') > -1)
        browserName = 'Mozilla Firefox';
      else if (userAgent.indexOf('Chrome') > -1) browserName = 'Google Chrome';
      else if (userAgent.indexOf('Safari') > -1) browserName = 'Apple Safari';
      else if (userAgent.indexOf('SamsungBrowser') > -1)
        browserName = 'Samsung Internet Browser';
      else if (userAgent.match(/Brave\//)) browserName = 'Brave';

      return `${os} - ${browserName}`;
    }
  }

  // #region NATIONS
  // restituisce un array di oggetti contenenti nomi e sigle delle nazioni mondiali secondo lo standard ISO 3166-1 alpha-2
  nations() {
    return [
      {
        name: 'Afghanistan',
        code: 'AF',
      },
      {
        name: 'Åland Islands',
        code: 'AX',
      },
      {
        name: 'Albania',
        code: 'AL',
      },
      {
        name: 'Algeria',
        code: 'DZ',
      },
      {
        name: 'American Samoa',
        code: 'AS',
      },
      {
        name: 'Andorra',
        code: 'AD',
      },
      {
        name: 'Angola',
        code: 'AO',
      },
      {
        name: 'Anguilla',
        code: 'AI',
      },
      {
        name: 'Antarctica',
        code: 'AQ',
      },
      {
        name: 'Antigua and Barbuda',
        code: 'AG',
      },
      {
        name: 'Argentina',
        code: 'AR',
      },
      {
        name: 'Armenia',
        code: 'AM',
      },
      {
        name: 'Aruba',
        code: 'AW',
      },
      {
        name: 'Australia',
        code: 'AU',
      },
      {
        name: 'Austria',
        code: 'AT',
      },
      {
        name: 'Azerbaijan',
        code: 'AZ',
      },
      {
        name: 'Bahamas',
        code: 'BS',
      },
      {
        name: 'Bahrain',
        code: 'BH',
      },
      {
        name: 'Bangladesh',
        code: 'BD',
      },
      {
        name: 'Barbados',
        code: 'BB',
      },
      {
        name: 'Belarus',
        code: 'BY',
      },
      {
        name: 'Belgium',
        code: 'BE',
      },
      {
        name: 'Belize',
        code: 'BZ',
      },
      {
        name: 'Benin',
        code: 'BJ',
      },
      {
        name: 'Bermuda',
        code: 'BM',
      },
      {
        name: 'Bhutan',
        code: 'BT',
      },
      {
        name: 'Bolivia (Plurinational State of)',
        code: 'BO',
      },
      {
        name: 'Bonaire, Sint Eustatius and Saba',
        code: 'BQ',
      },
      {
        name: 'Bosnia and Herzegovina',
        code: 'BA',
      },
      {
        name: 'Botswana',
        code: 'BW',
      },
      {
        name: 'Bouvet Island',
        code: 'BV',
      },
      {
        name: 'Brazil',
        code: 'BR',
      },
      {
        name: 'British Indian Ocean Territory',
        code: 'IO',
      },
      {
        name: 'United States Minor Outlying Islands',
        code: 'UM',
      },
      {
        name: 'Virgin Islands (British)',
        code: 'VG',
      },
      {
        name: 'Virgin Islands (U.S.)',
        code: 'VI',
      },
      {
        name: 'Brunei Darussalam',
        code: 'BN',
      },
      {
        name: 'Bulgaria',
        code: 'BG',
      },
      {
        name: 'Burkina Faso',
        code: 'BF',
      },
      {
        name: 'Burundi',
        code: 'BI',
      },
      {
        name: 'Cambodia',
        code: 'KH',
      },
      {
        name: 'Cameroon',
        code: 'CM',
      },
      {
        name: 'Canada',
        code: 'CA',
      },
      {
        name: 'Cabo Verde',
        code: 'CV',
      },
      {
        name: 'Cayman Islands',
        code: 'KY',
      },
      {
        name: 'Central African Republic',
        code: 'CF',
      },
      {
        name: 'Chad',
        code: 'TD',
      },
      {
        name: 'Chile',
        code: 'CL',
      },
      {
        name: 'China',
        code: 'CN',
      },
      {
        name: 'Christmas Island',
        code: 'CX',
      },
      {
        name: 'Cocos (Keeling) Islands',
        code: 'CC',
      },
      {
        name: 'Colombia',
        code: 'CO',
      },
      {
        name: 'Comoros',
        code: 'KM',
      },
      {
        name: 'Congo',
        code: 'CG',
      },
      {
        name: 'Congo (Democratic Republic of the)',
        code: 'CD',
      },
      {
        name: 'Cook Islands',
        code: 'CK',
      },
      {
        name: 'Costa Rica',
        code: 'CR',
      },
      {
        name: 'Croatia',
        code: 'HR',
      },
      {
        name: 'Cuba',
        code: 'CU',
      },
      {
        name: 'Curaçao',
        code: 'CW',
      },
      {
        name: 'Cyprus',
        code: 'CY',
      },
      {
        name: 'Czech Republic',
        code: 'CZ',
      },
      {
        name: 'Denmark',
        code: 'DK',
      },
      {
        name: 'Djibouti',
        code: 'DJ',
      },
      {
        name: 'Dominica',
        code: 'DM',
      },
      {
        name: 'Dominican Republic',
        code: 'DO',
      },
      {
        name: 'Ecuador',
        code: 'EC',
      },
      {
        name: 'Egypt',
        code: 'EG',
      },
      {
        name: 'El Salvador',
        code: 'SV',
      },
      {
        name: 'Equatorial Guinea',
        code: 'GQ',
      },
      {
        name: 'Eritrea',
        code: 'ER',
      },
      {
        name: 'Estonia',
        code: 'EE',
      },
      {
        name: 'Ethiopia',
        code: 'ET',
      },
      {
        name: 'Falkland Islands (Malvinas)',
        code: 'FK',
      },
      {
        name: 'Faroe Islands',
        code: 'FO',
      },
      {
        name: 'Fiji',
        code: 'FJ',
      },
      {
        name: 'Finland',
        code: 'FI',
      },
      {
        name: 'France',
        code: 'FR',
      },
      {
        name: 'French Guiana',
        code: 'GF',
      },
      {
        name: 'French Polynesia',
        code: 'PF',
      },
      {
        name: 'French Southern Territories',
        code: 'TF',
      },
      {
        name: 'Gabon',
        code: 'GA',
      },
      {
        name: 'Gambia',
        code: 'GM',
      },
      {
        name: 'Georgia',
        code: 'GE',
      },
      {
        name: 'Germany',
        code: 'DE',
      },
      {
        name: 'Ghana',
        code: 'GH',
      },
      {
        name: 'Gibraltar',
        code: 'GI',
      },
      {
        name: 'Greece',
        code: 'GR',
      },
      {
        name: 'Greenland',
        code: 'GL',
      },
      {
        name: 'Grenada',
        code: 'GD',
      },
      {
        name: 'Guadeloupe',
        code: 'GP',
      },
      {
        name: 'Guam',
        code: 'GU',
      },
      {
        name: 'Guatemala',
        code: 'GT',
      },
      {
        name: 'Guernsey',
        code: 'GG',
      },
      {
        name: 'Guinea',
        code: 'GN',
      },
      {
        name: 'Guinea-Bissau',
        code: 'GW',
      },
      {
        name: 'Guyana',
        code: 'GY',
      },
      {
        name: 'Haiti',
        code: 'HT',
      },
      {
        name: 'Heard Island and McDonald Islands',
        code: 'HM',
      },
      {
        name: 'Vatican City',
        code: 'VA',
      },
      {
        name: 'Honduras',
        code: 'HN',
      },
      {
        name: 'Hungary',
        code: 'HU',
      },
      {
        name: 'Hong Kong',
        code: 'HK',
      },
      {
        name: 'Iceland',
        code: 'IS',
      },
      {
        name: 'India',
        code: 'IN',
      },
      {
        name: 'Indonesia',
        code: 'ID',
      },
      {
        name: 'Ivory Coast',
        code: 'CI',
      },
      {
        name: 'Iran (Islamic Republic of)',
        code: 'IR',
      },
      {
        name: 'Iraq',
        code: 'IQ',
      },
      {
        name: 'Ireland',
        code: 'IE',
      },
      {
        name: 'Isle of Man',
        code: 'IM',
      },
      {
        name: 'Israel',
        code: 'IL',
      },
      {
        name: 'Italy',
        code: 'IT',
      },
      {
        name: 'Jamaica',
        code: 'JM',
      },
      {
        name: 'Japan',
        code: 'JP',
      },
      {
        name: 'Jersey',
        code: 'JE',
      },
      {
        name: 'Jordan',
        code: 'JO',
      },
      {
        name: 'Kazakhstan',
        code: 'KZ',
      },
      {
        name: 'Kenya',
        code: 'KE',
      },
      {
        name: 'Kiribati',
        code: 'KI',
      },
      {
        name: 'Kuwait',
        code: 'KW',
      },
      {
        name: 'Kyrgyzstan',
        code: 'KG',
      },
      {
        name: "Lao People's Democratic Republic",
        code: 'LA',
      },
      {
        name: 'Latvia',
        code: 'LV',
      },
      {
        name: 'Lebanon',
        code: 'LB',
      },
      {
        name: 'Lesotho',
        code: 'LS',
      },
      {
        name: 'Liberia',
        code: 'LR',
      },
      {
        name: 'Libya',
        code: 'LY',
      },
      {
        name: 'Liechtenstein',
        code: 'LI',
      },
      {
        name: 'Lithuania',
        code: 'LT',
      },
      {
        name: 'Luxembourg',
        code: 'LU',
      },
      {
        name: 'Macao',
        code: 'MO',
      },
      {
        name: 'North Macedonia',
        code: 'MK',
      },
      {
        name: 'Madagascar',
        code: 'MG',
      },
      {
        name: 'Malawi',
        code: 'MW',
      },
      {
        name: 'Malaysia',
        code: 'MY',
      },
      {
        name: 'Maldives',
        code: 'MV',
      },
      {
        name: 'Mali',
        code: 'ML',
      },
      {
        name: 'Malta',
        code: 'MT',
      },
      {
        name: 'Marshall Islands',
        code: 'MH',
      },
      {
        name: 'Martinique',
        code: 'MQ',
      },
      {
        name: 'Mauritania',
        code: 'MR',
      },
      {
        name: 'Mauritius',
        code: 'MU',
      },
      {
        name: 'Mayotte',
        code: 'YT',
      },
      {
        name: 'Mexico',
        code: 'MX',
      },
      {
        name: 'Micronesia (Federated States of)',
        code: 'FM',
      },
      {
        name: 'Moldova (Republic of)',
        code: 'MD',
      },
      {
        name: 'Monaco',
        code: 'MC',
      },
      {
        name: 'Mongolia',
        code: 'MN',
      },
      {
        name: 'Montenegro',
        code: 'ME',
      },
      {
        name: 'Montserrat',
        code: 'MS',
      },
      {
        name: 'Morocco',
        code: 'MA',
      },
      {
        name: 'Mozambique',
        code: 'MZ',
      },
      {
        name: 'Myanmar',
        code: 'MM',
      },
      {
        name: 'Namibia',
        code: 'NA',
      },
      {
        name: 'Nauru',
        code: 'NR',
      },
      {
        name: 'Nepal',
        code: 'NP',
      },
      {
        name: 'Netherlands',
        code: 'NL',
      },
      {
        name: 'New Caledonia',
        code: 'NC',
      },
      {
        name: 'New Zealand',
        code: 'NZ',
      },
      {
        name: 'Nicaragua',
        code: 'NI',
      },
      {
        name: 'Niger',
        code: 'NE',
      },
      {
        name: 'Nigeria',
        code: 'NG',
      },
      {
        name: 'Niue',
        code: 'NU',
      },
      {
        name: 'Norfolk Island',
        code: 'NF',
      },
      {
        name: "Korea (Democratic People's Republic of)",
        code: 'KP',
      },
      {
        name: 'Northern Mariana Islands',
        code: 'MP',
      },
      {
        name: 'Norway',
        code: 'NO',
      },
      {
        name: 'Oman',
        code: 'OM',
      },
      {
        name: 'Pakistan',
        code: 'PK',
      },
      {
        name: 'Palau',
        code: 'PW',
      },
      {
        name: 'Palestine, State of',
        code: 'PS',
      },
      {
        name: 'Panama',
        code: 'PA',
      },
      {
        name: 'Papua New Guinea',
        code: 'PG',
      },
      {
        name: 'Paraguay',
        code: 'PY',
      },
      {
        name: 'Peru',
        code: 'PE',
      },
      {
        name: 'Philippines',
        code: 'PH',
      },
      {
        name: 'Pitcairn',
        code: 'PN',
      },
      {
        name: 'Poland',
        code: 'PL',
      },
      {
        name: 'Portugal',
        code: 'PT',
      },
      {
        name: 'Puerto Rico',
        code: 'PR',
      },
      {
        name: 'Qatar',
        code: 'QA',
      },
      {
        name: 'Republic of Kosovo',
        code: 'XK',
      },
      {
        name: 'Réunion',
        code: 'RE',
      },
      {
        name: 'Romania',
        code: 'RO',
      },
      {
        name: 'Russian Federation',
        code: 'RU',
      },
      {
        name: 'Rwanda',
        code: 'RW',
      },
      {
        name: 'Saint Barthélemy',
        code: 'BL',
      },
      {
        name: 'Saint Helena, Ascension and Tristan da Cunha',
        code: 'SH',
      },
      {
        name: 'Saint Kitts and Nevis',
        code: 'KN',
      },
      {
        name: 'Saint Lucia',
        code: 'LC',
      },
      {
        name: 'Saint Martin (French part)',
        code: 'MF',
      },
      {
        name: 'Saint Pierre and Miquelon',
        code: 'PM',
      },
      {
        name: 'Saint Vincent and the Grenadines',
        code: 'VC',
      },
      {
        name: 'Samoa',
        code: 'WS',
      },
      {
        name: 'San Marino',
        code: 'SM',
      },
      {
        name: 'Sao Tome and Principe',
        code: 'ST',
      },
      {
        name: 'Saudi Arabia',
        code: 'SA',
      },
      {
        name: 'Senegal',
        code: 'SN',
      },
      {
        name: 'Serbia',
        code: 'RS',
      },
      {
        name: 'Seychelles',
        code: 'SC',
      },
      {
        name: 'Sierra Leone',
        code: 'SL',
      },
      {
        name: 'Singapore',
        code: 'SG',
      },
      {
        name: 'Sint Maarten (Dutch part)',
        code: 'SX',
      },
      {
        name: 'Slovakia',
        code: 'SK',
      },
      {
        name: 'Slovenia',
        code: 'SI',
      },
      {
        name: 'Solomon Islands',
        code: 'SB',
      },
      {
        name: 'Somalia',
        code: 'SO',
      },
      {
        name: 'South Africa',
        code: 'ZA',
      },
      {
        name: 'South Georgia and the South Sandwich Islands',
        code: 'GS',
      },
      {
        name: 'Korea (Republic of)',
        code: 'KR',
      },
      {
        name: 'Spain',
        code: 'ES',
      },
      {
        name: 'Sri Lanka',
        code: 'LK',
      },
      {
        name: 'Sudan',
        code: 'SD',
      },
      {
        name: 'South Sudan',
        code: 'SS',
      },
      {
        name: 'Suriname',
        code: 'SR',
      },
      {
        name: 'Svalbard and Jan Mayen',
        code: 'SJ',
      },
      {
        name: 'Swaziland',
        code: 'SZ',
      },
      {
        name: 'Sweden',
        code: 'SE',
      },
      {
        name: 'Switzerland',
        code: 'CH',
      },
      {
        name: 'Syrian Arab Republic',
        code: 'SY',
      },
      {
        name: 'Taiwan',
        code: 'TW',
      },
      {
        name: 'Tajikistan',
        code: 'TJ',
      },
      {
        name: 'Tanzania, United Republic of',
        code: 'TZ',
      },
      {
        name: 'Thailand',
        code: 'TH',
      },
      {
        name: 'Timor-Leste',
        code: 'TL',
      },
      {
        name: 'Togo',
        code: 'TG',
      },
      {
        name: 'Tokelau',
        code: 'TK',
      },
      {
        name: 'Tonga',
        code: 'TO',
      },
      {
        name: 'Trinidad and Tobago',
        code: 'TT',
      },
      {
        name: 'Tunisia',
        code: 'TN',
      },
      {
        name: 'Turkey',
        code: 'TR',
      },
      {
        name: 'Turkmenistan',
        code: 'TM',
      },
      {
        name: 'Turks and Caicos Islands',
        code: 'TC',
      },
      {
        name: 'Tuvalu',
        code: 'TV',
      },
      {
        name: 'Uganda',
        code: 'UG',
      },
      {
        name: 'Ukraine',
        code: 'UA',
      },
      {
        name: 'United Arab Emirates',
        code: 'AE',
      },
      {
        name: 'United Kingdom of Great Britain and Northern Ireland',
        code: 'GB',
      },
      {
        name: 'United States of America',
        code: 'US',
      },
      {
        name: 'Uruguay',
        code: 'UY',
      },
      {
        name: 'Uzbekistan',
        code: 'UZ',
      },
      {
        name: 'Vanuatu',
        code: 'VU',
      },
      {
        name: 'Venezuela (Bolivarian Republic of)',
        code: 'VE',
      },
      {
        name: 'Vietnam',
        code: 'VN',
      },
      {
        name: 'Wallis and Futuna',
        code: 'WF',
      },
      {
        name: 'Western Sahara',
        code: 'EH',
      },
      {
        name: 'Yemen',
        code: 'YE',
      },
      {
        name: 'Zambia',
        code: 'ZM',
      },
      {
        name: 'Zimbabwe',
        code: 'ZW',
      },
    ];
  }
  // #endregion

  // #region groupByKey
  /**
   * Raggruppa gli oggetti di un array per parametro
   * @param {array} list un array di oggetti
   * @param {string} key la chiave con cui raggrupparli
   * @param {bool} omitKey indica se eliminare dagli oggetti la chiave con cui sono stati raggruppati
   *
   * Esempi di utilizzo
   * var cars = [{'make':'audi','model':'r8','year':'2012'},{'make':'audi','model':'rs5','year':'2013'},{'make':'ford','model':'mustang','year':'2012'},{'make':'ford','model':'fusion','year':'2015'},{'make':'kia','model':'optima','year':'2012'}];
   * let res = groupByKey(cars, 'make', {omitKey:true})
   *
   * res => {
              "audi": [
                {
                  "model": "r8",
                  "year": "2012"
                },
                {
                  "model": "rs5",
                  "year": "2013"
                }
              ],
              "ford": [
                {
                  "model": "mustang",
                  "year": "2012"
                },
                {
                  "model": "fusion",
                  "year": "2015"
                }
              ],
              "kia": [
                {
                  "model": "optima",
                  "year": "2012"
                }
              ]
            }
   */
  groupByKey(list, key, { omitKey = false }) {
    list.reduce(
      (hash, { [key]: value, ...rest }) => ({
        ...hash,
        [value]: (hash[value] || []).concat(
          omitKey ? { ...rest } : { [key]: value, ...rest }
        ),
      }),
      {}
    );
  }
  // #endregion

  // #region verifyPassword
  verifyPassword(password) {
    let msg = '';
    if (password.length < 8) {
      msg += `<i class="fa fa-exclamation-triangle"></i> lunghezza minima 8 caratteri<br />`;
    }
    let regex = this.regex('upperCase'); // new RegExp('^(?=.*?[A-Z])');
    if (!regex.test(password)) {
      msg += `<i class="fa fa-exclamation-triangle"></i> almeno una lettera maiuscola<br />`;
    }
    regex = this.regex('lowerCase');
    if (!regex.test(password)) {
      msg += `<i class="fa fa-exclamation-triangle"></i> almeno una lettera minuscola<br />`;
    }
    regex = this.regex('number');
    if (!regex.test(password)) {
      msg += `<i class="fa fa-exclamation-triangle"></i> almeno un numero<br />`;
    }
    regex = this.regex('specialChar');
    if (!regex.test(password)) {
      msg += `<i class="fa fa-exclamation-triangle"></i> almeno un carattere speciale (#, ?, !, @, $, %, ^, &, *, -)<br />`; // lunghezza minima 8 caratteri
    }

    return msg;
  }
  // #endregion
}
