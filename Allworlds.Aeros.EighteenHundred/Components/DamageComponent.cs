using AllWorlds.GameEngine;

namespace AllWorlds.Aeros.EighteenHundred.Components
{
    public record DamageComponent(int Amount, QueueComponent? Next = null) : QueueComponent(Next);
}