using System;
using System.Collections.Generic;
using AllWorlds.Aeros.EighteenHundred.Components;
using AllWorlds.Aeros.EighteenHundred.Library;
using AllWorlds.Aeros.EighteenHundred.Systems;
using AllWorlds.GameEngine;
using Moq;
using Xunit;

namespace AllWorlds.Aeros.EighteenHundred.UnitTests.Systems;

public sealed class MortalSystemTests
{
    private readonly Mock<Entity> _defaultEntityMock;
    private readonly Mock<EntityPool> _defaultEntityPoolMock;
    private readonly Mock<IMortalStrategy> _defaultMortalStrategyMock;

    public MortalSystemTests()
    {
        var defaultMocks = new DefaultMocks();

        #region _defaultEntityPoolMock Setup

        _defaultEntityPoolMock = new Mock<EntityPool>();
        _defaultEntityPoolMock.Setup(static x => x.HasEntity(It.IsAny<Entity>())).Returns(true);
        _defaultEntityPoolMock.Setup(static x => x.AddEntityOnNextUpdate(It.IsAny<Entity>()));
        _defaultEntityPoolMock.Setup(static x => x.AddEntitiesOnNextUpdate(It.IsAny<IEnumerable<Entity>>()));
        _defaultEntityPoolMock.Setup(static x => x.RemoveEntityOnNextUpdate(It.IsAny<Entity>()));
        _defaultEntityPoolMock.Setup(static x => x.RemoveEntitiesOnNextUpdate(It.IsAny<IEnumerable<Entity>>()));
        _defaultEntityPoolMock.Setup(static x => x.Update());

        #endregion

        #region _defaultMortalStrategyMock Setup

        _defaultMortalStrategyMock = new Mock<IMortalStrategy>();

        _defaultMortalStrategyMock.Setup(static x => x.IsCurrentDamageInvalid(It.IsAny<Entity>())).Returns(false);
        _defaultMortalStrategyMock.Setup(static x => x.FixCurrentDamage(It.IsAny<Entity>())).Returns(static x => x);
        _defaultMortalStrategyMock.Setup(static x => x.UpdateMaxDamage(It.IsAny<Entity>())).Returns(static x => x);
        _defaultMortalStrategyMock.Setup(static x => x.UpdateCurrentDamage(It.IsAny<Entity>())).Returns(static x => x);

        _defaultMortalStrategyMock.Setup(static x => x.IsCurrentStressInvalid(It.IsAny<Entity>())).Returns(false);
        _defaultMortalStrategyMock.Setup(static x => x.FixCurrentStress(It.IsAny<Entity>())).Returns(static x => x);
        _defaultMortalStrategyMock.Setup(static x => x.UpdateMaxStress(It.IsAny<Entity>())).Returns(static x => x);
        _defaultMortalStrategyMock.Setup(static x => x.UpdateCurrentStress(It.IsAny<Entity>())).Returns(static x => x);

        _defaultMortalStrategyMock.Setup(static x => x.ShouldEntityBeDead(It.IsAny<Entity>())).Returns(false);

        #endregion

        _defaultEntityMock = defaultMocks.EntityMock;
    }

