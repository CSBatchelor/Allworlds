using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AllWorlds.GameEngine;
using System.Timers;

namespace AllWorldsCharacterSheet.Shared
{
    public class EngineRunner : IDisposable
    {
        private readonly Engine _engine;

        private readonly Task _task;

        public bool IsRunning = false;

        public EngineRunner(IEnumerable<SystemBase> systems, IEnumerable<Entity> entities)
        {
            _engine = new Engine(systems, entities);
            _task = GetTask();
            Start();
        }
        private Task GetTask() => new(async () =>
        {
            while (IsRunning)
            {
                try
                {
                    _engine.Update();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                await Task.Delay(1);
            }
        });

        public void Start()
        {
            if (IsRunning == false)
            {
                IsRunning = true;
                _task.Start();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
            }
        }

        public void Dispose()
        {
            if (IsRunning)
                Stop();

            _task.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
