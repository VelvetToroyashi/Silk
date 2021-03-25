using System;
using Silk.Core.Types;
using Xunit;

namespace Silk.Core.Tests.Types
{
    public class LoopedListTest
    {
        private readonly LoopedList<int> _loopedIntList = new() {1, 2, 3, 4};
        private readonly LoopedList<int> _emptyIntList = new();
        
        
        [Fact]
        public void LoopedIntList_Allows_Last_Index()
        {
            //Arrange
            int index = _loopedIntList.Count - 1;
            int expected = 4;
            int result;
            //Act
            result = _loopedIntList[index];
            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void LoopedIntList_CyclesNext()
        {
            //Arrange
            int result;
            int expected = 1;
            //Act
            result = _loopedIntList.Next();
            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void LoopedIntList_Returns_Proper_Overflow_Index()
        {
            //Arrange
            int result;
            int expected = 3;
            
            //Act
            result = _loopedIntList[6];
            
            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void LoopedIntList_DoesNot_DivideByZero_When_Empty()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _emptyIntList.Next());
        }
        
        
    }
}