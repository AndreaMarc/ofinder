using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Core.IoC;
using System;

namespace MIT.Fwk.Infrastructure.AutoMapper
{
    public class AllMappingProfile : MapperProfile
    {
        public AllMappingProfile()
        {
            try
            {
                System.Collections.Generic.List<object> mapperList = ReflectionHelper.ResolveAll<ICustomMapper>();

                foreach (ICustomMapper customMap in mapperList)
                {
                    customMap.DomainToViewModel(this);
                    customMap.ViewModelToDomain(this);
                    customMap.MapAll(this);
                    Console.WriteLine(string.Format("Loading {0}", customMap.GetType().Name));
                }

                Console.WriteLine("Custom profiles loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WARNING: Unable to find a custom Domain Map to load -> ex: {ex.Message}");
                throw new Exception("Unable to find a custom Domain Map to load, please check Plugins path");
            }
        }
    }
}
