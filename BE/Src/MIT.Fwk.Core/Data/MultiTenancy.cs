using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace MIT.Fwk.Core.Data
{
    /// <summary>
    /// Interface for entities that support multi-tenancy.
    /// Entities implementing this interface will be automatically filtered by tenant context.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public interface IHasTenant
    {
        int TenantId { get; set; }
    }

    /// <summary>
    /// Provides the current tenant context.
    /// Injected as scoped service to maintain tenant isolation per request.
    /// </summary>
    public interface ITenantProvider
    {
        int TenantId { get; }
    }

    /// <summary>
    /// Default implementation of tenant provider.
    /// Currently returns hardcoded tenant ID = 1.
    /// Override this class to implement custom tenant resolution logic.
    /// </summary>
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int TenantId
        {
            get
            {
                return 1;
                // Example: Obtain tenant ID from a custom HTTP header
                //if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-Tenant-ID", out Microsoft.Extensions.Primitives.StringValues tenantIdHeader))
                //{
                //    if (int.TryParse(tenantIdHeader, out int tenantId))
                //    {
                //        return tenantId;
                //    }
                //}

                // Example: Obtain tenant ID from a query string parameter
                //if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("tenantId", out Microsoft.Extensions.Primitives.StringValues tenantIdQuery))
                //{
                //    if (int.TryParse(tenantIdQuery, out int tenantId))
                //    {
                //        return tenantId;
                //    }
                //}

                // Example: Obtain tenant ID from the route
                //if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("tenantId", out Microsoft.Extensions.Primitives.StringValues tenantIdRoute))
                //{
                //    if (int.TryParse(tenantIdRoute.ToString(), out int tenantId))
                //    {
                //        return tenantId;
                //    }
                //}

                // Example: Obtain tenant ID from authentication token
                // This would depend on how the token is parsed and validated in your application

                //throw new InvalidOperationException("Tenant ID not found in the request.");
            }
        }
    }
}
