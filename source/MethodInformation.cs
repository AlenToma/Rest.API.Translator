using System;
using System.Collections.Generic;

namespace Rest.API.Translator
{
    /// <summary>
    /// Present the method information of the interface 
    /// </summary>
    public sealed class MethodInformation
    {
        /// <summary>
        /// Argemunts extracted from the expression
        /// </summary>
        public InternalDictionary<string, object> Arguments { get; internal set; } = new InternalDictionary<string, object>();

        /// <summary>
        /// Generated FullUrl
        /// </summary>
        public string FullUrl { get; internal set; }

        /// <summary>
        /// Operation Type
        /// </summary>
        public MethodType HttpMethod { get; internal set; }

        /// <summary>
        /// Has return data or not
        /// </summary>
        public bool IsVoid { get; internal set; }

        /// <summary>
        /// Method return data type
        /// </summary>
        public Type CleanReturnType { get; internal set; }

        /// <summary>
        /// when APIController build a query 
        /// Instead of ?Name=test it will be /test
        /// </summary>
        public bool ParameterIntendFormat { get; internal set; }

        /// <summary>
        /// Tostring
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FullUrl + HttpMethod;
        }

        internal string BaseUrl { get; set; }

        /// <summary>
        /// return a Quary eg http:test.com?name=test
        /// </summary>
        /// <param name="args">override this.Arguments with this value</param>
        /// <returns></returns>
        public string ToQuary(Dictionary<string, object> args = null)
        {
            var arguments = args ?? Arguments?.ToDictionary();
            var url = FullUrl;
            foreach (var arg in arguments)
            {
                if (!ParameterIntendFormat)
                {
                    if (!url.EndsWith("?"))
                        url += $"?{arg.Key}={arg.Value}";
                    else url += $"&{arg.Key}={arg.Value}";
                }
                else url = Helper.UrlCombine(url, arg.Value?.ToString() ?? "");
            }
            return url;
        }
    }
}