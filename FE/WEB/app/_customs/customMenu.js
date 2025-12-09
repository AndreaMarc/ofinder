export default {
  /**
   * IL MENU' PRINCIPALE DELL'APPLICATIVO
   * che compare nell'header, a destra del Cambio Tenant.
   * In questo file devi solo compilare i campi sottostanti,
   * il codice html/js vero e proprio è contenuto nel componente custom-menu
   *
   * @param {bool} active stabilisce se mostrare o meno il menù
   * @param {string} icon l'icona del pulsante
   * @param {array} label il testo del pulsante (desktop) e del popup (desktop e app)
   * @param {array} subLabel il testo secondario nel popup (desktop e app)
   * @param {string} colorClass la classe di colore per il popup. Lasciare vuoto per utilizzare i colori del tema
   * @param {array} permissions i permessi necessari per visualizzarlo. Se array vuoto, è sempre visibile.
   *
   */
  customMenu: {
    active: true,
    icon: 'fa fa-cogs',
    label: 'ERP',
    subLabel: 'Impostazioni generali',
    colorClass: '',
    permissions: [],
  },
};
