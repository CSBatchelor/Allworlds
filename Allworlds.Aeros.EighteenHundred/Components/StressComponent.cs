using AllWorlds.GameEngine;

namespace AllWorlds.Aeros.EighteenHundred.Components
{
    public record StressComponent(int Amount, QueueComponent? Next = null) : QueueComponent(Next);
}