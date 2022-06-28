using System;
using Xunit;

namespace AllWorlds.GameEngine.UnitTests;

public sealed class EntityBuilderTests
{
    private sealed record TestComponent1 : Component;

    private sealed record TestComponent2 : Component;

    #region AddComponent

    [Fact]
    public void EntityBuilder_OnAddComponent_AddingComponent_DoesNotThrow()
    {
        // Arrange
        var entityBuilder = new EntityBuilder();

        // Act
        var exception = Record.Exception(() => entityBuilder.AddComponent(new TestComponent1()));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EntityBuilder_OnAddComponent_AddingTwoComponentsOfSameClass_ThrowsArgumentException()
    {
        // Arrange
        var entityBuilder = new EntityBuilder();

        void Action()
        {
            entityBuilder.AddComponent(new TestComponent1());
        }

        // Act
        Action();
        var exception = Record.Exception(Action);

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    #endregion

    #region AddComponents

    [Fact]
    public void EntityBuilder_OnAddComponents_AddingComponents_DoesNotThrow()
    {
        // Arrange
        var entityBuilder = new EntityBuilder();
        var componentsToAdd = new Component[] {new TestComponent1(), new TestComponent2()};

        // Act
        var exception = Record.Exception(() => entityBuilder.AddComponents(componentsToAdd));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EntityBuilder_OnAddComponents_AddingTwoInstancesOfTheSameComponentClass_ThrowsArgumentException()
    {
        // Arrange
        var entityBuilder = new EntityBuilder();
        var componentsToAdd = new Component[] {new TestComponent1(), new TestComponent1()};

        // Act
        var exception = Record.Exception(() => entityBuilder.AddComponents(componentsToAdd));

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    #endregion

    #region Construct

    [Fact]
    public void EntityBuilder_OnConstruct_WithNoComponents_ReturnsEntity()
    {
        // Arrange
        var entityBuilder = new EntityBuilder();

        // Act
        var entity = entityBuilder.Construct();

        // Assert
        Assert.Equal(typeof(Entity), entity.GetType());
    }

    [Fact]
    public void EntityBuilder_OnConstruct_WithAddedComponents_ReturnsEntityWithThoseComponents()
    {
        // Arrange
        var entityBuilder = new EntityBuilder();

        // Act
        var entity = entityBuilder.AddComponent(new TestComponent1()).AddComponent(new TestComponent2()).Construct();

        // Assert
        Assert.True(entity.HasComponent<TestComponent1>());
        Assert.True(entity.HasComponent<TestComponent2>());
    }

    #endregion
}