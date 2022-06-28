using AllWorlds.GameEngine;
using System;

namespace AllWorlds.Aeros.EighteenHundred.Components
{
    /// <summary>
    /// An entity tagged with this component is dead.
    /// </summary>
    public record DeadComponent() : Component
    {
        public override Component HandleDuplicate(Component newComponent)
        {
            throw new ArgumentException("This entity is already dead. You cannot kill a dead entity.");
        }
    }
}
