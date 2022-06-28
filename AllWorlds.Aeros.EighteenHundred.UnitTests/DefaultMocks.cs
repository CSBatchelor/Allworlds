using System;
using AllWorlds.Aeros.EighteenHundred.Components;
using AllWorlds.Aeros.EighteenHundred.Library;
using AllWorlds.GameEngine;
using Moq;
using StatsComponent = AllWorlds.Aeros.EighteenHundred.Components.StatsComponent<
    AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.Attributes,
    AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.Skills,
    AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.CombatSkills>;

namespace AllWorlds.Aeros.EighteenHundred.UnitTests;

internal sealed class DefaultMocks
{
    internal readonly DamageComponent DamageComponent = new(1);
    internal readonly Mock<Entity> EntityMock;
    internal readonly MortalComponent MortalComponent = new(0, 0, 0, 0);
    internal readonly StatsComponent StatsComponent;

    internal DefaultMocks()
    {
        #region EntityMock Setup

        EntityMock = new Mock<Entity>();
        EntityMock.SetupGet(static x => x.Id).Returns("");
        EntityMock.Setup(static x => x.AddComponentOnNextUpdate(It.IsAny<Component>()));
        EntityMock.Setup(static x => x.RemoveComponentOnNextUpdate<Component>());
        EntityMock.Setup(static x => x.RemoveComponentOnNextUpdate(It.IsAny<Type>()));
        EntityMock.Setup(static x => x.GetComponentByType<Component>());
        EntityMock.Setup(static x => x.GetComponentByType(It.IsAny<Type>()));
        EntityMock.Setup(static x => x.UpdateComponentByType(It.IsAny<Func<Component, Component>>()));
        EntityMock.Setup(static x => x.HasComponent<Component>()).Returns(true);
        EntityMock.Setup(static x => x.HasComponent(It.IsAny<Type>())).Returns(true);

        #endregion

        #region StatsComponent Setup

        var attributes = new[]
        {
            new StatsComponent.Attribute(AerosStatEnums.Attributes.Intelligence),
            new StatsComponent.Attribute(AerosStatEnums.Attributes.Will),
            new StatsComponent.Attribute(AerosStatEnums.Attributes.Constitution),
            new StatsComponent.Attribute(AerosStatEnums.Attributes.Fitness)
        };

        var skills = new[]
        {
            new StatsComponent.Skill(AerosStatEnums.Skills.Agility),
            new StatsComponent.Skill(AerosStatEnums.Skills.Attunement),
            new StatsComponent.Skill(AerosStatEnums.Skills.Dexterity),
            new StatsComponent.Skill(AerosStatEnums.Skills.Focus),
            new StatsComponent.Skill(AerosStatEnums.Skills.Fortitude),
            new StatsComponent.Skill(AerosStatEnums.Skills.Intimidation),
            new StatsComponent.Skill(AerosStatEnums.Skills.Lore),
            new StatsComponent.Skill(AerosStatEnums.Skills.Observation),
            new StatsComponent.Skill(AerosStatEnums.Skills.Persuasion),
            new StatsComponent.Skill(AerosStatEnums.Skills.Sneaking),
            new StatsComponent.Skill(AerosStatEnums.Skills.Stamina),
            new StatsComponent.Skill(AerosStatEnums.Skills.Strength),
            new StatsComponent.Skill(AerosStatEnums.Skills.Survival),
            new StatsComponent.Skill(AerosStatEnums.Skills.AnimalHandling),
            new StatsComponent.Skill(AerosStatEnums.Skills.MagicUse),
            new StatsComponent.Skill(AerosStatEnums.Skills.PainTolerance)
        };

        var combatSkills = new[]
        {
            new StatsComponent.CombatSkill(AerosStatEnums.CombatSkills.Archery),
            new StatsComponent.CombatSkill(AerosStatEnums.CombatSkills.Exotic),
            new StatsComponent.CombatSkill(AerosStatEnums.CombatSkills.Shooting),
            new StatsComponent.CombatSkill(AerosStatEnums.CombatSkills.Throwing),
            new StatsComponent.CombatSkill(AerosStatEnums.CombatSkills.Unarmed),
            new StatsComponent.CombatSkill(AerosStatEnums.CombatSkills.OneHanded),
            new StatsComponent.CombatSkill(AerosStatEnums.CombatSkills.TwoHanded)
        };


        StatsComponent = new StatsComponent(attributes, skills, combatSkills);

        #endregion
    }
}