using System;
using Xunit;

namespace AllWorlds.GameEngine.UnitTests;

public sealed class QueueComponentTests
{
    [Fact]
    private void QueueComponent_OnConstruction_WithDifferentTypeAsNext_ThrowsNotSupportedException()
    {
        // Arrange
        var derivedQueueComponent = new DerivedQueueComponent(null);

        // Act (kind of)
        void Action()
        {
            var _ = new QueueComponent(derivedQueueComponent);
        }

        // Assert
        Assert.Throws<NotSupportedException>(Action);
    }

    [Fact]
    private void QueueComponent_OnHandleDuplicate_QueueNewComponent()
    {
        // Arrange
        var queueComponent = new QueueComponent(null);

        // Act
        var newQueueComponent = queueComponent.HandleDuplicate(queueComponent);

        // Assert
        Assert.True(newQueueComponent.Next == queueComponent);
    }

    [Fact]
    private void QueueComponent_OnHandleDuplicate_WithDifferentTypeAsNewComponent_ThrowsArgumentException()
    {
        // Arrange
        var queueComponent = new QueueComponent(null);
        var derivedQueueComponent = new DerivedQueueComponent(null);

        // Act (kind of)
        void Action()
        {
            queueComponent.HandleDuplicate(derivedQueueComponent);
        }

        // Assert
        Assert.Throws<ArgumentException>(Action);
    }

    [Fact]
    private void QueueComponent_OnHandleDuplicate_WithNonQueueComponentAsNewComponent_ThrowsArgumentException()
    {
        // Arrange
        var queueComponent = new QueueComponent(null);
        var component = new Component();

        // Act
        void Action()
        {
            queueComponent.HandleDuplicate(component);
        }

        // Assert
        Assert.Throws<ArgumentException>(Action);
    }

    private sealed record DerivedQueueComponent(QueueComponent? Next) : QueueComponent(Next);
}