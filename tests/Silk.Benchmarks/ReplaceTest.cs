using System;
using System.Linq;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace Silk.Benchmarks
{
    [MemoryDiagnoser]
    [TailCallDiagnoser]
    public class ReplaceTest
    {
        private readonly string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        [Benchmark]
        public string Replace()
        {
            return LoremIpsum.Replace(" ", null);
        }

        [Benchmark]
        public string Except()
        {
            return string.Join(null, LoremIpsum.Except(LoremIpsum.Where(s => s is ' ')).ToArray());
        }

        [Benchmark]
        public string For()
        {
            string s = LoremIpsum;
            var n = new Span<char>(s.ToCharArray());
            var f = 0;
            for (var i = 0; i < s.Length; i++)
                if (s[i] is not ' ')
                {
                    f++;
                    n[i] = s[i];
                }
            n = n.Slice(0, f);
            return n.ToString();
        }


        [Benchmark]
        public string ForUnsafe()
        {
            string s = LoremIpsum;

            Span<char> n = s.AsSpan().AsSpan();
            var f = 0;
            for (var i = 0; i < s.Length; i++)
                if (s[i] is not ' ')
                {
                    f++;
                    n[i] = s[i];
                }
            n = n.Slice(0, f);
            return n.ToString();
        }

        [Benchmark]
        public string OneSpanFor()
        {
            Span<char> s = LoremIpsum.ToArray().AsSpan();

            var f = 0;
            for (var i = 1; i < s.Length; i++)
                if (s[i] is not ' ')
                {
                    f++;
                    s[i++] = s[i];
                }
            s = s.Slice(0, f);
            return s.ToString();
        }

        [Benchmark]
        public string John()
        {
            return LoremIpsum.ReplaceSingleChar(' ', default);
        }
    }

    public static class A
    {
        public static string ReplaceSingleChar(this string s, char old, char @new)
        {
            return string.Create(s.Length, (old, @new), static(buff, pair) =>
            {
                (char old, char @new) = pair;
                foreach (ref char c in buff)
                {
                    if (c == old) c = @new;
                }
            });
        }
        public static Span<T> AsSpan<T>(this ReadOnlySpan<T> t)
        {
            return MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(t), t.Length);
        }
        public static string Replace2n(this string haystack, char needle)
        {
            int ol = haystack.Length;
            int nl = haystack.Length;
            for (int i = ol - 1; i >= 0; --i)
                if (haystack[i] == needle)
                    --nl;

            return string.Create(nl, (needle, haystack), static(buff, st) =>
            {
                (char n, string h) = st;
                int sl = h.Length;
                int sp = buff.Length - 1;
                for (int i = sl - 1; i >= 0; --i)
                    if (h[i] != n)
                        buff[sp--] = h[i];
            });
        }
    }
}