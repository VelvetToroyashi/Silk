using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Engines;

namespace Silk.Benchmarks
{
    [MemoryDiagnoser]
    [TailCallDiagnoser]
    public class SilkStringBlockSplitTest
    {
        private const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        private readonly Consumer consumer = new();

        [Benchmark]
        public void BlockSplitLINQSkipTake() => _ = BlockSplitLINQSkipAndTake();
        [Benchmark]
        public void BlockSplitWithRange() => _ = BlockSplitRangeOperator();

        public IEnumerable<string> BlockSplitLINQSkipAndTake()
        {
            string[] split = LoremIpsum.Split(',');
            for (int i = 0; i < split.Length / 4; i++)
                yield return string.Join(string.Empty, split.Skip(i * 4).Take(4));
        }


        public IEnumerable<string> BlockSplitRangeOperator()
        {
            string[] split = LoremIpsum.Split(',');
            for (int i = 0; i < split.Length / 4; i++)
                yield return string.Join(string.Empty, split[(i * 4)..(i * 4 + (4 + 1))]);
        }


    }
}