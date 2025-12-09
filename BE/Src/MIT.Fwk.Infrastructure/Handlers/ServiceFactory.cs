using Microsoft.Extensions.DependencyInjection;
using MIT.Fwk.Infrastructure.Interfaces;

namespace MIT.Fwk.Infrastructure.Handlers
{
    public class ServiceFactory
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }
    }
}
