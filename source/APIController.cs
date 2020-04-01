using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Net.Http.Headers;

namespace Rest.API.Translator
{
    /// <summary>
    /// The api translator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class APIController<T> : Config<T>, IDisposable
    {
        /// <summary>
        /// The baseUrl
        /// </summary>
        public readonly string BaseUrl;

        /// <summary>
        /// The httpClient
        /// </summary>
        public readonly HttpHandler HttpHandler;

        /// <summary>
        /// APIController
        /// </summary>
        /// <param name="baseUrl">The baseUrl for the rest api</param>
        public APIController(string baseUrl = null, Action<HttpRequestHeaders> onAuth = null)
        {
            if (!typeof(T).IsInterface)
                throw new Exception("Rest.API.Translator: T must be an interface");
            BaseUrl = baseUrl;
            HttpHandler = new HttpHandler(null, onAuth);

        }


        /// <summary>
        /// APIController, Make sure that baseaddress is not specified, as we already have baseUrl
        /// </summary>
        /// <param name="baseUrl">The baseUrl for the rest api</param>
        /// <param name="client">your own HttpClient handler, if you would like to have your own HttpClient with your own settings</param>
        public APIController(HttpClient client, string baseUrl)
        {
            if (!typeof(T).IsInterface)
                throw new Exception("Rest.API.Translator: T must be an interface");
            BaseUrl = baseUrl;
            if (client != null)
                client.BaseAddress = null;
            HttpHandler = new HttpHandler(client);
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

        /// <summary>
        /// Extract the MethodInformation from the expression
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="expression"></param>
        /// <param name="skipArgs"></param>
        /// <returns></returns>
        public MethodInformation GetInfo<P>(Expression<Func<T, P>> expression, bool skipArgs = false)
        {
            lock (this)
            {
                MethodCallExpression callExpression = expression.Body as MethodCallExpression;
                var method = callExpression.Method;
                var argument = callExpression.Arguments;
                var key = GenerateKey(expression);
                var cached = _cachedMethodInformation.Exist(key);
                var item = _cachedMethodInformation.Add(key, new MethodInformation());

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
                        var attr = pr.GetCustomAttribute<FromQueryAttribute>();
                        var arg = argument[index];
                        var value = arg is ConstantExpression constExp ? constExp.Value : Expression.Lambda(arg).Compile().DynamicInvoke();
                        item.Arguments.Add(pr.Name, value, arg.Type, attr);
                        index++;
                    }

                if (!cached || item.BaseUrl != BaseUrl)
                {
                    var headerAttr = method.GetCustomAttributes<Header>();
                    if (headerAttr != null && headerAttr.Count() > 0)
                        foreach (var h in headerAttr)
                            item.Arguments.Headers.Add(h.Name, h.Value);
                    var mRoute = _cachedMethodRoute.Exist(key) ? _cachedMethodRoute[key] : method.GetCustomAttribute<Route>();
                    var classRoute = _cachedTRoutes.Exist(typeof(T)) ? _cachedTRoutes[typeof(T)] : typeof(T).GetCustomAttribute<Route>();
                    var controller = classRoute == null || !classRoute.FullUrl ? ControllerNameResolver(typeof(T).Name) : "";
                    item.ParameterIntendFormat = mRoute?.ParameterIntendFormat ?? false;
                    if (mRoute == null || !mRoute.FullUrl)
                        item.FullUrl = Helper.UrlCombine(BaseUrl, classRoute?.RelativeUrl, controller, (mRoute != null && mRoute.RelativeUrl != null ? mRoute.RelativeUrl : method.Name));
                    else item.FullUrl = mRoute.RelativeUrl;
                    item.HttpMethod = mRoute?.HttpMethod ?? MethodType.GET;
                    item.BaseUrl = BaseUrl;
                }
                return item;
            }
        }



        /// <summary>
        /// Make your calls to the api
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public P Execute<P>(Expression<Func<T, P>> expression)
        {
           return ExecuteAsync(expression).Await();
        }

        /// <summary>
        /// Make your calls to the api
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<P> ExecuteAsync<P>(Expression<Func<T, P>> expression)
        {
            object result = null;
            try
            {
                MethodInformation item = GetInfo(expression);
                switch (item?.HttpMethod ?? MethodType.GET)
                {
                    case MethodType.GET:
                        result = await HttpHandler.GetAsync(item.FullUrl, item.Arguments, item.IsVoid ? null : item.CleanReturnType, item.ParameterIntendFormat);
                        break;

                    case MethodType.POST:
                        result = await HttpHandler.PostAsync(item.FullUrl, item.Arguments, item.IsVoid ? null : item.CleanReturnType);
                        break;

                    case MethodType.JSONPOST:
                        result = await HttpHandler.PostAsJsonAsync(item.FullUrl, item.Arguments, item.IsVoid ? null : item.CleanReturnType);
                        break;
                    case MethodType.HTML:
                        result = HttpHandler.DownloadWebPage(item.FullUrl, item.Arguments, item.ParameterIntendFormat);
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

        /// <summary>
        /// Dispose the httpclient
        /// </summary>
        public void Dispose()
        {
            HttpHandler.Dispose();
        }
    }
}
