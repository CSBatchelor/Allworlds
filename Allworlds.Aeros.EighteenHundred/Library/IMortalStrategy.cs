using System;
using System.Runtime.CompilerServices;
using AllWorlds.Aeros.EighteenHundred.Components;
using AllWorlds.GameEngine;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("AllWorlds.Aeros.EighteenHundred.UnitTests")]

namespace AllWorlds.Aeros.EighteenHundred.Library;

public interface IMortalStrategy
{
    #region Other

    public bool ShouldEntityBeDead(Entity entity);

    #endregion

    #region Damage

    public Func<MortalComponent, MortalComponent> UpdateMaxDamage(Entity entity);

    public Func<MortalComponent, MortalComponent> UpdateCurrentDamage(Entity entity);
    public bool IsCurrentDamageInvalid(Entity entity);

    public Func<MortalComponent, MortalComponent> FixCurrentDamage(Entity entity);

    #endregion

    #region Stress

    public Func<MortalComponent, MortalComponent> UpdateMaxStress(Entity entity);

    public Func<MortalComponent, MortalComponent> UpdateCurrentStress(Entity entity);

    public bool IsCurrentStressInvalid(Entity entity);

    public Func<MortalComponent, MortalComponent> FixCurrentStress(Entity entity);

    #endregion
}