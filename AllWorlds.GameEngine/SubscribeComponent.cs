using System;
using System.Collections.Generic;
using System.Linq;

namespace AllWorlds.GameEngine;

public sealed record SubscribeComponent : Component
{
    public SubscribeComponent(Type componentType, Action<Entity, Component?, Component?> callback) : base(
        new Component())
    {
        SubscriptionDictionary = new Dictionary<Type, Action<Entity, Component?, Component?>[]>
        {
            {componentType, new[] {callback}}
        };
    }

    internal Dictionary<Type, Action<Entity, Component?, Component?>[]> SubscriptionDictionary { get; private init; }

    public override SubscribeComponent HandleDuplicate(Component newComponent)
    {
        if (newComponent is SubscribeComponent newSubscribeComponent)
            // ReSharper disable once WithExpressionModifiesAllMembers
            return this with
            {
                SubscriptionDictionary = SubscriptionDictionary
                    .Concat(newSubscribeComponent.SubscriptionDictionary)
                    .ToLookup(static kvp => kvp.Key, static kvp => kvp.Value)
                    .ToDictionary(static grouping => grouping.Key,
                        static grouping => grouping.SelectMany(static x => x).Distinct().ToArray())
            };


        throw new ArgumentException("Duplicate must be the same type as the SubscribeComponent.");
    }

    public SubscribeComponent Unsubscribe(Type componentType, Action<Entity, Component?, Component?> action)
    {
        if (!SubscriptionDictionary.ContainsKey(componentType) ||
            !SubscriptionDictionary[componentType].Contains(action))
            throw new ArgumentException("Subscription not found.");

        var subscriptionDictionaryCopy =
            SubscriptionDictionary.ToDictionary(static kvp => kvp.Key, static kvp => kvp.Value);
        subscriptionDictionaryCopy[componentType] =
            subscriptionDictionaryCopy[componentType].Where(x => x != action).ToArray();

        // ReSharper disable once WithExpressionModifiesAllMembers
        return this with {SubscriptionDictionary = subscriptionDictionaryCopy};
    }
}