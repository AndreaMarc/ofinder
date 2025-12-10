using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Extension
{
    public class BaseHttpRequestFilter
    {
        protected readonly RequestDelegate _next;

        public BaseHttpRequestFilter(RequestDelegate next)
        {
            _next = next;
        }

        public virtual async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] BaseHttpRequestFilter.Invoke: {ex.Message} -> inner ex: {ex.InnerException} -> stack trace: {ex.StackTrace}");
                throw;
            }
        }

    }
}
