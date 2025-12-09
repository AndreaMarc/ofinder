using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.Infrastructure.Services;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Extension
{
    public class JwtSignInManager
    {
        protected readonly HttpContext _context;
        protected readonly JwtAuthenticationHeaderValue _authenticationHeaderValue;
        protected readonly UserManager<MITApplicationUser> _userManager;
        protected readonly RoleManager<MITApplicationRole> _roleManager;
        protected readonly SignInManager<MITApplicationUser> _signInManager;
        protected readonly IEmailSender _emailSender;
        protected readonly ILogService _logService;
        protected MITApplicationUser _user;
        protected readonly JsonApiDbContext _myDb;
        protected readonly JsonApiManualService _jsonApiManualService;

        public JwtSignInManager(HttpContext context, JwtAuthenticationHeaderValue authenticationHeaderValue = null)
        {
            _context = context;
            _userManager = _context.RequestServices.GetRequiredService<UserManager<MITApplicationUser>>();
            _roleManager = _context.RequestServices.GetRequiredService<RoleManager<MITApplicationRole>>();
            _signInManager = _context.RequestServices.GetRequiredService<SignInManager<MITApplicationUser>>();

            _authenticationHeaderValue = authenticationHeaderValue;

            _myDb = _context.RequestServices.GetRequiredService<JsonApiDbContext>();
            _emailSender = _context.RequestServices.GetRequiredService<IEmailSender>();
            IConfiguration configuration = _context.RequestServices.GetRequiredService<IConfiguration>();
            _jsonApiManualService = new JsonApiManualService(_myDb, _emailSender, configuration);
            _logService = _context.RequestServices.GetRequiredService<ILogService>();
        }

        public JwtSignInManager(HttpContext context, UserManager<MITApplicationUser> userManager
            , RoleManager<MITApplicationRole> roleManager
            , SignInManager<MITApplicationUser> signInManager
            , JsonApiDbContext myDb
            , IEmailSender emailSender
            , ILogService logService
            , IConfiguration configuration
            , JwtAuthenticationHeaderValue authenticationHeaderValue = null)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;

            _authenticationHeaderValue = authenticationHeaderValue;

            _myDb = myDb;
            _emailSender = emailSender;
            _jsonApiManualService = new JsonApiManualService(_myDb, _emailSender, configuration);
            _logService = logService;
        }

        public async Task<bool> LogToMongo(bool useBus = true)
        {
            LogOptions logOptions = _context.RequestServices.GetRequiredService<IOptions<LogOptions>>().Value;
            useBus = logOptions.LogWithBus;

            try
            {
                MITApplicationUser currentuser = null;
                if (_authenticationHeaderValue.JwtToken != null)
                {
                    JwtSecurityToken jwt = new(_authenticationHeaderValue.JwtToken);
                    string userEmail = jwt.Claims.ToList().Find(x => x.Type == "username").Value;
                    User user = await _jsonApiManualService.GetUserByEmail(userEmail);
                    currentuser = new MITApplicationUser
                    {
                        Id = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                        LastAccess = user.LastAccess,
                        LastIp = user.LastIp,
                    };
                }

                string routePath = _context.Request.Path + _context.Request.QueryString;
                string headers = Newtonsoft.Json.JsonConvert.SerializeObject(_context.Request.Headers.ToList());
                string payload = "";
                Stream bodyContent = new MemoryStream();
                using (StreamReader reader = new(_context.Request.Body, Encoding.UTF8))
                {
                    payload = await reader.ReadToEndAsync();
                }

                if (!payload.StartsWith('['))
                {
                    payload = $"[{payload}]";
                }

                JsonWriterSettings jsonWriterSettings = new() { OutputMode = JsonOutputMode.CanonicalExtendedJson }; // utilizza l'output JSON standard

                // FASE 8A: Always use modern ForMongo() approach (bus-based)
                LogToMongo logToMongo = new()
                {
                    Model = null,
                    CurrentUser = currentuser,
                    RoutePath = routePath,
                    Headers = headers,
                    PayLoad = payload,
                    RequestMethod = _context.Request.Method,
                    LogType = "Middleware"
                };

                _logService.ForMongo(Newtonsoft.Json.JsonConvert.SerializeObject(logToMongo));

                return true;
            }
            catch (Exception ex)
            {
                Exception x = ex;
                return false;
            }
        }


        public async Task<bool> TrySignInUser(string deviceHash, string tenantId)
        {
            if (_authenticationHeaderValue.IsValid)
            {
                JwtSecurityToken jwt = new(_authenticationHeaderValue.JwtToken);
                string userEmail = jwt.Claims.ToList().Find(x => x.Type == "username").Value;
                User user = await _jsonApiManualService.GetUserByEmail(userEmail);
                string userPasswordHash = user.PasswordHash;
                List<UserDevice> devices = await _jsonApiManualService.GetAllUserDevices(user.Id);

                List<Tenant> idSottotenant = await _jsonApiManualService.GetAllChildrenTenants(Convert.ToInt32(tenantId));
                JwtSecurityTokenHandler handler = new();

                TokenValidationParameters validationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    SignatureValidator = delegate (string token, TokenValidationParameters parameters)
                    {


                        JwtSecurityToken jwt = new(token);
                        //controllo che il tenant che mi passa corrisponda a quello del jwt
                        //o che almeno il tenant che mi passa sia un sottotenant di quello nel jwt
                        // TODO - attualmente si ferma solo al primo livello di parentela, sarebbe bello farlo ricorsivo leggendolo da un flag in setup

                        if (jwt.Claims.ToList().FirstOrDefault(x => x.Type == "tenantId") == null || (
                        jwt.Claims.ToList().Find(x => x.Type == "tenantId").Value != tenantId && !idSottotenant.Exists(x => x.Id.ToString() == jwt.Claims.ToList().Find(x => x.Type == "tenantId").Value)))
                        {
                            throw new Exception("Token signature validation failed.");
                        }

                        if (jwt.Claims.ToList().Find(x => x.Type == "password").Value != userPasswordHash)
                        {
                            throw new Exception("Token signature validation failed.");
                        }

                        if (deviceHash == "")
                        {
                            bool validDevice = false;
                            foreach (UserDevice device in devices)
                            {
                                if (device.salt == jwt.Claims.ToList().Find(x => x.Type == "salt").Value)
                                {
                                    validDevice = true;
                                    break;
                                }
                            }

                            if (!validDevice)
                            {
                                throw new Exception("Token signature validation failed.");
                            }
                        }
                        else
                        {
                            UserDevice device = devices.FirstOrDefault(x => x.deviceHash == deviceHash && x.userId == user.Id);
                            if (device == null || device.salt != jwt.Claims.ToList().Find(x => x.Type == "salt").Value)
                            {
                                throw new Exception("Token signature validation failed.");
                            }
                        }

                        //var hmac = new HMACSHA256(Convert.FromBase64String(clientSecret));

                        //var signingCredentials = new SigningCredentials(
                        //   new SymmetricSecurityKey(hmac.Key), SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);

                        //var signKey = signingCredentials.SigningKey as SymmetricSecurityKey;


                        //var encodedData = jwt.EncodedHeader + "." + jwt.EncodedPayload;
                        //var compiledSignature = Encode(encodedData, signKey.Key);

                        ////Validate the incoming jwt signature against the header and payload of the token
                        //if (compiledSignature != jwt.RawSignature)
                        //{
                        //    throw new Exception("Token signature validation failed.");
                        //}

                        //if(jwt.SecurityKey.KeyId != "C59FB46A-005E-43DA-AF15-1900DD3A803A")
                        //    throw new Exception("Token signature validation failed.");

                        return jwt;
                    },
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtTokenProvider.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                    RequireSignedTokens = false
                };

                SecurityToken validToken = null;
                try
                {
                    ClaimsPrincipal principal = handler.ValidateToken(_authenticationHeaderValue.JwtToken, validationParameters, out validToken);

                    bool succeded = (validToken != null && validToken.ValidFrom < DateTime.UtcNow && validToken.ValidTo > DateTime.UtcNow);
                    if (succeded)
                    {
                        _context.User = principal;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logService.Warn($"JwtSignInManager.TrySignInUser: {ex.Message} -> inner ex: {ex.InnerException} -> stack trace: {ex.StackTrace}");
                    return false;
                }
            }
            return false;
        }
        public async Task<bool> TrySignInRefresh(string deviceHash)
        {
            if (_authenticationHeaderValue.IsValid)
            {
                JwtSecurityToken jwt = new(_authenticationHeaderValue.JwtToken);
                string userEmail = jwt.Claims.ToList().Find(x => x.Type == "username").Value;
                User user = await _jsonApiManualService.GetUserByEmail(userEmail);
                string userPasswordHash = user.PasswordHash;
                List<UserDevice> devices = await _jsonApiManualService.GetAllUserDevices(user.Id);

                JwtSecurityTokenHandler handler = new();

                TokenValidationParameters validationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    SignatureValidator = delegate (string token, TokenValidationParameters parameters)
                    {
                        //var clientSecret = "not the real secret";

                        JwtSecurityToken jwt = new(token);

                        if (jwt.Claims.ToList().Find(x => x.Type == "password").Value != userPasswordHash)
                        {
                            throw new Exception("Token signature validation failed.");
                        }

                        if (jwt.Claims.ToList().FirstOrDefault(x => x.Type == "tokenType") == null || jwt.Claims.ToList().FirstOrDefault(x => x.Type == "tokenType").Value != "testToken") //control if is refreshToken, testType is to confuse external reader
                        {
                            throw new Exception("Not correct token.");
                        }

                        if (deviceHash == "")
                        {
                            bool validDevice = false;
                            foreach (UserDevice device in devices)
                            {
                                if (device.salt == jwt.Claims.ToList().Find(x => x.Type == "salt").Value)
                                {
                                    validDevice = true;
                                    break;
                                }
                            }

                            if (!validDevice)
                            {
                                throw new Exception("Token signature validation failed.");
                            }
                        }
                        else
                        {
                            UserDevice device = devices.FirstOrDefault(x => x.deviceHash == deviceHash);
                            if (device == null || device.salt != jwt.Claims.ToList().Find(x => x.Type == "salt").Value)
                            {
                                throw new Exception("Token signature validation failed.");
                            }
                        }

                        //var hmac = new HMACSHA256(Convert.FromBase64String(clientSecret));

                        //var signingCredentials = new SigningCredentials(
                        //   new SymmetricSecurityKey(hmac.Key), SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);

                        //var signKey = signingCredentials.SigningKey as SymmetricSecurityKey;


                        //var encodedData = jwt.EncodedHeader + "." + jwt.EncodedPayload;
                        //var compiledSignature = Encode(encodedData, signKey.Key);

                        ////Validate the incoming jwt signature against the header and payload of the token
                        //if (compiledSignature != jwt.RawSignature)
                        //{
                        //    throw new Exception("Token signature validation failed.");
                        //}

                        //if(jwt.SecurityKey.KeyId != "C59FB46A-005E-43DA-AF15-1900DD3A803A")
                        //    throw new Exception("Token signature validation failed.");

                        return jwt;
                    },
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtTokenProvider.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                    RequireSignedTokens = false
                };

                SecurityToken validToken = null;
                try
                {
                    ClaimsPrincipal principal = handler.ValidateToken(_authenticationHeaderValue.JwtToken, validationParameters, out validToken);

                    bool succeded = (validToken != null && validToken.ValidFrom < DateTime.UtcNow && validToken.ValidTo > DateTime.UtcNow);
                    if (succeded)
                    {
                        _context.User = principal;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logService.Warn($"JwtSignInManager.TrySignInUser: {ex.Message} -> inner ex: {ex.InnerException} -> stack trace: {ex.StackTrace}");
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> CheckClaim(string entityName, string methodType, string tenantId)
        {
            if (_authenticationHeaderValue.IsValid)
            {
                JwtSecurityToken jwt = new(_authenticationHeaderValue.JwtToken);
                string userEmail = jwt.Claims.ToList().Find(x => x.Type == "username").Value;
                switch (methodType)
                {
                    case "GET":
                        entityName += ".read";
                        break;
                    case "POST":
                        entityName += ".create";
                        break;
                    case "PATCH":
                        entityName += ".update";
                        break;
                    case "DELETE":
                        entityName += ".delete";
                        break;
                    default:
                        return false;
                }

                return await _jsonApiManualService.ExistClaim(userEmail, entityName, tenantId);
            }
            return false;
        }

        public Task<List<string>> GetClaimsPool(string tenantId)
        {
            if (_authenticationHeaderValue.IsValid)
            {
                JwtSecurityToken jwt = new(_authenticationHeaderValue.JwtToken);
                string userEmail = jwt.Claims.ToList().Find(x => x.Type == "username").Value;


                return Task.FromResult(_jsonApiManualService.GetClaimsPoolByUsername(userEmail, tenantId));
            }

            return Task.FromResult<List<string>>(null);
        }

        public virtual async Task<string> Login(string username, string password, string passwordHash, int tokenExpiresIn, string salt, bool superadmin, string tenantId)
        {
            if (JwtTokenProvider.Enabled)
            {
                await GetUserByUsernameOrEmail(username);

                if (_user != null)
                {
                    // Generazione JWT tramite Login classica: passa per CheckPasswordSignInAsync(password)
                    // Generazione JWT tramite Refresh token oppure Login di terze parti: passa per la seconda condizione (password scolpita a codice in RefreshTokenController)
                    bool succeded = (password == "559e2f95-ee64-45af-b6c7-30801842496f" && _user.PasswordHash == passwordHash) || await CheckPasswordSignInAsync(password);

                    if (succeded)
                    {
                        _context.User = await _signInManager.CreateUserPrincipalAsync(_user);

                        List<Claim> claims =
                        [
                            new Claim("username", username),
                            new Claim("password", passwordHash),
                            new Claim("salt", salt),
                            new Claim("superadmin", superadmin.ToString()),
                            new Claim("tenantId", tenantId)
                        ];

                        //claims.AddRange(_context.User.Claims);

                        //Generate Token for user 
                        JwtSecurityToken JWToken = new(
                            issuer: JwtTokenProvider.Issuer,
                            claims: claims, //GetUserClaims(_user),
                            notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                            expires: new DateTimeOffset(DateTime.Now.AddMinutes(tokenExpiresIn)).DateTime,
                            //Using HS512 Algorithm to encrypt Token
                            signingCredentials: new SigningCredentials(
                                                new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                                                SecurityAlgorithms.HmacSha256)
                        //SecurityAlgorithms.HmacSha512Signature)
                        );

                        JwtSecurityTokenHandler handler = new();

                        bool isValid = false;

                        //var validationParameters = new TokenValidationParameters
                        //{
                        //    ValidateIssuer = true,
                        //    ValidateAudience = false,
                        //    ValidateLifetime = true,
                        //    ValidateIssuerSigningKey = true,
                        //    ValidIssuer = JwtTokenProvider.Issuer,
                        //    IssuerSigningKey = new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                        //    RequireSignedTokens = false
                        //};

                        TokenValidationParameters validationParameters = new()
                        {
                            ValidateIssuer = true,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            SignatureValidator = delegate (string token, TokenValidationParameters parameters)
                            {
                                return new JwtSecurityToken(token);
                            },
                            ValidIssuer = JwtTokenProvider.Issuer,
                            IssuerSigningKey = new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                            RequireSignedTokens = false
                        };

                        SecurityToken validToken = null;
                        string tk = handler.WriteToken(JWToken);

                        while (!isValid)
                        {
                            try
                            {
                                ClaimsPrincipal principal = handler.ValidateToken(tk, validationParameters, out validToken);

                                isValid = (validToken != null && validToken.ValidFrom < DateTime.UtcNow && validToken.ValidTo > DateTime.UtcNow);

                            }
                            catch (Exception ex)
                            {
                                _logService.Warn($"JwtSignInManager.Login: {ex.Message} -> inner ex: {ex.InnerException} -> stack trace: {ex.StackTrace}");
                            }
                            finally
                            {
                                // try new one
                                if (!isValid)
                                {
                                    tk = handler.WriteToken(JWToken);
                                }
                            }
                        }

                        return tk;
                    }
                }
            }

            return null;
        }
        public virtual async Task<string> LoginRefresh(string username, string password, string passwordHash, int tokenExpiresIn, string salt, bool superadmin, string tenantId)
        {
            if (JwtTokenProvider.Enabled)
            {
                await GetUserByUsernameOrEmail(username);

                if (_user != null)
                {
                    bool succeded = password == "7e4d0d09-6c42-4afd-a537-ea4ee51cc8a6" || await CheckPasswordSignInAsync(password);

                    if (succeded)
                    {
                        _context.User = await _signInManager.CreateUserPrincipalAsync(_user);

                        List<Claim> claims =
                        [
                            new Claim("username", username),
                            new Claim("password", passwordHash),
                            new Claim("salt", salt),
                            new Claim("tokenType", "testToken"),
                            new Claim("superadmin", superadmin.ToString()),
                            new Claim("tenantId", tenantId)
                        ];

                        //claims.AddRange(_context.User.Claims);

                        //Generate Token for user 
                        JwtSecurityToken JWToken = new(
                            issuer: JwtTokenProvider.Issuer,
                            claims: claims, //GetUserClaims(_user),
                            notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                            expires: new DateTimeOffset(DateTime.Now.AddMinutes(tokenExpiresIn)).DateTime,
                            //Using HS512 Algorithm to encrypt Token
                            signingCredentials: new SigningCredentials(
                                                new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                                                SecurityAlgorithms.HmacSha256)
                        //SecurityAlgorithms.HmacSha512Signature)
                        );

                        JwtSecurityTokenHandler handler = new();

                        bool isValid = false;

                        //var validationParameters = new TokenValidationParameters
                        //{
                        //    ValidateIssuer = true,
                        //    ValidateAudience = false,
                        //    ValidateLifetime = true,
                        //    ValidateIssuerSigningKey = true,
                        //    ValidIssuer = JwtTokenProvider.Issuer,
                        //    IssuerSigningKey = new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                        //    RequireSignedTokens = false
                        //};

                        TokenValidationParameters validationParameters = new()
                        {
                            ValidateIssuer = true,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            SignatureValidator = delegate (string token, TokenValidationParameters parameters)
                            {
                                return new JwtSecurityToken(token);
                            },
                            ValidIssuer = JwtTokenProvider.Issuer,
                            IssuerSigningKey = new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                            RequireSignedTokens = false
                        };

                        SecurityToken validToken = null;
                        string tk = handler.WriteToken(JWToken);

                        while (!isValid)
                        {
                            try
                            {
                                ClaimsPrincipal principal = handler.ValidateToken(tk, validationParameters, out validToken);

                                isValid = (validToken != null && validToken.ValidFrom < DateTime.UtcNow && validToken.ValidTo > DateTime.UtcNow);

                            }
                            catch (Exception ex)
                            {
                                _logService.Warn($"JwtSignInManager.Login: {ex.Message} -> inner ex: {ex.InnerException} -> stack trace: {ex.StackTrace}");
                            }
                            finally
                            {
                                // try new one
                                if (!isValid)
                                {
                                    tk = handler.WriteToken(JWToken);
                                }
                            }
                        }

                        return tk;
                    }
                }
            }

            return null;
        }

        public virtual async Task<string> RefreshToken(int minutes, string tenantId = "-1", string userId = "-1")
        {
            if (JwtTokenProvider.Enabled)
            {
                JwtSecurityTokenHandler handler = new();

                TokenValidationParameters validationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    SignatureValidator = delegate (string token, TokenValidationParameters parameters)
                    {
                        return new JwtSecurityToken(token);
                    },
                    ValidIssuer = JwtTokenProvider.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                    RequireSignedTokens = false
                };

                SecurityToken validToken = null;
                try
                {
                    ClaimsPrincipal principal = handler.ValidateToken(_authenticationHeaderValue.JwtToken, validationParameters, out validToken);

                    bool succeded = (validToken != null && validToken.ValidFrom < DateTime.UtcNow && validToken.ValidTo > DateTime.UtcNow);
                    if (succeded)
                    {
                        _context.User = principal;

                        List<Claim> claims = [.. _context.User.Claims];

                        if (tenantId != "-1" && userId != "-1")
                        {
                            await _jsonApiManualService.SetUserCurrentTenant(tenantId, userId);

                            Claim tenantIdClaim = _context.User.Claims.First(x => x.Type == "tenantId");
                            claims.Remove(tenantIdClaim);
                            claims.Add(new Claim("tenantId", tenantId));
                        }

                        //Generate new Token for user 
                        JwtSecurityToken JWToken = new(
                            issuer: JwtTokenProvider.Issuer,
                            claims: claims, //GetUserClaims(_user),
                            notBefore: new DateTimeOffset(DateTime.Now.AddMinutes(-1)).DateTime,
                            expires: new DateTimeOffset(DateTime.Now.AddMinutes(minutes)).DateTime,
                            //Using HS512 Algorithm to encrypt Token
                            signingCredentials: new SigningCredentials(
                                                new SymmetricSecurityKey(JwtTokenProvider.SecretKey),
                                                SecurityAlgorithms.HmacSha256)
                        //SecurityAlgorithms.HmacSha512Signature)
                        );

                        bool isValid = false;
                        string tk = handler.WriteToken(JWToken);

                        while (!isValid)
                        {
                            try
                            {
                                principal = handler.ValidateToken(tk, validationParameters, out validToken);

                                isValid = (validToken != null && validToken.ValidFrom < DateTime.UtcNow && validToken.ValidTo > DateTime.UtcNow);

                                //isValid = ((tk.Length + 1) % 4 == 0);

                            }
                            catch (Exception ex)
                            {
                                _logService.Warn($"JwtSignInManager.RefreshToken: {ex.Message} -> inner ex: {ex.InnerException} -> stack trace: {ex.StackTrace}");
                            }
                            finally
                            {
                                // try new one
                                if (!isValid)
                                {
                                    tk = handler.WriteToken(JWToken);
                                }
                            }
                        }

                        return tk;
                    }
                }
                catch (Exception ex)
                {
                    _logService.Warn($"JwtSignInManager.RefreshToken: {ex.Message} -> inner ex: {ex.InnerException} -> stack trace: {ex.StackTrace}");
                }
            }

            return null;
        }

        public virtual async Task<string> ExternalToken(string username, int days)
        {
            if (JwtTokenProvider.Enabled)
            {
                JwtSecurityTokenHandler handler = new();

                TokenValidationParameters validationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    SignatureValidator = delegate (string token, TokenValidationParameters parameters)
                    {
                        return new JwtSecurityToken(token);
                    },
                    ValidIssuer = JwtTokenProvider.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                    RequireSignedTokens = false
                };

                SecurityToken validToken = null;
                try
                {
                    await GetUserByUsernameOrEmail(username);

                    if (_user != null)
                    {
                        _context.User = await _signInManager.CreateUserPrincipalAsync(_user);

                        //Generate new Token for user 
                        JwtSecurityToken JWToken = new(
                            issuer: JwtTokenProvider.Issuer,
                            claims: _context.User.Claims, //GetUserClaims(_user),
                            notBefore: new DateTimeOffset(DateTime.Now.AddMinutes(-1)).DateTime,
                            expires: new DateTimeOffset(DateTime.Now.AddDays(days)).DateTime,
                            //Using HS512 Algorithm to encrypt Token
                            signingCredentials: new SigningCredentials(
                                                new SymmetricSecurityKey(JwtTokenProvider.SecretKey),
                                                SecurityAlgorithms.HmacSha256)
                        //SecurityAlgorithms.HmacSha512Signature)
                        );

                        bool isValid = false;
                        string tk = handler.WriteToken(JWToken);

                        while (!isValid)
                        {
                            try
                            {
                                ClaimsPrincipal principal = handler.ValidateToken(tk, validationParameters, out validToken);

                                isValid = (validToken != null && validToken.ValidFrom < DateTime.UtcNow && validToken.ValidTo > DateTime.UtcNow);

                                //isValid = ((tk.Length + 1) % 4 == 0);

                            }
                            catch (Exception ex)
                            {
                                _logService.Warn($"JwtSignInManager.ExternalToken: {ex.Message} -> inner ex: {ex.InnerException} -> stack trace: {ex.StackTrace}");
                            }
                            finally
                            {
                                // try new one
                                if (!isValid)
                                {
                                    tk = handler.WriteToken(JWToken);
                                }
                            }
                        }

                        return tk;
                    }
                }
                catch (Exception ex)
                {
                    _logService.Warn($"JwtSignInManager.ExternalToken: {ex.Message} -> inner ex: {ex.InnerException} -> stack trace: {ex.StackTrace}");
                }
            }

            return null;
        }

        //public virtual async Task SignOutAsync()
        //{
        //	var utente = _db.Mc001UserStdExtensions.FirstOrDefault(u => u.Mc001AspNetUserGuid == _user.Id);

        //	utente.Mc001TokenPush = "";

        //	var newUser = _db.Update(utente);

        //	await _signInManager.SignOutAsync();
        //}

        private async Task GetUserByUsernameOrEmail(string userId)
        {
            _user = await _userManager.FindByEmailAsync(userId);

            _user ??= await _userManager.FindByNameAsync(userId);
        }

        private async Task<bool> CheckPasswordSignInAsync(string password)
        {
            //var result = await _signInManager.PasswordSignInAsync(_user.Email, password, false, true);
            //var result = await _signInManager.CheckPasswordSignInAsync(_user, password, true);

            //if (result.Succeeded)
            //{
            return (await _userManager.CheckPasswordAsync(_user, password));
            //}
            //else
            //	return false;
        }

        protected async Task<IList<Claim>> GetUserClaims(MITApplicationUser currentUser)
        {
            IList<Claim> claims = await _userManager.GetClaimsAsync(currentUser);
            foreach (string roleName in await _userManager.GetRolesAsync(currentUser))
            {
                MITApplicationRole role = await _roleManager.FindByNameAsync(roleName.Replace(" ", "_"));

                if (role != null)
                {
                    IList<Claim> roleClaims = await _roleManager.GetClaimsAsync(role);

                    foreach (Claim claim in roleClaims)
                    {
                        if (!claims.Contains(claim))
                        {
                            claims.Add(claim);
                        }
                    }
                }
            }

            return claims;
        }


    }
}
