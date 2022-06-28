using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AllWorlds.GameEngine.UnitTests;

public sealed class EngineTests
{
    private static Entity TestEntity => new EntityBuilder().Construct();

    private static SystemBase[] GetSystems()
    {
        return new SystemBase[] {new TestSystem1()};
    }

    private static Entity[] GetEntities()
    {
        return new[] {new Entity()};
    }

    private sealed class TestSystem1 : SystemBase
    {
        protected override Type[] RequiredComponents { get; } =
        {
            typeof(TestComponent1)
        };
    }

    private sealed record TestComponent1 : Component;

    private sealed record TestComponent2 : Component;

    #region Construction

    [Fact]
    public void Engine_OnConstruction_WithSystems_IsInitializedWithSystems()
    {
        // Arrange
        var systems = GetSystems();
        var engine = new Engine(systems);

        // Act
        var engineHasSystem = engine.HasSystem<TestSystem1>();

        // Assert
        Assert.True(engineHasSystem);
    }

    [Fact]
    public void Engine_OnConstruction_WithEntities_IsInitializedWithEntities()
    {
        // Arrange
        var systems = GetSystems();
        var entities = GetEntities();

        // Act
        var engine = new Engine(systems, entities);
        var engineHasEntity = engine.HasEntity(entities[0]);

        // Assert
        Assert.True(engineHasEntity);
    }

