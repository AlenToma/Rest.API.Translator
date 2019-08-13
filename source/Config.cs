using System;
using System.Linq.Expressions;

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
        }

        /// <summary>
        /// Cached MethodInformation
        /// </summary>
        protected static InternalDictionary<string, MethodInformation> _cachedMethodInformation = new InternalDictionary<string, MethodInformation>();

        /// <summary>
        /// Cached Attributes
        /// </summary>
        protected static InternalDictionary<string, Route> _cachedMethodRoute = new InternalDictionary<string, Route>();
        /// <summary>
        /// Add route attribute dynamicly, to a method
        /// Those settings will be saved in a static variable and its a global settings.
        /// So you only add those once, when your application start/restart.
        /// </summary>
        /// <typeparam name="PController"></typeparam>
        /// <param name="expression"></param>
        /// <param name="route">Route to be added </param>
        public Config<T> AddMethodRoute<PController>(Expression<Func<T, PController>> expression, Route route)
        {
            if (route == null)
                throw new Exception("route cant be null");
            var key = GenerateKey(expression);
            _cachedMethodRoute.Add(key, route, true);
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
            return typeof(T).ToString() + typeof(PController).ToString() + method.Name + method.ReflectedType.FullName;
        }
    }
}
