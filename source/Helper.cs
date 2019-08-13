using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rest.API.Translator
{
    /// <summary>
    /// Some Usefull Methods
    /// </summary>
    public static class Helper
    {

        /// <summary>
        /// This method works like Path.Combine except its for url
        /// ../../meta: go back tow times
        /// /meta: combine
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
        public static string UrlCombine(params string[] urls)
        {
            string result = null;
            foreach (var r in urls)
            {
                var url = r;
                if (string.IsNullOrWhiteSpace(url))
                    continue;
                while (url.StartsWith("../"))
                {
                    result = string.Join("/", result.Split('/').Reverse().Skip(1).Reverse().ToArray());
                    url = url.Substring(3);
                }


                var s = url.TrimEnd('/');

                result = result == null ? s : $"{result}/{s.TrimStart('/')}";
            }
            return result;
        }

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
