using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace Silk.Extensions;

public static class StringExtensions
{
    public static string Snake(this string input)
    {
        var size    = input.Length + input.Length / 2;
        var indices = ArrayPool<int>.Shared.Rent(size);
        try
        {
            var index   = 0;
            
            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && char.IsLower(input[i - 1]))
                {
                    indices[index++] = i;
                }
            }

            return string.Create(input.Length + indices.Length, (input, indices), static (span, state) =>
            {
                (var text, var indices) = state;

                var textSpan = text.AsSpan(); 
                span.Fill(' ');
            
                if (indices.Length < 1) // No word boundaries; just copy the span
                {
                    textSpan.CopyTo(span);
                    return;
                }
            
                textSpan[..indices[0]].CopyTo(span[..indices[0]]); // Copy up to the first boundary

                int lastIndex = indices[0];
            
                foreach (int indice in indices)
                {
                    span[lastIndex] = '_';
                    var inSlice  = textSpan.Slice(lastIndex + 1, indice);
                    var outSlice = span.Slice(lastIndex + 1, indice);
                
                    inSlice.CopyTo(outSlice);
                }
            });
        }
        finally
        {
            ArrayPool<int>.Shared.Return(indices);
        }
    }
    
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

        return string.Create(refLength, (start, text), static (Span<char> span, (int start, string str) state) =>
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

        if (range.End.IsFromEnd) return text[range];

        return text[range.Start..Math.Min(text.Length, range.End.Value)];
    }

    public static Stream AsStream(this string s) => new MemoryStream(Encoding.UTF8.GetBytes(s));
}