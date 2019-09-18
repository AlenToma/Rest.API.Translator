using System;

namespace Rest.API.Translator.Attributes
{
    /// <summary>
    /// Add this to your parameter so you could tell restapi to add it as a quary eg?test=value
    /// this is only applied to HttpPost
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromQuaryAttribute : Attribute
    {
    }
}
