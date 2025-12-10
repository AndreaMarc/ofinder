using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MIT.Fwk.Core.Helpers
{
    public static class ReflectionHelper
    {

        public static object GetPropertyValue(object obj, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentNullException.ThrowIfNull(propertyName);

            PropertyInfo property = GetPropertyInfo(obj, propertyName);
            return property.GetValue(obj);
        }

        public static void SetPropertyValue<T>(this T obj, string propertyName, object value)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                Type t = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                object safeValue = value == null || value == DBNull.Value
                    ? null
                    : t.FullName == "System.Guid" ? Guid.Parse(value.ToString()) : Convert.ChangeType(value, t);
                propertyInfo.SetValue(obj, safeValue, null);
            }
        }

        public static PropertyInfo GetPropertyInfo(this object obj, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentNullException.ThrowIfNull(propertyName);

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties(flags);
            PropertyInfo property = properties.SingleOrDefault(s => String.Equals(propertyName, s.Name));
            return property != null ? property : throw new Exception($"Missing property named {propertyName} of type {type.Name}");
        }

        public static object CreateInstance(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (type == typeof(string))
            {
                return String.Empty;
            }

            // type.IsGenericType - NOT IMPLEMENTED IN .NET CORE
            return type.GetTypeInfo().IsInterface || type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? null
                : Activator.CreateInstance(type);
        }

        public static object CreateInstance(string className)
        {
            Type type = Type.GetType(className);
            if (type != null)
            {
                return CreateInstance(type);
            }

            // AppDomain - NOT IMPLEMENTED IN .NET CORE
            IEnumerable<Assembly> assemblyList = AppDomain.CurrentDomain.GetAssemblies()
                .Where(ass => ass.FullName.StartsWith("MIT."));
            //ass.ExportedTypes
            //    .Where(et => et.DeclaringType.BaseType == typeof(BaseEntity)).Count() > 0);

            foreach (Assembly asm in assemblyList)
            {
                type = asm.GetType(className);
                if (type != null)
                {
                    return CreateInstance(type);
                }

                if (asm.ExportedTypes.FirstOrDefault(tp => tp.Name == className) != null)
                {
                    type = asm.ExportedTypes.FirstOrDefault(tp => tp.Name == className);
                    return CreateInstance(type);
                }
            }

            return null;
        }


        /// <summary>
        /// Load all assemblies from current AppDomain that contain types matching the specified base type.
        /// Plugin loading has been removed - only searches in already loaded assemblies.
        /// </summary>
        public static List<Assembly> LoadAllV2(Type tp)
        {
            List<Assembly> assemblyList = [];

            // Search only in already loaded assemblies (no external plugin loading)
            IEnumerable<Assembly> list = AppDomain.CurrentDomain.GetAssemblies()
                .Where(ass => ass.FullName.StartsWith("MIT."));

            foreach (Assembly asm in list)
            {
                if (asm.ExportedTypes.FirstOrDefault(t => t.BaseType == tp || (t.BaseType != null && t.BaseType.Name == tp.Name)) != null)
                {
                    if (assemblyList.FirstOrDefault(ass => ass.FullName == asm.FullName) == null)
                    {
                        assemblyList.Add(asm);
                    }
                }
            }

            return assemblyList;
        }

        /// <summary>
        /// Resolve a single instance of type T from loaded assemblies.
        /// Plugin loading has been removed - only searches in AppDomain assemblies.
        /// </summary>
        public static object Resolve<T>()
        {
            // Search only in already loaded assemblies
            IEnumerable<Assembly> assemblyList = AppDomain.CurrentDomain.GetAssemblies()
                .Where(ass => ass.FullName.StartsWith("MIT."));

            string tpName = typeof(T).Name;
            object obj = CreateInstance(tpName);
            if (obj != null)
            {
                return obj;
            }

            foreach (Assembly asm in assemblyList.ToList())
            {
                foreach (TypeInfo ti in asm.DefinedTypes)
                {
                    if (ti.ImplementedInterfaces.Contains(typeof(T)) || (ti.BaseType == typeof(T)))
                    {
                        return CreateInstance(ti.FullName);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Resolve all instances of type T from loaded assemblies.
        /// Plugin loading has been removed - searches only in AppDomain assemblies.
        public static List<object> ResolveAll<T>()
        {
            foreach (string dll in Directory.GetFiles(AppContext.BaseDirectory, "MIT.*.dll"))
            {
                try
                {
                    string name = Path.GetFileNameWithoutExtension(dll);
                    if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        Assembly.LoadFrom(dll);
                    }
                }
                catch
                {
                    // ignora le dll non caricabili
                }
            }

            List<object> list = [];

            try
            {
                // Search in all loaded MIT assemblies (including custom modules like MIT.Fwk.Examples)
                IEnumerable<Assembly> assemblyList = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(ass => ass.FullName.StartsWith("MIT."));

                foreach (Assembly asm in assemblyList)
                {
                    foreach (TypeInfo ti in asm.DefinedTypes)
                    {
                        if (ti.ImplementedInterfaces.Contains(typeof(T)) || (ti.BaseType == typeof(T)))
                        {
                            object obj = CreateInstance(ti.FullName);
                            if (obj != null && list.Find(o => o.GetType().FullName == ti.FullName) == null)
                            {
                                list.Add(obj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return list;
        }

        /// <summary>
        /// Get all types implementing or inheriting from T from loaded assemblies.
        /// Plugin loading has been removed - searches only in AppDomain assemblies.
        /// </summary>
        /// <returns>List of types implementing or inheriting from T</returns>
        public static List<Type> GetAllTypes<T>()
        {
            List<Type> list = [];

            try
            {
                // Search in all loaded MIT assemblies (including custom modules)
                IEnumerable<Assembly> assemblyList = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(ass => ass.FullName.StartsWith("MIT."));

                foreach (Assembly asm in assemblyList)
                {
                    foreach (TypeInfo ti in asm.DefinedTypes)
                    {
                        if (ti.ImplementedInterfaces.Contains(typeof(T)) || (ti.BaseType == typeof(T)))
                        {
                            list.Add(ti);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return list;
        }
    }
}
