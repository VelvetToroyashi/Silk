﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Shared.Types.Collections
{
    /// <summary>
    /// A dictionary that hides the underlying type that it holds, and casts on-demand.
    /// <remarks> Type and cast safety is not guaranteed by this class. Caution should be taken to ensure that values can be casted, by operator or inheritence.</remarks>
    /// </summary>
    /// <exception cref="System.InvalidCastException"><typeparamref name="TValueFrom"/> is not castable to <typeparamref name="TValueTo"/>.</exception>
    /// <typeparam name="TKey">The type to store as the key.</typeparam>
    /// <typeparam name="TValueFrom">The underlying type to store.</typeparam>
    /// <typeparam name="TValueTo">The type to cast elements to.</typeparam>
    public sealed class CastingDictionary<TKey, TValueFrom, TValueTo> : IDictionary<TKey, TValueTo> where TKey : notnull
    {
        private readonly bool _isCastable;
        private readonly Dictionary<TKey, TValueFrom> _underlyingDict = new();

        public CastingDictionary() : this(false) { }

        /// <param name="isCastableToBaseType">Dictates whether <see cref="TValueTo"/> can be casted back to <see cref="TValueFrom"/>.</param>
        public CastingDictionary(bool isCastableToBaseType)
        {
            _isCastable = isCastableToBaseType;
        }

        public IEnumerator<KeyValuePair<TKey, TValueTo>> GetEnumerator()
        {
            foreach (var (key, value) in _underlyingDict)
                yield return new(key, (TValueTo) (object) value!);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(KeyValuePair<TKey, TValueTo> item) => _underlyingDict.Add(item.Key, (TValueFrom) (object) item.Value!);

        public void Clear() => _underlyingDict.Clear();

        public bool Contains(KeyValuePair<TKey, TValueTo> item) => _isCastable && _underlyingDict.Contains(new(item.Key, (TValueFrom) (object) item.Value!));
        public void CopyTo(KeyValuePair<TKey, TValueTo>[] array, int arrayIndex) { }
        public bool Remove(KeyValuePair<TKey, TValueTo> item) => _underlyingDict.Remove(item.Key);

        public int Count => _underlyingDict.Count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValueTo value) => _underlyingDict.Add(key, (TValueFrom) (object) value!);

        public bool ContainsKey(TKey key) => _underlyingDict.ContainsKey(key);
        public bool Remove(TKey key) => _underlyingDict.Remove(key);
        public bool TryGetValue(TKey key, out TValueTo value)
        {
            var contains = _underlyingDict.TryGetValue(key, out TValueFrom? v);
            value = (TValueTo) (object) v!;
            return contains;
        }

        public TValueTo this[TKey key]
        {
            get => (TValueTo) (object) _underlyingDict[key]!;
            set => _underlyingDict[key] = (TValueFrom) (object) value!;
        }

        public ICollection<TKey> Keys => _underlyingDict.Keys;

        public ICollection<TValueTo> Values => _underlyingDict.Values.Select(v => (TValueTo) (object) v!).ToArray();
    }
}