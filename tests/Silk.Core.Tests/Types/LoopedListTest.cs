using System;
using NUnit.Framework;
using Silk.Shared.Types.Collections;

namespace Silk.Core.Tests.Types
{
    public class LoopedListTest
    {
        private readonly LoopedList<int> _emptyIntList = new();
        private readonly LoopedList<int> _loopedIntList = new() {1, 2, 3, 4};

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

        [Test]
        public void LoopedIntList_Setter_Sets_Index_Within_Bounds()
        {
            //Arrange
            int expected = 2;
            int result;
            LoopedList<int> list = new() {1, 2, 3, 4};
            //Act
            result = list[2] = expected;

            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void LoopedIntList_Setter_Sets_Overflowed_Indexer()
        {
            //Arrange
            LoopedList<int> expected = new() {1, 3, 3, 4};
            //Act
            _loopedIntList[5] = 3;
            //Assert
            Assert.AreEqual(expected, _loopedIntList);
        }

        [Test]
        public void LoopedIntList_Setter_Throws_When_Empty()
        {
            //Arrange
            int expected = 2;
            TestDelegate tDelegate;
            //Act
            tDelegate = () => _emptyIntList[0] = expected;
            //Assert
            Assert.Throws<ArgumentOutOfRangeException>(tDelegate);
        }
    }
}