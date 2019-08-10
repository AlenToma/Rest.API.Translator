using System;
using System.Collections.Generic;

namespace Rest.API.Translator
{
    public sealed class MethodInformation
    {
        public Dictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

        public string FullUrl { get; set; }

        public MethodType HttpMethod { get; set; }

        public bool IsVoid { get; set; }

        public Type CleanReturnType { get; set; }


        public override string ToString()
        {
            return FullUrl + HttpMethod;
        }

        /// <summary>
        /// return a Quary eg http:test.com?name=test
        /// </summary>
        /// <param name="args">override Arguments with this value</param>
        /// <returns></returns>
        public string ToQuary(Dictionary<string, object> args = null)
        {
            var arguments = args ?? Arguments;

            var url = FullUrl;
            foreach (var arg in arguments)
            {
                if (!url.EndsWith("?"))
                    url += $"?{arg.Key}={arg.Value}";
                else url += $"&{arg.Key}={arg.Value}";
            }

            return url;
        }
    }
}