    [Fact]
    public void MortalSystem_OnRun_CurrentDamageIsValid_SkipsFix()
    {
        // Arrange
        _defaultMortalStrategyMock.Setup(static x => x.IsCurrentDamageInvalid(It.IsAny<Entity>())).Returns(false);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();


        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.IsCurrentDamageInvalid(It.IsAny<Entity>()), Times.Once);
        _defaultMortalStrategyMock.Verify(static x => x.FixCurrentDamage(It.IsAny<Entity>()), Times.Never);
    }

    [Fact]
    public void MortalSystem_OnRun_CurrentDamageIsInvalid_RunsFix()
    {
        // Arrange
        _defaultMortalStrategyMock.Setup(static x => x.IsCurrentDamageInvalid(It.IsAny<Entity>())).Returns(true);
        Func<MortalComponent, MortalComponent> updateFunc = static x => x with {CurrentDamage = x.CurrentDamage + 1};
        _defaultMortalStrategyMock.Setup(static x => x.FixCurrentDamage(It.IsAny<Entity>())).Returns(updateFunc);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.IsCurrentDamageInvalid(It.IsAny<Entity>()), Times.Once);
        _defaultMortalStrategyMock.Verify(static x => x.FixCurrentDamage(It.IsAny<Entity>()), Times.Once);
        _defaultEntityMock.Verify(x => x.UpdateComponentByType(updateFunc), Times.Once);
    }

    [Fact]
    public void MortalSystem_OnRun_CurrentStressIsValid_SkipsFix()
    {
        // Arrange
        _defaultMortalStrategyMock.Setup(static x => x.IsCurrentStressInvalid(It.IsAny<Entity>())).Returns(false);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.IsCurrentStressInvalid(It.IsAny<Entity>()), Times.Once);
        _defaultMortalStrategyMock.Verify(static x => x.FixCurrentStress(It.IsAny<Entity>()), Times.Never);
    }

    [Fact]
    public void MortalSystem_OnRun_CurrentStressIsInvalid_RunsFix()
    {
        // Arrange
        _defaultMortalStrategyMock.Setup(static x => x.IsCurrentStressInvalid(It.IsAny<Entity>())).Returns(true);
        Func<MortalComponent, MortalComponent> updateFunc = static x => x with {CurrentStress = x.CurrentStress + 1};
        _defaultMortalStrategyMock.Setup(static x => x.FixCurrentStress(It.IsAny<Entity>())).Returns(updateFunc);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.IsCurrentStressInvalid(It.IsAny<Entity>()), Times.Once);
        _defaultMortalStrategyMock.Verify(static x => x.FixCurrentStress(It.IsAny<Entity>()), Times.Once);
        _defaultEntityMock.Verify(x => x.UpdateComponentByType(updateFunc), Times.Once);
    }

    [Fact]
    public void MortalSystem_OnRun_MaxDamageIsUpdated()
    {
        // Arrange
        Func<MortalComponent, MortalComponent> updateFunc = static x => x with {MaxDamage = x.MaxDamage + 1};
        _defaultMortalStrategyMock.Setup(static x => x.UpdateMaxDamage(It.IsAny<Entity>())).Returns(updateFunc);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.UpdateMaxDamage(It.IsAny<Entity>()), Times.Once);
        _defaultEntityMock.Verify(x => x.UpdateComponentByType(updateFunc), Times.Once);
    }

    [Fact]
    public void MortalSystem_OnRun_CurrentDamageIsUpdated()
    {
        // Arrange
        Func<MortalComponent, MortalComponent> updateFunc = static x => x with {CurrentDamage = x.CurrentDamage + 1};
        _defaultMortalStrategyMock.Setup(static x => x.UpdateCurrentDamage(It.IsAny<Entity>())).Returns(updateFunc);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.UpdateCurrentDamage(It.IsAny<Entity>()), Times.Once);
        _defaultEntityMock.Verify(x => x.UpdateComponentByType(updateFunc), Times.Once);
    }

    [Fact]
    public void MortalSystem_OnRun_MaxStressIsUpdated()
    {
        // Arrange
        Func<MortalComponent, MortalComponent> updateFunc = static x => x with {MaxStress = x.MaxStress + 1};
        _defaultMortalStrategyMock.Setup(static x => x.UpdateCurrentDamage(It.IsAny<Entity>())).Returns(updateFunc);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.UpdateMaxStress(It.IsAny<Entity>()), Times.Once);
        _defaultEntityMock.Verify(x => x.UpdateComponentByType(updateFunc), Times.Once);
    }

    [Fact]
    public void MortalSystem_OnRun_CurrentStressIsUpdated()
    {
        // Arrange
        Func<MortalComponent, MortalComponent> updateFunc = static x => x with {CurrentStress = x.CurrentStress + 1};
        _defaultMortalStrategyMock.Setup(static x => x.UpdateCurrentStress(It.IsAny<Entity>())).Returns(updateFunc);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.UpdateCurrentStress(It.IsAny<Entity>()), Times.Once);
        _defaultEntityMock.Verify(x => x.UpdateComponentByType(updateFunc), Times.Once);
    }

    [Fact]
    public void MortalSystem_OnRun_EntityShouldBeDeadAndIsNotDead_EntityIsKilled()
    {
        // Arrange
        _defaultMortalStrategyMock.Setup(static x => x.ShouldEntityBeDead(It.IsAny<Entity>())).Returns(true);
        _defaultEntityMock.Setup(static x => x.HasComponent<DeadComponent>()).Returns(false);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.ShouldEntityBeDead(It.IsAny<Entity>()), Times.Once);
        _defaultEntityMock.Verify(static x => x.HasComponent<DeadComponent>(), Times.Once);
        _defaultEntityMock.Verify(static x => x.AddComponentOnNextUpdate(new DeadComponent()), Times.Once);
        _defaultEntityMock.Verify(static x => x.RemoveComponentOnNextUpdate<DeadComponent>(), Times.Never);
    }

    [Fact]
    public void MortalSystem_OnRun_EntityShouldBeDeadAndIsDead_EntityRemainsDead()
    {
        // Arrange
        _defaultMortalStrategyMock.Setup(static x => x.ShouldEntityBeDead(It.IsAny<Entity>())).Returns(true);
        _defaultEntityMock.Setup(static x => x.HasComponent<DeadComponent>()).Returns(true);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.ShouldEntityBeDead(It.IsAny<Entity>()), Times.Once);
        _defaultEntityMock.Verify(static x => x.HasComponent<DeadComponent>(), Times.Once);
        _defaultEntityMock.Verify(static x => x.AddComponentOnNextUpdate(new DeadComponent()), Times.Never);
        _defaultEntityMock.Verify(static x => x.RemoveComponentOnNextUpdate<DeadComponent>(), Times.Never);
    }

    [Fact]
    public void MortalSystem_OnRun_EntityShouldNotBeDeadAndIsDead_EntityIsRevived()
    {
        // Arrange
        _defaultMortalStrategyMock.Setup(static x => x.ShouldEntityBeDead(It.IsAny<Entity>())).Returns(false);
        _defaultEntityMock.Setup(static x => x.HasComponent<DeadComponent>()).Returns(true);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.ShouldEntityBeDead(It.IsAny<Entity>()), Times.Once);
        _defaultEntityMock.Verify(static x => x.HasComponent<DeadComponent>(), Times.Once);
        _defaultEntityMock.Verify(static x => x.AddComponentOnNextUpdate(new DeadComponent()), Times.Never);
        _defaultEntityMock.Verify(static x => x.RemoveComponentOnNextUpdate<DeadComponent>(), Times.Once);
    }

    [Fact]
    public void MortalSystem_OnRun_EntityShouldNotBeDeadAndIsNotDead_EntityRemainsAlive()
    {
        // Arrange
        _defaultMortalStrategyMock.Setup(static x => x.ShouldEntityBeDead(It.IsAny<Entity>())).Returns(false);
        _defaultEntityMock.Setup(static x => x.HasComponent<DeadComponent>()).Returns(false);
        var system = new MortalSystem(_defaultMortalStrategyMock.Object);
        system.BindEntityPool(_defaultEntityPoolMock.Object);
        system.UpdateEntityRegistration(_defaultEntityMock.Object);

        // Act
        system.Run();

        // Assert
        _defaultMortalStrategyMock.Verify(static x => x.ShouldEntityBeDead(It.IsAny<Entity>()), Times.Once);
        _defaultEntityMock.Verify(static x => x.HasComponent<DeadComponent>(), Times.Once);
        _defaultEntityMock.Verify(static x => x.AddComponentOnNextUpdate(new DeadComponent()), Times.Never);
        _defaultEntityMock.Verify(static x => x.RemoveComponentOnNextUpdate<DeadComponent>(), Times.Never);
    }
}