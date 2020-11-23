using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SilkBot.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Push an object onto the end of a list. Unlike <see cref="List{T}.Add(T)"/>, the last element is changed, instead of adding another element. 
        /// </summary>
        /// <typeparam name="T">The type of list.</typeparam>
        /// <param name="list">This list.</param>
        /// <param name="obj">Object (of type T) to push.</param>
        /// <returns></returns>
        public static T ChangeLast<T>(this IList<T> list, T obj)
        {
            if (list.IsReadOnly) throw new InvalidOperationException($"Cannot push onto readonly list {nameof(list)}");
            if (list.Count == 0) throw new ArgumentOutOfRangeException($"List cannot be empty {nameof(list)}");
            T lastElement = list[list.Count - 1];
            list[list.Count - 1] = obj;
            return lastElement;
        }

        /// <summary>Returns the index of an element contained in a list if it is found, otherwise returns -1.</summary>
        public static int
            IndexOf<T>(this IReadOnlyList<T> list, T element) // IList doesn't implement IndexOf for some reason
        {
            for (var i = 0; i < list.Count; i++)
                if (list[i].Equals(element))
                    return i;
            return -1;
        }

        /// <summary>Fluid method that joins the members of a collection using the specified separator between them.</summary>
        public static string JoinString<T>(this IEnumerable<T> values, string separator = "")
        {
            return string.Join(separator, values);
        }

        /// <summary>Fluid method that joins the members of a collection using the specified separator between them.</summary>
        public static string JoinString<T>(this IEnumerable<T> values, char separator)
        {
            return string.Join(separator, values);
        }

        public static void AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key,
            TValue value)
        {
            dict.AddOrUpdate(key, value, (k, v) => v = value);
        }

        public static IEnumerable<string> WhereMoreThan(this IEnumerable<string> e, int count)
        {
            for (var i = 0; i < e.Count(); i++)
                if (e.ElementAt(i).Length > count)
                    yield return e.ElementAt(i);
        }
    }
}