    [Fact]
    public void Engine_OnConstruction_WithTwoSystemsOfSameType_DoesNotThrow()
    {
        // Arrange
        var systems = new SystemBase[] {new TestSystem1(), new TestSystem1()};

        // Act
        var exception = Record.Exception(() => new Engine(systems));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Engine_OnConstruction_WithDuplicateEntities_ThrowsArgumentException()
    {
        // Arrange
        var systems = GetSystems();
        var entity = new Entity();
        var entities = new[] {entity, entity};

        // Act
        var exception = Record.Exception(() => new Engine(systems, entities));

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    #endregion

    #region HasSystem

    [Fact]
    public void Engine_OnHasSystem_UsingGeneric_Works()
    {
        // Arrange
        var systems = GetSystems();
        var engine = new Engine(systems);

        // Act
        var engineHasSystem = engine.HasSystem<TestSystem1>();

        // Assert
        Assert.True(engineHasSystem);
    }

    [Fact]
    public void Engine_OnHasSystem_UsingParameter_Works()
    {
        // Arrange
        var systems = GetSystems();
        var engine = new Engine(systems);

        // Act
        var engineHasSystem = engine.HasSystem(typeof(TestSystem1));

        // Assert
        Assert.True(engineHasSystem);
    }

    [Fact]
    public void Engine_OnHasSystem_WithNonExistentSystem_ReturnsFalse()
    {
        // Arrange
        var systems = Array.Empty<SystemBase>();
        var engine = new Engine(systems);

        // Act
        var engineHasSystem = engine.HasSystem<TestSystem1>();

        // Assert
        Assert.False(engineHasSystem);
    }

    #endregion

    #region AddEntity

    [Fact]
    public void Engine_OnAddEntity_WithEntity_DoesNotThrow()
    {
        // Arrange
        var systems = GetSystems();
        var engine = new Engine(systems);
        var entity = new Entity();

        // Act
        var exception = Record.Exception(() => engine.AddEntityOnNextUpdate(entity));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Engine_OnAddEntity_WithDuplicateInEngine_ThrowsArgumentException()
    {
        // Arrange
        var systems = GetSystems();
        var entities = GetEntities();
        var engine = new Engine(systems, entities);
        var entity = entities[0];

        // Act
        var exception = Record.Exception(() => engine.AddEntityOnNextUpdate(entity));

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    [Fact]
    public void Engine_OnAddEntity_WithDuplicateInQueue_ThrowsArgumentException()
    {
        // Arrange
        var systems = GetSystems();
        var entities = GetEntities();
        var engine = new Engine(systems);
        var entity = entities[0];

        // Act
        engine.AddEntityOnNextUpdate(entities[0]);
        var exception = Record.Exception(() => engine.AddEntityOnNextUpdate(entity));

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    #endregion

    #region DeleteEntity

    [Fact]
    public void Engine_OnDeleteEntity_GivenExistingEntity_DoesNotThrow()
    {
        // Arrange
        var systems = GetSystems();
        var entities = GetEntities();
        var engine = new Engine(systems, entities);
        var entity = entities[0];

        // Act
        var exception = Record.Exception(() => engine.RemoveEntityOnNextUpdate(entity));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Engine_OnDeleteEntity_GivenExistingEntityId_DoesNotThrow()
    {
        // Arrange
        var systems = GetSystems();
        var entities = GetEntities();
        var engine = new Engine(systems, entities);
        var entity = entities[0];

        // Act
        var exception = Record.Exception(() => engine.RemoveEntityOnNextUpdate(entity));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Engine_OnDeleteEntity_GivenDuplicateInQueue_ThrowsArgumentException()
    {
        // Arrange
        var systems = GetSystems();
        var entities = GetEntities();
        var engine = new Engine(systems, entities);
        var entity = entities[0];

        // Act
        engine.RemoveEntityOnNextUpdate(entity);
        var exception = Record.Exception(() => engine.RemoveEntityOnNextUpdate(entity));

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    [Fact]
    public void Engine_OnDeleteEntity_GivenNonExistingEntity_ThrowsArgumentException()
    {
        // Arrange
        var systems = GetSystems();
        var engine = new Engine(systems);

        // Act
        var exception = Record.Exception(() => engine.RemoveEntityOnNextUpdate(new Entity()));

        // Assert
        Assert.Equal(typeof(ArgumentException), exception?.GetType());
    }

    #endregion

    #region Update

    [Fact]
    public void Engine_OnUpdate_WhenEntityIsAdded_IsAddedAfterUpdate()
    {
        // Arrange
        var systems = GetSystems();
        var engine = new Engine(systems);
        var entity = GetEntities()[0];

        // Act
        engine.AddEntityOnNextUpdate(entity);
        var hasEntityBeforeUpdate = engine.HasEntity(entity);
        engine.Update();
        var hasEntityAfterUpdate = engine.HasEntity(entity);

        // Assert
        Assert.False(hasEntityBeforeUpdate);
        Assert.True(hasEntityAfterUpdate);
    }

    [Fact]
    public void Engine_OnUpdate_WhenEntityIsAdded_IsRegisteredAfterUpdate()
    {
        // Arrange
        var systems = GetSystems();
        var engine = new Engine(systems);
        var entity = new Entity(new Dictionary<Type, Component> {{typeof(TestComponent1), new TestComponent1()}});

        // Act
        engine.AddEntityOnNextUpdate(entity);
        var registeredBeforeUpdate = systems[0].EntityIsRegistered(entity);
        engine.Update();
        var registeredAfterUpdate = systems[0].EntityIsRegistered(entity);

        // Assert
        Assert.False(registeredBeforeUpdate);
        Assert.True(registeredAfterUpdate);
    }

    [Fact]
    public void Engine_OnUpdate_WhenEntityIsDeleted_IsDeletedAfterUpdate()
    {
        // Arrange
        var systems = GetSystems();
        var entities = GetEntities();
        var entity = entities[0];
        var engine = new Engine(systems, entities);

        // Act
        engine.RemoveEntityOnNextUpdate(entity);
        var hasEntityBeforeUpdate = engine.HasEntity(entity);
        engine.Update();
        var hasEntityAfterUpdate = engine.HasEntity(entity);

        // Assert
        Assert.True(hasEntityBeforeUpdate);
        Assert.False(hasEntityAfterUpdate);
    }

    [Fact]
    public void Engine_OnUpdate_WhenEntitiesAreDeleted_AreDeletedAfterUpdate()
    {
        // Arrange
        var systems = GetSystems();
        var entities = GetEntities().Concat(new[] {new Entity()}).ToArray();
        var entityOne = entities[0];
        var entityTwo = entities[1];
        var engine = new Engine(systems, entities);

        // Act
        engine.RemoveEntitiesOnNextUpdate(new[] {entityOne, entityTwo});
        var hasEntityOneBeforeUpdate = engine.HasEntity(entityOne);
        var hasEntityTwoBeforeUpdate = engine.HasEntity(entityOne);
        engine.Update();
        var hasEntityOneAfterUpdate = engine.HasEntity(entityOne);
        var hasEntityTwoAfterUpdate = engine.HasEntity(entityOne);

        // Assert
        Assert.True(hasEntityOneBeforeUpdate);
        Assert.False(hasEntityOneAfterUpdate);
        Assert.True(hasEntityTwoBeforeUpdate);
        Assert.False(hasEntityTwoAfterUpdate);
    }

    [Fact]
    public void Engine_OnUpdate_WhenEntityIsDeleted_IsUnregisteredAfterUpdate()
    {
        // Arrange
        var systems = GetSystems();
        var entities = new[]
            {new Entity(new Dictionary<Type, Component> {{typeof(TestComponent1), new TestComponent1()}})};
        var entity = entities[0];
        var engine = new Engine(systems, entities);

        // Act
        engine.RemoveEntityOnNextUpdate(entity);
        var isRegisteredBeforeUpdate = systems[0].EntityIsRegistered(entity);
        engine.Update();
        var isRegisteredAfterUpdate = systems[0].EntityIsRegistered(entity);

        // Assert
        Assert.True(isRegisteredBeforeUpdate);
        Assert.False(isRegisteredAfterUpdate);
    }

    [Fact]
    public void Engine_OnUpdate_WhenEntitiesAreDeleted_AreUnregisteredAfterUpdate()
    {
        // Arrange
        var systems = GetSystems();
        var components = new Dictionary<Type, Component> {{typeof(TestComponent1), new TestComponent1()}};
        var entities = new[] {new Entity(components), new Entity(components)};
        var entityOne = entities[0];
        var entityTwo = entities[1];
        var engine = new Engine(systems, entities);

        // Act
        engine.RemoveEntitiesOnNextUpdate(new[] {entityOne, entityTwo});
        var isEntityOneRegisteredBeforeUpdate = systems[0].EntityIsRegistered(entityOne);
        var isEntityTwoRegisteredBeforeUpdate = systems[0].EntityIsRegistered(entityTwo);
        engine.Update();
        var isEntityOneRegisteredAfterUpdate = systems[0].EntityIsRegistered(entityOne);
        var isEntityTwoRegisteredAfterUpdate = systems[0].EntityIsRegistered(entityTwo);

        // Assert
        Assert.True(isEntityOneRegisteredBeforeUpdate);
        Assert.False(isEntityOneRegisteredAfterUpdate);
        Assert.True(isEntityTwoRegisteredBeforeUpdate);
        Assert.False(isEntityTwoRegisteredAfterUpdate);
    }

    [Fact]
    public void Engine_OnUpdate_WhenAddingComponentToEntity_IsAddedAfterUpdate()
    {
        // Arrange
        var entities = GetEntities();
        var systems = GetSystems();
        var entity = entities[0];
        var engine = new Engine(systems, entities);

        // Act
        entity.AddComponentOnNextUpdate(new TestComponent2());
        var hasComponentBeforeUpdate = entity.HasComponent<TestComponent2>();
        engine.Update();
        var hasComponentAfterUpdate = entity.HasComponent(typeof(TestComponent2));

        // Assert
        Assert.False(hasComponentBeforeUpdate);
        Assert.True(hasComponentAfterUpdate);
    }

    [Fact]
    public void Engine_OnUpdate_WhenAddingComponentToEntity_IsRegisteredAfterUpdate()
    {
        // Arrange
        var systems = GetSystems();
        var entity = TestEntity;
        var entities = new[]
        {
            entity
        };
        var engine = new Engine(systems, entities);

        // Act
        entity.AddComponentOnNextUpdate(new TestComponent1());
        var registeredBeforeUpdate = systems[0].EntityIsRegistered(entity);
        engine.Update();
        var registeredAfterUpdate = systems[0].EntityIsRegistered(entity);

        // Assert
        Assert.False(registeredBeforeUpdate);
        Assert.True(registeredAfterUpdate);
    }

    [Fact]
    public void Engine_OnUpdate_WhenRemovingComponentFromEntity_IsRemovedAfterUpdate()
    {
        // Arrange
        var entities = GetEntities();
        var systems = GetSystems();
        entities[0] = new EntityBuilder().AddComponent(new TestComponent1()).Construct();
        var entity = entities[0];
        var engine = new Engine(systems, entities);

        // Act
        entity.RemoveComponentOnNextUpdate<TestComponent1>();
        var hasComponentBeforeUpdate = entity.HasComponent<TestComponent1>();
        engine.Update();
        var hasComponentAfterUpdate = entity.HasComponent(typeof(TestComponent1));

        // Assert
        Assert.True(hasComponentBeforeUpdate);
        Assert.False(hasComponentAfterUpdate);
    }

    [Fact]
    public void Engine_OnUpdate_WhenRemovingComponentFromEntity_UnregistersAfterUpdate()
    {
        // Arrange
        var systems = GetSystems();
        var entities = GetEntities();
        entities[0] = new EntityBuilder().AddComponent(new TestComponent1()).Construct();
        var entity = entities[0];
        var engine = new Engine(systems, entities);

        // Act
        entity.RemoveComponentOnNextUpdate<TestComponent1>();
        var unregisteredBeforeUpdate = systems[0].EntityIsRegistered(entity);
        engine.Update();
        var unregisteredAfterUpdate = systems[0].EntityIsRegistered(entity);

        // Assert
        Assert.True(unregisteredBeforeUpdate);
        Assert.False(unregisteredAfterUpdate);
    }

    #endregion
}