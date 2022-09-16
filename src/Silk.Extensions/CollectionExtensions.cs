#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Silk.Extensions;

public static class CollectionExtensions
{
    /// <summary>Fluid method that joins the members of a collection using the specified separator between them.</summary>
    public static string Join<T>(this IEnumerable<T> values, string separator = "") => string.Join(separator, values);

    public static TResult? MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        => source.Any() ? source.Max(selector) : default;
}