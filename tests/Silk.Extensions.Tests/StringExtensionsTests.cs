using System;
using System.IO;
using NUnit.Framework;

namespace Silk.Extensions.Tests
{
    public class StringExtensionsTests
    {
        private const string CenterInputString = "This string should center!";

        [Theory]
        [TestCase("This is a\ttest string that\tshould be centered!", "             This string should center!             ")]
        [TestCase("This is a test string that should be centered against!", "              This string should center!              ")]
        public void Center_ReturnsCenteredString(string input, string expected)
        {
            //Act
            string actualCenterWithTabs = CenterInputString.Center(input);
            //Assert
            Assert.AreEqual(expected, actualCenterWithTabs);
        }

        [Test]
        public void Center_WhenOversizedInput_ReturnsOriginalString()
        {
            //Arrange
            const string input = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
            const string anchor = "This string is too short!";
            //Act
            string actual = input.Center(anchor);
            //Assert
            Assert.AreEqual(input, actual);
        }

        [Test]
        public void Pull_Returns_Entire_String_When_RangeEnd_Exceeds_Length()
        {
            //Arrange
            string input = "This is a short string!";
            string result;
            Range range = ..25;
            //Act
            result = input.Pull(range);
            //Assert
            Assert.AreEqual(input, result);
        }

        [Test]
        public void Pull_Returns_Entire_String_when_RangeStart_Exceeds_Length()
        {
            //Arrange
            string input = "This is a short string!";
            string result;
            Range range = 25..;
            //Act
            result = input.Pull(range);
            //Assert
            Assert.AreEqual(input, result);
        }

        [Test]
        public void Pull_Returns_Substring_When_Range_Does_Not_Exceed_Length()
        {
            //Arrange
            string input = "This is a short string!";
            string expected = "This is a ";
            string result;
            Range range = ..10;
            //Act
            result = input.Pull(range);
            //Assert
            Assert.AreNotEqual(input, result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Pull_Returns_Substring_When_Range_Has_From_End_Defined()
        {
            //Arrange
            string input = "This is a short string!";
            string expected = "This is a short string";
            string result;
            Range range = ..^1;
            //Act
            result = input.Pull(range);
            //Assert
            Assert.AreNotEqual(input, result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AsStream_Returns_Stream_When_String_Is_Not_Null()
        {
            //Arrange
            string input = "This is a really cool string!";
            Stream? stream;
            //Act
            stream = input.AsStream();
            //Assert
            Assert.IsNotNull(stream);
        }
    }
}