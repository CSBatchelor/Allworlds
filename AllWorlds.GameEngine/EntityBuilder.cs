using System;
using System.Collections.Generic;
using System.Linq;

namespace AllWorlds.GameEngine {
	public class EntityBuilder {
		private readonly Dictionary<Type, Component> _components = new Dictionary<Type, Component>();

		private void AddComponentToDict(Component component) => _components.Add(component.GetType(), component);

		public EntityBuilder AddComponent(Component component) {
			AddComponentToDict(component);
			return this;
		}

		public EntityBuilder AddComponents(Component[] components) {
			foreach (var component in components) {
				AddComponent(component);
			}
			return this;
		}

		public Entity Construct() => new Entity(_components);
	}
}
