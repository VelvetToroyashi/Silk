#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

#endregion

namespace Silk.Extensions
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
            T lastElement = list[^1];
            list[^1] = obj;
            return lastElement;
        }

        /// <summary>Returns the index of an element contained in a list if it is found, otherwise returns -1.</summary>
        public static int IndexOf<T>(this IReadOnlyList<T> list, T element) // IList doesn't implement IndexOf for some reason
        {
            for (var i = 0; i < list.Count; i++)
                if (list[i].Equals(element))
                    return i;
            return -1;
        }

        /// <summary>Fluid method that joins the members of a collection using the specified separator between them.</summary>
        public static string Join<T>(this IEnumerable<T> values, string separator = "")
        {
            return string.Join(separator, values);
        }

        /// <summary>Fluid method that joins the members of a collection using the specified separator between them.</summary>
        public static string Join<T>(this IEnumerable<T> values, char separator)
        {
            return string.Join(separator, values);
        }

        /// <summary>Attempts to parse a unicode or guild emoji from its mention</summary>
        public static DiscordEmoji ToEmoji(this string text)
        {
            var match = Regex.Match(text.Trim(), @"^<?a?:?([a-zA-Z0-9_]+:[0-9]+)>?$");
            return DiscordEmoji.FromUnicode(match.Success ? match.Groups[1].Value : text.Trim());
        }

        public static ulong ToEmojiId(this string emoji)
        {
            var match = Regex.Match(emoji.Trim(), @"^<?a?:([a-zA-Z0-9_]+:[0-9]+)>$");
            if (!match.Success) throw new ArgumentException("Not a valid emoji!");
            int matchIndex = match.Value.LastIndexOf(':');
            string subMatch = emoji.Trim()[++matchIndex..^1];

            if (!ulong.TryParse(subMatch, out ulong result)) throw new ArgumentException("Invalid emoji Id!");

            return result;
        }

        public static void AddOrUpdate<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dict, TKey key,
            TValue value)
        {
            dict.AddOrUpdate(key, value, (k, v) => v = value);
        }
    }
}