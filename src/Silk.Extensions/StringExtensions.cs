using System;
using System.IO;

namespace Silk.Extensions
{
    public static class StringExtensions
    {
        public static string Center(this string text, string anchor)
        {
            int refLength = anchor.Length;

            if (anchor.Contains('\t'))
                foreach (char t in anchor)
                    if (t is '\t')
                        refLength += 3;

            if (text.Length >= refLength)
                return text;

            int start = (refLength - text.Length) / 2;

            return string.Create(refLength, (start, text), static(Span<char> span, (int start, string str) state) =>
            {
                span.Fill(' ');
                state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
            });
        }
        public static string Pull(this string text, Range range)
        {
            if (range.End.Value >= text.Length)
                return text;
            if (range.Start.Value >= text.Length || range.Start.Value < 0)
                return text;

            if (!range.End.IsFromEnd)
            {
                return text[range.Start..Math.Min(text.Length, range.End.Value)];
            }
            else
            {
                return text[range];
            }
        }

        public static Stream AsStream(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(s);
            writer.Flush();

            stream.Position = 0;
            return stream;
        }
    }
}