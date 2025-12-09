# Lista Completa Commit - MIT.FWK v8.0 Refactoring

**Branch**: `refactor/fork-template`
**Totale Commit**: 116 commit (esclusi merge)
**Periodo**: 18 Giugno 2025 - 30 Ottobre 2025
**Autore Principale**: Emanuele Morganti

---

## Formato

Ogni riga contiene:
- **Hash**: Commit hash (primi 7 caratteri)
- **Data**: Data e ora commit (ISO 8601)
- **Messaggio**: Messaggio commit
- **Gruppo**: Categoria tematica assegnata

---

## Commit List (dal più recente al più vecchio)

| # | Hash | Data | Messaggio | Gruppo |
|---|------|------|-----------|--------|
| 1 | 47ee68e | 2025-10-30 14:12 | . | Misc |
| 2 | a5bdc45 | 2025-10-30 12:42 | Code Generator: Rilevamento duplicati + Fix summary position | Code Generator |
| 3 | 67d3c87 | 2025-10-30 12:36 | Fix grafiche | Misc |
| 4 | dfadffc | 2025-10-30 12:24 | Code Generator: Gestione conflitti naming + XML summary | Code Generator |
| 5 | a632d9f | 2025-10-30 12:11 | Code Generator: Implementato override Id per chiavi primarie custom | Code Generator |
| 6 | 5ce8dd6 | 2025-10-30 12:00 | Code Generator: Fix relazioni duplicate e self-references | Code Generator |
| 7 | f5546d8 | 2025-10-30 11:46 | Code Generator: Sanitizzazione nomi tabelle/colonne con caratteri speciali | Code Generator |
| 8 | 7ad0bf6 | 2025-10-30 11:35 | Aggiorna versione e rimuove logica di backup file | Misc |
| 9 | f273b57 | 2025-10-30 11:26 | Fix jsonapi | Misc |
| 10 | 0e9332a | 2025-10-30 11:07 | Aggiunti controlli per bypass middleware con attributi | JWT Refactoring |
| 11 | 5e42e0f | 2025-10-30 10:28 | Migliorie | Misc |
| 12 | e975c51 | 2025-10-30 09:58 | Code Generator: Aggiunto supporto appsettings.json e generazione test automatici | Code Generator |
| 13 | f0c322c | 2025-10-30 09:35 | Aggiunto code generator al posto di DBFactory | Code Generator |
| 14 | 20c0a21 | 2025-10-29 17:44 | Rimozione configurazioni legacy e pulizia del codice | Misc |
| 15 | e94326e | 2025-10-29 17:41 | Fix service lifetime mismatch in WebSocketNotificationService | Misc |
| 16 | 4e72e36 | 2025-10-29 17:21 | Refactoring framework per modernizzazione v9.0 | Core Modernization |
| 17 | a4efea7 | 2025-10-29 17:19 | Refactoring e miglioramenti gestione notifiche | Misc |
| 18 | 0bc9e07 | 2025-10-29 15:38 | Rimossi attributi deprecati | Misc |
| 19 | 14b5e05 | 2025-10-29 15:29 | UnitTest migliorati e funzionanti | Testing |
| 20 | 0615461 | 2025-10-29 12:59 | Avanzamento test | Testing |
| 21 | cfc341e | 2025-10-28 18:35 | AGGIORNAMENTO JWT-REFACTORING-PLAN.md: 87.5% completato (7/8 fasi) | JWT Refactoring |
| 22 | 715709d | 2025-10-28 18:35 | FASE 4+7 COMPLETATE: Cleanup config legacy + rimozione JwtAuthentication.cs | JWT Refactoring |
| 23 | bb35c99 | 2025-10-28 18:25 | DOC: Aggiornato JWT-REFACTORING-PLAN.md - FASE 6 completata (62.5% totale) | JWT Refactoring |
| 24 | a19074c | 2025-10-28 18:24 | FASE 6 COMPLETATA: Attribute-Based JWT Middleware Migration | JWT Refactoring |
| 25 | 09549a0 | 2025-10-28 17:21 | REFACTORING COMPLETO: Auto-Discovery per servizi custom + OtherManualService | Startup Modernization |
| 26 | ac008d9 | 2025-10-28 16:01 | Rimossi warnings | Misc |
| 27 | 5a9b64f | 2025-10-28 15:54 | Avanzamento refactoring | Misc |
| 28 | 2b1eb70 | 2025-10-28 03:51 | FIX: Configurazione primary key per UploadFile | Controller Refactoring |
| 29 | 2290f76 | 2025-10-28 03:49 | FIX: Registrazione IEmailService, IEncryptionService, ILogService | Core Modernization |
| 30 | c5ffeef | 2025-10-28 03:42 | FIX: Passare IConfiguration a AddFrameworkControllers | Startup Modernization |
| 31 | 48c31d1 | 2025-10-28 03:34 | REFACTORING STARTUP.CS: Migrazione completa a pattern moderni + OpenAPI 3.0 | Startup Modernization |
| 32 | dd7bcb5 | 2025-10-28 03:21 | REFACTORING PROGRAM.CS: Modernizzazione con WebApplication.CreateBuilder e pattern moderni | Startup Modernization |
| 33 | be4a331 | 2025-10-28 03:07 | FASE 10 COMPLETATA: Eliminazione progetto Domain + cleanup aggressivo Core | CQRS Cleanup |
| 34 | 02f039d | 2025-10-28 02:56 | FASE 9C COMPLETATA: Eliminazione eventi Document e FwkLog non utilizzati | CQRS Cleanup |
| 35 | 2a351d6 | 2025-10-28 02:54 | FASE 9B COMPLETATA: Eliminazione completa CQRS legacy (comandi, eventi, validazioni) | CQRS Cleanup |
| 36 | ed17639 | 2025-10-28 02:49 | FASE 9 COMPLETATA: Eliminazione completa classi obsolete | Core Modernization |
| 37 | 8108627 | 2025-10-28 02:38 | FASE 8C-8E COMPLETATE: Cleanup finale obsoleti + build pulita 0 errori | Core Modernization |
| 38 | e634107 | 2025-10-28 02:27 | FASE 8B COMPLETATA: License → ILicenseService con Dependency Injection | Core Modernization |
| 39 | cce7812 | 2025-10-28 02:24 | FASE 8A COMPLETATA (PARTE 1): Eliminazione metodi e interfacce obsoleti | Core Modernization |
| 40 | 3f98948 | 2025-10-27 17:57 | FASE 7 COMPLETATA: ConfigurationHelper eliminato - Migrazione a IConfiguration/IOptions | Core Modernization |
| 41 | 2ce0889 | 2025-10-27 16:51 | FASE 6 COMPLETATA: Pulito DatabaseTypes.cs - Rimossi DBParameters e FilterCondition | Infrastructure Cleanup |
| 42 | aeebe6c | 2025-10-27 16:49 | FASE 5 COMPLETATA: Eliminato LogHelper.cs - Migrazione a ILogService | Core Modernization |
| 43 | 7dbb97c | 2025-10-27 16:38 | FASE 4 COMPLETATA: Eliminato Repository Pattern - Migrazione a Factory Pattern | Infrastructure Cleanup |
| 44 | 0eabb42 | 2025-10-27 15:11 | FASE 3 COMPLETATA: Eliminati CQRS Commands obsoleti | CQRS Cleanup |
| 45 | 3076e05 | 2025-10-27 15:05 | FASE 2 COMPLETATA: Eliminato EncryptionHelper.cs - migrazione a IEncryptionService | Core Modernization |
| 46 | df69d7b | 2025-10-27 14:59 | FASE 1 COMPLETATA: Eliminato MailHelper.cs | Core Modernization |
| 47 | b23e9eb | 2025-10-27 14:56 | FASE 0 COMPLETATA: Eliminati file .old di backup | Misc |
| 48 | eb8eba1 | 2025-10-27 14:34 | Eliminato ApiController | Controller Refactoring |
| 49 | 1b032e0 | 2025-10-27 13:43 | Avanzamento refactoring | Misc |
| 50 | 74f0dcb | 2025-10-26 23:53 | Refactoring scheduler e license | Misc |
| 51 | dfa3813 | 2025-10-26 17:27 | 🎉 INFRASTRUCTURE REFACTORING COMPLETATO - Build Pulita 0 Errori 0 Avvisi | Infrastructure Cleanup |
| 52 | 8a3b064 | 2025-10-26 17:23 | FASE 10 COMPLETATA: Legacy CQRS commands e validazioni marcati come obsoleti | CQRS Cleanup |
| 53 | 596c13e | 2025-10-26 17:18 | FASE 9 COMPLETATA: DocumentService e FwkLogService refactorati - eliminata dipendenza comandi CQRS | CQRS Cleanup |
| 54 | 9ac35ca | 2025-10-26 14:07 | FASE 7-8 COMPLETATE: UnitOfWork e DTO legacy marcati [Obsolete] | Infrastructure Cleanup |
| 55 | 9154dc4 | 2025-10-26 13:59 | FASE 6 COMPLETATA: IEntity warnings soppressi in DomainCommandHandler + IAuditEntity [Obsolete] | Infrastructure Cleanup |
| 56 | e5b1260 | 2025-10-26 13:57 | FASE 5 COMPLETATA: DatabaseInformations marcato [Obsolete] con guida EF Core Metadata API | Infrastructure Cleanup |
| 57 | c5e6334 | 2025-10-26 13:52 | FASE 4 COMPLETATA: LogHelper → ILogger<T> | Core Modernization |
| 58 | 51d9bcc | 2025-10-26 13:46 | FASE 3.2 COMPLETATA: ConfigurationHelper → IConfiguration/IConnectionStringProvider | Core Modernization |
| 59 | cabe864 | 2025-10-26 13:33 | FASE 1-3.1: Infrastructure refactoring - Handlers, DbContext, IConnectionStringProvider | Infrastructure Cleanup |
| 60 | 6267c5e | 2025-10-26 13:00 | Refactoring completo Domain layer v9.0 - Eliminati pattern legacy e modernizzato CQRS | CQRS Cleanup |
| 61 | 424763f | 2025-10-25 16:56 | FASE 5 COMPLETATA: Build refactoring Core v8→v9 - 0 errori, pattern moderni | Core Modernization |
| 62 | e5f8e1c | 2025-10-25 16:38 | FASE 4 COMPLETATA: Deprecazione Repository Pattern e modernizzazione EncryptionHelper | Infrastructure Cleanup |
| 63 | 50e6e4c | 2025-10-25 16:34 | FASE 3 COMPLETATA: Eliminazione BaseEntity, IEntity e ValueObject | Core Modernization |
| 64 | 710f395 | 2025-10-25 16:29 | FASE 2 COMPLETATA: Modernizzazione Mapper, LogHelper e MailHelper | Core Modernization |
| 65 | 955772a | 2025-10-25 16:25 | FASE 1 COMPLETATA: Modernizzazione Configuration & Cache | Core Modernization |
| 66 | 99380c3 | 2025-10-25 12:26 | Refactoring struttura del progetto Core | Core Modernization |
| 67 | 0a36e5d | 2025-10-25 11:59 | FASE 9 COMPLETATA: Documentazione completa v8.0 - Guida moderna e migrazione | Misc |
| 68 | a507a28 | 2025-10-25 11:55 | Eliminazione warning residui | Misc |
| 69 | a0130dc | 2025-10-25 11:53 | FASE 8.5 COMPLETATA: Eliminati BaseAuthController e BaseController - tutti i controller estendono ApiController | Controller Refactoring |
| 70 | f96c820 | 2025-10-25 11:42 | FASE 8 COMPLETATA: Code Cleanup - Ottimizzazione performance e rimozione codice obsoleto | Misc |
| 71 | 59cc528 | 2025-10-25 11:34 | FASE 5 COMPLETATA: Eliminazione completa layer legacy - 23 file rimossi (~10,000+ righe codice) | Infrastructure Cleanup |
| 72 | 83b962a | 2025-10-25 01:05 | Aggiornata roadmap: FASE 4 completata con successo | CQRS Cleanup |
| 73 | 933ec9b | 2025-10-25 01:05 | FASE 4 COMPLETATA: Refactoring CQRS - DomainCommandHandler usa DbContext invece di Repository | CQRS Cleanup |
| 74 | 815075b | 2025-10-25 00:47 | Aggiornata roadmap: FASE 3 completata | Controller Refactoring |
| 75 | a077f00 | 2025-10-25 00:47 | FASE 3 COMPLETATA: Tutti i controller migrati - IAppService/IAppServiceV2 eliminati | Controller Refactoring |
| 76 | 79b10c1 | 2025-10-25 00:43 | FASE 3.3: UploadFileController refactorato - eliminato IAppService/IRepository | Controller Refactoring |
| 77 | d43ef9a | 2025-10-25 00:40 | FASE 1-2 completate: Audit e Setup per refactoring SqlManager/AppService | Infrastructure Cleanup |
| 78 | 66d8812 | 2025-10-24 23:45 | Aggiornato il progetto di esempio con un controller esplicito | Misc |
| 79 | 87f37c5 | 2025-10-24 23:39 | Riallineati tutti i namespace | Misc |
| 80 | dae33f8 | 2025-10-24 18:46 | Eseguita pulizia del codice tramite tool di VS | Misc |
| 81 | 6927d9d | 2025-10-24 18:38 | Eliminati tutti i warning e soppressi alcuni messaggi | Misc |
| 82 | b4290c6 | 2025-10-24 17:03 | Risolti altri warning | Misc |
| 83 | d38f9cb | 2025-10-24 16:34 | Rimossi alcuni warning | Misc |
| 84 | 77af225 | 2025-10-23 15:21 | Eseguita pulizia del codice | Misc |
| 85 | c20e1ad | 2025-10-23 15:11 | Refactoring strutturale e passaggio a .net 9 | Architecture Migration |
| 86 | 2a9031d | 2025-10-22 17:48 | Supporto migrazioni per IJsonApiDbContext | Architecture Migration |
| 87 | fe9e125 | 2025-10-22 16:04 | Passaggio a un'architettura basata su fork | Architecture Migration |
| 88 | 78d8a3b | 2025-10-22 14:33 | Aggiorna .gitignore per ignorare nuovi percorsi | Architecture Migration |
| 89 | 6a2b5ad | 2025-10-22 14:16 | Automatizza e documenta le migrazioni del database | Architecture Migration |
| 90 | f59c40a | 2025-10-08 17:24 | Refactor log handling and simplify filtering logic | Misc |
| 91 | 2ed87e1 | 2025-10-08 12:35 | Refactor and enhance MongoDB log querying logic | Misc |
| 92 | e0c8bbb | 2025-10-07 16:39 | Refactor logging and MongoDB processing logic | Misc |
| 93 | 1bc26db | 2025-10-07 16:06 | Improve file processing and logging in MongoLogBusManager | Misc |
| 94 | 8e8e8b0 | 2025-09-22 16:56 | Add SSL configuration to MailHelper and optimize file reading | Misc |
| 95 | 2674e84 | 2025-09-09 14:55 | Rename columns in UserPreference table | Misc |
| 96 | 8dbd4ed | 2025-09-09 14:48 | Fere pubblica su framework | Misc |
| 97 | 436c762 | 2025-09-04 12:49 | Add UserPreference entity and migration | Misc |
| 98 | 32177f1 | 2025-09-04 12:35 | aggiunta userpreference | Misc |
| 99 | 2493045 | 2025-07-21 13:08 | Update FWK_VERSION to 8.0 | Misc |
| 100 | afdd0ea | 2025-07-21 11:40 | Add staging environment configurations to CI/CD | Misc |
| 101 | fb38665 | 2025-07-18 16:04 | Update .gitlab-ci.yml Removed docker build of winx64 version | Misc |
| 102 | 0cc7238 | 2025-07-18 15:47 | Update file Dockerfile | Misc |
| 103 | a135586 | 2025-07-18 15:42 | Update Dockerfile | Misc |
| 104 | e442ba1 | 2025-07-18 17:22 | Optimize SaveChanges calls and update tests | Testing |
| 105 | a4d6ec4 | 2025-07-18 16:57 | Add migrations for role updates and new employee tables | Misc |
| 106 | 9570ee6 | 2025-07-18 16:33 | Update Microsoft.NET.Test.Sdk to version 17.14.1 | Testing |
| 107 | bbb7d4d | 2025-06-25 14:45 | Comment out AspNetUserAudit configuration | Misc |
| 108 | e50151d | 2025-06-25 14:45 | Disable user audit logging in AuditableSignInManager | Misc |
| 109 | 03b4d58 | 2025-06-25 14:30 | Remove user auditing functionality from the codebase | Misc |
| 110 | 959f345 | 2025-06-25 14:03 | Downgrade SkiaSharp packages to resolve compatibility | Misc |
| 111 | 8202759 | 2025-06-25 12:17 | Update SkiaSharp.NativeAssets package version | Misc |
| 112 | 837cd0d | 2025-06-23 12:27 | fix script post build | Misc |
| 113 | de404d9 | 2025-06-23 09:23 | Update OAuth parameter names and enhance error handling | Misc |
| 114 | f5f6bf3 | 2025-06-20 12:24 | Enhance Google authentication with new parameters | Misc |
| 115 | e705917 | 2025-06-19 12:05 | Refactor Google event methods to use ThirdPartsToken | Misc |
| 116 | 9486532 | 2025-06-19 08:47 | Add Google login endpoint to AccountController | Misc |

---

## Statistiche per Gruppo

| Gruppo | Commit Count | Percentuale |
|--------|--------------|-------------|
| Misc | 39 | 33.6% |
| Core Modernization | 18 | 15.5% |
| Infrastructure Cleanup | 12 | 10.3% |
| CQRS Cleanup | 9 | 7.8% |
| Controller Refactoring | 7 | 6.0% |
| Code Generator | 7 | 6.0% |
| JWT Refactoring | 6 | 5.2% |
| Architecture Migration | 5 | 4.3% |
| Startup Modernization | 5 | 4.3% |
| Testing | 4 | 3.4% |
| **TOTALE** | **116** | **100%** |

---

## Note

- I commit sono ordinati dal più recente (top) al più vecchio (bottom)
- Il gruppo "Misc" include: fix warning, documentazione, CI/CD, MongoDB logging, Google OAuth, versioning
- Alcuni commit potrebbero appartenere a più categorie, ma è stata scelta la categoria predominante
- Hash abbreviato a 7 caratteri per leggibilità (standard git)
- Data e ora in formato locale (UTC+0100)

---

**Generato**: 30 Ottobre 2025
**Fonte**: `git log refactor/fork-template --no-merges`
