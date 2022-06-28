using System;
using System.Collections.Generic;
using Xunit;

namespace AllWorlds.GameEngine.UnitTests;

public sealed class EntityTests
{
    private static Entity EntityFactory(EnEntityType enEntityType)
    {
        return enEntityType switch
        {
            EnEntityType.Basic => new Entity(),
            EnEntityType.OneComponent => new Entity(new Dictionary<Type, Component>
            {
                {new TestComponent1().GetType(), new TestComponent1()}
            }),
            EnEntityType.TwoComponent => new Entity(new Dictionary<Type, Component>
            {
                {new TestComponent1().GetType(), new TestComponent1()},
                {new TestComponent2().GetType(), new TestComponent2()}
            }),
            _ => throw new NotImplementedException()
        };
    }

    private sealed record TestComponent1 : Component;

    private sealed record TestComponent2 : Component; // ReSharper disable twice NotAccessedPositionalProperty.Local

    private sealed record TestComponent3(int ValueOne = 0, int ValueTwo = 0) : Component;

    private sealed record TestComponent4(int TimesHandleDuplicateWasCalled = 0) : Component
    {
        public override Component HandleDuplicate(Component newComponent)
        {
            var newTimesHandleDuplicateWasCalled = TimesHandleDuplicateWasCalled + 1;
            return new TestComponent4(newTimesHandleDuplicateWasCalled);
        }
    }

    private enum EnEntityType
    {
        Basic,
        OneComponent,
        TwoComponent
    }

    #region UpdateComponentByType

    [Fact]
    public void Entity_OnUpdateComponentByType_ComponentExists_ComponentIsUpdated()
    {
        // Arrange
        var testComponent = new TestComponent3();
        var entity = new Entity(new Dictionary<Type, Component> {{typeof(TestComponent3), new TestComponent3()}});

        static TestComponent3 Func(TestComponent3 component3)
        {
            return component3 with {ValueOne = 1};
        }

        // Act
        entity.UpdateComponentByType<TestComponent3>(Func);
        var actualComponent = entity.GetComponentByType<TestComponent3>();
        var expectedComponent = Func(testComponent);

        // Assert
        Assert.Equal(expectedComponent, actualComponent);
    }

    [Fact]
    public void Entity_OnUpdateComponentByType_ComponentDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var entity = new Entity();

        // Act
        void Action()
        {
            entity.UpdateComponentByType<TestComponent3>(static component3 => component3);
        }

        var exception = Record.Exception(Action);

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    #endregion

    #region Construction

    [Fact]
    public void Entity_OnConstruction_ParameterlessConstruction_Works()
    {
        // Arrange
        static void Action()
        {
            var _ = new Entity();
        }

        // Act
        var exception = Record.Exception(Action);

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Entity_OnConstruction_GivenNull_Works()
    {
        // Arrange
        static void Action()
        {
            var _ = new Entity(null);
        }

        // Act
        var exception = Record.Exception(Action);

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Entity_OnConstruction_GivenDictionary_Works()
    {
        // Arrange
        var componentsDict = new Dictionary<Type, Component>();

        void Action()
        {
            var _ = new Entity(componentsDict);
        }

        // Act
        var exception = Record.Exception(Action);

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Entity_OnConstruction_IdGivenUniqueValue()
    {
        // Arrange
        var entities = new HashSet<string>(); // HashSet ensures unique values
        var expected = 1000; // Test with 1000 instances.

        // Act
        for (var i = 0; expected > i; i++)
            entities.Add(new Entity(null).Id); // If a value is already in the HashSet then it's not added.
        var actual = entities.Count;

        // Assert
        Assert.Equal(expected, actual);
    }

    #endregion

    #region UpdateComponents

    [Fact]
    public void Entity_OnUpdateComponents_WhenComponentsChange_OutputsTrueOnlyOnce()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.Basic);

        // Act
        entity.AddComponentOnNextUpdate(new TestComponent1());
        entity.UpdateComponents(out var onAdd);
        entity.UpdateComponents(out var onAddTwo);

        entity.RemoveComponentOnNextUpdate<TestComponent1>();
        entity.UpdateComponents(out var onRemove);
        entity.UpdateComponents(out var onRemoveTwo);

        // Assert
        Assert.True(onAdd);
        Assert.False(onAddTwo);
        Assert.True(onRemove);
        Assert.False(onRemoveTwo);
    }

    [Fact]
    public void Entity_OnUpdateComponents_WhenComponentsNotChanged_OutputsFalse()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.Basic);

        // Act
        entity.UpdateComponents(out var componentsDidChange);

