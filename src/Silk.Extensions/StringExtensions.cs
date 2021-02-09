#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

#endregion

namespace Silk.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> BlockSplit(this string s, string seperator, int blockSize)
        {
            string[] split = s.Split(seperator);
            for (var i = 0; i < split.Length / blockSize; i++)
                yield return string.Join(string.Empty, split.Skip(i * blockSize).Take(blockSize));
        }

        public static string Pad(this string s, int totalLength)
        {
            string left = s.PadLeft(totalLength / 2);
            return left.PadRight(totalLength / 2);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static string Center(this string text, string anchor)
        {
            int refLength = anchor.Length;
            if (text.Contains('\t'))
            {
                refLength += text.Where(c => c == '\t').Sum(c => 3);
            }

            if (text.Length >= refLength)
                return text;
            
            int start = (refLength - text.Length) / 2;
            
            return string.Create(refLength, (start, text), static (Span<char> span, (int start, string str) state) => 
                {
                    span.Fill(' ');
                    state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
                });
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static string CenterWhereSum(this string text, string anchor)
        {
            int refLength = anchor.Length;
            if (text.Contains('\t'))
            {
                refLength += text.Sum(c => c is '\t' ? 3 : 0);

            }

            if (text.Length >= refLength) return text;
            
            int start = (refLength - text.Length) / 2;
            
            return string.Create(refLength, (start, text), static (Span<char> span, (int start, string str) state) => 
            {
                span.Fill(' ');
                state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
            });
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static string CenterSum(this string text, string anchor)
        {
            int refLength = anchor.Length;
            if (text.Contains('\t'))
            {
                refLength += text.Sum(c => c is '\t' ? 3 : 0);
            }

            if (text.Length >= refLength) return text;
            
            int start = (refLength - text.Length) / 2;
            
            return string.Create(refLength, (start, text), static (Span<char> span, (int start, string str) state) => 
            {
                span.Fill(' ');
                state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
            });
        }
        
        public static IEnumerable<string> Split(this string s, char delimeter)
        {
            for (var i = 0; i < s.Length; i++)
                if (s[i] == delimeter)
                    yield return s[i..^s.Length].TakeWhile(c => c != delimeter).Join();
        }
    }
}