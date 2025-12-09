// Definisce le voci del menù laterale
export default {
  /**
   * @description DEFINES THE ITEMS OF THE LEFT SIDE MENU
   *
   * Each item is defined as follows
   *
   * @param {string} label displayed item name
   * @param {string} icon desired item icon
   * @param {array} environments environments in which to publish that item. At least one required value. Allowed values: 'development', 'test', 'test-publish', 'production'
   * @param {link} link hyperlink (starts with /). Use '/' to point to home or '#' to not associate any links
   * @param {array} permissions user's permissions required to view the item (if empty, it is always shown). Refer to the permissions received from the BE during login
   * @param {bool} divider divider element that indicates a new menu section
   * @param {array} childrens list of any sub-items, defined as follows
   * ↳  @param {string} label displayed sub-item name
   * ↳  @param {string} icon icon of the desired sub-element
   * ↳  @param {array} environments environments in which to publish that sub-item. At least one required value. Allowed values: 'development', 'test', 'test-publish', 'production'
   * ↳  @param {link} link hyperlink (starts with /). Use '/' to point to home or '#' to not associate any links
   * ↳  @param {array} permissions user permissions required to view the sub-item (if empty, it is always shown). Refer to the permissions received from the BE during login
   *
   * @note if divider=true, link and childrens are ignored
   *
   * @note2 'test-publish' is the environment to test the released test environment
   */
  //
  menuJson: [
    {
      label: 'MENÙ',
      icon: '',
      devices: ['app', 'web'],
      environments: ['development', 'test', 'test-publish', 'production'],
      link: '#',
      permissions: [],
      divider: true,
      childrens: [],
    },
    {
      label: 'Home',
      icon: 'metismenu-icon pe-7s-home',
      devices: ['app', 'web'],
      environments: ['development', 'test', 'test-publish', 'production'],
      link: '/',
      permissions: [],
      divider: false,
      childrens: [],
    },
    {
      label: 'Esempi',
      icon: 'metismenu-icon pe-7s-coffee',
      devices: ['app', 'web'],
      environments: ['development', 'test', 'test-publish', 'production'],
      link: '#',
      permissions: ['isSuperAdmin', 'isDeveloper'],
      divider: false,
      childrens: [
        {
          label: 'Empty Logged Page',
          icon: '',
          devices: ['app', 'web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/empty-logged-page',
          permissions: ['isSuperAdmin', 'isDeveloper'],
        },
      ],
    },
    {
      label: 'MEDIA',
      icon: 'metismenu-icon pe-7s-file',
      devices: ['web'],
      environments: ['development', 'test', 'test-publish', 'production'],
      link: '#',
      permissions: ['r-p-media', 'r-p-media-categories'],
      divider: false,
      childrens: [
        {
          label: 'Categorie',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/media-categories',
          permissions: ['r-p-media-categories'],
        },
        {
          label: 'Files',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/media',
          permissions: ['r-p-media'],
        },
      ],
    },
    {
      label: 'CONTENUTI',
      icon: 'metismenu-icon pe-7s-box1',
      devices: ['web'],
      environments: ['development', 'test', 'test-publish', 'production'],
      link: '#',
      permissions: ['r-p-categories', 'r-p-templates'],
      divider: false,
      childrens: [
        {
          label: 'Categorie',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/categories',
          permissions: ['r-p-categories'],
        },
        {
          label: 'Template e-mail',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/templates',
          permissions: ['r-p-templates'],
        },
        {
          label: 'Documenti legali',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/legals',
          permissions: ['r-p-legals'],
        },
        {
          label: 'Articoli',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/articles',
          permissions: ['r-p-articles'],
        },
      ],
    },
    {
      label: 'UTENTI',
      icon: 'metismenu-icon pe-7s-users',
      devices: ['web'],
      environments: ['development', 'test', 'test-publish', 'production'],
      link: '/users',
      permissions: ['r-p-users'],
      divider: false,
      childrens: [],
    },
    {
      label: 'IMPOSTAZIONI',
      icon: 'metismenu-icon pe-7s-tools',
      devices: ['web'],
      environments: ['development', 'test', 'test-publish', 'production'],
      link: '#',
      permissions: ['isSuperAdmin', 'isDeveloper', 'isAdmin', 'isTranslator'],
      divider: false,
      childrens: [
        {
          label: 'Setup',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/setup',
          permissions: ['isSuperAdmin', 'isDeveloper', 'isAdmin'],
        },
        {
          label: 'Struttura traduzioni',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/translate-structure',
          permissions: ['isSuperAdmin', 'isDeveloper'],
        },
        {
          label: 'Traduzioni',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/translations',
          permissions: [
            'isSuperAdmin',
            'isDeveloper',
            'isMarketing',
            'isTranslator',
          ],
        },
        {
          label: 'Autorizzazioni',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/roles-permissions',
          permissions: ['isSuperAdmin', 'isDeveloper'],
        },
        {
          label: 'Audit',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/user-audit',
          permissions: ['isSuperAdmin', 'isDeveloper', 'isAudit'],
        },
        {
          label: 'Crud',
          icon: '',
          devices: ['web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/general-crud',
          permissions: ['isSuperAdmin', 'isDeveloper'],
        },
      ],
    },
    {
      label: 'GESTIONE TICKET',
      icon: 'metismenu-icon pe-7s-help2',
      devices: ['app', 'web'],
      environments: ['development', 'test', 'test-publish', 'production'],
      link: '#',
      permissions: [
        'isSuperAdmin',
        'isDeveloper',
        'isTicketManagement',
        'isTicketEnabled',
      ],
      divider: false,
      childrens: [
        {
          label: 'Amministrazione Ticket',
          icon: '',
          devices: ['app', 'web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/help-desk-admin',
          permissions: [
            'isSuperAdmin',
            'isDeveloper',
            'isTicketManagement',
            'isTicketEnabled',
          ],
        },
      ],
    },
    {
      label: `SVILUPPATORI`,
      icon: 'metismenu-icon pe-7s-star',
      devices: ['app', 'web'],
      environments: ['development', 'test', 'test-publish', 'production'],
      link: '#',
      permissions: ['isSuperAdmin', 'isDeveloper'],
      divider: false,
      childrens: [
        {
          label: 'Summary',
          icon: '',
          devices: ['app', 'web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/developer-summary',
          permissions: ['isSuperAdmin', 'isDeveloper'],
        },
        {
          label: 'Guida',
          icon: '',
          devices: ['app', 'web'],
          environments: ['development', 'test', 'test-publish', 'production'],
          link: '/developer-guide',
          permissions: ['isSuperAdmin', 'isDeveloper'],
        },
      ],
    },
    {
      label: `GUIDA ALL'USO`,
      icon: 'metismenu-icon pe-7s-notebook',
      devices: ['app', 'web'],
      environments: ['development', 'test', 'test-publish', 'production'],
      link: '/user-guide',
      permissions: [],
      divider: false,
      childrens: [],
    },
  ],
};
