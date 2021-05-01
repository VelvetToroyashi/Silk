using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Shared.Types.Collections
{
    /// <summary>
    /// A dictionary that hides the underlying type that it holds, and casts on-demand.
    /// <remarks>
    /// Type and cast safety are <b><i>not</i></b> guaranteed by this class. Caution should be taken to ensure that values can be casted, by operator or inheritence.
    /// <see cref="Add(System.Collections.Generic.KeyValuePair{TKey,TValueTo})"/> Is not cast-safe, and will throw.
    /// <see cref="Contains"/> will return false if <b><typeparamref name="TValueTo"/></b> is not castable to <b><typeparamref name="TValueFrom"/></b>, however.
    /// </remarks>
    /// </summary>
    /// <exception cref="System.InvalidCastException"> <b><typeparamref name="TValueFrom"/></b> is not castable to <b><typeparamref name="TValueTo"/></b>.</exception>
    /// <typeparam name="TKey">The type to store as the key.</typeparam>
    /// <typeparam name="TValueFrom">The underlying type to store.</typeparam>
    /// <typeparam name="TValueTo">The type to cast elements to.</typeparam>
    public sealed class LazyCastDictionary<TKey, TValueFrom, TValueTo> : IDictionary<TKey, TValueTo> where TKey : notnull
    {
        public bool IsReadOnly => false;
        public int Count => _underlyingDict.Count;

        public TValueTo this[TKey key]
        {
            get => (TValueTo) (object) _underlyingDict[key]!;
            set => _underlyingDict[key] = (TValueFrom) (object) value!;
        }

        private readonly bool _isCastable;
        private readonly Func<TValueFrom, TValueTo> _castFunc = (t) => (TValueTo) (object) t!;

        private readonly IDictionary<TKey, TValueFrom> _underlyingDict = new Dictionary<TKey, TValueFrom>();

        public LazyCastDictionary() : this(false) { }

        public LazyCastDictionary(IDictionary<TKey, TValueFrom> dictionary) : this(false) => _underlyingDict = dictionary;

        public LazyCastDictionary(IDictionary<TKey, TValueFrom> dictionary, Func<TValueFrom, TValueTo> castDelegate) :
            this(dictionary) => _castFunc = castDelegate;

        public LazyCastDictionary(IReadOnlyDictionary<TKey, TValueFrom> dictionary) : this(false)
        {
            var d = dictionary.Select(kvp => kvp);
            _underlyingDict = new Dictionary<TKey, TValueFrom>(d);
        }

        public LazyCastDictionary(IReadOnlyDictionary<TKey, TValueFrom> dictionary, Func<TValueFrom, TValueTo> castDelegate) :
            this(dictionary) => _castFunc = castDelegate;

        /// <param name="isCastableToBaseType">Dictates whether <see cref="TValueTo"/> can be casted back to <see cref="TValueFrom"/>.</param>
        public LazyCastDictionary(bool isCastableToBaseType)
        {
            _isCastable = isCastableToBaseType;
        }

        public IEnumerator<KeyValuePair<TKey, TValueTo>> GetEnumerator() =>
            _underlyingDict.Select(d => new KeyValuePair<TKey, TValueTo>(d.Key, _castFunc(d.Value))).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(KeyValuePair<TKey, TValueTo> item) => _underlyingDict.Add(item.Key, (TValueFrom) (object) item.Value!);

        public void Clear() => _underlyingDict.Clear();

        public bool Contains(KeyValuePair<TKey, TValueTo> item) => _isCastable && _underlyingDict.Contains(new(item.Key, (TValueFrom) (object) item.Value!));
        public void CopyTo(KeyValuePair<TKey, TValueTo>[] array, int arrayIndex) => throw new NotSupportedException("This dictionary does not support copying.");
        public bool Remove(KeyValuePair<TKey, TValueTo> item) => _underlyingDict.Remove(item.Key);

        public void Add(TKey key, TValueTo value) => _underlyingDict.Add(key, (TValueFrom) (object) value!);

        public bool ContainsKey(TKey key) => _underlyingDict.ContainsKey(key);
        public bool Remove(TKey key) => _underlyingDict.Remove(key);
        public bool TryGetValue(TKey key, out TValueTo value)
        {
            var contains = _underlyingDict.TryGetValue(key, out TValueFrom? v);
            value = _castFunc(v!);
            return contains;
        }

        public ICollection<TKey> Keys => _underlyingDict.Keys;

        public ICollection<TValueTo> Values => _underlyingDict.Values.Select(v => _castFunc(v!)).ToArray();
    }
}