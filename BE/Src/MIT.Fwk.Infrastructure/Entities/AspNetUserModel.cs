using Microsoft.AspNetCore.Http;
using MIT.Fwk.Core.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MIT.Fwk.Infrastructure.Entities
{
    public class AspNetUserModel : IUser
    {
        private readonly IHttpContextAccessor _accessor;

        public AspNetUserModel(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string Name
        {
            get
            {
                return _accessor != null && _accessor.HttpContext != null && _accessor.HttpContext.User.Identity != null && _accessor.HttpContext.User.Claims.ToList().FirstOrDefault(x => x.Type == "username") != null
                    ? _accessor.HttpContext.User.Claims.ToList().Find(x => x.Type == "username").Value
                    : "NaN";
            }
        }
        public bool IsAuthenticated()
        {
            return _accessor.HttpContext.User.Identity.IsAuthenticated;
        }

        public IEnumerable<Claim> GetClaimsIdentity()
        {
            return _accessor.HttpContext.User.Claims;
        }
    }
}
