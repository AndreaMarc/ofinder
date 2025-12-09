// CONFIGURAZIONE GENERALE
// Se all'avvio non fosse possibile recuperare il setup dal server
// (file utility/utils-stratup.js chiamato da routes/application.js),
// verrà utilizzata questa configurazione di funzionamento.
export default {
  minAppVersion: '',
  maintenance: false,
  useRemoteFiles: false,
  disableLog: false,
  publicRegistration: true,
  sliderPosition: 'left',
  sliderPics: [
    {
      page: 'login',
      title: 'Title 1',
      description: 'Description for login and welcome-page',
      url: 'assets/images/originals/city.jpg',
    },
    {
      page: 'registration',
      title: 'Title 2',
      description: 'Description 2 for registration page',
      url: 'assets/images/originals/city.jpg',
    },
    {
      page: 'terms',
      title: 'Title 1',
      description: 'Description for terms page',
      url: 'assets/images/originals/city.jpg',
    },
  ],
  availableLanguages: [{ code: 'it', label: 'Italiano', active: true }],
  defaultLanguage: 'it',
  failedLoginAttempts: 0,
  previousPasswordsStored: 0,
  defaultUserPassword: 'Maestrale2004!',
  // TODO : inserire tutte le voci mancanti!
};
