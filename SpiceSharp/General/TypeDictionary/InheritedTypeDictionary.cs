﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.General
{
    /// <summary>
    /// An <see cref="ITypeDictionary{V}"/> that tracks both inheritance and implemented interfaces.
    /// </summary>
    /// <typeparam name="V">The base value type.</typeparam>
    /// <seealso cref="ITypeDictionary{V}" />
    public class InheritedTypeDictionary<V> : ITypeDictionary<V>
    {
        private readonly Dictionary<Type, TypeValues<V>> _dictionary;
        private readonly HashSet<V> _values;

        /// <inheritdoc/>
        public V this[Type key]
        {
            get
            {
                TypeValues<V> result = _dictionary[key];
                if (result.IsAmbiguous)
                {
                    throw new AmbiguousTypeException(key);
                }

                return result.Value;
            }
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                return _values.Count;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Type> Keys
        {
            get
            {
                return _dictionary.Keys;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<V> Values
        {
            get
            {
                return _values;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritedTypeDictionary{V}"/> class.
        /// </summary>
        public InheritedTypeDictionary()
        {
            _dictionary = new Dictionary<Type, TypeValues<V>>();
            _values = new HashSet<V>();
        }

        /// <inheritdoc/>
        public void Add(Type key, V value)
        {
            key.ThrowIfNull(nameof(key));

            // We should always be able to access the type by itself, so remove any ambiguous elements if necessary
            if (_dictionary.TryGetValue(key, out TypeValues<V> values))
            {
                if (values.IsDirect)
                {
                    throw new ArgumentException(Properties.Resources.TypeAlreadyExists.FormatString(key.FullName));
                }
            }
            else
            {
                values = new TypeValues<V>();
                _dictionary.Add(key, values);
            }
            values.Add(value, true);
            _values.Add(value);

            foreach (Type type in InheritanceCache.Get(key).Union(InterfaceCache.Get(key)))
            {
                if (!_dictionary.TryGetValue(type, out values))
                {
                    values = new TypeValues<V>();
                    _dictionary.Add(type, values);
                }
                values.Add(value);
            }
        }

        /// <inheritdoc/>
        public bool Remove(Type key, V value)
        {
            key.ThrowIfNull(nameof(key));
            if (_dictionary.TryGetValue(key, out TypeValues<V> values))
            {
                if (!values.IsDirect || !values.Value.Equals(value))
                {
                    return false;
                }

                _values.Remove(value);
                foreach (Type type in InheritanceCache.Get(key).Union(InterfaceCache.Get(key)))
                {
                    _dictionary[type].Remove(value);
                }

                _dictionary.Remove(key);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _dictionary.Clear();
            _values.Clear();
        }

        /// <inheritdoc/>
        public bool ContainsKey(Type key)
        {
            return _dictionary.ContainsKey(key.ThrowIfNull(nameof(key)));
        }

        /// <inheritdoc/>
        public bool Contains(V value)
        {
            return _values.Contains(value.ThrowIfNull(nameof(value)));
        }

        /// <inheritdoc/>
        public IEnumerable<V> GetAllValues(Type key)
        {
            key.ThrowIfNull(nameof(key));
            if (_dictionary.TryGetValue(key, out TypeValues<V> result))
            {
                return result.Values.Cast<V>();
            }

            return Enumerable.Empty<V>();
        }

        /// <inheritdoc/>
        public int GetValueCount(Type key)
        {
            key.ThrowIfNull(nameof(key));
            if (_dictionary.TryGetValue(key, out TypeValues<V> result))
            {
                return result.Count;
            }

            return 0;
        }

        /// <inheritdoc/>
        public bool TryGetValue(Type key, out V value)
        {
            key.ThrowIfNull(nameof(key));
            if (_dictionary.TryGetValue(key, out TypeValues<V> result))
            {
                if (result.IsAmbiguous)
                {
                    throw new AmbiguousTypeException(key);
                }

                value = result.Value;
                return true;
            }
            value = default;
            return false;
        }
    }
}
