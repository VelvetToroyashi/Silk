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
        private readonly List<int> list = new(_baseList);
        private readonly HashSet<int> hashSet = new(_baseList);
        private readonly SortedSet<int> sortedSet = new(_baseList);
        private readonly LinkedList<int> linkedList = new(_baseList);

        [Benchmark]
        public void ListLookup() => list.Contains(5000);

        [Benchmark]
        public void HashLookup() => hashSet.Contains(5000);

        [Benchmark]
        public void SortedLookup() => sortedSet.Contains(5000);

        [Benchmark]
        public void LinkedLookup() => linkedList.Contains(5000);

    }
}