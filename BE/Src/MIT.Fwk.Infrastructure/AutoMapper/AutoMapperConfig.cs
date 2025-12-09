using AutoMapper;
using Microsoft.Extensions.Logging;
// using MIT.Fwk.Core.Mapper; // MapperWrapper removed - use IMapper with DI

namespace MIT.Fwk.Infrastructure.AutoMapper
{
    public class AutoMapperConfig
    {
        public static MapperConfiguration RegisterMappings()
        {
            MapperConfiguration cfg = new(cfg =>
            {
                cfg.AddProfile(new AllMappingProfile());
            }, new LoggerFactory());

            // MapperWrapper.Initialize(cfg); // MapperWrapper removed - IMapper registered via DI in NativeInjectorBootStrapper

            return cfg;
        }
    }

}
