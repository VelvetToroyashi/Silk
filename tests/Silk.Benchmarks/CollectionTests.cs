using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace Silk.Benchmarks
{
    [MemoryDiagnoser]
    [TailCallDiagnoser]
    [ThreadingDiagnoser]
    public class CollectionTests
    {
        private static readonly IEnumerable<int> _baseList = Enumerable.Range(0, 10000);
        private readonly HashSet<int> hashSet = new(_baseList);
        private readonly LinkedList<int> linkedList = new(_baseList);
        private readonly List<int> list = new(_baseList);
        private readonly SortedSet<int> sortedSet = new(_baseList);

        [Benchmark]
        public void ListForLoop()
        {
            for (var i = 0; i < list.Count; i++)
                _ = list[i];
        }
        [Benchmark]
        public void ListForeachLoop()
        {
            foreach (int i in list)
                _ = i;
        }

        [Benchmark]
        public void HashSetForLoop()
        {
            foreach (int i in hashSet)
                _ = i;
        }
    }
}