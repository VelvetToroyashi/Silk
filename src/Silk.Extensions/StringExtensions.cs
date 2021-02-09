#region

using System;
using System.Collections.Generic;
using System.Linq;

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

        public static string Center(this string text, string anchor)
        {
            int refLength = anchor.Length + text.Where(c => c is '\t').Sum(c => 3);
            int start = (refLength - text.Length) / 2;
            return string.Create(refLength, (start, text), static (Span<char> span, (int start, string str) state) => {
                span.Fill(' ');
                state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
            });
        }
        [Obsolete("Used for benchmark purposes only.")]
        public static string Center_OLD(this string s, string reference)
        {
            int padLength = reference.Length;
            if (s.Contains('\t'))
                foreach (char c in s)
                    if (c is '\t')
                        padLength += 3;
            return s.PadLeft(padLength / 2).PadRight(padLength / 2);
        }
        [Obsolete("Used for benchmark purposes only.")]
        public static string Center_OLD_LINQ(this string s, string reference)
        {
            int padLength = reference.Length;
            if (!s.Contains('\t')) return s.PadLeft(padLength / 2).PadRight(padLength / 2);
            padLength += s.Where(c => c is '\t').Sum(c => 3);
            return s.PadLeft(padLength / 2).PadRight(padLength / 2);
        }
        public static IEnumerable<string> Split(this string s, char delimeter)
        {
            for (var i = 0; i < s.Length; i++)
                if (s[i] == delimeter)
                    yield return s[i..^s.Length].TakeWhile(c => c != delimeter).Join();
        }
    }
}