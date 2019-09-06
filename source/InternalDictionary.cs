using FastDeepCloner;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rest.API.Translator
{
    /// <summary>
    /// Internal InternalDictionary
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="P"></typeparam>
    public class InternalDictionary<T, P>
    {
        private readonly SafeValueType<T, P> keyValuePairs = new SafeValueType<T, P>();

        private readonly SafeValueType<T, Type> types = new SafeValueType<T, Type>();

        private readonly SafeValueType<T, Attribute> attributes = new SafeValueType<T, Attribute>();

        internal InternalDictionary(Dictionary<T, P> dic = null)
        {
            keyValuePairs = new SafeValueType<T, P>(dic);
        }

        /// <summary>
        /// Get the value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public P this[T key]
        {
            get => keyValuePairs.Get(key);
        }

        internal P Add(T key, P value, bool overwrite = false)
        {
            return keyValuePairs.GetOrAdd(key, value, overwrite);
        }

        internal P Add(T key, P value, Type type, Attribute attr = null, bool overwrite = false)
        {
            attributes.TryAdd(key, attr, overwrite);
            types.TryAdd(key, type, overwrite);
            return keyValuePairs.GetOrAdd(key, value, overwrite);
        }


        internal Type GetValueType(T key)
        {
            return types.Get(key);
        }

        internal object GetAttribute<Item>(T key) where Item : Attribute
        {
            return attributes.Get(key) as Item;
        }

        /// <summary>
        /// Get the values
        /// </summary>
        public IEnumerable<P> Values { get => keyValuePairs.Values; }

        /// <summary>
        /// Get Keys
        /// </summary>
        public IEnumerable<T> Keys { get => keyValuePairs.Keys; }

        /// <summary>
        /// If key exist
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exist(T key) { return keyValuePairs.ContainsKey(key); }

        internal Dictionary<T, P> ToDictionary()
        {
            return keyValuePairs.Keys.ToDictionary(x => x, x => keyValuePairs[x]);
        }

        internal void Clear()
        {
            keyValuePairs.Clear();
        }

    }
}
