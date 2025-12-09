/* eslint-disable prettier/prettier */
/* eslint-disable no-unused-labels */
/**
 * Estrae dal BE l'elenco delle configurazioni mancanti
 *
 * @param {*} fetch servizio fetch
 * @param {*} session servizio session
 */

import { convertLangCode } from './convert-lang-code';
import { htmlSafe } from '@ember/template';

// Scarica dal server l'elenco delle configurazioni essenziali incomplete
export async function getIncomplete(fetch, session) {
  return new Promise((resolve) => {
    fetch
      .call('checkConfiguration', 'GET', null, {}, true, session)
      .then((res) => {
        //console.log(res);
        return _humanSyntax(res);
      })
      .then((res) => {
        resolve(res);
      })
      .catch((error) => {
        console.error(
          `Errore recupero endpoint checkConfiguration (per componente 'incomplete-configuration')`,
          error
        );
        //reject();
        resolve([]);
      });
  });
}

// converte il dato delle configurazioni incomplete in una forma utile per l'utente
function _humanSyntax(data) {
  return new Promise((resolve) => {
    if (data.length === 0) {
      resolve([]);
    } else {
      let x = [];
      data.forEach((element) => {
        // element = { errorType: '', language: '', name: '', id: 0 }
        //console.warn(element);
        switch (element.errorType) {
          case 'MaintenanceWeb':
            x.push({
              info: htmlSafe(`È attiva la manutenzione del sito Web`),
              link: 'setup',
              color: 'warning',
            });
          break;
          case 'MaintenanceApp':
            x.push({
              info: htmlSafe(`È attiva la manutenzione dell'App`),
              link: 'setup',
              color: 'warning',
            });
          break;
          case 'TemplateNotFound':
            x.push({
              info: htmlSafe(`Il template '${element.name}', in lingua <span class="incomplete-configuration-flag flag medium ${convertLangCode(element.language)}"></span> non esiste`),
              link: 'templates',
              color: 'danger',
            });
            break;
          case 'TemplateNoContentNotActive':
            x.push({
              info: htmlSafe(`Il template '${element.name}', in lingua <span class="incomplete-configuration-flag flag medium ${convertLangCode(element.language)}"></span> è vuoto e non è attivo`),
              link: 'template-details',
              id: element.id,
              queryParams: { showObject: true, showTags: false },
              color: 'danger',
            });
            break;
          case 'TemplateNoContent':
            x.push({
              info: htmlSafe(`Il template '${element.name}', in lingua <span class="incomplete-configuration-flag flag medium ${convertLangCode(element.language)}"></span> è vuoto`),
              link: `template-details`,
              id: element.id,
              queryParams: { showObject: true, showTags: false },
              color: 'danger',
            });
            break;
          case 'TemplateNotActive':
            x.push({
              info: htmlSafe(`Il template '${element.name}', in lingua <span class="incomplete-configuration-flag flag medium ${convertLangCode(element.language)}"></span> non è attivo`),
              link: `template-details`,
              id: element.id,
              queryParams: { showObject: true, showTags: false },
              color: 'danger',
            });
            break;
          case 'RecoveryTenantNotFound':
            x.push({
              info: htmlSafe(`Il Tenant di Recupero non esiste. Contattare uno sviluppatore.`),
              link: ``,
              color: 'danger',
            });
            break;
          case 'MultipleRecoveryTenant':
            x.push({
              info: htmlSafe(`Sono presenti 2 o più Tenant di Recupero. Contattare uno sviluppatore.`),
              link: ``,
              color: 'danger',
            });
            break;
          case 'LegalTermNotFound':
            x.push({
              info: htmlSafe(`Il documento legale '<em>${element.name}</em>', in lingua <span class="incomplete-configuration-flag flag medium ${convertLangCode(element.language)}"></span>, non esiste`),
              link: `legals`,
              //id: element.id,
              color: 'danger',
            });
            break;
          case 'LegalTermActiveNoContent':
            x.push({
              info: htmlSafe(`Il documento legale '<em>${element.name}</em>', in lingua <span class="incomplete-configuration-flag flag medium ${convertLangCode(element.language)}"></span>, è vuoto`),
              link: `legals`,
              //id: element.id,
              color: 'danger',
            });
            break;
          case 'NoOneLegalTermActive':
            x.push({
              info: htmlSafe(`Il documento legale '<em>${element.name}</em>', in lingua <span class="incomplete-configuration-flag flag medium ${convertLangCode(element.language)}"></span>, non è attivo`),
              link: `legals`,
              //id: element.id,
              color: 'danger',
            });
            break;
          case 'MoreThanOneLegalTermActive':
            x.push({
              info: htmlSafe(`Due o più documenti legali '<em>${element.name}</em>', in lingua <span class="incomplete-configuration-flag flag medium ${convertLangCode(element.language)}"></span>, risultano attivi. Contattare uno sviluppatore.`),
              //link: `legals`,
              id: element.id,
              color: 'danger',
            });
            break;
          case 'MismatchLegalTermsVersion':
            x.push({
              info: htmlSafe(`Le traduzioni del documento legale '<em>${element.name}</em>' hanno versioni diverse.`),
              link: `legals`,
              //id: element.id, // es: privacyPolicy
              color: 'warning',
            });
            break;
          default:
            x.push({
              info: `Altri elementi non completi. Contattare gli sviluppatori per dettagli`,
              link: '',
              color: 'danger',
            });
        }
      });
      resolve(x);
    }
  });
}
