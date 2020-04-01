using FastDeepCloner;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Rest.API.Translator
{
    /// <summary>
    /// Here exist httpclient operations
    /// </summary>
    public class HttpHandler : IDisposable
    {
        private Action<HttpRequestHeaders> OnAuth;

        /// <summary>
        /// Current HttpClient
        /// </summary>
        public readonly HttpClient Client;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="onAuth">When Auth is needed</param>
        public HttpHandler(HttpClient httpClient = null, Action<HttpRequestHeaders> onAuth = null)
        {
            OnAuth = onAuth;
            if (httpClient != null)
                Client = httpClient;
            else
            if (httpClient == null)
            {
                try
                {
                    var clientcert = new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual,
                        Proxy = null,
                        UseProxy = false,
                    };
                    Client = new HttpClient(clientcert);
                }
                catch
                {
                    // 
                }
            }
        }

        /// <summary>
        /// Make a post operation, and serilize and post the perameter as json.
        /// </summary>
        /// <typeparam name="item">Desired type of returned data</typeparam>
        /// <param name="url">FullUrl</param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<item> PostAsJsonAsync<item>(string url, object parameter)
        {
            lock (this)
                return (item)(PostAsJsonAsync(url, parameter, typeof(item)).Await());
        }

        /// <summary>
        /// Make Get Operation
        /// </summary>
        /// <typeparam name="item">Desired type of returned data</typeparam>
        /// <param name="url">FullUrl</param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<item> GetAsync<item>(string url, object parameter = null)
        {
            lock (this)
                return (item)(GetAsync(url, parameter, typeof(item)).Await());
        }

        /// <summary>
        /// Make a post operation
        /// </summary>
        /// <typeparam name="item">Desired type of returned data</typeparam>
        /// <param name="url">FullUrl</param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<item> PostAsync<item>(string url, object parameter)
        {
            lock (this)
                return (item)(PostAsync(url, parameter, typeof(item)).Await());
        }

        /// <summary>
        /// post data
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameter"></param>
        /// <param name="castToType">The return json should be converted to </param>
        /// <returns></returns>
        public async Task<object> PostAsync(string baseUrl, object parameter = null, Type castToType = null)
        {
            var url = baseUrl;
            var p = parameter;
            var intern = parameter as InternalDictionary<string, object>;
            if (intern != null)
                parameter = intern.ToDictionary();

            FormUrlEncodedContent content = null;
            if (parameter != null)
            {
                if (intern != null)
                {
                    foreach (var key in intern.Keys)
                    {
                        if (intern.GetAttribute<FromQueryAttribute>(key) != null)
                        {
                            if (!url.Contains("?"))
                                url += $"?{key}={intern[key]}&";
                            else url += $"{key}={intern[key]}&";
                            (parameter as Dictionary<string, object>).Remove(key);
                        }
                    }
                }
                url = url.TrimEnd('&');
                var values = new Dictionary<string, string>();
                if (parameter is Dictionary<string, object>)
                {
                    if ((parameter as Dictionary<string, object>).Any())
                        values = (parameter as Dictionary<string, object>).ToDictionary(x => x.Key, x => x.Value?.ToString());
                }
                else
                {
                    values = parameter.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(parameter)?.ToString());
                }
                content= values.Any() ? new FormUrlEncodedContent(values) : null;
            }
             

            if (intern != null)
            {
                foreach (var h in intern.Headers)
                    Client.DefaultRequestHeaders.Add(h.Key, h.Value);
            }
            try
            {
                using (var response = Client.PostAsync(new Uri(url), content).Await())
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized && OnAuth != null)
                    {
                        OnAuth(Client.DefaultRequestHeaders);
                        return PostAsync(baseUrl, p, castToType).Await();
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        if (castToType != null)
                        {
                            var contents = await response.Content.ReadAsStringAsync();
                            if (castToType == typeof(string))
                                return contents;
                            if (!string.IsNullOrEmpty(contents))
                                return JsonConvert.DeserializeObject(contents, castToType);
                        }
                    }
                    else throw new Exception(response.ReasonPhrase);
                }
            }
            finally
            {
                if (intern != null)
                {
                    foreach (var h in intern.Headers)
                        Client.DefaultRequestHeaders.Remove(h.Key);
                }
            }
            return null;
        }

        /// <summary>
        /// Post as json.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="objectItem"></param>
        /// <param name="castToType">The return json should be converted to </param>
        /// <returns></returns>
        public async Task<object> PostAsJsonAsync(string baseUrl, object objectItem, Type castToType = null)
        {
            var p = objectItem;
            var url = baseUrl;
            if (objectItem == null)
                throw new Exception("POST operation need a parameters");

            var intern = objectItem as InternalDictionary<string, object>;
            if (intern != null)
                objectItem = intern.ToDictionary();
            var dic = ((Dictionary<string, object>)objectItem);
            string json = "";


            if (dic != null)
            {
                var dictionary = new Dictionary<string, object>();

                if (dic.Values.Count > 1)
                {
                    if (intern != null)
                    {
                        foreach (var key in intern.Keys)
                        {
                            if (intern.GetAttribute<FromQueryAttribute>(key) != null)
                            {
                                if (!url.Contains("?"))
                                    url += $"?{key}={intern[key]}&";
                                else url += $"{key}={intern[key]}&";
                                dic.Remove(key);
                            }
                        }

                        dictionary = dic;

                    }
                    else
                    {

                        var ch = "?";
                        if (dic.Any(x => !(x.Value?.GetType().IsInternalType() ?? false)) || (intern?.Keys.Any(x => intern.GetValueType(x).IsInternalType()) ?? false))
                        {
                            foreach (var key in dic)
                            {
                                var type = intern != null ? intern.GetValueType(key.Key) : key.Value?.GetType();
                                if (type?.IsInternalType() ?? true)
                                {
                                    if (ch != null)
                                    {
                                        if (!url.Contains("?"))
                                            url += ch;
                                        ch = null;
                                    }
                                    url = $"{url}{key.Key}={key.Value}&";
                                }
                                else dictionary.Add(key.Key, key.Value);
                            }
                        }
                        else dictionary = dic;
                    }
                    url = url.TrimEnd('&');
                    if (dictionary.Values.Count > 1)
                        json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
                    else json = JsonConvert.SerializeObject(dictionary.Values.FirstOrDefault());


                }
                else json = JsonConvert.SerializeObject(dic.Values.FirstOrDefault());

            }
            else json = JsonConvert.SerializeObject(objectItem);


            if (intern != null)
            {
                foreach (var h in intern.Headers)
                    Client.DefaultRequestHeaders.Add(h.Key, h.Value);
            }
            try
            {
                HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                using (var response = Client.PostAsync(new Uri(url), contentPost).Await())
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized && OnAuth != null)
                    {
                        OnAuth(Client.DefaultRequestHeaders);
                        return PostAsJsonAsync(baseUrl, p, castToType).Await();
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        if (castToType != null)
                        {
                            var contents = await response.Content.ReadAsStringAsync();

                            if (castToType == typeof(string))
                                return contents;
                            if (!string.IsNullOrEmpty(contents))
                                return JsonConvert.DeserializeObject(contents, castToType);

                        }
                    }
                    else throw new Exception(response.ReasonPhrase);
                }
            }
            finally
            {
                if (intern != null)
                {
                    foreach (var h in intern.Headers)
                        Client.DefaultRequestHeaders.Remove(h.Key);
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameter"></param>
        /// <param name="castToType">The return json should be converted to </param>
        /// <param name="parameterIntendFormat">Instead of ?Name=test, it will be /test</param>
        /// <returns></returns>
        public async Task<object> GetAsync(string baseUrl, object parameter = null, Type castToType = null, bool parameterIntendFormat = false)
        {
            var p = parameter;
            var url = baseUrl;
            var intern = parameter as InternalDictionary<string, object>;
            if (parameter is IDictionary)
            {
                if (parameter != null)
                {
                    if (!parameterIntendFormat)
                        url += (!url.Contains("?") ? "?": "") + string.Join("&", (parameter as Dictionary<string, object>).Select(x => $"{x.Key}={x.Value ?? ""}"));
                    else
                    {
                        var arr = (parameter as Dictionary<string, object>).Select(x => x.Value?.ToString() ?? "").ToList();
                        arr.Insert(0, url);
                        url = Helper.UrlCombine(arr.ToArray());
                    }
                }
            }
            else
            {

                if (parameter is InternalDictionary<string, object>)
                {
                    var i = parameter as InternalDictionary<string, object>;

                    if (parameterIntendFormat)
                    {
                        var arr = i.Keys.Where(x => i.GetAttribute<FromQueryAttribute>(x) == null).Select(x => i[x]?.ToString() ?? "").ToList();
                        arr.Insert(0, url);
                        url = Helper.UrlCombine(arr.ToArray());
                        url += (!url.Contains("?") ? "?" : "") + string.Join("&", i.Keys.Where(x => i.GetAttribute<FromQueryAttribute>(x) != null).Select(x => $"{x}={i[x] ?? ""}"));

                    }
                    else
                        url += (!url.Contains("?") ? "?" : "") + string.Join("&", i.Keys.Select(x => $"{x}={i[x] ?? ""}"));
                }
                else
                {
                    var props = parameter?.GetType().GetProperties();
                    if (props != null)
                    {
                        if (!parameterIntendFormat)
                            url += (!url.Contains("?") ? "?" : "") + string.Join("&", props.Select(x => $"{x.Name}={x.GetValue(parameter)}"));
                        else
                        {
                            var arr = props.Select(x => x.GetValue(parameter)?.ToString()).ToList();
                            arr.Insert(0, url);
                            url = Helper.UrlCombine(arr.ToArray());
                        }

                    }
                }
            }
            if (intern != null)
            {
                foreach (var h in intern.Headers)
                    Client.DefaultRequestHeaders.Add(h.Key, h.Value);
            }
            try
            {
                url = url.TrimEnd('?');
                using (var response = Client.GetAsync(new Uri(url)).Await())
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized && OnAuth != null)
                    {
                        OnAuth(Client.DefaultRequestHeaders);
                        return GetAsync(baseUrl, p, castToType, parameterIntendFormat).Await();
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        if (castToType != null)
                        {
                            if (castToType == typeof(string))
                                return responseString;
                            if (!string.IsNullOrEmpty(responseString))
                                return JsonConvert.DeserializeObject(responseString, castToType);
                        }
                    }
                    else throw new Exception(response.ReasonPhrase);
                }

            }
            finally
            {
                if (intern != null)
                {
                    foreach (var h in intern.Headers)
                        Client.DefaultRequestHeaders.Remove(h.Key);
                }
            }

            return null;
        }

        public string DownloadWebPage(string baseUrl, object parameter = null, bool parameterIntendFormat = false)
        {
            var p = parameter;
            var url = baseUrl;
            var intern = parameter as InternalDictionary<string, object>;
            if (parameter is IDictionary)
            {
                if (parameter != null)
                {
                    if (!parameterIntendFormat)
                        url += (!url.Contains("?") ? "?" : "") + string.Join("&", (parameter as Dictionary<string, object>).Select(x => $"{x.Key}={x.Value ?? ""}"));
                    else
                    {
                        var arr = (parameter as Dictionary<string, object>).Select(x => x.Value?.ToString() ?? "").ToList();
                        arr.Insert(0, url);
                        url = Helper.UrlCombine(arr.ToArray());
                    }
                }
            }
            else
            {

                if (parameter is InternalDictionary<string, object>)
                {
                    var i = parameter as InternalDictionary<string, object>;

                    if (parameterIntendFormat)
                    {
                        var arr = i.Keys.Where(x => i.GetAttribute<FromQueryAttribute>(x) == null).Select(x => i[x]?.ToString() ?? "").ToList();
                        arr.Insert(0, url);
                        url = Helper.UrlCombine(arr.ToArray());
                        url += (!url.Contains("?") ? "?" : "") + string.Join("&", i.Keys.Where(x => i.GetAttribute<FromQueryAttribute>(x) != null).Select(x => $"{x}={i[x] ?? ""}"));

                    }
                    else
                        url += (!url.Contains("?") ? "?" : "") + string.Join("&", i.Keys.Select(x => $"{x}={i[x] ?? ""}"));
                }
                else
                {
                    var props = parameter?.GetType().GetProperties();
                    if (props != null)
                    {
                        if (!parameterIntendFormat)
                            url += (!url.Contains("?") ? "?" : "") + string.Join("&", props.Select(x => $"{x.Name}={x.GetValue(parameter)}"));
                        else
                        {
                            var arr = props.Select(x => x.GetValue(parameter)?.ToString()).ToList();
                            arr.Insert(0, url);
                            url = Helper.UrlCombine(arr.ToArray());
                        }

                    }
                }
            }
            url = url.TrimEnd('?');
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
            Request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (intern != null)
            {
                foreach (var h in intern.Headers)
                    Client.DefaultRequestHeaders.Add(h.Key, h.Value);
            }
            Request.Proxy = null;
            Request.Method = "GET";
            try
            {
                using (var Response = Request.GetResponse())
                {
                    if (((HttpWebResponse)Response).StatusCode == HttpStatusCode.Unauthorized && OnAuth != null)
                    {
                        OnAuth(Client.DefaultRequestHeaders);
                        return DownloadWebPage(baseUrl, p, parameterIntendFormat);
                    }

                    using (StreamReader Reader = new StreamReader(Response.GetResponseStream()))
                    {
                        return Reader.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (intern != null)
                {
                    foreach (var h in intern.Headers)
                        Client.DefaultRequestHeaders.Remove(h.Key);
                }
            }
        }

        /// <summary>
        /// Dispose HttpClient
        /// </summary>
        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
