using BenchmarkDotNet.Running;

namespace Silk.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {

            BenchmarkRunner.Run<CollectionTests>();
        }


    }
}