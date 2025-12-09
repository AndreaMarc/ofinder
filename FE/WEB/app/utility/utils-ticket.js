/**
 * RESTITUISCE INFO UTILI SUI TICKET
 *
 * richiede i seguenti prametri
 * @param {istance} store: istanza del servizio store
 * @param {istance} stp: istanza del servizio siteSetup
 * @param {istance} v4: istanza di v4
 * @param {int} tenantId: id del tenant di cui controllare la licenza e le impostazioni
 *
 *
 * Restituisce questo oggetto:
 * @return {object}:
 * {
 *    settings: {
 *      @param {bool} ticketActive: indica se in setup il servizio ticket è attivo
 *      @param {string} ticketUnableMessage: messagggio per licenza scaduta
 *      @param {string} ticketMode: "all"/"selected" modalità di funzionamento globale dei ticket
 *      @param {bool} unloggedTicket: indica se, globalmente, sonno ammessi i ticket degli utenti non loggati
 *
 *      @param {bool} addonTicket: indica se l'addon dei Ticket è installato
 *      @param {bool} addonToDo: indica se l'addon dei To-Do è installato
 *      @param {bool} addonProject: indica se l'addon dei Progetti è installato
 *    },
 *    license: {
 *      @param {bool} ticketTenantEnabled: indica se il tenant corrente può gestire ticket (licenza attiva non scaduta o licenza senza scadenza)
 *      @param {bool} unlimitedService: indica se il tenant corrente ha un licenza a tempo indeterminato o a scadenza
 *      @param {date} expirationDate: data di scadenza della licenza
 *
 *      @param {bool} canChoiseUnlogged: indica se il tenant corrente può consentire i ticket ai non loggati
 *      @param {bool} canManageProjects: indica se il tenant corrente può associare i ticket ai Progetti
 *      @param {bool} canUseTag: indica se il tenant corrente può creare i tag per i Ticket
 *      @param {bool} multiArea: indica se il tenant corrente può suddividere gli operatori in settori
 *      @param {bool} canUseToDo: indica se il tenant corrente può creare ToDo associati ai Ticket
 *      @param {bool} canUseLeader: indica se il tenant corrente può indicare i capoArea
 *      @param {bool} canRelateTicket: indica se il tenant corrente può correlare i ticket
 *      @param {bool} canUseRoles: indica se il tenant corrente può gestire i ruoli degli operatori
 *      @param {bool} canUseWorkflow: indica se il tenant corrente può usare tracker e workflow
 *      @param {bool} canUseScopes: indica se il tenant corrente può definire gli ambiti
 *      @param {bool} canUseTracker: indica se il tenant corrente può abilitre i follower
 *      @param {bool} canUseSla: indica se il tenant corrente può usare le SLA (penali per ritardi...)
 *    },
 *    custom: { // attivazioni scelte dal tenant
 *      @param {int} canChoiseUnlogged: indica l'opzione scelta del tenant in merito ai ticket per gli anonimi. 0 = non ammessi. 1: ammessi inserendo la mail. 2 = ammessi con mail e p.iva
 *      @param {bool} canManageProjects: indica l'opzione scelta del tenant in merito alla suddivisione dei ticket in Progetti.
 *      @param {bool} canUseTag: indica l'opzione scelta del tenant in merito all'utilizzo dei tag
 *      @param {bool} multiArea: indica l'opzione scelta del tenant in merito alla divisione degli operatori in reparti (multi-area)
 *      @param {bool} canUseToDo: indica l'opzione scelta del tenant in merito alla possibilià di abilitare i to-do
 *      @param {bool} canUseLeader: indica l'opzione scelta del tenant in merito ai capo-area
 *      @param {bool} canRelateTicket: indica l'opzione scelta del tenant in merito alle correlazioni tra ticket
 *      @param {bool} canUseRoles: indica l'opzione scelta del tenant in merito all'utilizzo dei ruoli
 *      @param {bool} canUseWorkflow: indica l'opzione scelta del tenant in merito all'utilizzo dei workflow (tracker, campi personalizzati)
 *      @param {bool} canUseScopes: indica l'opzione scelta dal tenant corrente per la definizione degli ambiti
 *      @param {bool} canUseTracker: indica l'opzione scelta dal tenant corrente per l'abilitazione dei follower
 *      @param {bool} canUseSla: indica l'opzione scelta del tenant in merito
 *    },
 *    options: { // configurazioni scelte dal tenant
 *      @param {int} areas: indica quanti reparti sono stati definiti dal tenant (se l'opzione multiArea è attiva)
 *      @param {int} roles: indica quanti ruoli sono stati definiti dal tenant (se l'opzione canUseRoles è attiva)
 *      @param {int} operators: indica quanti operatori sono stati definiti dal tenant (se l'opzione multiArea è attiva)
 *      @param {int} pertinences: indica quanti ambiti sono stati definiti dal tenant (se l'opzione multiArea è attiva)
 *      @param {int} tags: indica quanti tag sono stati definiti dal tenant (se l'opzione canUseTag è attiva)
 *      @param {int} leaders: indica quanti leaders sono stati definiti dal tenant
 *      @param {int} scopes: indica quanti ambiti sono stati definiti dal tenant
 *      @param {int} trackers: indica quanti trackers (categorie) sono state definite dal tenant (se l'opzione canUseWorkflow è attiva) *
 *    },
 *    licenseConstraints: { // indica quali sono i legami di attivabilità delle licenze. Ogni elemento dell'oggetto rappresenta un vincolo, in cui la chiave indica la licenza subordinata alla licenza indicata nel valore.
 *      canUseScopes: 'multiArea',
 *      canUseLeader: 'multiArea',
 *      canUseRoles: canUseWorkflow,
 *    }
 * }
 *
 *
 */
