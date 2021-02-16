using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Silk.Extensions;

namespace Silk.Benchmarks
{
    [MemoryDiagnoser]
    [TailCallDiagnoser]
    public class SilkStringCenterTest
    {
        private readonly string input = "Center";
        private readonly string anchor = "Longer\tCenter";


        public string CenterWithPad()
        {
            var interpretedAnchor = anchor.Replace("\t", "    ");
            return (input.PadLeft((interpretedAnchor.Length - input.Length) / 2)).PadRight(interpretedAnchor.Length);
        }

        [Benchmark]
        public string CenterWithExtensionMethod() => input.Center(anchor);

        public string CenterWithCountInsteadOfSum()
        {
            int refLength = anchor.Length;
            if (input.Contains('\t'))
            {
                refLength += anchor.Count(c => c is '\t') * 3;

            }

            if (input.Length >= refLength) return input;

            int start = (refLength - input.Length) / 2;

            return string.Create(refLength, (start, input), static(Span<char> span, (int start, string str) state) =>
            {
                span.Fill(' ');
                state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
            });
        }

        public string CenterWithWhereCount()
        {
            int refLength = anchor.Length;
            if (input.Contains('\t'))
            {
                refLength += anchor.Where(c => c is '\t').Count() * 3;

            }

            if (input.Length >= refLength) return input;

            int start = (refLength - input.Length) / 2;

            return string.Create(refLength, (start, input), static(Span<char> span, (int start, string str) state) =>
            {
                span.Fill(' ');
                state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
            });
        }
        
        public string CenterWithForLoop()
        {
            int refLength = anchor.Length;
            if (input.Contains('\t'))
            {
                for (int i = 0; i < anchor.Length; i++)
                    if (anchor[i] is '\t')
                        refLength += 3;

            }

            if (input.Length >= refLength) return input;

            int start = (refLength - input.Length) / 2;

            return string.Create(refLength, (start, input), static(Span<char> span, (int start, string str) state) =>
            {
                span.Fill(' ');
                state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
            });
        }
        
        public string CenterWithForeach()
        {
            int refLength = anchor.Length;
            if (input.Contains('\t'))
            {
                foreach (char t in anchor)
                    if (t is '\t')
                        refLength += 3;

            }

            if (input.Length >= refLength) return input;

            int start = (refLength - input.Length) / 2;

            return string.Create(refLength, (start, input), static(Span<char> span, (int start, string str) state) =>
            {
                span.Fill(' ');
                state.str.AsSpan().CopyTo(span.Slice(state.start, state.str.Length));
            });
        }

    }
}