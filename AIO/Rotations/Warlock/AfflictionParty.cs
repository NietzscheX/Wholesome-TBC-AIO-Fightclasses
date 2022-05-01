﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public class AfflictionParty : Warlock
    {
        public AfflictionParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            if (!Me.HaveBuff("Drink") || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // Unending Breath
                if (!Me.HaveBuff("Unending Breath")
                    && settings.UseUnendingBreath
                    && cast.OnSelf(UnendingBreath))
                    return;

                // Demon Skin
                if (!Me.HaveBuff("Demon Skin")
                    && !DemonArmor.KnownSpell
                    && !FelArmor.KnownSpell
                    && cast.OnSelf(DemonSkin))
                    return;

                // Demon Armor
                if ((!Me.HaveBuff("Demon Armor"))
                    && !FelArmor.KnownSpell
                    && cast.OnSelf(DemonArmor))
                    return;

                // Fel Armor
                if (!Me.HaveBuff("Fel Armor")
                    && cast.OnSelf(FelArmor))
                    return;

                // Health Funnel OOC
                if (ObjectManager.Pet.HealthPercent < 50
                    && Me.HealthPercent > 40
                    && ObjectManager.Pet.GetDistance < 19
                    && !ObjectManager.Pet.InCombatFlagOnly
                    && settings.HealthFunnelOOC)
                {
                    Lua.LuaDoString("PetWait();");
                    MovementManager.StopMove();
                    Fight.StopFight();

                    if (WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker"))
                        cast.PetSpell("Consume Shadows", false, true);

                    if (cast.OnSelf(HealthFunnel))
                    {
                        Thread.Sleep(500);
                        Usefuls.WaitIsCasting();
                        Lua.LuaDoString("PetFollow();");
                        return;
                    }
                    Lua.LuaDoString("PetFollow();");
                }

                // Health Stone
                if (!WarlockPetAndConsumables.HaveHealthstone()
                    && cast.OnSelf(CreateHealthStone))
                    return;

                // Create Soul Stone
                if (!WarlockPetAndConsumables.HaveSoulstone()
                    && cast.OnSelf(CreateSoulstone))
                    return;

                // PARTY Soul Stone
                if (CreateSoulstone.KnownSpell
                    && WTItem.HaveOneInList(WarlockPetAndConsumables.SOULSTONES)
                    && ToolBox.GetItemCooldown(WarlockPetAndConsumables.SOULSTONES) <= 0)
                {
                    WoWPlayer noSoulstone = partyManager.GroupAndRaid
                        .Find(m => !m.HaveBuff("Soulstone Resurrection")
                            && m.GetDistance < 20
                            && m.Name != "Unknown"
                            && !TraceLine.TraceLineGo(Me.Position, m.Position)
                            && (m.WowClass == WoWClass.Paladin || m.WowClass == WoWClass.Priest || m.WowClass == WoWClass.Shaman || m.WowClass == WoWClass.Druid));
                    if (noSoulstone != null)
                    {
                        Logger.Log($"Using Soulstone on {noSoulstone.Name}");
                        MovementManager.StopMoveNewThread();
                        MovementManager.StopMoveToNewThread();
                        Lua.RunMacroText($"/target {noSoulstone.Name}");
                        if (ObjectManager.Target.Name == noSoulstone.Name)
                        {
                            ToolBox.UseFirstMatchingItem(WarlockPetAndConsumables.SOULSTONES);
                            Usefuls.WaitIsCasting();
                            Lua.RunMacroText("/cleartarget");
                            ToolBox.ClearCursor();
                        }
                    }
                }

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;

                // Life Tap
                if (Me.HealthPercent > Me.ManaPercentage
                    && settings.UseLifeTap
                    && cast.OnSelf(LifeTap))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            // Pet attack
            if (ObjectManager.Pet.Target != ObjectManager.Me.Target)
                Lua.LuaDoString("PetAttack();");

            // PARTY Seed of Corruption
            if (partyManager.EnemiesFighting.Count >= settings.PartySeedOfCorruptionAmount
                && SeedOfCorruption.KnownSpell)
            {
                List<WoWUnit> enemiesWithoutSeedOfCorruption = partyManager.EnemiesFighting
                    .Where(e => !e.HaveBuff("Seed of Corruption"))
                    .OrderBy(e => e.GetDistance)
                    .ToList();
                if (enemiesWithoutSeedOfCorruption.Count > 0
                   && cast.OnFocusUnit(SeedOfCorruption, enemiesWithoutSeedOfCorruption[0]))
                {
                    Thread.Sleep(1000);
                    return;
                }
            }

            // Curse of The Elements
            if (!ObjectManager.Target.HaveBuff("Curse of the Elements")
                && settings.PartyCurseOfTheElements
                && cast.OnTarget(CurseOfTheElements))
                return;

            // Amplify Curse
            if (!Me.HaveBuff("Amplify Curse")
                && cast.OnSelf(AmplifyCurse))
                return;

            // Siphon Life
            if (Me.HealthPercent < 90
                && settings.UseSiphonLife
                && !ObjectManager.Target.HaveBuff("Siphon Life")
                && cast.OnTarget(SiphonLife))
                return;

            // Unstable Affliction
            if (!ObjectManager.Target.HaveBuff("Unstable Affliction")
                && cast.OnTarget(UnstableAffliction))
                return;

            // Curse of Agony
            if (!ObjectManager.Target.HaveBuff("Curse of Agony")
                && cast.OnTarget(CurseOfAgony))
                return;

            // Corruption
            if (!ObjectManager.Target.HaveBuff("Corruption")
                && ObjectManager.Target.HaveBuff("Seed of Corruption")
                && cast.OnTarget(Corruption))
                return;

            // Immolate
            if (!ObjectManager.Target.HaveBuff("Immolate")
                && !Corruption.KnownSpell
                && cast.OnTarget(Immolate))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            WoWUnit target = ObjectManager.Target;

            // Soulshatter
            if (SoulShatter.IsSpellUsable
                && settings.UseSoulShatter
                && unitCache.CloseUnitsTargetingMe.Count > 0
                && WTItem.CountItemStacks("Soul Shard") > 0
                && cast.OnSelf(SoulShatter))
                return;

            // Life Tap
            if (Me.ManaPercentage < settings.PartyLifeTapManaThreshold
                && Me.HealthPercent > settings.PartyLifeTapHealthThreshold
                && settings.UseLifeTap
                && cast.OnSelf(LifeTap))
                return;

            // Shadow Trance
            if (Me.HaveBuff("Shadow Trance")
                && cast.OnTarget(ShadowBolt))
                return;

            // PARTY Seed of Corruption
            if (partyManager.EnemiesFighting.Count >= settings.PartySeedOfCorruptionAmount
                && SeedOfCorruption.KnownSpell)
            {
                List<WoWUnit> enemiesWithoutSeedOfCorruption = partyManager.EnemiesFighting
                    .Where(e => !e.HaveBuff("Seed of Corruption"))
                    .OrderBy(e => e.GetDistance)
                    .ToList();
                if (enemiesWithoutSeedOfCorruption.Count > 0
                   && cast.OnFocusUnit(SeedOfCorruption, enemiesWithoutSeedOfCorruption[0]))
                {
                    Thread.Sleep(1000);
                    return;
                }
            }

            if (CurseOfTheElements.KnownSpell
                && settings.PartyCurseOfTheElements)
            {
                // PARTY Curse of the Elements
                List<WoWUnit> enemiesWithoutCurseOfTheElements = partyManager.EnemiesFighting
                    .Where(e => !e.HaveBuff("Curse of the Elements"))
                    .OrderBy(e => e.GetDistance)
                    .ToList();
                if (enemiesWithoutCurseOfTheElements.Count > 0
                   && partyManager.EnemiesFighting.Count - enemiesWithoutCurseOfTheElements.Count < 3
                   && cast.OnFocusUnit(CurseOfTheElements, enemiesWithoutCurseOfTheElements[0]))
                    return;
            }
            else
            {
                // PARTY Curse of Agony
                List<WoWUnit> enemiesWithoutCurseOfAgony = partyManager.EnemiesFighting
                    .Where(e => !e.HaveBuff("Curse of Agony"))
                    .OrderBy(e => e.GetDistance)
                    .ToList();
                if (enemiesWithoutCurseOfAgony.Count > 0
                   && partyManager.EnemiesFighting.Count - enemiesWithoutCurseOfAgony.Count < 3
                   && cast.OnFocusUnit(CurseOfAgony, enemiesWithoutCurseOfAgony[0]))
                    return;
            }

            // PARTY Unstable Affliction
            List<WoWUnit> enemiesWithoutUnstableAff = partyManager.EnemiesFighting
                .Where(e => !e.HaveBuff("Unstable Affliction"))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutUnstableAff.Count > 0
               && partyManager.EnemiesFighting.Count - enemiesWithoutUnstableAff.Count < 3
               && cast.OnFocusUnit(UnstableAffliction, enemiesWithoutUnstableAff[0]))
                return;

            // PARTY Corruption
            List<WoWUnit> enemiesWithoutCorruption = partyManager.EnemiesFighting
                .Where(e => !e.HaveBuff("Corruption") && !e.HaveBuff("Seed of Corruption"))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutCorruption.Count > 0
               && partyManager.EnemiesFighting.Count - enemiesWithoutCorruption.Count < 3
               && cast.OnFocusUnit(Corruption, enemiesWithoutCorruption[0]))
                return;

            // PARTY Immolate
            List<WoWUnit> enemiesWithoutImmolate = partyManager.EnemiesFighting
                .Where(e => !e.HaveBuff("Immolate"))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutImmolate.Count > 0
               && partyManager.EnemiesFighting.Count - enemiesWithoutImmolate.Count < 3
               && cast.OnFocusUnit(Immolate, enemiesWithoutImmolate[0]))
                return;

            // PARTY Siphon Life
            List<WoWUnit> enemiesWithoutSiphonLife = partyManager.EnemiesFighting
                .Where(e => !e.HaveBuff("Siphon Life"))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutSiphonLife.Count > 0
               && partyManager.EnemiesFighting.Count - enemiesWithoutSiphonLife.Count < 3
               && cast.OnFocusUnit(SiphonLife, enemiesWithoutSiphonLife[0]))
                return;

            // Drain Soul
            bool _shouldDrainSoul = WTItem.CountItemStacks("Soul Shard") < settings.NumberOfSoulShards || settings.AlwaysDrainSoul;
            if (_shouldDrainSoul
                && ObjectManager.Target.HealthPercent < settings.DrainSoulHP
                && ObjectManager.Target.Level >= Me.Level - 8
                && !UnitImmunities.Contains(ObjectManager.Target, "Drain Soul(Rank 1)"))
            {
                if (settings.DrainSoulLevel1
                    && cast.OnTarget(DrainSoulRank1))
                    return;
                else if (cast.OnTarget(DrainSoul))
                    return;
            }

            // Shadow Bolt
            if (cast.OnTarget(ShadowBolt))
                return;

            // Stop wand if banned
            if (WTCombat.IsSpellRepeating(5019)
                && UnitImmunities.Contains(ObjectManager.Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(ObjectManager.Target, "Shoot"))
                if (cast.OnTarget(ShadowBolt))
                    return;

            // Use Wand
            if (!WTCombat.IsSpellRepeating(5019)
                && _iCanUseWand
                && cast.OnTarget(UseWand, false))
                return;
        }
    }
}
