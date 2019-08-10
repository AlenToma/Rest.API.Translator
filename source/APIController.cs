using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using FastDeepCloner;

namespace Rest.API.Translator
{
    public class APIController<T> : IDisposable
    {
        private readonly string _baseUrl;

        private readonly HttpHandler httpHandler;
        /// <summary>
        /// APIController
        /// </summary>
        /// <param name="baseUrl">The baseUrl for the rest api</param>
        public APIController(string baseUrl)
        {
            if (!typeof(T).IsInterface)
                throw new Exception("T must be an interface");
            _baseUrl = baseUrl;
            httpHandler = new HttpHandler();

        }
        /// <summary>
        /// APIController, Make sure that no baseaddress is not specified, as we already have baseUrl
        /// </summary>
        /// <param name="baseUrl">The baseUrl for the rest api</param>
        /// <param name="client">your own HttpClient handler, if you would like to have your own HttpClient with your own settings</param>
        public APIController(HttpClient client, string baseUrl)
        {
            if (!typeof(T).IsInterface)
                throw new Exception("T must be an interface");
            _baseUrl = baseUrl;
            if (client != null)
                client.BaseAddress = null;
            httpHandler = new HttpHandler(client);
        }

        /// <summary>
        /// Extract the ControllerName from the interface
        /// eg IHomeController => Home
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual string ControllerNameResolver(string name)
        {
            return name.Substring(1).Replace("Controller", "");
        }

        private static SafeValueType<string, MethodInformation> _cachedMethodInformation = new SafeValueType<string, MethodInformation>();
        /// <summary>
        /// Extract the MethodInformation from the expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="expression"></param>
        /// <param name="skipArgs"></param>
        /// <returns></returns>
        public MethodInformation GetInfo<P>(Expression<Func<T, P>> expression, bool skipArgs = false)
        {
            MethodCallExpression callExpression = expression.Body as MethodCallExpression;
            var method = callExpression.Method;
            var argument = callExpression.Arguments;
            var key = typeof(T).ToString() + typeof(P).ToString() + method.Name + method.ReflectedType.FullName;
            var cached = _cachedMethodInformation.ContainsKey(key);
            var item = _cachedMethodInformation.GetOrAdd(key, new MethodInformation());

            if (!cached)
            {
                item.IsVoid = method.ReturnType == typeof(void) || method.ReturnType == typeof(Task);
                if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    item.CleanReturnType = method.ReturnType.GetActualType();
                else item.CleanReturnType = method.ReturnType;
            }
            var index = 0;
            item.Arguments.Clear();
            if (!skipArgs)
                foreach (var pr in method.GetParameters())
                {
                    var arg = argument[index];
                    var value = arg is ConstantExpression constExp ? constExp.Value : Expression.Lambda(arg).Compile().DynamicInvoke();
                    item.Arguments.Add(pr.Name, value);
                    index++;
                }

            if (!cached)
            {
                var mRoute = method.GetCustomAttribute<Route>();
                var classRoute = typeof(T).GetCustomAttribute<Route>()?.RelativeUrl ?? "";
                var controller = ControllerNameResolver(typeof(T).Name);
                item.FullUrl = Path.Combine(_baseUrl, classRoute, controller, (mRoute != null && !string.IsNullOrWhiteSpace(mRoute.RelativeUrl) ? mRoute.RelativeUrl : method.Name)).Replace("\\", "/");
                item.HttpMethod = mRoute?.HttpMethod ?? MethodType.GET;
            }
            return item;
        }


        public P Execute<P>(Expression<Func<T, P>> expression)
        {
            return AsyncExtension.Await(async () =>
            {
                return await ExecuteAsync(expression);
            });
        }

        /// <summary>
        /// ExecuteAsync
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="expression"></param>
        /// <param name="afterOperation">trigget after the operation is done(Optional)</param>
        /// <returns></returns>
        private async Task<P> ExecuteAsync<P>(Expression<Func<T, P>> expression)
        {
            object result = null;
            try
            {
                MethodInformation item = GetInfo(expression);
                switch (item?.HttpMethod ?? MethodType.GET)
                {
                    case MethodType.GET:
                        result = await httpHandler.GetAsync(item.FullUrl, item.Arguments, item.IsVoid ? null : item.CleanReturnType);
                        break;

                    case MethodType.POST:
                        result = await httpHandler.PostAsync(item.FullUrl, item.Arguments, item.IsVoid ? null : item.CleanReturnType);
                        break;

                    case MethodType.JSONPOST:
                        result = await httpHandler.PostAsJsonAsync(item.FullUrl, item.Arguments, item.IsVoid ? null : item.CleanReturnType);
                        break;
                }

                if (typeof(P).IsGenericType && typeof(P).GetGenericTypeDefinition() == typeof(Task<>))
                    result = new DynamicTaskCompletionSource(result, item.CleanReturnType).Task;
                else if (item.IsVoid) result = Task.CompletedTask;
                return (P)result;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Dispose()
        {
            httpHandler.Dispose();
        }
    }
}
