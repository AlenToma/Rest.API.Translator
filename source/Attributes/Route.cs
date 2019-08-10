using System;

namespace Rest.API.Translator
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class Route : Attribute
    {
        public readonly string RelativeUrl;

        public readonly MethodType HttpMethod;

        /// <summary>
        /// Empty url mean the method name is the is the url
        /// </summary>
        /// <param name="relativeUrl">eg api/</param>
        /// <param name="httpMethod"> Default Get</param>
        public Route(string relativeUrl = null, MethodType httpMethod = MethodType.GET)
        {
            RelativeUrl = relativeUrl;
            HttpMethod = httpMethod;
        }

    }
}
