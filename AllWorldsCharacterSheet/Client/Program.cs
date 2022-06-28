using AllWorlds.Aeros.EighteenHundred.Components;
using AllWorlds.Aeros.EighteenHundred.Library;
using AllWorlds.Aeros.EighteenHundred.Systems;
using AllWorlds.GameEngine;
using AllWorldsCharacterSheet.Client;
using AllWorldsCharacterSheet.Shared;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StatsComponent = AllWorlds.Aeros.EighteenHundred.Components.StatsComponent<
	AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.Attributes,
	AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.Skills,
	AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.CombatSkills>;

MortalStrategy? mortalStrategy = new MortalStrategy();

SystemBase[]? systems = new SystemBase[]
{
	new SubscriptionSystem(),
	new MortalSystem(mortalStrategy)
};

Entity? playerEntity = new EntityBuilder()
	.AddComponent(new MortalComponent(10, 0, 10, 0))
	.AddComponent(new StatsComponent(
		new[] {
			new StatsComponent.Attribute(AerosStatEnums.Attributes.Intelligence, 3),
			new StatsComponent.Attribute(AerosStatEnums.Attributes.Will, 4),
			new StatsComponent.Attribute(AerosStatEnums.Attributes.Constitution, 3),
			new StatsComponent.Attribute(AerosStatEnums.Attributes.Fitness, 2)
		},
		new[] {
			new StatsComponent.Skill(AerosStatEnums.Skills.Education, 3),
			new StatsComponent.Skill(AerosStatEnums.Skills.Observation, 4),
			new StatsComponent.Skill(AerosStatEnums.Skills.Persuasion, 3),
			new StatsComponent.Skill(AerosStatEnums.Skills.TechKnowledge, 2),
			new StatsComponent.Skill(AerosStatEnums.Skills.AnimalHandling, 2),
			new StatsComponent.Skill(AerosStatEnums.Skills.Focus, 6),
			new StatsComponent.Skill(AerosStatEnums.Skills.Intimidation, 5),
			new StatsComponent.Skill(AerosStatEnums.Skills.MagicUse, 5),
			new StatsComponent.Skill(AerosStatEnums.Skills.Fortitude, 3),
			new StatsComponent.Skill(AerosStatEnums.Skills.Recovery, 2),
			new StatsComponent.Skill(AerosStatEnums.Skills.Resilience, 6),
			new StatsComponent.Skill(AerosStatEnums.Skills.Stamina, 3),
			new StatsComponent.Skill(AerosStatEnums.Skills.Dexterity, 2),
			new StatsComponent.Skill(AerosStatEnums.Skills.Agility, 4),
			new StatsComponent.Skill(AerosStatEnums.Skills.Sneaking, 3),
			new StatsComponent.Skill(AerosStatEnums.Skills.Strength, 3),
		},
		Array.Empty<StatsComponent.CombatSkill>()))
	.Construct();

Entity[]? entities = new[]
{
	playerEntity
};

EngineRunner? engineRunner = new EngineRunner(systems, entities);

WebAssemblyHostBuilder? builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton(_ => engineRunner);
builder.Services.AddSingleton(_ => playerEntity);

await builder.Build().RunAsync();