using System;
using AllWorlds.Aeros.EighteenHundred.Components;
using AllWorlds.Aeros.EighteenHundred.Library;
using AllWorlds.GameEngine;
using Moq;
using Xunit;
using StatsComponent = AllWorlds.Aeros.EighteenHundred.Components.StatsComponent<
    AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.Attributes,
    AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.Skills,
    AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.CombatSkills>;

namespace AllWorlds.Aeros.EighteenHundred.UnitTests.Library;

public sealed class MortalStrategyTests
{
    private readonly Mock<Entity> _defaultEntityMock;
    private readonly DefaultMocks defaultMocks = new();

    public MortalStrategyTests()
    {
        #region _defaultEntityMock Setup

        _defaultEntityMock = defaultMocks.EntityMock;
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<StatsComponent>())
            .Returns(defaultMocks.StatsComponent);

        #endregion
    }

    [Fact]
    public void MortalStrategy_OnUpdateMaxDamage_EntityDoesNotHaveStats_MaxDamageIsSetToOne()
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        var entityMock = new Mock<Entity>();
        entityMock.Setup(static entity => entity.HasComponent<StatsComponent>()).Returns(false);

        // Act
        var func = mortalStrategy.UpdateMaxDamage(entityMock.Object);
        var newMortalComponent = func(defaultMocks.MortalComponent);

        // Assert
        Assert.Equal(1, newMortalComponent.MaxDamage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void MortalStrategy_OnUpdateMaxDamage_EntityHasStats_MaxDamageIsSetToConstitutionMultipliedByThree
    (
        int constitutionLevel
    )
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        var statsComponent = new StatsComponent(
            new[] {new StatsComponent.Attribute(AerosStatEnums.Attributes.Constitution, constitutionLevel)},
            Array.Empty<StatsComponent.Skill>(),
            Array.Empty<StatsComponent.CombatSkill>());
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<StatsComponent>()).Returns(statsComponent);

        // Act
        var func = mortalStrategy.UpdateMaxDamage(_defaultEntityMock.Object);
        var newMortalComponent = func(defaultMocks.MortalComponent);

        // Assert
        Assert.Equal(statsComponent.Attributes[AerosStatEnums.Attributes.Constitution].Level * 3,
            newMortalComponent.MaxDamage);
    }

    [Fact]
    public void MortalStrategy_OnUpdateCurrentDamage_EntityHasNoIncomingDamage_CurrentDamageIsUnchanged()
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        _defaultEntityMock.Setup(static entity => entity.HasComponent<DeadComponent>()).Returns(false);

        // Act
        var func = mortalStrategy.UpdateCurrentDamage(_defaultEntityMock.Object);
        var newMortalComponent = func(defaultMocks.MortalComponent);

        // Assert
        Assert.Equal(defaultMocks.MortalComponent, newMortalComponent);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void MortalStrategy_OnUpdateCurrentDamage_EntityHasIncomingDamage_CurrentDamageIsChanged(int amountOfDamage)
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        var damageComponent = new DamageComponent(amountOfDamage);
        _defaultEntityMock.Setup(static entity => entity.HasComponent<DamageComponent>()).Returns(true);
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<DamageComponent>())
            .Returns(damageComponent);

        // Act
        var func = mortalStrategy.UpdateCurrentDamage(_defaultEntityMock.Object);
        var newMortalComponent = func(defaultMocks.MortalComponent);

        // Assert
        Assert.Equal(damageComponent.Amount,
            newMortalComponent.CurrentDamage - defaultMocks.MortalComponent.CurrentDamage);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(3, 4)]
    [InlineData(5, 6)]
    public void MortalStrategy_OnUpdateCurrentDamage_EntityHasMultipleDamageComponents_CurrentDamageIsChanged
    (
        int amountOfDamageOne, int amountOfDamageTwo
    )
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        var damageComponent = new DamageComponent(amountOfDamageOne, new DamageComponent(amountOfDamageTwo));
        _defaultEntityMock.Setup(static entity => entity.HasComponent<DamageComponent>()).Returns(true);
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<DamageComponent>())
            .Returns(damageComponent);

        // Act
        var func = mortalStrategy.UpdateCurrentDamage(_defaultEntityMock.Object);
        var newMortalComponent = func(defaultMocks.MortalComponent);

        // Assert
        Assert.Equal(damageComponent.Amount + ((DamageComponent) damageComponent.Next!).Amount,
            newMortalComponent.CurrentDamage - defaultMocks.MortalComponent.CurrentDamage);
    }

    [Fact]
    public void MortalStrategy_OnUpdateCurrentDamage_EntityHasIncomingDamage_DamageComponentIsRemoved()
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        var damageComponent = new DamageComponent(1);
        _defaultEntityMock.Setup(static entity => entity.HasComponent<DamageComponent>()).Returns(true);
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<DamageComponent>())
            .Returns(damageComponent);

        // Act
        var func = mortalStrategy.UpdateCurrentDamage(_defaultEntityMock.Object);
        func(defaultMocks.MortalComponent);

        // Assert
        _defaultEntityMock.Verify(static entity => entity.RemoveComponentOnNextUpdate<DamageComponent>(), Times.Once);
    }

    [Theory]
    [InlineData(1, 2, true)]
    [InlineData(1, 1, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, -1, true)]
    [InlineData(1, 10, true)]
    public void MortalStrategy_OnIsCurrentDamageInvalid_Works(int maxDamage, int currentDamage, bool expected)
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        var mortalComponent = new MortalComponent(maxDamage, currentDamage, 0, 0);
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<MortalComponent>())
            .Returns(mortalComponent);

        // Act
        var actual = mortalStrategy.IsCurrentDamageInvalid(_defaultEntityMock.Object);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1, 2, 1)] // If Max < Current then set Current = Max
    [InlineData(1, 3, 1)]
    [InlineData(1, 4, 1)]
    [InlineData(1, 0, 0)] // If Max > Current and Current >= 0 then set Current = Current
    [InlineData(4, 2, 2)]
    [InlineData(1, -1, 0)] // If Current < 0 then set Current = 0
    [InlineData(1, -19, 0)]
    public void MortalStrategy_OnFixCurrentDamage_Works(int maxDamage, int currentDamage, int expectedNewCurrentDamage)
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        var mortalComponent = new MortalComponent(maxDamage, currentDamage, 0, 0);
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<MortalComponent>())
            .Returns(mortalComponent);

        // Act
        var func = mortalStrategy.FixCurrentDamage(_defaultEntityMock.Object);
        var newMortalComponent = func(mortalComponent);

        // Assert
        Assert.Equal(expectedNewCurrentDamage, newMortalComponent.CurrentDamage);
    }

    [Theory]
    [InlineData(false, 0, 1)] // If no stats, set Max = 1
    [InlineData(true, 1, 4)]
    [InlineData(true, 2, 8)] // If stats, set Max = Fortitude * 4
    [InlineData(true, 3, 12)]
    [InlineData(true, 4, 16)]
    public void MortalStrategy_OnUpdateMaxStress_Works(bool hasStats, int fortitudeLevel, int expectedMaxStress)
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        _defaultEntityMock.Setup(static entity => entity.HasComponent<StatsComponent>()).Returns(hasStats);
        var statsComponent = new StatsComponent(
            Array.Empty<StatsComponent.Attribute>(),
            new[] {new StatsComponent.Skill(AerosStatEnums.Skills.Fortitude, fortitudeLevel)},
            Array.Empty<StatsComponent.CombatSkill>());
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<StatsComponent>()).Returns(statsComponent);

        // Act
        var func = mortalStrategy.UpdateMaxStress(_defaultEntityMock.Object);
        var actualMaxStress = func(defaultMocks.MortalComponent).MaxStress;

        // Assert
        Assert.Equal(expectedMaxStress, actualMaxStress);
    }

    [Theory]
    [InlineData(false, 0, false, 0, 0)]
    [InlineData(true, 1, false, 0, 1)]
    [InlineData(true, 2, false, 0, 2)]
    [InlineData(true, 1, true, 1, 2)]
    [InlineData(true, 2, true, 5, 7)]
    public void MortalStrategy_OnUpdateCurrentStress_Works
    (
        bool hasFirstStressComponent, int firstStressAmount,
        bool hasSecondStressComponent, int secondStressAmount,
        int expectedAmountOfStressTaken
    )
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        _defaultEntityMock.Setup(static entity => entity.HasComponent<StressComponent>())
            .Returns(hasFirstStressComponent);
        var firstStressComponent = new StressComponent(firstStressAmount);
        var secondStressComponent = new StressComponent(secondStressAmount);
        if (hasSecondStressComponent) firstStressComponent = firstStressComponent with {Next = secondStressComponent};
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<StressComponent>())
            .Returns(firstStressComponent);

        // Act
        var func = mortalStrategy.UpdateCurrentStress(_defaultEntityMock.Object);
        var actualAmountOfStressTaken = func(defaultMocks.MortalComponent).CurrentStress -
                                        defaultMocks.MortalComponent.CurrentStress;

        // Assert
        Assert.Equal(expectedAmountOfStressTaken, actualAmountOfStressTaken);
    }

    [Theory]
    [InlineData(1, 2, true)]
    [InlineData(1, 1, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, -1, true)]
    [InlineData(1, 10, true)]
    public void MortalStrategy_OnIsCurrentStressInvalid_Works(int maxStress, int currentStress, bool expected)
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        var mortalComponent = new MortalComponent(0, 0, maxStress, currentStress);
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<MortalComponent>())
            .Returns(mortalComponent);

        // Act
        var actual = mortalStrategy.IsCurrentStressInvalid(_defaultEntityMock.Object);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1, 2, 1)] // If Max < Current then set Current = Max
    [InlineData(1, 3, 1)]
    [InlineData(1, 4, 1)]
    [InlineData(1, 0, 0)] // If Max > Current and Current >= 0 then set Current = Current
    [InlineData(4, 2, 2)]
    [InlineData(1, -1, 0)] // If Current < 0 then set Current = 0
    [InlineData(1, -19, 0)]
    public void MortalStrategy_OnFixCurrentStress_Works(int maxStress, int currentStress, int expectedNewCurrentStress)
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        var mortalComponent = new MortalComponent(0, 0, maxStress, currentStress);
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<MortalComponent>())
            .Returns(mortalComponent);

        // Act
        var func = mortalStrategy.FixCurrentStress(_defaultEntityMock.Object);
        var newMortalComponent = func(mortalComponent);

        // Assert
        Assert.Equal(expectedNewCurrentStress, newMortalComponent.CurrentStress);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, true)]
    [InlineData(1, 0, 1, 0, false)]
    [InlineData(1, 1, 1, 0, true)]
    [InlineData(1, 0, 1, 1, true)]
    public void MortalStrategy_OnShouldEntityBeDead_Works
    (
        int maxDamage, int currentDamage, int maxStress, int currentStress, bool expectedShouldEntityBeDead
    )
    {
        // Arrange
        var mortalStrategy = new MortalStrategy();
        var mortalComponent = new MortalComponent(maxDamage, currentDamage, maxStress, currentStress);
        _defaultEntityMock.Setup(static entity => entity.GetComponentByType<MortalComponent>())
            .Returns(mortalComponent);

        // Act
        var actualShouldEntityBeDead = mortalStrategy.ShouldEntityBeDead(_defaultEntityMock.Object);

        // Assert
        Assert.Equal(expectedShouldEntityBeDead, actualShouldEntityBeDead);
    }
}