using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Allworlds.GameEngine.UnitTests")]
[assembly: InternalsVisibleTo("AllWorlds.Aeros.EighteenHundred.UnitTests")]

namespace AllWorlds.GameEngine;

public abstract class SystemBase
{
    private readonly Dictionary<string, Entity> _registeredEntities = new();

    private EntityPool? _entityPool;

    protected abstract Type[] RequiredComponents { get; }

    protected List<Entity> RegisteredEntities => _registeredEntities.Values.ToList();

    internal void BindEntityPool(EntityPool entityPool)
    {
        _entityPool = entityPool;
    }

    private bool EntityHasRequiredComponents(Entity entity)
    {
        return RequiredComponents.All(entity.HasComponent);
    }

    internal bool EntityIsRegistered(Entity entity)
    {
        return _registeredEntities.ContainsKey(entity.Id);
    }

    private void RegisterEntity(Entity entity)
    {
        _registeredEntities.Add(entity.Id, entity);
        OnEntityRegistered(entity);
    }

    private void UnregisterEntity(Entity entity)
    {
        _registeredEntities.Remove(entity.Id);
        OnEntityUnregistered(entity);
    }

    protected virtual void OnEntityRegistered(Entity entity)
    {
    }

    protected virtual void OnUpdate()
    {
    }

    protected virtual void OnEntityUnregistered(Entity entity)
    {
    }

    protected void DeleteEntityOnNextUpdate(Entity entity)
    {
        _entityPool?.RemoveEntityOnNextUpdate(entity);
    }

    protected void CreateEntityOnNextUpdate(Entity entity)
    {
        _entityPool?.AddEntityOnNextUpdate(entity);
    }

    private bool ShouldEntityBeRegistered(Entity entity)
    {
        return !EntityIsRegistered(entity) && EntityHasRequiredComponents(entity) &&
               (_entityPool?.HasEntity(entity) ?? false);
    }

    private bool ShouldEntityBeUnregistered(Entity entity)
    {
        return ((!_entityPool?.HasEntity(entity) ?? false) && EntityIsRegistered(entity)) ||
               (EntityIsRegistered(entity) && !EntityHasRequiredComponents(entity));
    }

    internal void UpdateEntityRegistration(Entity entity)
    {
        if (ShouldEntityBeRegistered(entity))
            RegisterEntity(entity);
        else if (ShouldEntityBeUnregistered(entity)) UnregisterEntity(entity);
    }

    internal void Run()
    {
        OnUpdate();
    }
}