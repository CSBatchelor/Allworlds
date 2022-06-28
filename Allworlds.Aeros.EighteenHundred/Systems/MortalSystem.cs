using System;
using System.Runtime.CompilerServices;
using AllWorlds.Aeros.EighteenHundred.Components;
using AllWorlds.Aeros.EighteenHundred.Library;
using AllWorlds.GameEngine;

[assembly: InternalsVisibleTo("AllWorlds.Aeros.EighteenHundred.UnitTests")]

namespace AllWorlds.Aeros.EighteenHundred.Systems;

public sealed class MortalSystem : SystemBase
{
    private readonly IMortalStrategy _mortalStrategy;

    public MortalSystem(IMortalStrategy mortalStrategy)
    {
        _mortalStrategy = mortalStrategy;
    }

    protected override Type[] RequiredComponents { get; } =
    {
        typeof(MortalComponent)
    };

    protected override void OnUpdate()
    {
        foreach (var entity in RegisteredEntities)
        {
            if (_mortalStrategy.IsCurrentDamageInvalid(entity))
                entity.UpdateComponentByType(_mortalStrategy.FixCurrentDamage(entity));

            if (_mortalStrategy.IsCurrentStressInvalid(entity))
                entity.UpdateComponentByType(_mortalStrategy.FixCurrentStress(entity));

            entity.UpdateComponentByType(_mortalStrategy.UpdateMaxDamage(entity));
            entity.UpdateComponentByType(_mortalStrategy.UpdateCurrentDamage(entity));
            entity.UpdateComponentByType(_mortalStrategy.UpdateMaxStress(entity));
            entity.UpdateComponentByType(_mortalStrategy.UpdateCurrentStress(entity));

            var shouldEntityBeDead = _mortalStrategy.ShouldEntityBeDead(entity);
            var entityIsDead = entity.HasComponent<DeadComponent>();
            if (shouldEntityBeDead && !entityIsDead)
                entity.AddComponentOnNextUpdate(new DeadComponent());
            else if (!shouldEntityBeDead && entityIsDead)
                entity.RemoveComponentOnNextUpdate<DeadComponent>();
        }
    }
}