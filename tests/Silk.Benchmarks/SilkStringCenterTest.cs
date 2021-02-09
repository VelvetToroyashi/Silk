using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Silk.Extensions;

namespace Silk.Benchmarks
{
    [MemoryDiagnoser]
    [TailCallDiagnoser]
    public class SilkStringCenterTest
    {
        private string input = "Center";
        private string anchor = "Longer\tCenter";
        
        [Benchmark]
        public string BaseCenter() => input.Center(anchor);


    }
}