export async function ticketUtlity(store, stp, v4, tenantId) {
  let ret = {
    settings: {
      ticketActive: false,
      ticketUnableMessage:
        'Il servizio di Assistenza Clienti è momentaneamente disattivato. Riprovare in un secondo momento. (cod. 3)',
      ticketMode: 'selected',

      addonTicket: false,
      addonToDo: false,
      addonProject: false,
    },
    license: {
      ticketTenantEnabled: false,
      unlimitedService: 'false',
      expirationDate: new Date(Date.now() - 1),
      canChoiseUnlogged: false,
      canManageProjects: false,
      canUseTag: false,
      multiArea: false,
      canUseToDo: false,
      canUseLeader: false,
      canRelateTicket: false,
      canUseRoles: false,
      canUseWorkflow: false,
      canUseSla: false,
      canUseScopes: false,
      canUseTracker: false,
    },
    custom: {
      canChoiseUnlogged: 0,
      canManageProjects: false,
      canUseTag: false,
      multiArea: false,
      canUseToDo: false,
      canUseLeader: false,
      canRelateTicket: false,
      canUseRoles: false,
      canUseWorkflow: false,
      canUseSla: false,
      canUseScopes: false,
      canUseTracker: false,
    },
    options: {
      areas: 0,
      roles: 0,
      operators: 0,
      pertinences: 0,
      tags: 0,
      leaders: 0,
      trackers: 0,
      scopes: 0,
    },
    licenseConstraints: {
      canUseScopes: 'multiArea',
      canUseLeader: 'multiArea',
      canUseRoles: 'canUseWorkflow',
    },
  };

  try {
    // mi ricavo l'elenco degli addons installati
    let addons = await store.findAll('fwk-addon');

    let ticketExists =
      addons.filter((item) => item.addonsCode === 1).length > 0;
    let toDoExists = addons.filter((item) => item.addonsCode === 2).length > 0;
    let projectExists =
      addons.filter((item) => item.addonsCode === 3).length > 0;

    ret.settings.addonTicket = ticketExists;
    ret.settings.addonToDo = toDoExists;
    ret.settings.addonProject = projectExists;

    // estraggo le opzioni dei Ticket da setup
    if (
      stp.siteSetup &&
      stp.siteSetup.ticketService &&
      typeof stp.siteSetup.ticketService === 'object' &&
      ticketExists
    ) {
      ret.settings.ticketActive = !!stp.siteSetup.ticketService.active;

      if (!ret.settings.ticketActive) {
        return ret;
      }

      ret.settings.ticketUnableMessage =
        typeof stp.siteSetup.ticketService.message !== 'undefined' &&
        stp.siteSetup.ticketService.message !== ''
          ? stp.siteSetup.ticketService.message
          : 'Il servizio di Assistenza Clienti è momentaneamente disattivato. Riprovare in un secondo momento. (cod. 4)';

      ret.settings.ticketMode =
        typeof stp.siteSetup.ticketService.mode !== 'undefined'
          ? stp.siteSetup.ticketService.mode
          : 'selected';

      ret.settings.unloggedTicket =
        typeof stp.siteSetup.ticketService.unloggedTicket !== 'undefined'
          ? !!stp.siteSetup.ticketService.unloggedTicket
          : false;

      //
      // ricavo la licenza di utilizzo
      //
      let license = await store.queryRecord('ticket-license', {
        include: 'tenantDestination',
        filter: `equals(tenantDestinationId,'${tenantId}')`,
      });

      if (ret.settings.ticketMode === 'all') {
        ret.license.ticketTenantEnabled = true;
      } else if (ret.settings.ticketMode === 'selected') {
        if (license) {
          // leggo quali features dei ticket è attiva per il Tenant corrente
          ret.license.unlimitedService = license.unlimitedService || false;
          ret.license.expirationDate =
            license.expirationDate || new Date(Date.now() - 1);

          if (!ret.license.unlimitedService) {
            let exp = new Date(ret.license.expirationDate);
            if (exp > Date.now()) {
              ret.license.ticketTenantEnabled = true;
            }
          }

          ret.license.canChoiseUnlogged = !!license.canChoiseUnlogged || false;
          ret.license.canManageProjects = projectExists
            ? !!license.canManageProjects
            : false;
          ret.license.canUseTag = !!license.canUseTag || false;
          ret.license.multiArea = !!license.multiArea || false;
          ret.license.canUseToDo = toDoExists ? !!license.canUseToDo : false;
          ret.license.canUseLeader = !!license.canUseLeader || false;
          ret.license.canRelateTicket = !!license.canRelateTicket || false;
          ret.license.canUseRoles = !!license.canUseRoles || false;
          ret.license.canUseWorkflow = !!license.canUseWorkflow || false;
          ret.license.canUseSla = !!license.canUseSla || false;
          ret.license.canUseScopes = !!license.canUseScopes || false;
        } else {
          return ret;
        }
      }

      //
      // recupero il ticket-custom-setup
      //
      const customSetup = await getTicketCustomSetup(
        store,
        v4,
        tenantId,
        license
      );
      const ticketSetup = { ...customSetup.ticketSetup };

      ret.custom.canChoiseUnlogged = ticketSetup.canChoiseUnlogged || false;
      ret.custom.canManageProjects = projectExists
        ? !!ticketSetup.canManageProjects
        : false;
      ret.custom.canUseTag = !!ticketSetup.canUseTag || false;
      ret.custom.multiArea = !!ticketSetup.multiArea || false;
      ret.custom.canUseToDo = toDoExists ? !!ticketSetup.canUseToDo : false;
      ret.custom.canUseLeader = !!ticketSetup.canUseLeader || false;
      ret.custom.canRelateTicket = !!ticketSetup.canRelateTicket || false;
      ret.custom.canUseRoles = !!ticketSetup.canUseRoles || false;
      ret.custom.canUseWorkflow = !!ticketSetup.canUseWorkflow || false;
      ret.custom.canUseSla = !!ticketSetup.canUseSla || false;
      ret.custom.canUseScopes = !!ticketSetup.canUseScopes || false;

      //
      // Ricavo le scelta del tenant per le options
      //

      // verifico se sono state impostate le aree (reparti)
      let areas = await store.query('area', {
        filter: `equals(tenantDestinationId,'${tenantId}')`,
      });
      ret.options.areas = areas.length;

      // verifico se sono stati impostati i ruoli
      let roles = await store.query('role', {
        filter: `and(equals(tenantId,'${tenantId}'),not(equals(name,'isTicketEnabled')))`,
      });
      ret.options.roles = roles.length;

      // verifico se sono stati definiti gli ambiti
      let assignedPertinences = await store.query('ticket-pertinence-mapping', {
        include: `ticketPertinence`,
        filter: `equals(ticketPertinence.tenantDestinationId,'${tenantId}')`,
      });
      ret.options.scopes = assignedPertinences.length;

      // verifico se sono stati impostati gli operatori
      let users = await store.findAll('user');
      users = users.filter((item) => item.tenantId === tenantId); // solo di questo tenant
      let tenantsOperatorsWithRole = await store.query('user-role', {
        include: 'role',
        filter: `and(equals(role.name,'TicketManagement'),equals(tenantId,'${tenantId}'))`,
      });
      tenantsOperatorsWithRole = tenantsOperatorsWithRole.map((x) => x.userId);
      let tenantOperators = users.filter((item) => {
        return tenantsOperatorsWithRole.includes(item.id);
      });
      ret.options.operators = tenantOperators.length;
    }

    return ret;
  } catch (e) {
    throw new Error(e);
  }
}

