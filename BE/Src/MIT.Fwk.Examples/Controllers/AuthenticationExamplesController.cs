using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIT.Fwk.Core.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Examples.Controllers
{
    /// <summary>
    /// ESEMPI PRATICI: Configurazione Authentication & Authorization
    ///
    /// Questo controller dimostra tutte le opzioni di autenticazione/autorizzazione disponibili:
    ///
    /// 1. [AllowAnonymous] - Pubblico (no auth richiesta)
    /// 2. [SkipJwtAuthentication] - Salta JWT auth per metodi HTTP specifici
    /// 3. [SkipClaimsValidation] - Richiede auth ma non valida claims
    /// 4. [Authorize] - Richiede autenticazione (default)
    /// 5. [Authorize(Policy = "NeededRoleLevelX")] - Richiede livello ruolo specifico
    ///
    /// ORDINE MIDDLEWARE (importante!):
    /// 1. JwtAuthenticationMiddleware - Valida token JWT
    /// 2. JwtClaimsValidationMiddleware - Valida claims entity-level
    /// 3. JwtLoggingMiddleware - Log richieste (fire-and-forget)
    ///
    /// FILE CORRELATI:
    /// - Middleware/JwtAuthenticationMiddleware.cs
    /// - Middleware/JwtClaimsValidationMiddleware.cs
    /// - Core/Attributes/JwtAuthenticationAttributes.cs
    /// - Configurations/MiddlewareExtensions.cs (ordine pipeline)
    /// </summary>
    [Route("api/examples/auth")]
    public class AuthenticationExamplesController : ControllerBase
    {
        public AuthenticationExamplesController()
        {
        }

        // ===== ESEMPIO 1: ENDPOINT PUBBLICO (NO AUTH) =====

        /// <summary>
        /// ESEMPIO 1A: Endpoint pubblico con [AllowAnonymous]
        /// QUANDO USARE: Endpoint accessibili a tutti (login, registrazione, health check)
        /// </summary>
        /// <remarks>
        /// - NO autenticazione richiesta
        /// - NO validazione claims
        /// - Accessibile da chiunque
        ///
        /// Esempi reali nel framework:
        /// - AccountController (login, registrazione, recupero password)
        /// - Health check endpoints
        /// - Public API endpoints
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("public")]
        public IActionResult Example01A_PublicEndpoint()
        {
            return Ok(new
            {
                Message = "Questo endpoint è pubblico - accessibile senza autenticazione",
                IsAuthenticated = User?.Identity?.IsAuthenticated ?? false,
                User = User?.Identity?.Name ?? "Anonymous"
            });
        }

        /// <summary>
        /// ESEMPIO 1B: Endpoint pubblico con [SkipJwtAuthentication]
        /// QUANDO USARE: Saltare autenticazione JWT per tutti i metodi HTTP
        /// </summary>
        /// <remarks>
        /// DIFFERENZA con [AllowAnonymous]:
        /// - [AllowAnonymous] = Standard ASP.NET Core
        /// - [SkipJwtAuthentication] = Custom attribute con più opzioni (metodi HTTP specifici)
        ///
        /// Uso consigliato: [AllowAnonymous] (più standard)
        /// </remarks>
        [SkipJwtAuthentication]
        [HttpGet("public-skip-jwt")]
        public IActionResult Example01B_PublicWithSkipJwt()
        {
            return Ok(new
            {
                Message = "Endpoint pubblico con [SkipJwtAuthentication]",
                IsAuthenticated = User?.Identity?.IsAuthenticated ?? false
            });
        }

        // ===== ESEMPIO 2: SKIP JWT AUTH PER METODI HTTP SPECIFICI =====

        /// <summary>
        /// ESEMPIO 2: Skip JWT solo per GET (lettura pubblica), POST/PUT/DELETE richiedono auth
        /// QUANDO USARE: API read-only pubbliche ma write protette
        /// </summary>
        /// <remarks>
        /// Pattern comune:
        /// - GET = Pubblico (chiunque può leggere)
        /// - POST/PUT/DELETE = Protetto (solo utenti autenticati possono modificare)
        ///
        /// Esempio reale: Blog pubblico ma commenti solo per utenti registrati
        /// </remarks>
        [SkipJwtAuthentication(JwtHttpMethod.GET)]
        [HttpGet("conditional/public-read")]
        public IActionResult Example02_PublicRead()
        {
            return Ok(new
            {
                Message = "GET è pubblico - tutti possono leggere",
                Method = "GET",
                RequiresAuth = false
            });
        }

        /// <summary>
        /// POST richiede autenticazione (perché [SkipJwtAuthentication] vale solo per GET)
        /// </summary>
        [SkipJwtAuthentication(JwtHttpMethod.GET)]
        [HttpPost("conditional/protected-write")]
        public IActionResult Example02_ProtectedWrite()
        {
            // Se arrivi qui, sei autenticato (POST non ha skip)
            return Ok(new
            {
                Message = "POST richiede autenticazione - solo utenti autenticati",
                Method = "POST",
                RequiresAuth = true,
                User = User?.Identity?.Name
            });
        }

        // ===== ESEMPIO 3: SKIP MULTIPLE METODI HTTP =====

        /// <summary>
        /// ESEMPIO 3: Skip JWT per GET e POST (usando OR bit-wise)
        /// QUANDO USARE: Pubblico per più metodi ma non tutti
        /// </summary>
        [SkipJwtAuthentication(JwtHttpMethod.GET | JwtHttpMethod.POST)]
        [HttpGet("multi-method")]
        public IActionResult Example03_MultiMethod_Get()
        {
            return Ok(new
            {
                Message = "GET e POST sono pubblici, PUT/DELETE richiedono auth",
                PublicMethods = "GET, POST",
                ProtectedMethods = "PUT, PATCH, DELETE"
            });
        }

        [SkipJwtAuthentication(JwtHttpMethod.GET | JwtHttpMethod.POST)]
        [HttpPost("multi-method")]
        public IActionResult Example03_MultiMethod_Post()
        {
            return Ok(new { Message = "POST pubblico" });
        }

        [SkipJwtAuthentication(JwtHttpMethod.GET | JwtHttpMethod.POST)]
        [HttpPut("multi-method")]
        public IActionResult Example03_MultiMethod_Put()
        {
            // PUT richiede auth (non è in skip)
            return Ok(new
            {
                Message = "PUT protetto - richiede autenticazione",
                User = User?.Identity?.Name
            });
        }

        // ===== ESEMPIO 4: SKIP CLAIMS VALIDATION =====

        /// <summary>
        /// ESEMPIO 4: Richiede autenticazione ma NON valida claims entity-level
        /// QUANDO USARE: Endpoint che richiedono login ma non permessi specifici
        /// </summary>
        /// <remarks>
        /// DIFFERENZA:
        /// - SENZA [SkipClaimsValidation]: Valida permessi entity-level (es. "canRead:Products")
        /// - CON [SkipClaimsValidation]: Solo controlla che sei loggato, non i permessi
        ///
        /// Esempi reali:
        /// - /health (richiede login ma non permessi specifici)
        /// - /config (idem)
        /// - /profile (accesso al proprio profilo)
        /// </remarks>
        [SkipClaimsValidation]
        [HttpGet("authenticated-no-claims")]
        public IActionResult Example04_AuthenticatedNoClaimsValidation()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            return Ok(new
            {
                Message = "Richiede autenticazione ma non valida claims specifici",
                User = User.Identity.Name,
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        // ===== ESEMPIO 5: ENDPOINT PROTETTO (DEFAULT) =====

        /// <summary>
        /// ESEMPIO 5: Endpoint protetto - richiede autenticazione + validazione claims
        /// QUANDO USARE: Default per la maggior parte degli endpoint
        /// </summary>
        /// <remarks>
        /// Se NON specifichi nessun attributo, il comportamento di default è:
        /// - Richiede JWT token valido
        /// - Valida claims entity-level (se l'entity ha claims configurati)
        /// - Logga la richiesta
        ///
        /// NOTA: Il [Authorize] è opzionale se JWT è abilitato globalmente
        /// </remarks>
        [Authorize]  // Esplicito ma opzionale (già default)
        [HttpGet("protected")]
        public IActionResult Example05_ProtectedEndpoint()
        {
            return Ok(new
            {
                Message = "Endpoint protetto - richiede autenticazione completa",
                User = User.Identity.Name,
                IsAuthenticated = User.Identity.IsAuthenticated,
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        // ===== ESEMPIO 6: ROLE-BASED AUTHORIZATION =====

        /// <summary>
        /// ESEMPIO 6A: Policy-based authorization - Livello 10 (Admin)
        /// QUANDO USARE: Endpoint che richiedono ruoli specifici
        /// </summary>
        /// <remarks>
        /// LIVELLI RUOLO (definiti nel framework):
        /// - NeededRoleLevel0: SuperAdmin
        /// - NeededRoleLevel10: Admin
        /// - NeededRoleLevel50: Standard User
        /// - NeededRoleLevel100: Guest/Limited
        ///
        /// Configurati in: Configurations/IdentityExtensions.cs
        /// </remarks>
        [Authorize(Policy = "NeededRoleLevel10")]
        [HttpGet("admin-only")]
        public IActionResult Example06A_AdminOnly()
        {
            return Ok(new
            {
                Message = "Solo Admin (livello 10 o inferiore)",
                User = User.Identity.Name,
                Roles = User.Claims.Where(c => c.Type.Contains("role"))
                                   .Select(c => c.Value)
            });
        }

        /// <summary>
        /// ESEMPIO 6B: SuperAdmin only (livello 0)
        /// QUANDO USARE: Operazioni critiche, configurazioni di sistema
        /// </summary>
        [Authorize(Policy = "NeededRoleLevel0")]
        [HttpDelete("superadmin-only")]
        public IActionResult Example06B_SuperAdminOnly()
        {
            return Ok(new
            {
                Message = "Solo SuperAdmin può eseguire questa operazione",
                User = User.Identity.Name
            });
        }

        /// <summary>
        /// ESEMPIO 6C: Standard user (livello 50)
        /// QUANDO USARE: Endpoint accessibili a tutti gli utenti autenticati
        /// </summary>
        [Authorize(Policy = "NeededRoleLevel50")]
        [HttpGet("user")]
        public IActionResult Example06C_StandardUser()
        {
            return Ok(new
            {
                Message = "Accessibile a tutti gli utenti autenticati (livello 50+)",
                User = User.Identity.Name
            });
        }

        // ===== ESEMPIO 7: SKIP REQUEST LOGGING =====

        /// <summary>
        /// ESEMPIO 7: Skip request logging (evita loop o sovraccarico)
        /// QUANDO USARE: Controller di logging stesso, health checks frequenti
        /// </summary>
        /// <remarks>
        /// IMPORTANTE: Usare su controller di logging per evitare loop infiniti
        /// (logging che logga se stesso)
        ///
        /// Esempio reale: FwkLogController ha [SkipRequestLogging]
        /// </remarks>
        [SkipRequestLogging]
        [HttpGet("no-logging")]
        public IActionResult Example07_NoLogging()
        {
            return Ok(new
            {
                Message = "Questa richiesta NON viene loggata (fire-and-forget skipped)",
                Logged = false
            });
        }

        // ===== ESEMPIO 8: COMBINAZIONI =====

        /// <summary>
        /// ESEMPIO 8: Combinazione di attributi
        /// QUANDO USARE: Scenari specifici con requisiti complessi
        /// </summary>
        /// <remarks>
        /// Questo endpoint:
        /// - Richiede autenticazione (NO skip JWT)
        /// - NON valida claims entity-level ([SkipClaimsValidation])
        /// - NON viene loggato ([SkipRequestLogging])
        /// - Richiede livello Admin ([Authorize Policy])
        ///
        /// Caso d'uso: Endpoint admin che non deve loggare dati sensibili
        /// </remarks>
        [Authorize(Policy = "NeededRoleLevel10")]
        [SkipClaimsValidation]
        [SkipRequestLogging]
        [HttpPost("complex")]
        public IActionResult Example08_ComplexCombination()
        {
            return Ok(new
            {
                Message = "Combinazione: Auth + Admin + No Claims + No Logging",
                User = User.Identity.Name,
                Configuration = new
                {
                    RequiresAuth = true,
                    RequiresAdmin = true,
                    ValidatesClaims = false,
                    LogsRequest = false
                }
            });
        }

        // ===== ESEMPIO 9: USER CONTEXT =====

        /// <summary>
        /// ESEMPIO 9: Accesso ai dati dell'utente autenticato
        /// QUANDO USARE: Sempre in endpoint protetti per identificare l'utente
        /// </summary>
        [Authorize]
        [HttpGet("user-context")]
        public IActionResult Example09_UserContext()
        {
            return Ok(new
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                Username = User.Identity.Name,
                UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value,
                Email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                TenantId = User.Claims.FirstOrDefault(c => c.Type == "tenantId")?.Value,
                Roles = User.Claims.Where(c => c.Type.Contains("role"))
                                   .Select(c => c.Value)
                                   .ToList(),
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        // ===== ESEMPIO 10: SUMMARY =====

        /// <summary>
        /// ESEMPIO 10: Riepilogo di tutti gli attributi disponibili
        /// </summary>
        [AllowAnonymous]
        [HttpGet("summary")]
        public IActionResult Example10_Summary()
        {
            return Ok(new
            {
                Message = "Riepilogo attributi autenticazione/autorizzazione",
                Attributes = new[]
                {
                    new
                    {
                        Attribute = "[AllowAnonymous]",
                        Scope = "Controller o Action",
                        Description = "Endpoint pubblico - NO autenticazione",
                        Example = "GET /api/examples/auth/public"
                    },
                    new
                    {
                        Attribute = "[SkipJwtAuthentication]",
                        Scope = "Controller, Action o Entity",
                        Description = "Salta JWT auth (opzionale: solo per metodi HTTP specifici)",
                        Example = "[SkipJwtAuthentication(JwtHttpMethod.GET)]"
                    },
                    new
                    {
                        Attribute = "[SkipClaimsValidation]",
                        Scope = "Controller, Action o Entity",
                        Description = "Richiede auth ma non valida claims entity-level",
                        Example = "[SkipClaimsValidation]"
                    },
                    new
                    {
                        Attribute = "[SkipRequestLogging]",
                        Scope = "Controller o Action",
                        Description = "Non logga la richiesta (evita loop, performance)",
                        Example = "[SkipRequestLogging]"
                    },
                    new
                    {
                        Attribute = "[Authorize]",
                        Scope = "Controller o Action",
                        Description = "Richiede autenticazione (default se JWT abilitato)",
                        Example = "[Authorize]"
                    },
                    new
                    {
                        Attribute = "[Authorize(Policy = \"...\")]",
                        Scope = "Controller o Action",
                        Description = "Richiede livello ruolo specifico",
                        Example = "[Authorize(Policy = \"NeededRoleLevel10\")]"
                    }
                },
                RoleLevels = new
                {
                    Level0 = "SuperAdmin (massimo potere)",
                    Level10 = "Admin",
                    Level50 = "Standard User",
                    Level100 = "Guest/Limited"
                },
                JwtHttpMethods = new
                {
                    All = "Tutti i metodi HTTP",
                    GET = "Solo GET",
                    POST = "Solo POST",
                    PUT = "Solo PUT",
                    PATCH = "Solo PATCH",
                    DELETE = "Solo DELETE",
                    Combined = "GET | POST (bitwise OR per combinazioni)"
                }
            });
        }
    }
}
