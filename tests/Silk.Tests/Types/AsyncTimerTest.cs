using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Silk.Shared.Types;

namespace Silk.Tests.Types;

public class AsyncTimerTest
{
    [Test]
    public void SuccessfullyExecutesTask()
    {
        //Arrange
        var executed = false;
        var timer = new AsyncTimer(() =>
        {
            executed = true;
            return Task.CompletedTask;
        }, TimeSpan.FromSeconds(1));

        //Act
        timer.Start();
        timer.Stop();

        //Assert
        Assert.True(executed);
    }

    [Test]
    public void SuccessfullyExecutesTaskWithParameters()
    {
        //Arrange
        var executed = false;

        var timer = new AsyncTimer(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        }, null!, TimeSpan.FromSeconds(1));

        //Act
        timer.Start();
        timer.Stop();

        //Assert
        Assert.True(executed);
    }

    [Test]
    public void AllowsParallelTaskExecution()
    {
        //Arrange
        var num = 1;

        var timer = new AsyncTimer(async () =>
        {
            num++;
            await Task.Delay(2000);
        }, TimeSpan.FromSeconds(1));

        //Act
        timer.Start();
        Thread.Sleep(1200);
        timer.Stop();

        //Assert
        Assert.AreEqual(3, num);
    }

    [Test]
    public void CanWaitOnTaskCorrectly()
    {
        //Arrange
        var num = 1;
        var timer = new AsyncTimer(async () =>
        {
            num++;
            await Task.Delay(3000);
        }, TimeSpan.FromSeconds(2), true);

        //Act
        timer.Start();
        Thread.Sleep(2500);
        timer.Stop();

        //Assert
        Assert.AreEqual(2, num);
    }

    [Test]
    public void ThrowsWhenAlreadyStarted()
    {
        //Arange
        using var timer = new AsyncTimer(() => Task.FromResult(0), TimeSpan.FromSeconds(1));

        //Act
        timer.Start();
        Thread.Sleep(200);

        //Assert
        Assert.Throws<InvalidOperationException>(() => timer.Start());
    }

    [Test]
    public void ThrowsOnStopIfNotStarted()
    {
        //Arrange
        var timer = new AsyncTimer(() => Task.CompletedTask, TimeSpan.Zero);

        //Act

        //Assert
        Assert.Throws<InvalidOperationException>(() => timer.Stop());
    }

    [Test]
    public void StartingDoesNotThrowWhenTaskThrows()
    {
        //Arrange
        using var timer = new AsyncTimer(() => Task.FromException<Exception>(new()), TimeSpan.FromSeconds(1));

        //Act
        Assert.DoesNotThrow(() => timer.Start());
    }

    [Test]
    public void InvokesErrorHandlerWithDetachedTask()
    {
        //Arrange
        var       errored = false;
        using var timer   = new AsyncTimer(() => Task.FromException<Exception>(new()), TimeSpan.FromSeconds(1));
        timer.Errored += (_, _) => errored = true;

        //Act
        timer.Start();
        Thread.Sleep(100);
        //Assert
        Assert.True(errored);
    }

    [Test]
    public void InvokesErrorHandlerWithAttachedTask()
    {
        //Arrange
        var       errored = false;
        using var timer   = new AsyncTimer(() => Task.FromException<Exception>(new()), TimeSpan.FromSeconds(1));
        timer.Errored += (_, _) => errored = true;

        //Act
        timer.Start();
        Thread.Sleep(100);
        //Assert
        Assert.True(errored);
    }
}