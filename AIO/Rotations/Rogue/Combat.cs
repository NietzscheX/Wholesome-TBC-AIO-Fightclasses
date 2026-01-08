using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Rogue
{
    public class Combat : Rogue
    {
        public Combat(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Solo;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            base.BuffRotation();
        }

        // Check if target can be pickpocketed (language-independent)
        private bool CanDismantle()
        {
            return Target.IsHumanoid ||
             Target.IsUndead ||
             Target.IsDemon ||
             Target.IsGiant;
        }

        protected override void Pull()
        {
            base.Pull();

            // Check if caster in list
            if (_casterEnemies.Contains(Target.Name))
                _fightingACaster = true;

            // Pull logic
            if (ToolBox.Pull(cast, settings.SC_AlwaysPull || WTEffects.HasPoisonDebuff(), new List<AIOSpell> { Shoot, Throw }, unitCache))
            {
                _combatMeleeTimer = new Timer(2000);
                return;
            }

            // Stealth
            if (!Me.HasAura(Stealth)
                && Target.GetDistance > 15f
                && Target.GetDistance < 25f
                && unitCache.GetClosestHostileFrom(Target, 20) == null
                && settings.SC_StealthApproach
                && Backstab.KnownSpell
                && (!WTEffects.HasPoisonDebuff() || settings.SC_StealthWhenPoisoned)
                && cast.OnSelf(Stealth))
                return;

            // Stealth approach
            if (Me.HasAura(Stealth)
                && Target.GetDistance > 3f
                && !_isStealthApproching)
                StealthApproach();

            // Auto
            if (Target.GetDistance < 6f && !Me.HasAura(Stealth))
                ToggleAutoAttack(true);

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool _shouldBeInterrupted = WTCombat.TargetIsCasting();


            // 搜索  + 偷袭
            if (Me.HasAura(Stealth) && Target.GetDistance < 6f)
            {
                if (CanDismantle())
                {
                    cast.OnTarget(PickPocket)
                }
                if (cast.OnTarget(CheapShot))
                    return;
            }


            // Force melee
            if (_combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Check if interruptable enemy is in list
            if (_shouldBeInterrupted)
            {
                _fightingACaster = true;
                RangeManager.SetRangeToMelee();
                if (!_casterEnemies.Contains(Target.Name))
                    _casterEnemies.Add(Target.Name);
            }

            // Kick interrupt
            // 脚踢
            if (_shouldBeInterrupted)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(Kick) || cast.OnTarget(Gouge) || cast.OnTarget(KidneyShot))
                    return;
            }

            // Adrenaline Rush
            // 冲动
            if ((unitCache.EnemiesAttackingMe.Count > 1 || Target.IsElite) && !Me.HasAura(AdrenalineRush))
                if (cast.OnTarget(AdrenalineRush))
                    return;

            // Blade Flurry
            // 剑刃乱舞
            if (unitCache.EnemiesAttackingMe.Count > 1 && !Me.HasAura(BladeFlurry))
                if (cast.OnTarget(BladeFlurry))
                    return;

            // Riposte (language-independent creature type check)
            if (Riposte.IsSpellUsable && (Target.IsHumanoid || settings.SC_RiposteAll))
                if (cast.OnTarget(Riposte))
                    return;

            // Bandage
            // 绷带？
            if (Target.HasAura(Blind))
            {
                MovementManager.StopMoveTo(true, 500);
                ItemsManager.UseItemByNameOrId(_myBestBandage);
                Logger.Log("Using " + _myBestBandage);
                Usefuls.WaitIsCasting();
                return;
            }

            // Blind
            // 致盲
            if (Me.HealthPercent < 40
                && !WTEffects.HasDebuff("Recently Bandaged")
                && _myBestBandage != null
                && settings.SC_UseBlindBandage)
                if (cast.OnTarget(Blind))
                    return;

            // Evasion
            // 闪避
            if (unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 30 && !Me.HasAura(Evasion) && Target.HealthPercent > 50)
                if (cast.OnTarget(Evasion))
                    return;

            // Cloak of Shadows
            // 暗影斗篷
            if (Me.HealthPercent < 30
                && !Me.HasAura(CloakOfShadows)
                && Target.HealthPercent > 50)
                if (cast.OnTarget(CloakOfShadows))
                    return;

            // Backstab in combat
            // 背刺
            if (IsTargetStunned()
                && WTGear.GetMainHandWeaponType().Equals("Daggers"))
                if (cast.OnTarget(Backstab))
                    return;

            // Slice and Dice
            // 切割
            if (!Me.HasAura(SliceAndDice)
                && Me.ComboPoint > 1
                && Target.HealthPercent > 40)
                if (cast.OnTarget(SliceAndDice))
                    return;

            // Eviscerate logic
            // 剔骨
            if (Me.ComboPoint > 0 && Target.HealthPercent < 30
                || Me.ComboPoint > 1 && Target.HealthPercent < 45
                || Me.ComboPoint > 2 && Target.HealthPercent < 60
                || Me.ComboPoint > 3 && Target.HealthPercent < 70)
                if (cast.OnTarget(Eviscerate))
                    return;

            // GhostlyStrike
            // 鬼魅攻击
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > GhostlyStrike.Cost + Kick.Cost))
                if (cast.OnTarget(GhostlyStrike))
                    return;

            // Hemohrrage
            // 出血
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > Hemorrhage.Cost + Kick.Cost))
                if (cast.OnTarget(Hemorrhage))
                    return;

            // Sinister Strike
            // 邪恶攻击 
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > SinisterStrike.Cost + Kick.Cost))
                if (cast.OnTarget(SinisterStrike))
                    return;
        }
    }
}
