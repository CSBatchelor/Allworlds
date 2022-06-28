using AllWorlds.GameEngine;
using AllWorlds.Core.Components;
using AllWorlds.Core.Systems;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace TestConsoleApp {
	class Program {
		private static readonly SystemBase[] systems = new SystemBase[] {
			new InputSystem(),
			new MortalSystem(),
			new PoisionSystem(),
			new ConsoleRenderSystem(),
			new ConsoleHudSystem()
		};

		private static readonly Engine engine = new Engine(systems);

		static void Main() {
			var PlayerEntity = new EntityBuilder()
				.AddComponent(new ConsoleVisualComponent('@'))
				.AddComponent(new PositionComponent(10, 10, 1))
				.AddComponent(new PlayerComponent())
				.AddComponent(new MortalComponent())
				.AddComponent(new PoisionComponent())
				.Construct();

			engine.AddEntityOnNextUpdate(PlayerEntity);
			var roomEntities = CreateRoom(5, 5, 15, 15);

			foreach (var entity in roomEntities) {
				engine.AddEntityOnNextUpdate(entity);
			}

			var done = false;

			using var input = new ConsoleInput();
			while (!done) {
				engine.Update(input.Key.ToString());
				Thread.Sleep(1);
			}
		}

		private static Entity[] CreateRoom(int x1, int y1, int x2, int y2) {
			Entity CreateVerticalWall(int x, int y) => new EntityBuilder().AddComponent(new PositionComponent(x, y, 1)).AddComponent(new ConsoleVisualComponent('|')).Construct();
			Entity CreateHorizontalWall(int x, int y) => new EntityBuilder().AddComponent(new PositionComponent(x, y, 1)).AddComponent(new ConsoleVisualComponent('-')).Construct();
			Entity CreateFloor(int x, int y) => new EntityBuilder().AddComponent(new PositionComponent(x, y)).AddComponent(new ConsoleVisualComponent('.')).Construct();
			var entities = new List<Entity>();
			var width = x2 - x1;
			var height = y2 - y1;
			for (var i = 0; i < height; i++) {
				entities.Add(CreateVerticalWall(x1, y1 + i));
				for (var j = 1; j < width; j++) {
					if (i == 0 || i == height - 1) {
						entities.Add(CreateHorizontalWall(x1 + j, y1 + i));
					} else {
						entities.Add(CreateFloor(x1 + j, y1 + i));
					}
				}
				entities.Add(CreateVerticalWall(x2, y1 + i));
			}
			return entities.ToArray();
		}
	}
}