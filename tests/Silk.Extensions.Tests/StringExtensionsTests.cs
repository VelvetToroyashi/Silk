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
    }
}