
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Silk.Core.Types
{
  public delegate Task AsyncTimerDelegate();

  public delegate Task AsyncTimerDelegate<in T1>(T1 t1);

  public delegate Task AsyncTimerDelegate<in T1, in T2>(T1 t1, T2 t2);
  
  
  /// <summary>
  /// An asynchonrous timer that can yield to tasks if necessary.
  /// </summary>
	public sealed class AsyncTimer : IDisposable
  {
    /// <summary>
    /// Whether this timer has been started.
    /// </summary>
    public bool Started => _started;
    
    /// <summary>
    /// Whether the timer yields when executing the callback.
    /// </summary>
    public bool YieldsWhenRunning => _yieldToTask;
    
    /// <summary>
    /// An event fired when the callback throws an exception.
    /// </summary>
    public event EventHandler<Exception> Errored;

    private readonly TimeSpan _interval;

    private readonly bool _yieldToTask;
    
    private bool _started;
    private bool _running;
    
    private readonly Delegate _taskDelegate;
    private readonly object[]? _args = Array.Empty<object>();
    private bool _isDisposed;


    /// <summary>
    /// Constructs an instance of an AsyncTimer.
    /// </summary>
    /// <param name="method">The method to invoke.</param>
    /// <param name="interval">How often the timer should fire.</param>
    /// <param name="yieldToTask">Whether the timer should yield when invoking the callback. This will prevent the callback from being called multiple times if the callback's execution time is greater than the interval.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="method.Target"/> or <paramref name="interval"/> are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the interval is less than zero or greater than <see cref="TimeSpan.MaxValue"/>.</exception>
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

    /// <summary>
    /// Constructs an instance of an AsyncTimer.
    /// </summary>
    /// <param name="method">The method to invoke.</param>
    /// <param name="parameter">A state parameter to pass to <paramref name="method"/> to avoid a closure.</param>
    /// <param name="interval">How often the timer should fire.</param>
    /// <param name="yieldToTask">Whether the timer should yield when invoking the callback. This will prevent the callback from being called multiple times if the callback's execution time is greater than the interval.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="method.Target"/> or <paramref name="interval"/> are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the interval is less than zero or greater than <see cref="TimeSpan.MaxValue"/>.</exception>
    public AsyncTimer(AsyncTimerDelegate<object> method, object parameter, TimeSpan interval, bool yieldToTask = false)
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
      _args = new[] {parameter};
      _yieldToTask = yieldToTask;
    }
    
    /// <summary>
    /// Constructs an instance of an AsyncTimer.
    /// </summary>
    /// <param name="method">The method to invoke.</param>
    /// <param name="parameters">A collection of objects to pass to <paramref name="method"/> to avoid a closure.</param>
    /// <param name="interval">How often the timer should fire.</param>
    /// <param name="yieldToTask">Whether the timer should yield when invoking the callback. This will prevent the callback from being called multiple times if the callback's execution time is greater than the interval.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="method.Target"/> or <paramref name="interval"/> are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the interval is zero or greater than <see cref="TimeSpan.MaxValue"/>.</exception>
    public AsyncTimer(AsyncTimerDelegate<object, object> method, IEnumerable<object>? parameters, TimeSpan interval, bool yieldToTask = false)
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
      _args = parameters as object[] ?? parameters?.ToArray();
      _yieldToTask = yieldToTask;
    }

    /// <summary>
    /// Starts the timer. The callback is executed immediately on the first call.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the timer is already started.</exception>
    public void Start()
    {
      if (_started)
        throw new InvalidOperationException("Timer is already started.");

      _started = true;
      _running = true;
      StartInternal();
    }


    internal async void StartInternal()
    {
      do
      {
        DateTime invoketime = DateTime.UtcNow;
        Task task = _taskDelegate is AsyncTimerDelegate del ? del() : (Task)_taskDelegate.DynamicInvoke(_args)!;
        
        
        _ = task.ContinueWith((t, timer) =>
        {
          var time = Unsafe.As<AsyncTimer>(timer)!;
          time.Errored?.Invoke(time, t.Exception!.Flatten());
        }, this, TaskContinuationOptions.OnlyOnFaulted);
        
        if (_yieldToTask && !task.IsCompleted)
        {
          try { await task; }
          catch { /* Handled in continutation */ }
        }
        /* Else we just let it run in the background. */
        
          
        TimeSpan execTime = DateTime.UtcNow - invoketime;

        if (_interval == TimeSpan.Zero)
          continue;
        
        if (execTime > _interval)
          continue;

        TimeSpan remainingIntervalTime = _interval - execTime;

        await Task.Delay(remainingIntervalTime);
      } while (_running);
    }

    public void Stop()
    {
      if (!_running && !_isDisposed)
        throw new InvalidOperationException("Timer is not running.");
      
      _running = false;
      _started = false;
    }

    public void Dispose()
    {
      _isDisposed = true;
      _running = false;
      _started = false;
    }
  }
}