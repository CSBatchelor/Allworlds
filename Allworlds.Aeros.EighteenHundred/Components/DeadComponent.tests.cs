using System;
using Xunit;

namespace AllWorlds.Aeros.EighteenHundred.Components
{
    public class DeadComponentTests
    {
        [Fact]
        public void DeadComponent_OnHandleDuplicate_ThrowsArgumentException()
        {
            // Arrange
            var componentOne = new DeadComponent();
            var componentTwo = new DeadComponent();

            // Act
            var exception = Record.Exception(() => componentOne.HandleDuplicate(componentTwo));

            // Assert
            Assert.Equal(typeof(ArgumentException), exception?.GetType());
        }
    }
}
