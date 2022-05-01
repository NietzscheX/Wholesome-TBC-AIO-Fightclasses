﻿using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class Shadow : Priest
    {
        public Shadow(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Solo;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            // OOC Cure Disease
            if (WTEffects.HasDiseaseDebuff()
                && cast.OnSelf(CureDisease))
                return;

            // OOC Renew
            if (Me.HealthPercent < 70
                && !Me.HaveBuff("Renew")
                && cast.OnSelf(Renew))
                return;

            // OOC Power Word Shield
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Power Word: Shield")
                && !WTEffects.HasDebuff("Weakened Soul")
                && ObjectManager.GetNumberAttackPlayer() > 0
                && settings.UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // OOC Psychic Scream
            if (Me.HealthPercent < 30
                && ObjectManager.GetNumberAttackPlayer() > 1
                && cast.OnSelf(PsychicScream))
                return;

            // OOC Power Word Fortitude
            if (!Me.HaveBuff("Power Word: Fortitude")
                && cast.OnSelf(PowerWordFortitude))
                return;

            // OOC Divine Spirit
            if (!Me.HaveBuff("Divine Spirit")
                && cast.OnSelf(DivineSpirit))
                return;

            // OOC Inner Fire
            if (!Me.HaveBuff("Inner Fire")
                && settings.UseInnerFire
                && cast.OnSelf(InnerFire))
                return;

            // OOC Shadowguard
            if (!Me.HaveBuff("Shadowguard")
                && settings.UseShadowGuard
                && cast.OnSelf(Shadowguard))
                return;

            // OOC Shadow Protection
            if (!Me.HaveBuff("Shadow Protection")
                && ShadowProtection.KnownSpell
                && settings.UseShadowProtection
                && cast.OnSelf(ShadowProtection))
                return;

            // OOC ShadowForm
            if (!Me.HaveBuff("ShadowForm")
                && ObjectManager.GetNumberAttackPlayer() < 1
                && cast.OnSelf(Shadowform))
                return;
        }

        protected override void Pull()
        {
            // Pull ShadowForm
            if (!Me.HaveBuff("ShadowForm")
                && cast.OnSelf(Shadowform))
                return;

            // Power Word Shield
            if (!WTEffects.HasDebuff("Weakened Soul")
                && settings.UseShieldOnPull
                && !Me.HaveBuff("Power Word: Shield")
                && settings.UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // Vampiric Touch
            if (Me.HaveBuff("ShadowForm")
                && !ObjectManager.Target.HaveBuff("Vampiric Touch")
                && cast.OnTarget(VampiricTouch))
                return;

            // MindBlast
            if (Me.HaveBuff("ShadowForm")
                && !VampiricTouch.KnownSpell
                && cast.OnTarget(MindBlast))
                return;

            // Shadow Word Pain
            if (Me.HaveBuff("ShadowForm")
                && (!MindBlast.KnownSpell || !MindBlast.IsSpellUsable)
                && !ObjectManager.Target.HaveBuff("Shadow Word: Pain")
                && cast.OnTarget(ShadowWordPain))
                return;

            // Holy Fire
            if (!Me.HaveBuff("ShadowForm")
                && cast.OnTarget(HolyFire))
                return;

            // Smite
            if (!HolyFire.KnownSpell
                && !Me.HaveBuff("ShadowForm")
                && cast.OnTarget(Smite))
                return;
        }

        protected override void CombatRotation()
        {
            bool _hasMagicDebuff = settings.UseDispel ? WTEffects.HasMagicDebuff() : false;
            bool _hasDisease = settings.CureDisease ? WTEffects.HasDiseaseDebuff() : false;
            bool _hasWeakenedSoul = WTEffects.HasDebuff("Weakened Soul");
            double _myManaPC = Me.ManaPercentage;
            bool _inShadowForm = Me.HaveBuff("ShadowForm");
            int _mindBlastCD = WTCombat.GetSpellCooldown(MindBlast.Name);
            int _innerFocusCD = WTCombat.GetSpellCooldown(InnerFocus.Name);
            bool _shoulBeInterrupted = WTCombat.TargetIsCasting();
            WoWUnit Target = ObjectManager.Target;

            // Power Word Shield on multi aggro
            if (!Me.HaveBuff("Power Word: Shield")
                && !_hasWeakenedSoul
                && ObjectManager.GetNumberAttackPlayer() > 1
                && settings.UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // Power Word Shield
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Power Word: Shield")
                && !_hasWeakenedSoul
                && settings.UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // Renew
            if (Me.HealthPercent < 70
                && !Me.HaveBuff("Renew")
                && !_inShadowForm
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Renew))
                return;

            // Psychic Scream
            if (Me.HealthPercent < 50
                && ObjectManager.GetNumberAttackPlayer() > 1
                && cast.OnSelf(PsychicScream))
                return;

            // Flash Heal
            if (Me.HealthPercent < 50
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(FlashHeal))
                return;

            // Heal
            if (Me.HealthPercent < 50
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Heal))
                return;

            // Lesser Heal
            if (Me.HealthPercent < 50
                && !FlashHeal.KnownSpell
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(LesserHeal))
                return;

            // Silence
            if (_shoulBeInterrupted)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(Silence))
                    return;
            }

            // Cure Disease
            if (_hasDisease && !_inShadowForm)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(CureDisease))
                    return;
            }

            // Dispel Magic self
            if (_hasMagicDebuff
                && _myManaPC > 10
                && DispelMagic.KnownSpell
                && DispelMagic.IsSpellUsable
                && (_dispelTimer.ElapsedMilliseconds > 10000 || _dispelTimer.ElapsedMilliseconds <= 0))
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(DispelMagic))
                    return;
            }

            // Vampiric Touch
            if (!Target.HaveBuff("Vampiric Touch")
                && _myManaPC > _innerManaSaveThreshold
                && Target.HealthPercent > _wandThreshold
                && cast.OnTarget(VampiricTouch))
                return;

            // Vampiric Embrace
            if (!Target.HaveBuff("Vampiric Embrace") && !Me.HaveBuff("Vampiric Embrace")
                && _myManaPC > _innerManaSaveThreshold
                && cast.OnTarget(VampiricEmbrace))
                return;

            // ShadowFiend
            if (ObjectManager.GetNumberAttackPlayer() > 1
                && cast.OnTarget(Shadowfiend))
                return;

            // Shadow Word Pain
            if (_myManaPC > 10
                && Target.HealthPercent > 15
                && !Target.HaveBuff("Shadow Word: Pain")
                && cast.OnTarget(ShadowWordPain))
                return;

            // Inner Fire
            if (!Me.HaveBuff("Inner Fire")
                && settings.UseInnerFire
                && InnerFire.KnownSpell
                && _myManaPC > _innerManaSaveThreshold
                && Target.HealthPercent > _wandThreshold
                && cast.OnSelf(InnerFire))
                return;

            // Shadowguard
            if (!Me.HaveBuff("Shadowguard")
                && _myManaPC > _innerManaSaveThreshold
                && settings.UseShadowGuard
                && Target.HealthPercent > _wandThreshold
                && cast.OnSelf(Shadowguard))
                return;

            // Shadow Protection
            if (!Me.HaveBuff("Shadow Protection")
                && _myManaPC > 70
                && settings.UseShadowProtection
                && cast.OnSelf(ShadowProtection))
                return;

            // Devouring Plague
            if (!Target.HaveBuff("Devouring Plague")
                && Target.HealthPercent > settings.DevouringPlagueThreshold
                && cast.OnTarget(DevouringPlague))
                return;

            // Shadow Word Death
            if (_myManaPC > _innerManaSaveThreshold
                && settings.UseShadowWordDeath
                && Target.HealthPercent < 15
                && cast.OnTarget(ShadowWordDeath))
                return;

            // Mind Blast + Inner Focus
            if (!_inShadowForm
                && _myManaPC > _innerManaSaveThreshold
                && Target.HealthPercent > 50
                && _mindBlastCD <= 0
                && (Target.HealthPercent > _wandThreshold || !_iCanUseWand))
            {
                if (InnerFocus.KnownSpell && _innerFocusCD <= 0)
                    cast.OnSelf(InnerFocus);

                if (cast.OnTarget(MindBlast))
                    return;
            }

            // Shadow Form Mind Blast + Inner Focus
            if (_inShadowForm
                && _myManaPC > _innerManaSaveThreshold
                && _mindBlastCD <= 0
                && Target.HealthPercent > _wandThreshold)
            {
                if (InnerFocus.KnownSpell && _innerFocusCD <= 0)
                    cast.OnSelf(InnerFocus);

                if (cast.OnTarget(MindBlast))
                    return;
            }

            // Mind FLay
            if ((Me.HaveBuff("Power Word: Shield") || !settings.UsePowerWordShield)
                && _myManaPC > _innerManaSaveThreshold
                && Target.HealthPercent > _wandThreshold
                && cast.OnTarget(MindFlay))
                return;

            // Low level Smite
            if (Me.Level < 5 && (Target.HealthPercent > 30 || Me.ManaPercentage > 80)
                && _myManaPC > _innerManaSaveThreshold
                && cast.OnTarget(Smite))
                return;

            // Smite
            if (!_inShadowForm
                && _myManaPC > _innerManaSaveThreshold
                && Me.Level >= 5
                && Target.HealthPercent > 20
                && (Target.HealthPercent > settings.WandThreshold || !_iCanUseWand)
                && cast.OnTarget(Smite))
                return;

            // Stop wand if banned
            if (WTCombat.IsSpellRepeating(5019)
                && UnitImmunities.Contains(ObjectManager.Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(ObjectManager.Target, "Shoot"))
                if (cast.OnTarget(MindBlast) || cast.OnTarget(Smite))
                    return;

            // Use Wand
            if (!WTCombat.IsSpellRepeating(5019)
                && _iCanUseWand
                && cast.OnTarget(UseWand, false))
                return;

            // Go in melee because nothing else to do
            if (!WTCombat.IsSpellRepeating(5019)
                && !_iCanUseWand
                && !RangeManager.CurrentRangeIsMelee()
                && Target.IsAlive)
            {
                Logger.Log("Going in melee");
                RangeManager.SetRangeToMelee();
                return;
            }
        }
    }
}
