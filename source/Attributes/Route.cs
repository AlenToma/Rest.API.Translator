using System;

namespace Rest.API.Translator
{

    /// <summary>
    /// Configurete your interface or methods in the interface with this attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class Route : Attribute
    {
        /// <summary>
        /// api/ or ../api its a realtive path to the baseUrl
        /// </summary>
        public readonly string RelativeUrl;

        /// <summary>
        /// Post, Get or JSONPOST
        /// </summary>
        public readonly MethodType HttpMethod;

        /// <summary>
        /// As full path , will ignore the baseUri and use the relativeUrl as full path.
        /// fullUrl Above the interface will mean that it will ignore the interface name and only use the realtiveurl
        /// </summary>
        public readonly bool FullUrl;

        /// <summary>
        /// Instead of ?Name=test it will be /test
        /// </summary>
        public bool ParameterIntendFormat { get; set; }

        /// <summary>
        /// Empty relativeUrl mean the method name is the relativeUrl
        /// </summary>
        /// <param name="relativeUrl">api/ or ../api its a realtive path to the baseUrl</param>
        /// <param name="httpMethod"> Default Get</param>
        /// <param name="fullUrl"> As full path , will ignore the baseUri and use the relativeUrl as full path. fullUrl above the interface will mean that it will ignore the interface name and only use the reltiveurl </param>
        /// <param name="parameterIntendFormat">Instead of ?Name=test it will be /test</param>
        public Route(string relativeUrl = null, MethodType httpMethod = MethodType.GET, bool fullUrl = false, bool parameterIntendFormat= false)
        {
            RelativeUrl = relativeUrl;
            HttpMethod = httpMethod;
            FullUrl = fullUrl;
            ParameterIntendFormat = parameterIntendFormat;
        }
    }
}
