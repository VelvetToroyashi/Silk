
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Silk.Core.Types
{
  public delegate Task AsyncTimerDelegate();

  public delegate Task AsyncTimerDelegate<in T1>(T1 t1);

  public delegate Task AsyncTimerDelegate<in T1, in T2>(T1 t1, T2 t2);
  
  
	public sealed class AsyncTimer
  {
    public bool Started => _started;
    public bool YieldsWhenRunning => _yieldToTask;

    public event EventHandler<Exception> Errored;
    
    private readonly TimeSpan _interval;
    private readonly bool _yieldToTask;
    
    private bool _started;
    private bool _running;
    
    private readonly Delegate _taskDelegate;
    private readonly object[]? _args = Array.Empty<object>();


    public AsyncTimer(AsyncTimerDelegate method, TimeSpan interval, bool yieldToTask = false)
    {
      if (interval == null)
        throw new ArgumentNullException(nameof(interval), "Interval must be non-null.");

      if (interval < TimeSpan.Zero)
        throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than 0.");

      if (interval > TimeSpan.MaxValue)
        throw new ArgumentOutOfRangeException(nameof(interval), $"Interval must be smaller than {nameof(TimeSpan.MaxValue)}");

      if (method.Target is null)
        throw new ArgumentNullException(nameof(method), "Delegate cannot point to null target.");
      
      _interval = interval;
      _yieldToTask = yieldToTask;
      _taskDelegate = method;
    }

    public AsyncTimer(AsyncTimerDelegate<object> method, object paremeter, TimeSpan interval, bool yieldToTask = false)
    {
      if (interval == null)
        throw new ArgumentNullException(nameof(interval), "Interval must be non-null.");

      if (interval < TimeSpan.Zero)
        throw new ArgumentOutOfRangeException(nameof(interval), $"Interval must be greater than {nameof(TimeSpan.Zero)}.");

      if (interval > TimeSpan.MaxValue)
        throw new ArgumentOutOfRangeException(nameof(interval), $"Interval must be smaller than {nameof(TimeSpan.MaxValue)}.");

      if (method.Target is null)
        throw new ArgumentNullException(nameof(method), "Delegate cannot point to null target.");

      _interval = interval;
      _taskDelegate = method;
      _args = new[] {paremeter};
      _yieldToTask = yieldToTask;
    }
    

    public async void Start()
    {
      if (_started)
        throw new InvalidOperationException("Timer is already started.");

      _started = true;
      _running = true;
      do
      {
        DateTime invoketime = DateTime.UtcNow;
        Task task = _taskDelegate is AsyncTimerDelegate del ? del() : (Task)_taskDelegate.DynamicInvoke(_args)!;
        
        if (_yieldToTask && !task.IsCompleted)
        {
          try { await task; }
          catch { /* Handled in continutation */ }
        }
        /* Else we just let it run in the background. */

        _ = task.ContinueWith((t, timer) =>
        {
          if (!t.IsFaulted) return;
          
          var time = Unsafe.As<AsyncTimer>(timer)!;
          time.Errored?.Invoke(time, t.Exception!.Flatten());
        }, this);

          
        TimeSpan execTime = DateTime.UtcNow - invoketime;
        
        if (execTime > _interval)
          continue;

        TimeSpan remainingIntervalTime = _interval - execTime;

        await Task.Delay(remainingIntervalTime);
      } while (_running);
    }

    public void Stop()
    {
      if (!_running)
        throw new InvalidOperationException("Timer is not running.");
      
      _running = false;
      _started = false;
    }
    
  }
}