using System;
using System.Threading;
using Remora.Rest.Core;

namespace Silk.Shared.Types;

public class CommandBucket
{
    public Snowflake ID { get; }
    
    public int Limit { get; }

    public  DateTimeOffset ResetTimestampAt => _resetTimestamp;
    private DateTimeOffset _resetTimestamp;

    public           TimeSpan ResetInterval => _resetInterval;
    private readonly TimeSpan _resetInterval;
    
    public  int UsesRemaining => _remaining;
    private int _remaining;

    private readonly SemaphoreSlim _resetLock = new(1);
    

    public CommandBucket(Snowflake ID, int limit, TimeSpan resetInterval)
    {
        this.ID = ID;
        this.Limit = limit;
        this._resetInterval = resetInterval;
        this._remaining = limit;
        this._resetTimestamp = DateTimeOffset.UtcNow.Add(resetInterval);
    }

    public bool Use()
    {
        _resetLock.Wait();
        try
        {
            CheckForReset();
            if (_remaining > 0)
            {
                _remaining--;
                return true;
            }

            return false;
        }
        finally
        {
            _resetLock.Release(); 
        }
    }

    private void CheckForReset()
    {
    
        if (_resetTimestamp <= DateTimeOffset.UtcNow)
        {
            _remaining      = Limit;
            _resetTimestamp = DateTimeOffset.UtcNow.Add(_resetInterval);
        }
    }
}