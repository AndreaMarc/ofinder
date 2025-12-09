using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MIT.Fwk.Core.IoC
{
    /// <summary>
    /// Handler interface for registering application-level services.
    /// Implement this interface in custom modules to auto-register services via DI.
    /// Framework auto-discovers implementations via reflection.
    /// </summary>
    public interface IApplicationServiceHandler
    {
        void Configure(IServiceCollection services);
    }

    /// <summary>
    /// Handler interface for registering domain-level services.
    /// Implement this interface in custom modules for domain service registration.
    /// Framework auto-discovers implementations via reflection.
    /// </summary>
    public interface IDomainServiceHandler
    {
        void Configure(IServiceCollection services);
    }

    /// <summary>
    /// Handler interface for configuring application builder middleware pipeline.
    /// Implement this interface in custom modules to add custom middleware.
    /// Framework auto-discovers implementations via reflection.
    /// </summary>
    public interface IApplicationBuilderHandler
    {
        void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            IServiceProvider serviceProvider);
    }

    /// <summary>
    /// Handler interface for custom AutoMapper configuration.
    /// Implement this interface to define custom DTO mappings.
    /// </summary>
    public interface ICustomMapper
    {
        void DomainToViewModel(MapperProfile prf);
        void ViewModelToDomain(MapperProfile prf);
        void MapAll(MapperProfile prf);
    }
}
