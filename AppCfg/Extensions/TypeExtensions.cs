using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AppCfg
{
    internal static class TypeExtensions
    {
        public static IEnumerable<PropertyInfo> GetAllPropertiesRecursive(this Type type)
        {
            if (type == null)
            {
                return Enumerable.Empty<PropertyInfo>();
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            return type.GetProperties(flags).Concat(GetAllPropertiesRecursive(type.BaseType));
        }

        public static bool IsDictionaryType(this Type type)
        {
            // Implementation mostly from Newtonsoft.Json.Utilities.CollectionUtils.IsDictionaryType

#if HAVE_READ_ONLY_COLLECTIONS
            if (type.ImplementsGenericDefinition(typeof(IReadOnlyDictionary<,>)))
	        {
                 return true;
	        }
#endif
            return typeof(IDictionary).IsAssignableFrom(type)
                || type.ImplementsGenericDefinition(typeof(IDictionary<,>));
        }

        public static bool ImplementsGenericDefinition(this Type type, Type genericInterfaceDefinition)
        {
            // Implementation mostly from Newtonsoft.Json.Utilities.ReflectionUtils.ImplementsGenericDefinition

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (genericInterfaceDefinition == null)
            {
                throw new ArgumentNullException(nameof(genericInterfaceDefinition));
            }

            if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
            {
                throw new ArgumentNullException($"'{genericInterfaceDefinition}' is not a generic interface definition.");
            }

            return (type.IsInterface && type.IsGenericType && genericInterfaceDefinition == type.GetGenericTypeDefinition())
                || type.GetInterfaces().Any(i => i.IsGenericType && genericInterfaceDefinition == i.GetGenericTypeDefinition());
        }
    }
}