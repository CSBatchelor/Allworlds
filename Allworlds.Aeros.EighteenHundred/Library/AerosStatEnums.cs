using AllWorlds.Aeros.EighteenHundred.Components;

namespace AllWorlds.Aeros.EighteenHundred.Library;

public static class AerosStatEnums
{
	public enum Attributes
	{
		Intelligence,
		Will,
		Constitution,
		Fitness
	}

	public enum CombatSkills
	{
		[ParentAttribute((int)Attributes.Intelligence)]
		Archery,

		[ParentAttribute((int)Attributes.Will)]
		Exotic,

		[ParentAttribute((int)Attributes.Fitness)]
		OneHanded,

		[ParentAttribute((int)Attributes.Intelligence)]
		Shooting,

		[ParentAttribute((int)Attributes.Fitness)]
		Throwing,

		[ParentAttribute((int)Attributes.Fitness)]
		TwoHanded,

		[ParentAttribute((int)Attributes.Fitness)]
		Unarmed
	}

	public enum Skills
	{
		[ParentAttribute((int)Attributes.Intelligence)]
		Education,

		[ParentAttribute((int)Attributes.Intelligence)]
		Observation,

		[ParentAttribute((int)Attributes.Intelligence)]
		Persuasion,

		[ParentAttribute((int)Attributes.Intelligence)]
		TechKnowledge,

		[ParentAttribute((int)Attributes.Will)]
		AnimalHandling,

		[ParentAttribute((int)Attributes.Will)]
		Focus,

		[ParentAttribute((int)Attributes.Will)]
		Intimidation,

		[ParentAttribute((int)Attributes.Will)]
		MagicUse,

		[ParentAttribute((int)Attributes.Constitution)]
		Fortitude,

		[ParentAttribute((int)Attributes.Constitution)]
		Recovery,

		[ParentAttribute((int)Attributes.Constitution)]
		Resilience,

		[ParentAttribute((int)Attributes.Constitution)]
		Stamina,

		[ParentAttribute((int)Attributes.Fitness)]
		Dexterity,

		[ParentAttribute((int)Attributes.Fitness)]
		Agility,

		[ParentAttribute((int)Attributes.Fitness)]
		Sneaking,

		[ParentAttribute((int)Attributes.Fitness)]
		Strength
	}
}