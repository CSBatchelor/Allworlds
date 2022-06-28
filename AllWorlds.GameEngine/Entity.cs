using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AllWorlds.Aeros.EighteenHundred.UnitTests")]

namespace AllWorlds.GameEngine;

public class Entity
{
    private readonly Dictionary<Type, Component> components;

    private readonly Dictionary<Type, Component> componentsToAdd = new();

    private readonly HashSet<Type> componentsToRemove = new();

    public Entity() : this(null)
    {
    }

    public Entity([NotNull] Dictionary<Type, Component>? components)
    {
        components ??= new Dictionary<Type, Component>();
        Id = Guid.NewGuid().ToString();
        this.components = components;
    }

    public virtual string Id { get; }

    protected internal virtual void UpdateComponents(out bool componentsChanged)
    {
        componentsChanged = componentsToAdd.Count + componentsToRemove.Count > 0;

        foreach (var component in componentsToAdd)
            AddComponent(component.Value);

        foreach (var componentType in componentsToRemove)
            RemoveComponentByType(componentType);

        componentsToAdd.Clear();
        componentsToRemove.Clear();
    }

    private void RemoveComponent(Component component)
    {
        components.Remove(component.GetType());
    }

    private void RemoveComponentByType(Type componentType)
    {
        components.Remove(componentType);
    }

    private void AddComponent(Component component)
    {
        var componentType = component.GetType();
        if (components.ContainsKey(componentType)) RemoveComponentByType(componentType);
        components.Add(componentType, component);
    }

    public virtual void AddComponentOnNextUpdate(Component component)
    {
        var componentType = component.GetType();
        if (componentsToAdd.ContainsKey(componentType))
        {
            component = componentsToAdd[componentType].HandleDuplicate(component);
            componentsToAdd.Remove(componentType);
        }
        else if (HasComponent(componentType))
        {
            component = components[componentType].HandleDuplicate(component);
        }

        componentsToAdd.Add(componentType, component);
    }

    public virtual void RemoveComponentOnNextUpdate<T>() where T : Component
    {
        RemoveComponentOnNextUpdate(typeof(T));
    }

    public virtual void RemoveComponentOnNextUpdate(Type componentType)
    {
        if (componentsToRemove.Contains(componentType))
            throw new ArgumentException("Attempted to remove a component from this entity twice.");

        if (components.ContainsKey(componentType))
            componentsToRemove.Add(componentType);
        else
            throw new ArgumentException("Attempted to remove a component that does not exist from this entity.");
    }

    public virtual T GetComponentByType<T>() where T : Component
    {
        try
        {
            return (T) components[typeof(T)];
        }
        catch (KeyNotFoundException ex)
        {
            throw new ArgumentException("Attempted to get a component that does not exist from this entity.", ex);
        }
    }

    public virtual Component GetComponentByType(Type componentType)
    {
        try
        {
            return components[componentType];
        }
        catch (KeyNotFoundException ex)
        {
            throw new ArgumentException("Attempted to get a component that does not exist from this entity.", ex);
        }
    }

    public virtual void UpdateComponentByType<T>(Func<T, T> f) where T : Component
    {
        var oldComponent = GetComponentByType<T>();
        var newComponent = f(oldComponent);
        RemoveComponent(oldComponent);
        AddComponent(newComponent);
    }

    public virtual bool HasComponent<T>() where T : Component
    {
        return HasComponent(typeof(T));
    }

    public virtual bool HasComponent(Type componentType)
    {
        return components.ContainsKey(componentType);
    }
}