async function getTicketCustomSetup(store, v4, tenantId, license) {
  try {
    let record = await store.queryRecord('ticket-custom-setup', {
      filter: `equals(tenantDestinationId,'${tenantId}')`,
    });

    if (record && record.ticketSetup) {
      if (
        typeof record.ticketSetup.canChoiseUnlogged === 'undefined' ||
        typeof record.ticketSetup.canManageProjects === 'undefined' ||
        typeof record.ticketSetup.canUseTag === 'undefined' ||
        typeof record.ticketSetup.multiArea === 'undefined' ||
        typeof record.ticketSetup.canUseLeader === 'undefined' ||
        typeof record.ticketSetup.canUseToDo === 'undefined' ||
        typeof record.ticketSetup.canRelateTicket === 'undefined' ||
        typeof record.ticketSetup.canUseRoles === 'undefined' ||
        typeof record.ticketSetup.canUseWorkflow === 'undefined' ||
        typeof record.ticketSetup.canUseSla === 'undefined' ||
        //
        record.ticketSetup.multiArea !== license.multiArea ||
        record.ticketSetup.canUseRoles !== license.canUseRoles ||
        record.ticketSetup.canUseLeader !== license.canUseLeader ||
        record.ticketSetup.canRelateTicket !== license.canRelateTicket ||
        record.ticketSetup.canUseWorkflow !== license.canUseWorkflow ||
        record.ticketSetup.canUseScopes !== license.canUseScopes ||
        record.ticketSetup.canUseSla !== license.canUseSla
      ) {
        if (typeof record.ticketSetup.canChoiseUnlogged === 'undefined')
          record.ticketSetup.canChoiseUnlogged = 0;
        if (typeof record.ticketSetup.canManageProjects === 'undefined')
          record.ticketSetup.canManageProjects = false;
        if (typeof record.ticketSetup.canUseTag === 'undefined')
          record.ticketSetup.canUseTag = false;
        if (typeof record.ticketSetup.multiArea === 'undefined')
          record.ticketSetup.multiArea = false;
        if (typeof record.ticketSetup.canUseLeader === 'undefined')
          record.ticketSetup.canUseLeader = false;
        if (typeof record.ticketSetup.canUseToDo === 'undefined')
          record.ticketSetup.canUseToDo = false;
        if (typeof record.ticketSetup.canRelateTicket === 'undefined')
          record.ticketSetup.canRelateTicket = false;
        if (typeof record.ticketSetup.canUseRoles === 'undefined')
          record.ticketSetup.canUseRoles = false;
        if (typeof record.ticketSetup.canUseWorkflow === 'undefined')
          record.ticketSetup.canUseWorkflow = false;
        if (typeof record.ticketSetup.canUseScopes === 'undefined')
          record.ticketSetup.canUseScopes = false;
        if (typeof record.ticketSetup.canUseSla === 'undefined')
          record.ticketSetup.canUseSla = false;

        // imposto il valore dei parametri non modificabili (che cioè dipendono dalla licenza e non dalla scelta dell'utente)
        record.ticketSetup.multiArea =
          license && license.multiArea ? license.multiArea : false;
        record.ticketSetup.canUseRoles =
          license && license.canUseRoles ? license.canUseRoles : false;
        record.ticketSetup.canUseLeader =
          license && license.canUseLeader ? license.canUseLeader : false;
        record.ticketSetup.canRelateTicket =
          license && license.canRelateTicket ? license.canRelateTicket : false;
        record.ticketSetup.canUseWorkflow =
          license && license.canUseWorkflow ? license.canUseWorkflow : false;
        record.ticketSetup.canUseScopes =
          license && license.canUseScopes ? license.canUseScopes : false;
        record.ticketSetup.canUseSla =
          license && license.canUseSla ? license.canUseSla : false;

        await record.save(); // così il db è allineato a tutte le features esistenti!
      }

      return record;
    } else {
      throw new Error();
    }
  } catch (e) {
    return createRecord(store, v4, tenantId);
  }
}

async function createRecord(store, v4, tenantId) {
  try {
    let nr = await store.createRecord('ticket-custom-setup', {
      id: v4(),
      tenantDestinationId: tenantId,
      ticketSetup: {
        canChoiseUnlogged: 0,
        canManageProjects: false,
        canUseTag: false,
        multiArea: false,
        canUseLeader: false,
        canUseToDo: false,
        canRelateTicket: false,
        canUseRoles: false,
        canUseWorkflow: false,
        canUseScopes: false,
        canUseSla: false,
      },
    });
    await nr.save();
    return nr;
  } catch (e) {
    console.error(e, 'Errore creazione del record di Ticket Custom Setup');
    return {
      canChoiseUnlogged: 0,
      canManageProjects: false,
      canUseTag: false,
      multiArea: false,
      canUseLeader: false,
      canUseToDo: false,
      canRelateTicket: false,
      canUseRoles: false,
      canUseWorkflow: false,
      canUseScopes: false,
      canUseSla: false,
    };
  }
}
