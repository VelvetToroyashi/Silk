using System;
using System.Linq;
using Remora.Results;

namespace Silk.Extensions.Remora;

public static class ResultExtensions
{
    
    /// <summary>
    /// Attempts to unpack an error if it is an <see cref="AggregateError"/>, otherwise returning the error's message directly
    /// </summary>
    /// <param name="result">The result to unpack.</param>
    /// <returns>A string containing either the error's message, or all error messages, if the error is aggregated.</returns>
    public static string TryUnpack(this IResultError result)
        => result is AggregateError ae ? Unpack(ae) : result.Message;
    
    /// <summary>
    /// Unpacks an <see cref="AggregateError"/>, using recursion as necessary.
    /// </summary>
    /// <param name="error">The error to unpack.</param>
    /// <returns>A stringified version of the initial error, delimited by a newline.</returns>
    public static string Unpack(this AggregateError error)
        => error.Errors.Aggregate("", (c, n) => c + Environment.NewLine + (n.Error is AggregateError ae ? Unpack(ae) : n.Error!.Message));

    public static IResultError? GetDeepestError(this IResult error)
        => error.IsSuccess 
            ? error.Error
            : error.Error is AggregateError ag      
                ? GetDeepestError(ag.Errors.First())
                : error.Inner is null 
                    ? error.Error 
                    : GetDeepestError(error.Inner!);
}