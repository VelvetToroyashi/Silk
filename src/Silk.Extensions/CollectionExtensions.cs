#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Silk.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>Returns the index of an element contained in a list if it is found, otherwise returns -1.</summary>
        public static int IndexOf<T>(this IReadOnlyList<T> list, T element) // IList doesn't implement IndexOf for some reason
        {
            for (var i = 0; i < list.Count; i++)
                if (list[i]?.Equals(element) ?? false)
                    return i;
            return -1;
        }

        /// <summary>Fluid method that joins the members of a collection using the specified separator between them.</summary>
        public static string Join<T>(this IEnumerable<T> values, string separator = "")
        {
            return string.Join(separator, values);
        }
        
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize) 
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}