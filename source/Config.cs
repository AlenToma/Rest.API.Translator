using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rest.API.Translator
{
    /// <summary>
    /// Configrate your apiController
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Config<T>
    {
        /// <summary>
        /// Clear all cached settings
        /// </summary>
        public void Clear()
        {
            _cachedMethodRoute.Clear();
            _cachedMethodInformation.Clear();
            _cachedTRoutes.Clear();
        }

        /// <summary>
        /// Cached interface routes
        /// </summary>
        protected static InternalDictionary<Type, Route> _cachedTRoutes = new InternalDictionary<Type, Route>();

        /// <summary>
        /// Cached MethodInformation
        /// </summary>
        protected static InternalDictionary<string, MethodInformation> _cachedMethodInformation = new InternalDictionary<string, MethodInformation>();

        /// <summary>
        /// Cached Attributes
        /// </summary>
        protected static InternalDictionary<string, Route> _cachedMethodRoute = new InternalDictionary<string, Route>();
        /// <summary>
        /// Add or override route attribute dynamicly, to a method
        /// Those settings will be saved in a static variable and its a global settings.
        /// So you only add those once, when your application start/restart.
        /// </summary>
        /// <param name="nameofMethod"></param>
        /// <param name="relativeUrl">api/ or ../api its a realtive path to the baseUrl</param>
        /// <param name="httpMethod"> Default Get</param>
        /// <param name="fullUrl"> As full path , will ignore the baseUri and use the relativeUrl as full path. fullUrl above the interface will mean that it will ignore the interface name and only use the reltiveurl </param>
        /// <param name="parameterIntendFormat">Instead of ?Name=test it will be /test</param>
        /// <returns></returns>
        public Config<T> AddMethodRoute(
            string nameofMethod,
            string relativeUrl = null,
            MethodType httpMethod = MethodType.GET,
            bool fullUrl = false,
            bool parameterIntendFormat = false)
        {
            var route = new Route(relativeUrl, httpMethod, fullUrl, parameterIntendFormat);
            var method = typeof(T).GetMethod(nameofMethod);
            var key = GenerateKey(method);
            _cachedMethodRoute.Add(key, route,null, true);
            return this;
        }

        /// <summary>
        /// Add or override route attribute dynamicly to the interface
        /// </summary>
        /// <param name="relativeUrl">api/ or ../api its a realtive path to the baseUrl</param>
        /// <param name="httpMethod"> Default Get</param>
        /// <param name="fullUrl"> As full path , will ignore the baseUri and use the relativeUrl as full path. fullUrl above the interface will mean that it will ignore the interface name and only use the reltiveurl </param>
        /// <param name="parameterIntendFormat">Instead of ?Name=test it will be /test</param>
        /// <returns></returns>
        public Config<T> AddInterfaceRoute(
            string relativeUrl = null,
            MethodType httpMethod = MethodType.GET,
            bool fullUrl = false,
            bool parameterIntendFormat = false)
        {
            var route = new Route(relativeUrl, httpMethod, fullUrl, parameterIntendFormat);
            _cachedTRoutes.Add(typeof(T), route,null, true);
            return this;
        }

        /// <summary>
        /// Generate special key
        /// </summary>
        /// <typeparam name="PController"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected string GenerateKey<PController>(Expression<Func<T, PController>> expression)
        {
            MethodCallExpression callExpression = expression.Body as MethodCallExpression;
            var method = callExpression.Method;
            return GenerateKey(method);
        }

        /// <summary>
        /// Generate special key
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        protected string GenerateKey(MethodInfo methodInfo)
        {
            return typeof(T).ToString() + methodInfo.ReturnType.ToString() + methodInfo.Name + methodInfo.ReflectedType.FullName;
        }
    }
}
