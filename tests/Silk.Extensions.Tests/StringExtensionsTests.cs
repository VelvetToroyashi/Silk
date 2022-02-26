using System;
using System.IO;
using NUnit.Framework;

namespace Silk.Extensions.Tests;

public class StringExtensionsTests
{
    private const string CenterInputString = "This string should center!";

    [Theory]
    [TestCase("This is a\ttest string that\tshould be centered!", "             This string should center!             ")]
    [TestCase("This is a test string that should be centered against!", "              This string should center!              ")]
    public void CentersCorrectly(string input, string expected)
    {
        //Act
        string actualCenterWithTabs = CenterInputString.Center(input);
        //Assert
        Assert.AreEqual(expected, actualCenterWithTabs);
    }

    [Test]
    public void DoesNotPadWhenLargerThanReference()
    {
        //Arrange
        const string input  = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
        const string anchor = "This string is too short!";
        //Act
        var actual = input.Center(anchor);
        //Assert
        Assert.AreEqual(input, actual);
    }

    [Test]
    [TestCase("Lorem Ipsum", "", 0)]
    [TestCase("Lorem Ipsum", "Lorem", 5)]
    [TestCase("Lorem Ipsum", "Lorem Ipsum", 12)]
    [TestCase("Lorem Ipsum", "Lorem Ipsum", 13)]
    public void CorrectlyTruncatesString
    (
        string input,
        string expected,
        int maxLength
    )
    {
        //Arrange
        var  range = Range.EndAt(maxLength);
        //Act
        var actual = input.Pull(range);
        //Assert
        Assert.AreEqual(expected, actual);
    }
    
    [Test]
    public void ReturnsPopulatedStream()
    {
        //Arrange
        var     input = "This is a really cool string!";
        Stream? stream;
        //Act
        stream = input.AsStream();
        //Assert
        Assert.IsNotNull(stream);
    }
    
    [Test]
    public void ReturnsEmptyStreamForEmptyInput()
    {
        //Arrange
        var input = "";
        
        //Act
        var stream = input.AsStream();
        
        //Assert
        Assert.AreEqual(0, stream.Length);
    }
    
    [Test]
    public void ReturnsReadableAndSeekableStream()
    {
        //Arrange
        var input = "This is a really cool string!";
        
        //Act
        var stream = input.AsStream();
        
        //Assert
        Assert.IsTrue(stream.CanRead);
        Assert.IsTrue(stream.CanSeek);
    }
}