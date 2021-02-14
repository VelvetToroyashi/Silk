using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Extensions
{
    public static class StringExtensions
    {
        public static string Center(this string text, string anchor)
        {
            int refLength = anchor.Length;

            if (anchor.Contains('\t'))
                refLength += anchor.Count(c => c is '\t') * 3;
            
            if (text.Length >= refLength)
                return text;
            
            int start = (refLength - text.Length) / 2;
            
            return string.Create(refLength, (start, text), static (Span<char> span, (int start, string str) state) => 
                {
                    span.Fill(' ');
                    state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
                });
        }
        
    }
}