using System;
using NUnit.Framework;
using Silk.Shared.Types.Collections;

namespace Silk.Tests.Types;

public class LoopedListTest
{
    private readonly LoopedList<int> _emptyIntList  = new();
    private readonly LoopedList<int> _loopedIntList = new() { 1, 2, 3, 4 };

    [Test]
    public void IndexerCorrectlyReturnsLastElement()
    {
        //Arrange
        int index    = _loopedIntList.Count - 1;
        var expected = 4;
        int result;
        //Act
        result = _loopedIntList[index];
        //Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void NextCorrectlyLoopsToFirstElement()
    {
        //Arrange
        int result;
        var expected = 1;
        //Act
        result = _loopedIntList.Next();
        //Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void IndexerOverflowsCorrectly()
    {
        //Arrange
        int result;
        var expected = 3;

        //Act
        result = _loopedIntList[6];

        //Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void NextThrowsWithoutElements()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _emptyIntList.Next());
    }

    [Test]
    public void IndexerSetsCorrectElement()
    {
        //Arrange
        var             expected = 2;
        int             result;
        LoopedList<int> list = new() { 1, 2, 3, 4 };
        //Act
        result = list[2] = expected;

        //Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void IndexerSetterSetsElementAfterOverflow()
    {
        //Arrange
        LoopedList<int> expected = new() { 1, 3, 3, 4 };
        //Act
        _loopedIntList[5] = 3;
        //Assert
        Assert.AreEqual(expected, _loopedIntList);
    }

    [Test]
    public void IndexerSetterThrowsWhenEmpty()
    {
        //Arrange
        var          expected = 2;
        TestDelegate tDelegate;
        //Act
        tDelegate = () => _emptyIntList[0] = expected;
        //Assert
        Assert.Throws<ArgumentOutOfRangeException>(tDelegate);
    }
}