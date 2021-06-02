using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace Silk.Benchmarks
{
    [MemoryDiagnoser]
    [TailCallDiagnoser]
    public class RegexTest
    {
        private const string input = "This is a string that contains Users ({u}), Server name {s} and mentions {@u}!";
        private const string
            server = "Silk!",
            user = "Silk Canary",
            mention = "@Silk Canary";
        private readonly Regex mentionRegex = new(@"({@u[ser]?})", RegexOptions.Compiled);
        private readonly Regex serverRegex = new(@"({s[erver]?})", RegexOptions.Compiled);

        private readonly Regex userRegex = new(@"({u[ser]?)}", RegexOptions.Compiled);



        [Benchmark]
        public void StringReplace()
        {
            _ = input.Replace("{u}", user).Replace("{@u}", mention).Replace("{s}", server);
        }

        [Benchmark]
        public void RegexReplaceUncompiled()
        {
            string placeholder = Regex.Replace(input, @"({u[ser]?)}", user);
            placeholder = Regex.Replace(placeholder, @"({@u[ser]?})", mention);
            _ = Regex.Replace(placeholder, @"({s[erver]?})", server);
        }

        [Benchmark]
        public void RegexReplaceCompiled()
        {
            string placeholder = userRegex.Replace(input, user);
            placeholder = mentionRegex.Replace(placeholder, mention);
            _ = serverRegex.Replace(placeholder, server);
        }
    }
}