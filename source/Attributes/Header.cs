using System;
using System.Collections.Generic;
using System.Text;

namespace Rest.API.Translator
{
    /// <summary>
    /// Add attribute to a Method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Header : Attribute
    {
        public string Name { get; private set; }

        public string Value { get; private set; }

        public Header(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
