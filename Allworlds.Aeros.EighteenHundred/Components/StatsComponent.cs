using AllWorlds.GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AllWorlds.Aeros.EighteenHundred.Components;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class ParentAttributeAttribute : Attribute
{
	public ParentAttributeAttribute(int parentAttributeId)
	{
		ParentAttributeId = parentAttributeId;
	}

	public int ParentAttributeId { get; }
}

public sealed record StatsComponent<TAttribute, TSkill, TCombatSkill> : Component
	where TAttribute : Enum
	where TSkill : Enum
	where TCombatSkill : Enum
{
	public StatsComponent(IEnumerable<Attribute> attributes, IEnumerable<Skill> skills,
		IEnumerable<CombatSkill> combatSkills)
	{
		foreach (StatsComponent<TAttribute, TSkill, TCombatSkill>.Attribute? attribute in attributes)
		{
			Attributes.Add(attribute.AttributeEnum, attribute);
		}

		foreach (StatsComponent<TAttribute, TSkill, TCombatSkill>.Skill? skill in skills)
		{
			Skills.Add(skill.SkillEnum, skill);
		}

		foreach (StatsComponent<TAttribute, TSkill, TCombatSkill>.CombatSkill? combatSkill in combatSkills)
		{
			CombatSkills.Add(combatSkill.combatSkillEnum, combatSkill);
		}
	}

	public Dictionary<TAttribute, Attribute> Attributes { get; set; } = new();
	public Dictionary<TSkill, Skill> Skills { get; set; } = new();
	public Dictionary<TCombatSkill, CombatSkill> CombatSkills { get; } = new();

	private static ParentAttributeAttribute GetParentAttribute<T>(T skillEnum) where T : notnull
	{
		ParentAttributeAttribute? parentAttribute = null;
		string? skillString = skillEnum.ToString();
		if (skillString != null)
		{
			System.Reflection.MemberInfo[]? memberInfos = typeof(T).GetMember(skillString);
			System.Reflection.MemberInfo? enumValueMemberInfo = memberInfos.FirstOrDefault(static m => m.DeclaringType == typeof(T));
			if (enumValueMemberInfo != null)
			{
				object[]? valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(ParentAttributeAttribute), false);
				parentAttribute = (ParentAttributeAttribute)valueAttributes[0];
			}
		}

		if (parentAttribute != null)
		{
			return parentAttribute;
		}

		throw new Exception($"{skillString} in {skillEnum.GetType().Name} does not have a valid Parent Attribute.");
	}

	public sealed record Attribute(TAttribute AttributeEnum, int Level = 1, int Experience = 0);

	public sealed record Skill
	{
		public TAttribute AttributeEnum;

		public int Level;

		public Skill(TSkill skillEnum, int level = 1)
		{
			ParentAttributeAttribute? parentAttribute = GetParentAttribute(skillEnum);
			SkillEnum = skillEnum;
			Level = level;
			AttributeEnum = (TAttribute)Enum.ToObject(typeof(TAttribute), parentAttribute.ParentAttributeId);
		}

		public TSkill SkillEnum { get; init; }
	}

	public sealed record CombatSkill(TCombatSkill combatSkillEnum, int Level = 1);
}