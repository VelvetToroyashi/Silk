using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Silk.Extensions;


namespace Silk.Benchmarks
{
    [MemoryDiagnoser]
    [TailCallDiagnoser]
    public class SilkStringCenterTest
    {
        private const string Input = "Center";
        private const string Anchor = "Longer Center";
        
        
        
        [Benchmark]
        public string NewStringCenter() => Input.Replace("\t", "    ").PadLeft(Anchor.Length / 2 - Input.Length / 2).PadRight(Anchor.Length / 2 - Input.Length / 2);

        [Benchmark]
        public string SpanStringCenterWithLinq() => Input.Center(Anchor);

        [Benchmark]
        public string OldStringCenter() => Input.Center_OLD(Anchor);

        [Benchmark]
        public string OldStringCenterWithLinq() => Input.Center_OLD_LINQ(Anchor);

    }
}