using System;
using System.Collections.Generic;

namespace Silk__Extensions
{
    public static class ListExtensions
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
    }
}
