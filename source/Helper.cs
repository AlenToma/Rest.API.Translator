using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rest.API.Translator
{
    internal static class Helper
    {
        private static readonly Dictionary<Type, Type> CachedActualType = new Dictionary<Type, Type>();
        /// <summary>
        /// Get Internal type of IList
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetActualType(this Type type)
        {
            if (CachedActualType.ContainsKey(type))
                return CachedActualType[type];
            var tType = type;
            if (type.GetTypeInfo().IsArray)
                tType = type.GetElementType();
            else if (type.GenericTypeArguments.Any())
                tType = type.GenericTypeArguments.First();
            else if (type.FullName?.Contains("List`1") ?? false)
                tType = type.GetRuntimeProperty("Item").PropertyType;

            CachedActualType.Add(type, tType);
            return tType;
        }
    }
}
