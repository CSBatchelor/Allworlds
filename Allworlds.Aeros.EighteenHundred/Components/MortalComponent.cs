using AllWorlds.GameEngine;

namespace AllWorlds.Aeros.EighteenHundred.Components;

/// <summary>
///     A mortal is anything that can die.
///     A mortal has a physical health pool (CurrentHealth and MaxHealth) and a mental health pool (CurrentStress and
///     MaxStress).
/// </summary>
public sealed record MortalComponent(int MaxDamage, int CurrentDamage, int MaxStress, int CurrentStress) : Component;