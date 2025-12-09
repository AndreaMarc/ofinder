using AutoMapper;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.Infrastructure.Services;
using MIT.Fwk.WebApi.Extension;
using MIT.Fwk.WebApi.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MIT.Fwk.WebApi.Configurations
{
    /// <summary>
    /// Extension methods for configuring ASP.NET Core Identity, Authentication, and related services.
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Configures ASP.NET Core Identity with custom user and role managers.
        /// </summary>
        public static IServiceCollection AddFrameworkIdentity(
            this IServiceCollection services,
            IConfiguration configuration,
            JsonApiDbContext dbContext)
        {
            // Configure Identity options from database
            int loginAttempts = 5;
            int lockedTimeInMinutes = 5;

            try
            {
                var setup = dbContext.Setups.FirstOrDefault(x => x.environment == "web");
                if (setup != null)
                {
                    loginAttempts = setup.failedLoginAttempts;
                    lockedTimeInMinutes = setup.blockingPeriodDuration;
                }
            }
            catch
            {
                // Use defaults if database query fails
            }

            // Add Identity with custom user manager
            // NOTE: Uses MITApplicationDbContext (not JsonApiDbContext) for Identity entities
            services.AddIdentity<MITApplicationUser, MITApplicationRole>()
                .AddEntityFrameworkStores<MITApplicationDbContext>()
                .AddUserManager<MITUserManager>()
                .AddDefaultTokenProviders();

            // Configure Identity options
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings (very permissive - should be configured per deployment)
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredLength = 32;

                // Lockout settings
                options.Lockout.MaxFailedAccessAttempts = loginAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockedTimeInMinutes);

                // Token provider settings - .NET 10 compatibility fix
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
            });

            // Configure DataProtectorTokenProvider with dynamic lifespan from database
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                // Retrieve token expiration from database setup
                int tokenExpiresInMinutes = 60; // Default fallback
                try
                {
                    var setup = dbContext.Setups.FirstOrDefault(x => x.environment == "web");
                    if (setup != null)
                    {
                        tokenExpiresInMinutes = setup.mailTokenExpiresIn;
                    }
                }
                catch
                {
                    // Use default if database query fails
                }

                options.TokenLifespan = TimeSpan.FromMinutes(tokenExpiresInMinutes);
            });

            // Add custom SignInManager
            services.AddScoped<SignInManager<MITApplicationUser>, AuditableSignInManager<MITApplicationUser>>();

            // Add authorization handlers
            services.AddScoped<IAuthorizationHandler, RoleLevelHandler>();
            services.AddScoped<IAuthorizationHandler, ClaimsHandler>();

            return services;
        }

        /// <summary>
        /// Configures JWT Bearer authentication.
        /// </summary>
        public static IServiceCollection AddFrameworkAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(auth =>
                {
                    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(token =>
                {
                    token.RequireHttpsMetadata = false;
                    token.SaveToken = true;
                    token.TokenValidationParameters = JwtTokenProvider.GetValidationParameters();
                });

            return services;
        }

        /// <summary>
        /// Configures AutoMapper with automatic profile discovery.
        /// </summary>
        public static IServiceCollection AddFrameworkAutoMapper(this IServiceCollection services)
        {
            MapperConfiguration mapperConfig = new(cfg =>
            {
                // Scan for AutoMapper profiles in all loaded assemblies
                cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
            }, new LoggerFactory());

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            return services;
        }

        /// <summary>
        /// Configures session state.
        /// </summary>
        public static IServiceCollection AddFrameworkSession(this IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
            });

            return services;
        }

        /// <summary>
        /// Configures MediatR for CQRS and domain events.
        /// </summary>
        public static IServiceCollection AddFrameworkMediatR(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            return services;
        }

        /// <summary>
        /// Registers framework-specific application services.
        /// </summary>
        public static IServiceCollection AddFrameworkServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Firebase App singleton (required for NotificationService)
            services.AddSingleton(sp =>
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    var firebaseConfig = configuration.GetSection("Firebase").Get<Dictionary<string, object>>();
                    if (firebaseConfig != null)
                    {
                        // Use CredentialFactory pattern (non-obsolete method for .NET 10)
                        string jsonString = JsonConvert.SerializeObject(firebaseConfig);

                        // Create ServiceAccountCredential using CredentialFactory
                        var serviceAccountCredential = CredentialFactory.FromJson<ServiceAccountCredential>(jsonString);

                        // Convert to GoogleCredential and add scopes
                        var credential = serviceAccountCredential
                            .ToGoogleCredential()
                            .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

                        return FirebaseApp.Create(new AppOptions { Credential = credential });
                    }
                }
                return FirebaseApp.DefaultInstance;
            });

            // Core services
            services.AddScoped<IJsonApiManualService, JsonApiManualService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IGoogleService, GoogleService>();
            services.AddScoped<IGoogleMapsService, GoogleMapsService>();
            services.AddScoped<IFwkLogService, FwkLogService>();

            // NOTE: DatabaseMigrationService is now registered in DatabaseExtensions.AddFrameworkDatabases
            // to ensure it's available before migrations are applied during startup

            return services;
        }

        /// <summary>
        /// Configures HTTPS redirection if enabled in configuration.
        /// Note: HTTPS redirection is configured via Kestrel, not here.
        /// This method is kept for consistency but does nothing.
        /// </summary>
        public static IServiceCollection AddFrameworkHttps(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // HTTPS redirection is handled by middleware in MiddlewareExtensions
            // No service registration needed here
            return services;
        }

        /// <summary>
        /// Configures CORS policy.
        /// </summary>
        public static IServiceCollection AddFrameworkCors(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddCors(o =>
            {
                o.AddPolicy("AllowSpecificOrigin", options =>
                {
                    string allowedOrigin = configuration["AllowedCorsOrigin"] ?? "*";

                    if (allowedOrigin != "*")
                    {
                        options.WithOrigins(allowedOrigin);
                    }
                    else
                    {
                        options.AllowAnyOrigin();
                    }

                    options.AllowAnyHeader();
                    options.AllowAnyMethod();
                });
            });

            return services;
        }

        /// <summary>
        /// Registers JWT middleware services for attribute-based authentication, claims validation, and logging.
        /// FASE 2-3 refactoring: Replaces monolithic JwtAuthentication.cs with separate concerns.
        /// </summary>
        /// <remarks>
        /// Services registered:
        /// - IJwtAuthenticationService: Token validation and user sign-in
        /// - IJwtClaimsService: Entity-level authorization with granular claims (e.g., "Invoice.Read")
        /// - IRequestLoggingService: HTTP request logging to MongoDB (fire-and-forget, non-blocking)
        ///
        /// These services support the new JWT middleware pipeline:
        /// - JwtAuthenticationMiddleware
        /// - JwtClaimsValidationMiddleware
        /// - JwtLoggingMiddleware
        /// </remarks>
        public static IServiceCollection AddJwtServices(this IServiceCollection services)
        {
            services.AddScoped<IJwtAuthenticationService, JwtAuthenticationService>();
            services.AddScoped<IJwtClaimsService, JwtClaimsService>();
            services.AddScoped<IRequestLoggingService, RequestLoggingService>();

            return services;
        }
    }
}
