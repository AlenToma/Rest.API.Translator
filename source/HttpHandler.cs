using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rest.API.Translator
{
    public class HttpHandler : IDisposable
    {
        private readonly HttpClient _client;
        public HttpHandler(HttpClient httpClient = null)
        {
            if (httpClient != null)
                _client = httpClient;
            else
            if (httpClient == null)
            {
                try
                {
                    System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) =>
                    {
                        return true;
                    };
                    var clientcert = new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual
                    };
                    _client = new HttpClient(clientcert);
                }
                catch
                {
                    // 
                }
            }

        }

        public async Task<item> PostAsJsonAsync<item>(string url, object parameter)
        {
            return (item)await (PostAsJsonAsync(url, parameter, typeof(item)));
        }

        public async Task<item> GetTAsync<item>(string url, object parameter = null)
        {
            return (item)await (GetAsync(url, parameter, typeof(item)));
        }

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
            if (parameter == null)
                throw new Exception("POST operation need a parameters");
            var values = new Dictionary<string, string>();
            if (parameter is Dictionary<string, object>)
                values = (parameter as Dictionary<string, object>).ToDictionary(x => x.Key, x => x.Value?.ToString());
            else
            {
                values = parameter.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(parameter)?.ToString());
            }

            var content = new FormUrlEncodedContent(values);
            using (var response = await _client.PostAsync(new Uri(url), content))
            {
                if (castToType != null)
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(contents))
                        return JsonConvert.DeserializeObject(contents, castToType);
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
        public async Task<object> PostAsJsonAsync(string url, object objectItem, Type castToType = null)
        {
            if (objectItem == null)
                throw new Exception("POST operation need a parameters");
            var item = objectItem is IDictionary<string, object> ? ((IDictionary<string, object>)objectItem).Values.FirstOrDefault() : objectItem;
            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
            using (var response = await _client.PostAsync(new Uri(url), contentPost))
            {
                if (castToType != null)
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(contents))
                        return JsonConvert.DeserializeObject(contents, castToType);

                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <param name="castToType">The return json should be converted to </param>
        /// <returns></returns>
        public async Task<object> GetAsync(string url, object parameter = null, Type castToType = null)
        {
            if (parameter is IDictionary)
            {
                if (parameter != null)
                {
                    url += "?" + string.Join("&", (parameter as Dictionary<string, object>).Select(x => $"{x.Key}={x.Value ?? ""}"));
                }
            }
            else
            {
                var props = parameter?.GetType().GetProperties();
                if (props != null)
                    url += "?" + string.Join("&", props.Select(x => $"{x.Name}={x.GetValue(parameter)}"));
            }

            var responseString = await _client.GetStringAsync(new Uri(url));
            if (castToType != null)
            {
                if (!string.IsNullOrEmpty(responseString))
                    return JsonConvert.DeserializeObject(responseString, castToType);
            }

            return null;
        }
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
