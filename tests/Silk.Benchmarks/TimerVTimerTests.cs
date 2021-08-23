using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Silk.Core.Types;

namespace Silk.Benchmarks
{
	[MemoryDiagnoser]
	[TailCallDiagnoser]
	[ThreadingDiagnoser]
	public class TimerVTimerTests
	{
		[Benchmark]
		public void ThreadingTimer()
		{
			async void Callback(object? state)
			{
				await Task.Delay(2);
			}

			var t = new Timer(Callback);
			
			Thread.Sleep(5);
			t.Dispose();
		}

		[Benchmark]
		public void AsyncTimerWithoutYield()
		{
			var t = new AsyncTimer(() => Task.Delay(2), TimeSpan.FromMilliseconds(2));
			t.Start();
			Thread.Sleep(5);
			t.Stop();
		}
		
		[Benchmark]
		public void AsyncTimerWithYield()
		{
			var t = new AsyncTimer(() => Task.Delay(2), TimeSpan.FromMilliseconds(2), true);
			t.Start();
			Thread.Sleep(5);
			t.Stop();
		}
	}
}