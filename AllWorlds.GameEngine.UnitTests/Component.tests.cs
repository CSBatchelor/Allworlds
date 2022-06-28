using System;
using Xunit;

namespace AllWorlds.GameEngine.UnitTests;

public sealed class ComponentTests
{
    [Fact]
    private void Component_OnHandleDuplicate_ThrowArgumentException()
    {
        // Arrange
        Component component = new();

        // Act (kind of)
        void Action()
        {
            component.HandleDuplicate(component);
        }

        // Assert
        Assert.Throws<ArgumentException>(Action);
    }
}