        // Assert
        Assert.False(componentsDidChange);
    }

    #endregion

    #region AddComponentOnNextUpdate

    [Fact]
    public void
        Entity_OnAddComponentOnNextUpdate_AddingComponentThatDoesNotExit_AddsComponentToEntityAfterUpdateComponents()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.Basic);

        bool ComponentCheck()
        {
            return entity.HasComponent<TestComponent1>();
        }

        // Act
        entity.AddComponentOnNextUpdate(new TestComponent1());
        var entityHasComponentsBeforeUpdate = ComponentCheck();
        entity.UpdateComponents(out var _);
        var entityHasComponentsAfterUpdate = ComponentCheck();

        // Assert
        Assert.False(entityHasComponentsBeforeUpdate);
        Assert.True(entityHasComponentsAfterUpdate);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void
        Entity_OnAddComponentOnNextUpdate_AddingComponentOfSameTypeTwice_DelegatesDuplicateHandlingToTheComponent(
            int numberOfDuplicates)
    {
        // Arrange
        var entity = new Entity
        (
            new Dictionary<Type, Component>
            {
                {typeof(TestComponent4), new TestComponent4()}
            }
        );

        // Act
        for (var i = 0; i < numberOfDuplicates; i++) entity.AddComponentOnNextUpdate(new TestComponent4());

        entity.UpdateComponents(out _);
        var newComponent = entity.GetComponentByType<TestComponent4>();

        // Assert
        Assert.Equal(numberOfDuplicates, newComponent.TimesHandleDuplicateWasCalled);
    }

    #endregion

    #region RemoveComponentOnNextUpdate

    [Fact]
    public void
        Entity_OnRemoveComponentOnNextUpdate_RemovingAComponentThatDoesExist_RemovesComponentFromEntityAfterUpdateComponents()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.TwoComponent);

        bool ComponentCheck()
        {
            return entity.HasComponent<TestComponent1>() && entity.HasComponent<TestComponent2>();
        }

        // Act
        entity.RemoveComponentOnNextUpdate<TestComponent1>();
        entity.RemoveComponentOnNextUpdate(new TestComponent2().GetType());
        var entityHasComponentsBeforeUpdate = ComponentCheck();
        entity.UpdateComponents(out var _);
        var entityHasComponentsAfterUpdate = ComponentCheck();

        // Assert
        Assert.True(entityHasComponentsBeforeUpdate);
        Assert.False(entityHasComponentsAfterUpdate);
    }

    [Fact]
    public void Entity_OnRemoveComponentOnNextUpdate_RemovingAComponentThatDoesntExist_ThrowsArgumentException()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.Basic);

        // Act (kind of)
        void Action()
        {
            entity.RemoveComponentOnNextUpdate<TestComponent1>();
        }

        // Assert
        Assert.Throws<ArgumentException>(Action);
    }

    [Fact]
    public void Entity_OnRemoveComponentOnNextUpdate_RemovingAComponentTwice_ThrowsArgumentException()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.OneComponent);

        // Act (kind of)
        void Action()
        {
            entity.RemoveComponentOnNextUpdate<TestComponent1>();
        }

        Action();

        // Assert
        Assert.Throws<ArgumentException>(Action);
    }

    #endregion

    #region GetComponentByType

    [Fact]
    public void Entity_OnGetComponentByTypeGeneric_ComponentExists_ReturnsComponent()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.OneComponent);

        // Act
        var component = entity.GetComponentByType<TestComponent1>();

        // Assert
        Assert.Equal(typeof(TestComponent1), component.GetType());
    }

    [Fact]
    public void Entity_OnGetComponentByTypeGeneric_ComponentDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.Basic);

        void Action()
        {
            entity.GetComponentByType<TestComponent1>();
        }

        // Act
        var exception = Record.Exception(Action);

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    [Fact]
    public void Entity_OnGetComponentByTypeParameter_ComponentExists_ReturnsComponent()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.OneComponent);

        // Act
        var component = entity.GetComponentByType(typeof(TestComponent1));

        // Assert
        Assert.Equal(typeof(TestComponent1), component.GetType());
    }

    [Fact]
    public void Entity_OnGetComponentByTypeParameter_ComponentDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.Basic);

        void Action()
        {
            entity.GetComponentByType(typeof(TestComponent1));
        }

        // Act
        var exception = Record.Exception(Action);

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    #endregion

    #region HasComponent

    [Fact]
    public void Entity_OnHasComponent_EntityHasComponent_ReturnTrue()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.OneComponent);

        // Act
        var entityHasComponentFirstOverload = entity.HasComponent<TestComponent1>();
        var entityHasComponentSecondOverload = entity.HasComponent(typeof(TestComponent1));

        // Assert
        Assert.True(entityHasComponentFirstOverload);
        Assert.True(entityHasComponentSecondOverload);
    }

    [Fact]
    public void Entity_OnHasComponent_EntityDoesNotHaveComponent_ReturnFalse()
    {
        // Arrange
        var entity = EntityFactory(EnEntityType.Basic);

        // Act
        var entityHasComponentFirstOverload = entity.HasComponent<TestComponent1>();
        var entityHasComponentSecondOverload = entity.HasComponent(typeof(TestComponent1));

        // Assert
        Assert.False(entityHasComponentFirstOverload);
        Assert.False(entityHasComponentSecondOverload);
    }

    #endregion
}