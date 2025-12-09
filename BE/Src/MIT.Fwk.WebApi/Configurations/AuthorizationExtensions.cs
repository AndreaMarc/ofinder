using Microsoft.Extensions.DependencyInjection;
using MIT.Fwk.WebApi.Extension;

namespace MIT.Fwk.WebApi.Configurations
{
    /// <summary>
    /// Extension methods for configuring authorization policies.
    /// </summary>
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Configures authorization policies for role-level and claims-based access control.
        /// </summary>
        public static IServiceCollection AddFrameworkAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Role-Level Based Policies (0-100 scale)
                // Lower number = higher privilege (0 = SuperAdmin, 100 = lowest privilege)

                // SuperAdmin only
                options.AddPolicy("NeededRoleLevel0", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(0)));

                // Admin level
                options.AddPolicy("NeededRoleLevel10", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(10)));

                // Manager level
                options.AddPolicy("NeededRoleLevel20", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(20)));

                options.AddPolicy("NeededRoleLevel30", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(30)));

                options.AddPolicy("NeededRoleLevel40", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(40)));

                // Standard user level
                options.AddPolicy("NeededRoleLevel50", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(50)));

                options.AddPolicy("NeededRoleLevel60", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(60)));

                options.AddPolicy("NeededRoleLevel70", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(70)));

                options.AddPolicy("NeededRoleLevel80", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(80)));

                options.AddPolicy("NeededRoleLevel90", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(90)));

                // Lowest privilege level
                options.AddPolicy("NeededRoleLevel100", policy =>
                    policy.Requirements.Add(new RoleLevelRequirement(100)));

                // Claims-Based Policies
                options.AddPolicy("isAudit", policy =>
                    policy.Requirements.Add(new ClaimsRequirement("isAudit")));

                // Add additional custom policies here
                // Example:
                // options.AddPolicy("CanManageUsers", policy =>
                //     policy.Requirements.Add(new ClaimsRequirement("CanManageUsers")));
            });

            return services;
        }
    }
}
