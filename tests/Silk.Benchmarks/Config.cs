using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;

namespace Silk.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            Add(MemoryDiagnoser.Default);
            Add(new InliningDiagnoser());
            Add(new EtwProfiler());
            
        }
    }
}