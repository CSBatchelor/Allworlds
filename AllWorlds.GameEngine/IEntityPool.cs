using System.Collections.Generic;

namespace AllWorlds.GameEngine {
	public interface EntityPool {

		bool HasEntity(Entity entity);

		void AddEntityOnNextUpdate(Entity entity);

		void AddEntitiesOnNextUpdate(IEnumerable<Entity> entities);

		void RemoveEntityOnNextUpdate(Entity entity);

		void RemoveEntitiesOnNextUpdate(IEnumerable<Entity> entities);

		void Update();

	}
}
