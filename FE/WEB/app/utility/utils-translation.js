// verifica se sono disponibili traduzioni e stabilisce quale traduzione utilizzare
export async function languageManager(siteSetup) {
  return new Promise((resolve) => {
    let result = {
      languageAvailable: false,
      currentLang: 'it',
    };

    if (
      siteSetup.availableLanguages &&
      siteSetup.availableLanguages !== '' &&
      siteSetup.availableLanguages !== '""' &&
      siteSetup.defaultLanguage &&
      siteSetup.defaultLanguage !== ''
    ) {
      let availableLanguages =
        typeof siteSetup.availableLanguages === 'string'
          ? JSON.parse(siteSetup.availableLanguages)
          : siteSetup.availableLanguages;
      let available = availableLanguages.filter((item) => {
        return item.active === true;
      });

      if (available && available.length > 0) {
        // eslint-disable-next-line prettier/prettier
        let userLang = (navigator.language || navigator.userLanguage).substring(0, 2); // es: 'it-IT' => 'it'
        let languageChosen = localStorage.getItem('poc-user-lang');

        if (languageChosen && languageChosen !== '') {
          let verify = available.filter((item) => {
            return item.code === languageChosen;
          });
          if (verify && verify.length > 0) {
            result.currentLang = languageChosen;
          } else {
            result.currentLang = siteSetup.defaultLanguage;
          }
        } else {
          let verify = available.filter((item) => {
            return item.code === userLang;
          });

          if (verify && verify.length > 0) {
            result.currentLang = userLang;
          } else {
            result.currentLang = siteSetup.defaultLanguage;
          }
        }
        result.languageAvailable = true;
      } else {
        result.currentLang = siteSetup.defaultLanguage;
      }
    }
    resolve(result);
  });
}
