﻿using robotManager.Helpful;
using System.Collections.Generic;
using System.Threading;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    public class Cast
    {
        private AIOSpell DefaultBaseSpell { get; }
        private bool CombatDebugON { get; }
        private AIOSpell WandSpell { get; }
        private bool AutoDetectImmunities { get; }
        private ulong CurrentEnemyGuid { get; set; }

        public bool IsBackingUp { get; set; }
        public bool PlayingManaClass { get; set; }
        public List<string> BannedSpells { get; set; }

        public Cast(AIOSpell defaultBaseSpell, bool combatDebugON, AIOSpell wandSpell, bool autoDetectImmunities)
        {
            AutoDetectImmunities = autoDetectImmunities;
            DefaultBaseSpell = defaultBaseSpell;
            CombatDebugON = combatDebugON;
            WandSpell = wandSpell;
            PlayingManaClass = ObjectManager.Me.WowClass != WoWClass.Rogue && ObjectManager.Me.WowClass != WoWClass.Warrior;
            BannedSpells = new List<string>();
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += LuaEventsHandler;
        }

        public bool PetSpell(string spellName, bool onFocus = false, bool noTargetNeeded = false)
        {
            int spellIndex = ToolBox.GetPetSpellIndex(spellName);
            if (ToolBox.PetKnowsSpell(spellName)
                && ToolBox.GetPetSpellReady(spellName)
                && !BannedSpells.Contains(spellName)
                && (ObjectManager.Pet.HasTarget || noTargetNeeded))
            {
                Thread.Sleep(ToolBox.GetLatency() + 100);
                Logger.Combat($"Cast (Pet) {spellName}");
                if (!onFocus)
                    Lua.LuaDoString($"CastPetAction({spellIndex});");
                else
                    Lua.LuaDoString($"CastPetAction({spellIndex}, \'focus\');");

                return true;
            }
            return false;
        }

        public void Dispose()
        {
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= LuaEventsHandler;
        }

        public bool PetSpellIfEnoughForGrowl(string spellName, uint spellCost)
        {
            if (ObjectManager.Pet.Focus >= spellCost + 15
                && ObjectManager.Me.InCombatFlagOnly
                && ToolBox.PetKnowsSpell(spellName))
                if (PetSpell(spellName))
                    return true;
            return false;
        }

        public bool Normal(AIOSpell s, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast);
        }

        public bool OnSelf(AIOSpell s, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast, true);
        }

        public bool OnFocusPlayer(AIOSpell s, WoWPlayer onPlayerFocus, bool stopWandAndCast = true, bool onDeadTarget = false)
        {
            return AdvancedCast(s, stopWandAndCast, onPlayerFocus: onPlayerFocus, onDeadTarget: onDeadTarget);
        }

        public bool OnFocusUnit(AIOSpell s, WoWUnit onUnitFocus, bool stopWandAndCast = true, bool onDeadTarget = false)
        {
            return AdvancedCast(s, stopWandAndCast, onUnitFocus: onUnitFocus, onDeadTarget: onDeadTarget);
        }

        public bool AdvancedCast(AIOSpell s, bool stopWandAndCast = true, bool onSelf = false, WoWPlayer onPlayerFocus = null, WoWUnit onUnitFocus = null, bool onDeadTarget = false)
        {
            WoWPlayer Me = ObjectManager.Me;
            WoWUnit Target = ObjectManager.Target;

            // Change and clear guid + banned list
            if (ObjectManager.Target.Guid != CurrentEnemyGuid)
            {
                BannedSpells.Clear();
                CurrentEnemyGuid = ObjectManager.Target.Guid;
            }

            if (onUnitFocus != null && onUnitFocus.IsDead && !onDeadTarget)
                return false;

            if (onPlayerFocus != null && onPlayerFocus.IsDead && !onDeadTarget)
                return false;

            if (onPlayerFocus == null 
                && onUnitFocus == null 
                && Target.Guid > 0 
                && Target.IsDead 
                && !onDeadTarget)
                return false;

            if (!s.KnownSpell 
                || IsBackingUp 
                || Me.IsCast 
                || Me.CastingTimeLeft > Usefuls.Latency
                || Me.IsStunned)
                return false;

            if (BannedSpells.Count > 0 && BannedSpells.Contains(s.Name))
                return false;

            CombatDebug("*----------- INTO CAST FOR " + s.Name);

            // CHECK COST
            if (s.PowerType == -2 && Me.Health < s.Cost)
            {
                CombatDebug($"{s.Name}: Not enough health {s.Cost}/{Me.Health}, SKIPPING");
                return false;
            }
            else if (s.PowerType == 0 && Me.Mana < s.Cost)
            {
                CombatDebug($"{s.Name}: Not enough mana {s.Cost}/{Me.Mana}, SKIPPING");
                return false;
            }
            else if (s.PowerType == 1 && Me.Rage < s.Cost)
            {
                CombatDebug($"{s.Name}: Not enough rage {s.Cost}/{Me.Rage}, SKIPPING");
                return false;
            }
            else if (s.PowerType == 2 && ObjectManager.Pet.Focus < s.Cost)
            {
                CombatDebug($"{s.Name}: Not enough pet focus {s.Cost}/{ObjectManager.Pet.Focus}, SKIPPING");
                return false;
            }
            else if (s.PowerType == 3 && Me.Energy < s.Cost)
            {
                CombatDebug($"{s.Name}: Not enough energy {s.Cost}/{Me.Energy}, SKIPPING");
                return false;
            }

            // DON'T CAST BECAUSE WANDING
            if (WandSpell != null 
                && ToolBox.UsingWand() 
                && !stopWandAndCast)
            {
                CombatDebug("Didn't cast because we were wanding");
                return false;
            }

            // COOLDOWN CHECKS
            float _spellCD = s.GetCurrentCooldown;
            CombatDebug($"Cooldown is {_spellCD}");

            if (_spellCD >= 2f)
            {
                CombatDebug("Didn't cast because cd is too long");
                return false;
            }
            
            // STOP WAND FOR CAST
            if (WandSpell != null
                && ToolBox.UsingWand()
                && stopWandAndCast)
                StopWandWaitGCD(WandSpell, s);

            if (_spellCD < 2f && _spellCD > 0f)
            {
                if (s.CastTime < 1f)
                {
                    CombatDebug($"{s.Name} is instant and low CD, recycle");
                    return true;
                }

                int t = 0;
                while (s.GetCurrentCooldown > 0)
                {
                    Thread.Sleep(100);
                    t += 100;
                    if (t > 2000)
                    {
                        CombatDebug($"{s.Name}: waited for tool long, give up");
                        return false;
                    }
                }
                Thread.Sleep(100 + Usefuls.Latency);
                CombatDebug($"{s.Name}: waited {t + 100} for it to be ready");
            }

            if (!s.IsSpellUsable)
            {
                CombatDebug("Didn't cast because spell somehow not usable");
                return false;
            }

            if (onSelf && !Target.IsAttackable)
                Lua.RunMacroText("/cleartarget");

            bool stopMove = s.CastTime > 0;

            if (onPlayerFocus != null || onUnitFocus != null)
            {
                if (onPlayerFocus != null && (!onPlayerFocus.IsValid || onPlayerFocus.GetDistance > 50))
                    return false;
                if (onUnitFocus != null && (!onUnitFocus.IsValid || onUnitFocus.GetDistance > 50))
                    return false;

                string focusName = onPlayerFocus != null ? onPlayerFocus.Name : onUnitFocus.Name;
                float focusDistance = onPlayerFocus != null ? onPlayerFocus.GetDistance : onUnitFocus.GetDistance;
                Vector3 focusPosition = onPlayerFocus != null ? onPlayerFocus.Position : onUnitFocus.Position;
                ulong focusGuid = onPlayerFocus != null ? onPlayerFocus.Guid : onUnitFocus.Guid;

                if (focusDistance > s.MaxRange || TraceLine.TraceLineGo(focusPosition))
                {
                    if (Me.HaveBuff("Spirit of Redemption"))
                        return false;

                    Logger.Log($"Approaching {focusName}");
                    List<Vector3> path = PathFinder.FindPath(focusPosition);
                    if (path.Count <= 0)
                    {
                        Logger.Log($"Couldn't make a path toward {focusName}, skipping");
                        return false;
                    }
                    MovementManager.Go(path, false);

                    int limit = 3000;
                    while (MovementManager.InMoveTo
                    && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && Me.IsAlive
                    && focusDistance > 20 || TraceLine.TraceLineGo(focusPosition)
                    && limit >= 0)
                    {
                        focusDistance = onPlayerFocus != null ? onPlayerFocus.GetDistance : onUnitFocus.GetDistance;
                        focusPosition = onPlayerFocus != null ? onPlayerFocus.Position : onUnitFocus.Position;
                        Thread.Sleep(1000);
                        limit -= 1000;
                    }
                }

                Logger.LogFight($"Casting {s.Name} on {focusName}");
                MovementManager.StopMove();
                //Lua.LuaDoString($"FocusUnit(\"{focusName}\")");
                ObjectManager.Me.FocusGuid = focusGuid;
                Lua.RunMacroText($"/cast [target=focus] {s.Name}");
                Usefuls.WaitIsCasting();
                return true;
            }

            s.Launch(stopMove, true, true);

            return true;
        }

        // Stops using wand and waits for its CD to be over
        private void StopWandWaitGCD(AIOSpell wandSpell, AIOSpell spellToWaitFor)
        {
            CombatDebug("Stopping Wand and waiting for GCD");
            wandSpell.Launch();
            int c = 0;
            while (!spellToWaitFor.IsSpellUsable && c <= 1500)
            {
                c += 50;
                Thread.Sleep(50);
            }
            CombatDebug("Waited for GCD : " + c);
            if (c > 1500)
                wandSpell.Launch();
        }

        private void CombatDebug(string s)
        {
            if (CombatDebugON)
                Logger.CombatDebug(s);
        }

        private void LuaEventsHandler(string id, List<string> args)
        {
            if (AutoDetectImmunities && args[11] == "IMMUNE" && !BannedSpells.Contains(args[9]))
            {
                Logger.Log($"{ObjectManager.Target.Name} is immune to {args[9]}, banning spell for this fight");
                BannedSpells.Add(args[9]);
            }
        }
    }
}
