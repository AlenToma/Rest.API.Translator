using FastDeepCloner;
using Newtonsoft.Json;
using Rest.API.Translator.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rest.API.Translator
{
    /// <summary>
    /// Here exist httpclient operations
    /// </summary>
    public class HttpHandler : IDisposable
    {
        /// <summary>
        /// Current HttpClient
        /// </summary>
        public readonly HttpClient Client;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClient"></param>
        public HttpHandler(HttpClient httpClient = null)
        {


            if (httpClient != null)
                Client = httpClient;
            else
            if (httpClient == null)
            {
                try
                {

                    //System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) =>
                    //{
                    //    return true;
                    //};
                    var clientcert = new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual
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
            return (item)(await PostAsJsonAsync(url, parameter, typeof(item)));
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
            return (item)(await GetAsync(url, parameter, typeof(item)));
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
            return (item)await (PostAsync(url, parameter, typeof(item)));
        }

        /// <summary>
        /// post data
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameter"></param>
        /// <param name="castToType">The return json should be converted to </param>
        /// <returns></returns>
        public async Task<object> PostAsync(string url, object parameter, Type castToType = null)
        {
            var intern = parameter as InternalDictionary<string, object>;
            if (intern != null)
                parameter = intern.ToDictionary();

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

            var content = values.Any() ? new FormUrlEncodedContent(values) : null;
            using (var response = Client.PostAsync(new Uri(url), content).Await())
            {
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
            return null;
        }

        /// <summary>
        /// Post as json.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="objectItem"></param>
        /// <param name="castToType">The return json should be converted to </param>
        /// <returns></returns>
        public async Task<object> PostAsJsonAsync(string url, object objectItem, Type castToType = null)
        {
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
                                        if (!url.EndsWith("?"))
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

            HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
            using (var response = Client.PostAsync(new Uri(url), contentPost).Await())
            {
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
        public async Task<object> GetAsync(string url, object parameter = null, Type castToType = null, bool parameterIntendFormat = false)
        {
            if (parameter is InternalDictionary<string, object>)
                parameter = (parameter as InternalDictionary<string, object>).ToDictionary();
            if (parameter is IDictionary)
            {
                if (parameter != null)
                {
                    if (!parameterIntendFormat)
                        url += "?" + string.Join("&", (parameter as Dictionary<string, object>).Select(x => $"{x.Key}={x.Value ?? ""}"));
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
                var props = parameter?.GetType().GetProperties();
                if (props != null)
                {
                    if (!parameterIntendFormat)
                        url += "?" + string.Join("&", props.Select(x => $"{x.Name}={x.GetValue(parameter)}"));
                    else
                    {
                        var arr = props.Select(x => x.GetValue(parameter)?.ToString()).ToList();
                        arr.Insert(0, url);
                        url = Helper.UrlCombine(arr.ToArray());
                    }

                }
            }

            using (var response = Client.GetAsync(new Uri(url)).Await())
            {
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

            return null;
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
