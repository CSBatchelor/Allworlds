using System;
using System.Collections.Generic;

namespace AllWorlds.GameEngine;

public sealed class SubscriptionSystem : SystemBase
{
	private readonly Dictionary<string, Dictionary<Type, Component?>> lastKnownValues = new();
	protected override Type[] RequiredComponents { get; } = { typeof(SubscribeComponent) };

	protected override void OnEntityRegistered(Entity entity)
	{
		lastKnownValues.Add(entity.Id, new Dictionary<Type, Component?>());
		SubscribeToNewEntity(entity);
	}

	private void SubscribeToNewEntity(Entity entity)
	{
		entity.UpdateComponentByType<SubscribeComponent>(component =>
			component.HandleDuplicate(new SubscribeComponent(typeof(SubscribeComponent), OnSubscribeComponentChanged)));
		OnSubscribeComponentChanged(entity, null, entity.GetComponentByType<SubscribeComponent>());
	}

	private void OnSubscribeComponentChanged(Entity entity, Component? oldComponent,
		Component? newComponent)
	{
		if (newComponent == null)
		{
			return;
		}

		if (oldComponent is not null && oldComponent is not SubscribeComponent)
		{
			throw new ArgumentException("oldComponent is not SubscribeComponent");
		}
		SubscribeComponent? oldSubscribeComponent = (SubscribeComponent?)oldComponent;

		if (newComponent is not SubscribeComponent)
		{
			throw new ArgumentException("newComponent is not SubscribeComponent");
		}
		SubscribeComponent newSubscribeComponent = (SubscribeComponent)newComponent;

		foreach (Type? componentType in newSubscribeComponent.SubscriptionDictionary.Keys)
		{
			if (oldSubscribeComponent is not null && oldSubscribeComponent.SubscriptionDictionary.ContainsKey(componentType))
			{
				continue;
			}

			Component? component = null;
			if (entity.HasComponent(componentType))
			{
				component = entity.GetComponentByType(componentType);
			}

			UpdateLastKnownValue(entity, componentType, component);
		}
	}

	protected override void OnEntityUnregistered(Entity entity)
	{
		lastKnownValues.Remove(entity.Id);
	}

	protected override void OnUpdate()
	{
		foreach (Entity? entity in RegisteredEntities)
		{
			SubscribeComponent? subscribeComponent = entity.GetComponentByType<SubscribeComponent>();
			Type? typeOfSubscribeComponent = typeof(SubscribeComponent);
			if (DidComponentChange(entity, typeOfSubscribeComponent, subscribeComponent))
			{
				TryGetLastKnownValue(entity, typeOfSubscribeComponent, out Component? oldSubscribeComponent);
				UpdateLastKnownValue(entity, typeOfSubscribeComponent, subscribeComponent);
				foreach (Action<Entity, Component?, Component?>? action in subscribeComponent.SubscriptionDictionary[typeOfSubscribeComponent])
				{
					action(entity, oldSubscribeComponent, subscribeComponent);
				}
			}

			foreach (Type? componentType in subscribeComponent.SubscriptionDictionary.Keys)
			{
				if (componentType == typeOfSubscribeComponent)
				{
					continue;
				}

				Component? component = null;
				if (entity.HasComponent(componentType))
				{
					component = entity.GetComponentByType(componentType);
				}

				if (!DidComponentChange(entity, componentType, component))
				{
					continue;
				}

				TryGetLastKnownValue(entity, componentType, out Component? oldComponent);
				UpdateLastKnownValue(entity, componentType, component);
				foreach (Action<Entity, Component?, Component?>? action in subscribeComponent.SubscriptionDictionary[componentType])
				{
					action(entity, oldComponent, component);
				}
			}
		}

		base.OnUpdate();
	}

	private bool TryGetLastKnownValue(Entity entity, Type componentType, out Component? component)
	{
		return lastKnownValues[entity.Id].TryGetValue(componentType, out component);
	}

	private void UpdateLastKnownValue(Entity entity, Type componentType, Component? component)
	{
		lastKnownValues[entity.Id][componentType] = component;
	}

	private bool DidComponentChange(Entity entity, Type componentType, Component? component)
	{
		TryGetLastKnownValue(entity, componentType, out Component? oldComponent);
		return oldComponent != component;
	}
}