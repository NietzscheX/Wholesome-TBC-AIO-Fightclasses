using WholesomeTBCAIO.Managers.UnitCache;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static WholesomeTBCAIO.Helpers.Enums;

namespace WholesomeTBCAIO.Helpers
{
    public static class StatusChecker
    {
        // Cache player auras lookup for performance
        private static AuraHelper _auraHelper = new AuraHelper();

        public static bool InCombat()
        {
            return BasicConditions()
                && (!ObjectManager.Me.IsMounted || _auraHelper.PlayerHasAuraById(SpellIds.GhostWolf))
                && ObjectManager.Me.Target > 0
                && ObjectManager.Target.IsAttackable
                && ObjectManager.Target.IsAlive
                && ObjectManager.Me.InCombatFlagOnly;
        }
        public static bool InCombatNoTarget()
        {
            return BasicConditions()
                && (!ObjectManager.Me.IsMounted || _auraHelper.PlayerHasAuraById(SpellIds.GhostWolf))
                && (!ObjectManager.Me.HasTarget || ObjectManager.Target.IsDead || !ObjectManager.Target.IsValid || !ObjectManager.Target.IsAttackable)
                && ObjectManager.Me.InCombatFlagOnly;
        }

        public static bool InPull()
        {
            return BasicConditions()
                && Fight.InFight
                && !ObjectManager.Me.InCombatFlagOnly;
        }

        public static bool OutOfCombat(RotationRole rotationRole)
        {
            if (BasicConditions()
                && !ObjectManager.Me.IsMounted
                && !ObjectManager.Me.IsCast
                && !Fight.InFight
                && !ObjectManager.Me.InCombatFlagOnly
                && (!_auraHelper.PlayerHasDrinkAura() || ObjectManager.Me.ManaPercentage >= 95)
                && (!_auraHelper.PlayerHasFoodAura() || ObjectManager.Me.HealthPercent >= 95)
                /*&& !MovementManager.InMovement*/)
            {
                // Remove Earth Shield if not tank
                if (rotationRole != RotationRole.Tank
                    && rotationRole != RotationRole.None
                    && _auraHelper.PlayerHasAuraById(SpellIds.EarthShield))
                    _auraHelper.CancelPlayerBuffById(SpellIds.EarthShield);

                return true;
            }
            return false;
        }

        public static bool OOCMounted()
        {
            return BasicConditions()
                && ObjectManager.Me.IsMounted
                && !Fight.InFight
                && !ObjectManager.Me.InCombatFlagOnly;
        }

        public static bool BasicConditions()
        {
            return Conditions.InGameAndConnectedAndProductStartedNotInPause
                && ObjectManager.Me.IsAlive
                && Main.IsLaunched
                && !Main.HMPrunningAway;
        }
    }
}
