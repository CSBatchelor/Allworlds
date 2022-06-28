using AllWorlds.GameEngine;
using System;
using AllWorlds.Aeros.EighteenHundred.Components;
using StatsComponent = AllWorlds.Aeros.EighteenHundred.Components.StatsComponent<AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.Attributes, AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.Skills, AllWorlds.Aeros.EighteenHundred.Library.AerosStatEnums.CombatSkills>;
using MortalComponent = AllWorlds.Aeros.EighteenHundred.Components.MortalComponent;

namespace AllWorlds.Aeros.EighteenHundred.Library
{
    public sealed class MortalStrategy : IMortalStrategy
    {
        #region Damage

        #region Public

        public Func<MortalComponent, MortalComponent> UpdateMaxDamage(Entity entity)
            => mortalComponent =>
                mortalComponent with {MaxDamage = CalculateMaxDamage(entity)};

        public Func<MortalComponent, MortalComponent> UpdateCurrentDamage(Entity entity)
        {
            if (!entity.HasComponent<DamageComponent>())
                return static mortalComponent
                    => mortalComponent with {CurrentDamage = mortalComponent.CurrentDamage};

            var damage = 0;
            var damageComponent = entity.GetComponentByType<DamageComponent>();
            while (damageComponent != null)
            {
                damage += damageComponent.Amount;
                damageComponent = (DamageComponent?) damageComponent.Next;
            }

            entity.RemoveComponentOnNextUpdate<DamageComponent>();
            return mortalComponent => mortalComponent with {CurrentDamage = mortalComponent.CurrentDamage + damage};
        }

        public bool IsCurrentDamageInvalid(Entity entity)
            => IsCurrentDamageMoreThanMax(entity) || IsCurrentDamageLessThanZero(entity);

        public Func<MortalComponent, MortalComponent> FixCurrentDamage(Entity entity)
            => mortalComponent =>
            {
                if (IsCurrentDamageMoreThanMax(entity))
                    mortalComponent = mortalComponent with {CurrentDamage = mortalComponent.MaxDamage};
                else if (IsCurrentDamageLessThanZero(entity))
                    mortalComponent = mortalComponent with {CurrentDamage = 0};

                return mortalComponent;
            };

        #endregion

        #region Private

        private static int CalculateMaxDamage(Entity entity)
        {
            if (!entity.HasComponent<StatsComponent>()) return 1;

            var stats = entity.GetComponentByType<StatsComponent>();
            return stats.Attributes[AerosStatEnums.Attributes.Constitution].Level * 3;
        }

        private static bool IsCurrentDamageMoreThanMax(Entity entity)
        {
            var mortalComponent = entity.GetComponentByType<MortalComponent>();
            return mortalComponent.CurrentDamage > mortalComponent.MaxDamage;
        }

        private static bool IsCurrentDamageLessThanZero(Entity entity)
        {
            var mortalComponent = entity.GetComponentByType<MortalComponent>();
            return mortalComponent.CurrentDamage < 0;
        }

        #endregion

        #endregion

        #region Stress

        #region Public

        public Func<MortalComponent, MortalComponent> UpdateMaxStress(Entity entity)
            => mortalComponent => mortalComponent with {MaxStress = CalculateMaxStress(entity)};

        public Func<MortalComponent, MortalComponent> UpdateCurrentStress(Entity entity)
        {
            if (!entity.HasComponent<StressComponent>())
                return static mortalComponent =>
                    mortalComponent with {CurrentStress = mortalComponent.CurrentStress};

            var stress = 0;
            var stressComponent = entity.GetComponentByType<StressComponent>();
            while (stressComponent != null)
            {
                stress += stressComponent.Amount;
                stressComponent = (StressComponent?) stressComponent.Next;
            }

            entity.RemoveComponentOnNextUpdate<StressComponent>();
            return mortalComponent => mortalComponent with {CurrentStress = mortalComponent.CurrentStress + stress};
        }

        public bool IsCurrentStressInvalid(Entity entity)
            => IsCurrentStressMoreThanMax(entity) || IsCurrentStressLessThanZero(entity);

        public Func<MortalComponent, MortalComponent> FixCurrentStress(Entity entity)
            => mortalComponent =>
            {
                if (IsCurrentStressMoreThanMax(entity))
                    mortalComponent = mortalComponent with { CurrentStress = mortalComponent.MaxStress };
                else if (IsCurrentStressLessThanZero(entity)) mortalComponent = mortalComponent with { CurrentStress = 0 };

                return mortalComponent;
            };

        #endregion

        #region Private

        private static int CalculateMaxStress(Entity entity)
        {
            if (!entity.HasComponent<StatsComponent>()) return 1;

            var stats = entity.GetComponentByType<StatsComponent>();
            return stats.Skills[AerosStatEnums.Skills.Fortitude].Level * 4;
        }

        private static bool IsCurrentStressMoreThanMax(Entity entity)
        {
            var mortalComponent = entity.GetComponentByType<MortalComponent>();
            return mortalComponent.CurrentStress > mortalComponent.MaxStress;
        }

        private static bool IsCurrentStressLessThanZero(Entity entity)
        {
            var mortalComponent = entity.GetComponentByType<MortalComponent>();
            return mortalComponent.CurrentStress < 0;
        }

        #endregion

        #endregion

        #region Other

        #region Public

        public bool ShouldEntityBeDead(Entity entity)
        {
            var mortalComponent = entity.GetComponentByType<MortalComponent>();
            return mortalComponent.CurrentDamage >= mortalComponent.MaxDamage ||
                   mortalComponent.CurrentStress >= mortalComponent.MaxStress;
        }

        #endregion

        #region Private

        #endregion

        #endregion
    }
}
