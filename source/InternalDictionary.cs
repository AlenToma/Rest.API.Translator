﻿using FastDeepCloner;
using System.Collections.Generic;

namespace Rest.API.Translator
{
    /// <summary>
    /// Internal InternalDictionary
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="P"></typeparam>
    public class InternalDictionary<T, P>
    {
        private SafeValueType<T, P> keyValuePairs = new SafeValueType<T, P>();
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

        internal Dictionary<T,P> ToDictionary()
        {
            return keyValuePairs;
        }

        internal void Clear()
        {
            keyValuePairs.Clear();
        }

    }
}