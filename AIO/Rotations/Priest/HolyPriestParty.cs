﻿using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class HolyPriestParty : Priest
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // PARTY Circle of Healing
            if (AoEHeal())
                return;

            List<AIOPartyMember> aliveMembers = AIOParty.Group
                .FindAll(m => m.IsAlive && m.GetDistance < 60)
                .ToList();

            // PARTY Greater heal
            List<AIOPartyMember> needGreaterHeal = aliveMembers
                .FindAll(m => m.HealthPercent < 50)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needGreaterHeal.Count > 0 && cast.OnFocusUnit(GreaterHeal, needGreaterHeal[0]))
                return;

            // PARTY Heal
            List<AIOPartyMember> needHeal = aliveMembers
                .FindAll(m => m.HealthPercent < 80)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needHeal.Count > 0 && cast.OnFocusUnit(FlashHeal, needHeal[0]))
                return;

            if (!FlashHeal.KnownSpell)
            {
                // PARTY Lesser Heal
                List<AIOPartyMember> needLesserHeal = aliveMembers
                    .FindAll(m => m.HealthPercent < 80)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needLesserHeal.Count > 0 && cast.OnFocusUnit(LesserHeal, needLesserHeal[0]))
                    return;
            }

            // PARTY Renew
            List<AIOPartyMember> needRenew = aliveMembers
                .FindAll(m => m.HealthPercent < 100 && !m.HaveBuff(Renew.Name))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needRenew.Count > 0 && cast.OnFocusUnit(Renew, needRenew[0]))
                return;

            if (BuffParty())
                return;
                
            // OOC Inner Fire
            if (!Me.HaveBuff("Inner Fire")
                && settings.UseInnerFire
                && cast.OnSelf(InnerFire))
                return;

            // PARTY Drink
            if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                return;
        }

        protected override void HealerCombat()
        {
            List<AIOPartyMember> aliveMembers = AIOParty.Group
                .FindAll(m => m.IsAlive)
                .OrderBy(m => m.HealthPercent)
                .ToList();

            List<AIOPartyMember> needDispelMagic = aliveMembers
                    .FindAll(m => ToolBox.HasMagicDebuff(m.Name))
                    .OrderBy(m => m.HealthPercent)
                    .ToList();

            // Fade
            if (AIOParty.EnemiesClose.Exists(m => m.IsTargetingMe)
                && cast.OnSelf(Fade))
                return;

            // ShadowFiend
            if (Shadowfiend.IsSpellUsable && Me.ManaPercentage < 50)
            {
                WoWUnit unit = AIOParty.EnemiesClose.OrderBy(m => m.Health).Last();
                if (unit != null && cast.OnFocusUnit(Shadowfiend, unit))
                    return;
            }

            // PARTY Mass Dispel
            if (settings.PartyMassDispel && needDispelMagic.Count > 4)
            {
                // Get unit in the middle of the pack
                WoWUnit unit = ToolBox.GetBestAoETarget(40, needDispelMagic);
                if (unit != null && cast.OnSelf(MassDispel))
                {
                    ClickOnTerrain.Pulse(unit.Position);
                    return;
                }
            }

            // Prioritize self healing over other things in case of danger
            if (Me.HealthPercent < 40)
            {
                ToolBox.UseConsumableToSelfHeal();
                if (SingleTargetHeal(Me))
                    return;
            }

            // PARTY Circle of Healing
            if (AoEHeal())
                return;

            // Cure Disease
            if (settings.PartyCureDisease)
            {
                // Party Cure Disease
                WoWPlayer needCureDisease = aliveMembers
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // Party Dispel Magic
            if (settings.PartyDispelMagic)
            {
                if (needDispelMagic.Count > 0 && cast.OnFocusUnit(DispelMagic, needDispelMagic[0]))
                    return;
            }

            SingleTargetHeal(aliveMembers[0]);
        }

        private bool SingleTargetHeal(WoWPlayer player)
        {
            if (!player.IsAlive)
                return false;
            if (player.HealthPercent < 30 && cast.OnFocusUnit(FlashHeal, player))
                return true;
            if (player.HealthPercent < 50
                && PowerWordShield.IsSpellUsable
                && player.RagePercentage <= 0
                && player.HaveBuff("Power Word: Shield")
                && !ToolBox.HasDebuff("Weakened Soul", player.Name)
                && cast.OnFocusUnit(PowerWordShield, player))
                return true;
            if (player.HealthPercent < 60 && cast.OnFocusUnit(GreaterHeal, player))
                return true;
            if (player.HealthPercent < 80 && !player.HaveBuff(Renew.Name) && cast.OnFocusUnit(Renew, player))
                return true;
            if (player.HealthPercent < 95 && !player.HaveBuff(Renew.Name) && cast.OnFocusUnit(RenewRank8, player))
                return true;
            if (player.HealthPercent < 100)
            {
                if (PrayerOfMending.IsSpellUsable && cast.OnFocusUnit(PrayerOfMending, player))
                    return true;
            }
            return false;
        }

        private bool AoEHeal()
        {
            if (CircleOfHealing.KnownSpell)
            {
                List<AIOPartyMember> needCircleOfHealing = new List<AIOPartyMember>();
                foreach (var item in AIOParty.RaidGroups)
                {
                    List<AIOPartyMember> subGroupNeedCircleOfHealing = item.Value
                        .FindAll(m => m.IsAlive && m.HealthPercent < settings.PartyCircleOfHealingThreshold)
                        .OrderBy(m => m.HealthPercent)
                        .ToList();
                    if (subGroupNeedCircleOfHealing.Count > 2)
                    {
                        needCircleOfHealing.Add(subGroupNeedCircleOfHealing[0]);
                    }
                }
                if (needCircleOfHealing.Count > 0)
                {
                    List<AIOPartyMember> needCircleOfHealingOrdered = needCircleOfHealing
                        .OrderBy(m => m.HealthPercent)
                        .ToList();
                    if (cast.OnFocusUnit(CircleOfHealing, needCircleOfHealingOrdered[0]))
                        return true;
                }
            }
            return false;
        }
    }
}
