using System;
using NUnit.Framework;
using Silk.Core.Types;

namespace Silk.Core.Tests.Types
{
    public class LoopedListTest
    {
        private readonly LoopedList<int> _loopedIntList = new() {1, 2, 3, 4};
        private readonly LoopedList<int> _emptyIntList = new();

        [Test]
        public void LoopedIntList_Indexer_Accesses_LastElement()
        {
            //Arrange
            int index = _loopedIntList.Count - 1;
            int expected = 4;
            int result;
            //Act
            result = _loopedIntList[index];
            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void LoopedIntList_Next_Returns_FirstElement()
        {
            //Arrange
            int result;
            int expected = 1;
            //Act
            result = _loopedIntList.Next();
            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void LoopedIntList_Indexer_Overflows()
        {
            //Arrange
            int result;
            int expected = 3;

            //Act
            result = _loopedIntList[6];

            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void LoopedIntList_ThrowsWhen_Empty()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _emptyIntList.Next());
        }


    }
}