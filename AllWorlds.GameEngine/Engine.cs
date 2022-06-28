using System;
using System.Collections.Generic;
using AllWorlds.GameEngine;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;

namespace AllWorlds.GameEngine {
	public class Engine : EntityPool {

		#region Properties & Fields

		private readonly HashSet<Entity> entities = new();

		private readonly Dictionary<Type, List<SystemBase>> systemDictionary = new();

		private readonly HashSet<Entity> entitiesToCreate = new();

		private readonly HashSet<Entity> entitiesToDelete = new();

		private readonly HashSet<Entity> updatedEntities = new();

		#endregion

		#region Constructors

		public Engine(IEnumerable<SystemBase> systems) : this(systems, Array.Empty<Entity>()) { }
		public Engine(IEnumerable<SystemBase> systems, IEnumerable<Entity> entities) {
			AddSystems(systems);
			AddEntitiesOnNextUpdate(entities);
			Update();
		}

		#endregion

		#region System

		public bool HasSystem<T>() => HasSystem(typeof(T));

		public bool HasSystem(Type stystemType) => systemDictionary.ContainsKey(stystemType);

		public void AddSystem(SystemBase system)
		{
			var systemType = system.GetType();
			if(systemDictionary.ContainsKey(systemType))
				systemDictionary[systemType].Add(system);
			else
				systemDictionary.Add(system.GetType(), new List<SystemBase> { system });
			system.BindEntityPool(this);
		}

		public void AddSystems(IEnumerable<SystemBase> systems) {
			foreach(var system in systems)
				AddSystem(system);
		}

		private void UpdateSystemRegistration()
		{
			foreach (var entity in updatedEntities)
			{
				foreach (var systems in systemDictionary.Values)
				{
					foreach (var system in systems)
					{
						system.UpdateEntityRegistration(entity);
					}
					
				}
			}
			updatedEntities.Clear();
		}

		private void RunSystems() {
			foreach (var systems in systemDictionary.Values) {
				foreach (var system in systems)
				{
					system.Run();
				}
			}
		}

		#endregion

		#region Entity Code

		public bool HasEntity(Entity entity) => entities.Contains(entity);

		public void AddEntityOnNextUpdate(Entity entity) {
			if (HasEntity(entity)) {
				throw new ArgumentException("Attempted to add an entity that is already in the engine.");
			}

			var entityWasAddedToQueue = entitiesToCreate.Add(entity);

			if (!entityWasAddedToQueue) {
				throw new ArgumentException("Attempted to add the same entity twice.");
			}
		}

		public void AddEntitiesOnNextUpdate(IEnumerable<Entity> entities) {
			foreach (var entity in entities)
				AddEntityOnNextUpdate(entity);
		}

		public void RemoveEntityOnNextUpdate(Entity entity) {
			if (!HasEntity(entity)) {
				throw new ArgumentException("Attempted to delete an entity that does not exist.");
			} else if (entitiesToDelete.Contains(entity)) {
				throw new ArgumentException("Attempted to delete the same entity twice.");
			}
			entitiesToDelete.Add(entity);
		}

		public void RemoveEntitiesOnNextUpdate(IEnumerable<Entity> entities) {
			foreach (var entity in entities)
				RemoveEntityOnNextUpdate(entity);
		}

		private void AddEntities() {
			foreach (var entity in entitiesToCreate) {
				entities.Add(entity);
				updatedEntities.Add(entity);
			}
			entitiesToCreate.Clear();
		}

		private void DeleteEntities() {
			foreach (var entity in entitiesToDelete) {
				entities.Remove(entity);
				updatedEntities.Add(entity);
			}
			entitiesToDelete.Clear();
		}

		#endregion

		#region Component Code
		private void UpdateComponents() {
			foreach (var entity in entities) {
				entity.UpdateComponents(out bool componentsChanged);
				if (componentsChanged) {
					updatedEntities.Add(entity);
				}
			}
			return;
		}

		#endregion

		#region Update
		
		public void Update() {
			AddEntities();
			DeleteEntities();
			UpdateComponents();
			UpdateSystemRegistration();
			RunSystems();
		}

		#endregion

	}
}
