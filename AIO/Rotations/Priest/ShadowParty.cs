﻿using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class ShadowParty : Priest
    {
        public ShadowParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            if (!Me.HaveBuff("Drink") || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // OOC Shadowguard
                if (!Me.HaveBuff("Shadowguard")
                    && settings.UseShadowGuard
                    && cast.OnSelf(Shadowguard))
                    return;

                // OOC ShadowForm
                if (!Me.HaveBuff("ShadowForm")
                    && cast.OnSelf(Shadowform))
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void Pull()
        {
            // Pull ShadowForm
            if (!Me.HaveBuff("ShadowForm")
                && cast.OnSelf(Shadowform))
                return;

            // Vampiric Touch
            if (!ObjectManager.Target.HaveBuff("Vampiric Touch")
                && cast.OnTarget(VampiricTouch))
                return;

            // Shadow Word Pain
            if (!ObjectManager.Target.HaveBuff("Shadow Word: Pain")
                && cast.OnTarget(ShadowWordPain))
                return;
        }

        protected override void CombatRotation()
        {
            WoWUnit Target = ObjectManager.Target;

            // Fade
            if (unitCache.CloseUnitsTargetingMe.Count > 0
                && cast.OnSelf(Fade))
                return;

            // Inner Focus  + spell
            if (Me.HaveBuff("Inner Focus")
                && Target.HealthPercent > 80)
            {
                cast.OnTarget(DevouringPlague);
                cast.OnTarget(ShadowWordPain);
                return;
            }

            // Power Word Shield
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Power Word: Shield")
                && !WTEffects.HasDebuff("Weakened Soul")
                && settings.UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // Silence
            if (WTCombat.TargetIsCasting()
                && cast.OnTarget(Silence))
                return;

            // Cure Disease
            if (settings.PartyCureDisease)
            {
                // PARTY Cure Disease
                WoWPlayer needCureDisease = partyManager.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // PARTY Dispel Magic
            if (settings.PartyDispelMagic)
            {
                WoWPlayer needDispelMagic = partyManager.GroupAndRaid
                    .Find(m => WTEffects.HasMagicDebuff(m.Name));
                if (needDispelMagic != null && cast.OnFocusUnit(DispelMagic, needDispelMagic))
                    return;
            }

            // Combat ShadowForm
            if (!Me.HaveBuff("ShadowForm")
                && cast.OnSelf(Shadowform))
                return;

            // ShadowFiend
            if (Me.ManaPercentage < 30
                && cast.OnTarget(Shadowfiend))
                return;

            // Vampiric Touch
            if (!Target.HaveBuff("Vampiric Touch")
                && cast.OnTarget(VampiricTouch))
                return;

            if (settings.PartyVampiricEmbrace)
            {
                // Vampiric Embrace
                if (!Target.HaveBuff("Vampiric Embrace")
                    && Target.HaveBuff("Vampiric Touch")
                    && cast.OnTarget(VampiricEmbrace))
                    return;
            }

            // Inner Focus
            if (Target.HealthPercent > 80
                && cast.OnSelf(InnerFocus))
                return;

            // Devouring Plague
            if (!Target.HaveBuff("Devouring Plague")
                && Target.HealthPercent > 80
                && cast.OnTarget(DevouringPlague))
                return;

            // PARTY Shadow Word Pain
            List<WoWUnit> enemiesWithoutPain = partyManager.EnemiesFighting
                .Where(e => e.InCombatFlagOnly && !e.HaveBuff("Shadow Word: Pain"))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutPain.Count > 0
               && partyManager.EnemiesFighting.Count - enemiesWithoutPain.Count < 3
               && cast.OnFocusUnit(ShadowWordPain, enemiesWithoutPain[0]))
                return;

            // Mind Blast
            if (Me.ManaPercentage > settings.PartyMindBlastThreshold
                && cast.OnTarget(MindBlast))
                return;

            // Shadow Word Death
            if (Me.HealthPercent > settings.PartySWDeathThreshold
                && settings.UseShadowWordDeath
                && cast.OnTarget(ShadowWordDeath))
                return;

            // Mind FLay
            if (cast.OnTarget(MindFlay))
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
        }
    }
}
