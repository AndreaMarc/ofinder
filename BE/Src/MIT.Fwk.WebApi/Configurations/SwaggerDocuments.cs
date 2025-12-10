//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MIT.Fwk.WebApi.Configurations
{
    public static class SwaggerDocuments
    {
        private static List<SwaggerDocument> _apiGroups = null;
        public static List<SwaggerDocument> APIGroups
        {
            get
            {
                if (_apiGroups == null)
                {
                    _apiGroups = [];
                }

                return _apiGroups;
            }
        }

        public static void Load(TypeInfo api)
        {
            IEnumerable<CustomAttributeData> attributes = api.BaseType.CustomAttributes;

            foreach (CustomAttributeData item in attributes)
            {
                //if (item.AttributeType == typeof(ApiVersionAttribute))
                //{
                foreach (CustomAttributeTypedArgument version in item.ConstructorArguments)
                {
                    if (APIGroups.Where(g => g.Version == version.Value.ToString()).Count() == 0)
                    {
                        // SwaggerDocuments.APIGroups.Add(new SwaggerDocument() { Namespace = api.DeclaringType.Namespace.Replace("<Namespace Prefix I want Removed>.", "").Split('.').First(), Version = version.Value.ToString() });
                        APIGroups.Add(new SwaggerDocument() { Namespace = "MIT.Fwk", Version = version.Value.ToString() });
                    }
                }
                //}
            }
        }

        //        public static List<SwaggerDocument> GetSwaggerDocumentList()
        //        {
        //            var APIGroups = new List<SwaggerDocument>();
        //            Assembly asm = Assembly.GetExecutingAssembly();
        //            var controllers = asm.GetTypes()
        //                .Where(type => typeof(Controller).IsAssignableFrom(type))
        //                .SelectMany(type => type.GetMethods())
        //                .Where(method => method.IsPublic && !method.IsDefined(typeof(NonActionAttribute)));

        //            foreach (var controller in controllers)
        //            {
        //                Type t = controller.GetType();
        //                var Namespace = controller.DeclaringType.Namespace;

        //                var attributes = controller.DeclaringType.CustomAttributes;

        //                foreach (var item in attributes)
        //                {
        //                    if (item.AttributeType == typeof(ApiVersionAttribute))
        //                    {
        //                        foreach (var version in item.ConstructorArguments)
        //                        {
        //                            APIGroups.Add(new SwaggerDocument() { Namespace = Namespace.Replace("<Namespace Prefix I want Removed>.", "").Split('.').First(), Version = version.Value.ToString() });
        //                        }
        //                    }
        //                }
        //            }

        //            return APIGroups.GroupBy(x => new { x.Namespace, x.Version }).Select(x => new SwaggerDocument { Namespace = x.Key.Namespace, Version = x.Key.Version }).ToList();
        //        }

    }

    public class SwaggerDocument
    {
        public string Namespace { get; set; }
        public string Version { get; set; }
        public bool IsDeprecated { get; set; }
    }
}
