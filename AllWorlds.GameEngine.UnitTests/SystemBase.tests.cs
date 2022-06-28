using System;
using Moq;
using Xunit;

namespace AllWorlds.GameEngine.UnitTests;

public sealed class SystemBaseTests
{
    [Fact]
    private void SystemBase_OnDeleteEntityOnNextUpdate_DelegatesToTheEntityPool()
    {
        // Arrange
        var entityPoolMock = new Mock<EntityPool>();
        entityPoolMock.Setup(static pool => pool.RemoveEntityOnNextUpdate(It.IsIn<Entity>()));

        var system = new ConcreteSystem();
        system.BindEntityPool(entityPoolMock.Object);

        var entityMock = new Mock<Entity>();

        // Act
        system.DeleteEntityOnNextUpdate(entityMock.Object);

        // Assert
        entityPoolMock.Verify(static pool => pool.RemoveEntityOnNextUpdate(It.IsAny<Entity>()), Times.Once);
    }

    [Fact]
    private void SystemBase_OnCreateEntityOnNextUpdate_DelegatesToTheEntityPool()
    {
        // Arrange
        var entityPoolMock = new Mock<EntityPool>();
        entityPoolMock.Setup(static pool => pool.AddEntityOnNextUpdate(It.IsIn<Entity>()));

        var system = new ConcreteSystem();
        system.BindEntityPool(entityPoolMock.Object);

        var entityMock = new Mock<Entity>();

        // Act
        system.CreateEntityOnNextUpdate(entityMock.Object);

        // Assert
        entityPoolMock.Verify(static pool => pool.AddEntityOnNextUpdate(It.IsAny<Entity>()), Times.Once);
    }

    private sealed class ConcreteSystem : SystemBase
    {
        protected override Type[] RequiredComponents { get; } = {typeof(Component)};

        internal new void DeleteEntityOnNextUpdate(Entity entity)
        {
            base.DeleteEntityOnNextUpdate(entity);
        }

        internal new void CreateEntityOnNextUpdate(Entity entity)
        {
            base.CreateEntityOnNextUpdate(entity);
        }
    }
}