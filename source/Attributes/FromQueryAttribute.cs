using System;

namespace Rest.API.Translator
{
    /// <summary>
    /// Add this to your parameter so you could tell restapi to add it as a quary eg?test=value
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromQueryAttribute : Attribute
    {
    }
}
