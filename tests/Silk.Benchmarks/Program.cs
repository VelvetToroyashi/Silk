using BenchmarkDotNet.Running;

namespace Silk.Benchmarks
{
    class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TimerVTimerTests>();
        }
    }
}