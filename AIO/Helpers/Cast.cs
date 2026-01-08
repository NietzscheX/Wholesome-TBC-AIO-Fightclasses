using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Managers.UnitCache;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Helpers
{
    public class Cast
    {
        private readonly IUnitCache _unitCache;
        private readonly AIOSpell _wandSpell;
        private readonly bool autoDetectImmunities;
        private readonly bool _combatDebugON;
        private readonly AIOSpell _defaultBaseSpell;
        private readonly bool _combatLogON;
        private readonly Random rng = new Random();
        private IWoWUnit _currentSpellTarget;
        private AIOSpell _currentSpell;
        private Vector3 _currentSpellLocation;
        //private Dictionary<string, int> _petSpells = new Dictionary<string, int>(); // spell name => spell index in spellbook
        private List<AIOPetSpell> _petSpells = new List<AIOPetSpell>();

        public bool IsBackingUp { get; set; }
        public bool IsApproachingTarget { get; set; }

        public Cast(AIOSpell defaultBaseSpell, AIOSpell wandSpell, BaseSettings settings, IUnitCache unitCache)
        {
            _unitCache = unitCache;
            autoDetectImmunities = settings.AutoDetectImmunities;
            _defaultBaseSpell = defaultBaseSpell;
            _combatDebugON = settings.ActivateCombatDebug;
            _wandSpell = wandSpell;
            IsApproachingTarget = false;
            _combatLogON = settings.ActivateCombatLog;
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsLuaStringWithArgsHandler;
            FightEvents.OnFightLoop += FightLoopHandler;
            FightEvents.OnFightStart += FightStartHandler;
            UpdatePetSpells();
        }

        public void Dispose()
        {
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsLuaStringWithArgsHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
            FightEvents.OnFightStart -= FightStartHandler;
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            UpdatePetSpells();
        }

        private void UpdatePetSpells()
        {
            _petSpells.Clear();

            // Get pet spells with spell IDs using GetSpellLink to extract the ID
            string[] luaPetSpells = Lua.LuaDoString<string[]>($@"
                local result = {{}};
                for i=1,20 do
                    local name, rank, icon, powerCost, isFunnel, powerType, castingTime, minRange, maxRange = GetSpellInfo(i, 'pet');
                    if name ~= nil then
                        -- Extract spell ID from GetSpellLink
                        local link = GetSpellLink(i, 'pet');
                        local spellId = 0;
                        if link then
                            spellId = tonumber(string.match(link, 'spell:(%d+)')) or 0;
                        end
                        -- Extract rank number from rank string (works for any language)
                        local rankNum = 0;
                        if rank ~= nil and rank ~= '' then
                            local num = string.match(rank, '%d+');
                            if num ~= nil then
                                rankNum = num;
                            end
                        end
                        table.insert(result, i .. '$' .. spellId .. '$' .. name .. '$' .. (rank or '') .. '$' .. rankNum .. '$' .. powerCost .. '$' .. tostring(isFunnel) .. '$' .. powerType .. '$' .. castingTime .. '$' .. minRange .. '$' .. maxRange);
                    end
                end
                return unpack(result);
            ");

            foreach(string spellLine in luaPetSpells)
            {
                string[] spellProps = spellLine.Split('$');
                if (spellProps.Length == 11)
                {
                    int index = int.Parse(spellProps[0]);
                    uint spellId = uint.Parse(spellProps[1]);
                    string name = spellProps[2];
                    string localizedRankString = spellProps[3];
                    int rank = 0;
                    if (!string.IsNullOrEmpty(spellProps[4]))
                    {
                        int.TryParse(spellProps[4], out rank);
                    }
                    int powerCost = int.Parse(spellProps[5]);
                    bool isFunnel = spellProps[6] == "true" ? true : false;
                    int powerType = int.Parse(spellProps[7]);
                    int castingTime = int.Parse(spellProps[8]);
                    int minRange = int.Parse(spellProps[9]);
                    int maxRange = int.Parse(spellProps[10]);                    
                    _petSpells.Add(new AIOPetSpell(index, spellId, name, localizedRankString, rank, powerCost, isFunnel, powerType, castingTime, minRange, maxRange));
                }
            }
        }

        /// <summary>
        /// Cast a pet spell if there's enough focus for both the spell and Growl (using spell ID)
        /// </summary>
        /// <param name="spellId">The spell ID to cast</param>
        public bool PetSpellIfEnoughForGrowl(uint spellId)
        {
            AIOPetSpell spell = _petSpells.Where(s => s.SpellId == spellId).FirstOrDefault();
            AIOPetSpell growlSpell = _petSpells.Where(s => s.SpellId == PetSpellIds.Growl).FirstOrDefault();
            if (spell == null) return false;
            int growlCost = growlSpell == null ? 0 : growlSpell.Cost;

            if (ObjectManager.Pet.Focus >= spell.Cost + growlCost)
            {
                ObjectManager.Me.FocusGuid = ObjectManager.Pet.Target;
                if (PetSpellById(spellId, true))
                {
                    Lua.LuaDoString("ClearFocus();");
                    return true;
                }
                Lua.LuaDoString("ClearFocus();");
            }
            return false;
        }

        /// <summary>
        /// Cast a pet spell using spell ID (language-independent)
        /// </summary>
        /// <param name="spellId">The spell ID to cast</param>
        /// <param name="onFocus">Cast on focus target</param>
        /// <param name="noTargetNeeded">Cast without requiring a target</param>
        public bool PetSpellById(uint spellId, bool onFocus = false, bool noTargetNeeded = false)
        {
            AIOPetSpell spell = _petSpells.Where(s => s.SpellId == spellId).FirstOrDefault();
            if (spell == null) return false;

            if (WTPet.PetSpellReady(spell.Index)
                && !UnitImmunities.Contains(_unitCache.Target, spell.Name)
                && (ObjectManager.Pet.HasTarget || noTargetNeeded))
            {
                Thread.Sleep(ToolBox.GetLatency() + 100);
                Logger.Combat($"Cast (Pet) {spell.Name} (ID: {spellId})");
                if (!onFocus)
                {
                    Lua.LuaDoString($"CastSpell({spell.Index}, 'pet');");
                }
                else
                {
                    // Use localized rank string for casting
                    string rankString = !string.IsNullOrEmpty(spell.LocalizedRankString) ? $"({spell.LocalizedRankString})" : "";
                    Lua.RunMacroText($"/cast [target=focus] {spell.Name}{rankString}");
                }
                return true;
            }
            return false;
        }

        // Legacy method for backwards compatibility - tries to find spell by localized name
        public bool PetSpellIfEnoughForGrowl(string spellName)
        {
            AIOPetSpell spell = _petSpells.Where(s => s.Name == spellName).FirstOrDefault();
            AIOPetSpell growlSpell = _petSpells.Where(s => s.SpellId == PetSpellIds.Growl).FirstOrDefault();
            if (spell == null) return false;
            int growlCost = growlSpell == null ? 0 : growlSpell.Cost;

            if (ObjectManager.Pet.Focus >= spell.Cost + growlCost)
            {
                ObjectManager.Me.FocusGuid = ObjectManager.Pet.Target;
                if (PetSpell(spellName, true))
                {
                    Lua.LuaDoString("ClearFocus();");
                    return true;
                }
                Lua.LuaDoString("ClearFocus();");
            }
            return false;
        }

        // Legacy method for backwards compatibility - tries to find spell by localized name
        public bool PetSpell(string spellName, bool onFocus = false, bool noTargetNeeded = false)
        {
            AIOPetSpell spell = _petSpells.Where(s => s.Name == spellName).FirstOrDefault();
            if (spell == null) return false;

            if (WTPet.PetSpellReady(spell.Index)
                && !UnitImmunities.Contains(_unitCache.Target, spellName)
                && (ObjectManager.Pet.HasTarget || noTargetNeeded))
            {
                Thread.Sleep(ToolBox.GetLatency() + 100);
                Logger.Combat($"Cast (Pet) {spell.Name}");
                if (!onFocus)
                {
                    Lua.LuaDoString($"CastSpell({spell.Index}, 'pet');");
                }
                else
                {
                    // Use localized rank string for casting
                    string rankString = !string.IsNullOrEmpty(spell.LocalizedRankString) ? $"({spell.LocalizedRankString})" : "";
                    Lua.RunMacroText($"/cast [target=focus] {spell.Name}{rankString}");
                }
                return true;
            }
            return false;
        }

        public int GetPetSpellIndex(uint spellId)
        {
            AIOPetSpell spell = _petSpells.Where(s => s.SpellId == spellId).FirstOrDefault();
            return spell != null ? spell.Index : 0;
        }

        public string GetPetSpellLocalizedName(uint spellId)
        {
            AIOPetSpell spell = _petSpells.Where(s => s.SpellId == spellId).FirstOrDefault();
            return spell != null ? spell.Name : "";
        }

        public bool Buff(IWoWPlayer unit, AIOSpell spell, uint reagent = 0)
        {
            return Buff(new List<IWoWPlayer>() { unit }, spell, reagent);
        }

        public bool Buff(List<IWoWPlayer> units, AIOSpell spell, uint reagent = 0)
        {
            if (reagent != 0 && !ItemsManager.HasItemById(reagent))
                return false;
            List<IWoWPlayer> unitsNeedsBuff = units
                .FindAll(m => !m.HasAura(spell))
                .OrderBy(m => rng.Next())
                .ToList();
            if (unitsNeedsBuff.Count <= 0)
                return false;
            return OnFocusUnit(spell, unitsNeedsBuff[0]);
        }

        public bool OnTarget(AIOSpell s, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast);
        }

        public bool OnSelf(AIOSpell s, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast, true);
        }

        public bool OnLocation(AIOSpell s, Vector3 location, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast, location: location);
        }

        public bool OnFocusUnit(AIOSpell s, IWoWUnit onUnitFocus, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast, onUnitFocus: onUnitFocus);
        }

        public bool AdvancedCast(AIOSpell spell, bool stopWandAndCast = true, bool onSelf = false, IWoWUnit onUnitFocus = null, Vector3 location = null)
        {
            WoWUnit Me = ObjectManager.Me;
            float buffer = 600;

            if (IsApproachingTarget)
                return true;

            _currentSpell = spell;
            _currentSpellLocation = location;

            CombatDebug("*----------- INTO PRE CAST FOR " + _currentSpell.Name);

            if (!_currentSpell.KnownSpell
                || IsBackingUp
                || Me.CastingTimeLeft > buffer
                || !_currentSpell.IsForcedCooldownReady
                || Me.IsStunned)
                return false;

            // Define target
            if (onUnitFocus != null)
                _currentSpellTarget = onUnitFocus;
            else if (_currentSpellLocation != null)
                _currentSpellTarget = null;
            else if (onSelf)
                _currentSpellTarget = _unitCache.Me;
            else
            {
                if (_currentSpell.MaxRange <= 0 && _unitCache.Target.GetDistance > RangeManager.GetMeleeRangeWithTarget())
                    return false;
                _currentSpellTarget = _unitCache.Target;
            }

            CombatDebug("*----------- TARGET IS " + _currentSpellTarget?.Name);

            // Now that we know the target
            if (_currentSpellLocation == null)
            {
                if (_currentSpellTarget == null
                    || _currentSpellTarget.GetDistance > 100
                    || (_currentSpellTarget.IsDead && !_currentSpell.OnDeadTarget)
                    || (_currentSpell.MinRange > 0 && _currentSpellTarget.GetDistance <= _currentSpell.MinRange)
                    || UnitImmunities.Contains(_currentSpellTarget, _currentSpell.Name)
                    || (!_currentSpellTarget.IsValid && !_currentSpell.OnDeadTarget)) // double check this
                    return false;
            }

            CombatDebug("*----------- INTO CAST FOR " + _currentSpell.Name);

            // CHECK COST
            if (_currentSpell.PowerType == -2 && Me.Health < _currentSpell.Cost)
            {
                CombatDebug($"{_currentSpell.Name}: Not enough health {_currentSpell.Cost}/{Me.Health}, SKIPPING");
                return false;
            }
            else if (_currentSpell.PowerType == 0 && Me.Mana < _currentSpell.Cost)
            {
                CombatDebug($"{_currentSpell.Name}: Not enough mana {_currentSpell.Cost}/{Me.Mana}, SKIPPING");
                return false;
            }
            else if (_currentSpell.PowerType == 1 && Me.Rage < _currentSpell.Cost)
            {
                CombatDebug($"{_currentSpell.Name}: Not enough rage {_currentSpell.Cost}/{Me.Rage}, SKIPPING");
                return false;
            }
            else if (_currentSpell.PowerType == 2 && ObjectManager.Pet.Focus < _currentSpell.Cost)
            {
                CombatDebug($"{_currentSpell.Name}: Not enough pet focus {_currentSpell.Cost}/{ObjectManager.Pet.Focus}, SKIPPING");
                return false;
            }
            else if (_currentSpell.PowerType == 3 && Me.Energy < _currentSpell.Cost)
            {
                CombatDebug($"{_currentSpell.Name}: Not enough energy {_currentSpell.Cost}/{Me.Energy}, SKIPPING");
                return false;
            }

            // DON'T CAST BECAUSE WANDING
            if (_wandSpell != null
                && WTCombat.IsSpellRepeating(5019)
                && !stopWandAndCast)
            {
                CombatDebug("Didn't cast because we were wanding");
                return false;
            }

            // COOLDOWN CHECKS
            float _spellCD = _currentSpell.GetCurrentCooldown;
            CombatDebug($"Cooldown is {_spellCD}");

            if (_spellCD >= 500)
            {
                CombatDebug("Didn't cast because cd is too long");
                return _spellCD < 1500 && _wandSpell != null; // recycle if it's just global CD (avoid wand weave)
            }

            // STOP WAND FOR CAST
            if (_wandSpell != null
                && WTCombat.IsSpellRepeating(5019)
                && stopWandAndCast)
                StopWandWaitGCD(_wandSpell);


            // Wait for remaining Cooldown
            if (_spellCD > 0f && _spellCD < buffer)
            {
                CombatDebug($"{_currentSpell.Name} is almost ready, waiting");
                while (_currentSpell.GetCurrentCooldown > 0 && _currentSpell.GetCurrentCooldown < 500)
                    Thread.Sleep(50);
                Thread.Sleep(50); // safety
            }

            if (!_currentSpell.IsSpellUsable)
            {
                CombatDebug("Didn't cast because spell somehow not usable");
                return false;
            }

            bool stopMove = _currentSpell.CastTime > 0 || _currentSpell.IsChannel;

            if (_currentSpellLocation != null || _currentSpellTarget.Guid != Me.Guid)
            {
                Vector3 spellPosition = _currentSpellLocation != null ? _currentSpellLocation : _currentSpellTarget.PositionWithoutType;
                if (_currentSpell.MaxRange > 0 && spellPosition.DistanceTo(Me.Position) > _currentSpell.MaxRange || TraceLine.TraceLineGo(spellPosition))
                {
                    if (AuraHelper.UnitHasAuraById(Me, SpellIds.SpiritOfRedemption))
                        return false;

                    Logger.LogFight($"Target not in range/sight, recycling {_currentSpell.Name}");

                    if (Fight.InFight)
                        IsApproachingTarget = true;
                    else
                        ApproachSpellTarget();

                    return true;
                }
            }

            if (onUnitFocus != null)
                _unitCache.Me.SetFocus(_currentSpellTarget.Guid);

            string unit = onUnitFocus != null ? "focus" : "target";
            unit = onSelf || _currentSpellLocation != null ? "player" : unit;

            // Wait for remaining cast in case of buffer
            while (Me.CastingTimeLeft > 0)
                Thread.Sleep(25);

            if (stopMove)
                MovementManager.StopMoveNewThread();

            if (_combatLogON)
            {
                string rankString = _currentSpell.Rank > 0 ? $"(Rank {_currentSpell.Rank})" : "";
                string target = _currentSpellLocation != null ? _currentSpellLocation.ToString() : _currentSpellTarget.Name;
                Logger.Log($"[Spell] Casting {_currentSpell.Name.Replace("()", "")} {rankString} on {target}");
            }

            _currentSpell.Launch(stopMove, false, true, unit);

            if (location != null)
            {
                ClickOnTerrain.Pulse(_currentSpellLocation);
            }

            Thread.Sleep(100);

            ToolBox.ClearCursor();

            // Wait for channel to end
            if (_currentSpell.IsChannel)
            {
                CombatDebug($"{_currentSpell.Name} is channel, wait cast");
                while (WTCombat.GetChannelTimeLeft("player") > 0)
                    Thread.Sleep(50);

                _currentSpell.StartForcedCooldown();
                return true;
            }

            // Wait for instant cast GCD
            if (_currentSpell.CastTime <= 0)
            {
                _currentSpell.StartForcedCooldown();
                Timer gcdLimit = new Timer(1500);
                CombatDebug($"{_currentSpell.Name} is instant, wait GCD");
                while (_defaultBaseSpell.GetCurrentCooldown > buffer && !gcdLimit.IsReady)
                    Thread.Sleep(50);

                if (gcdLimit.IsReady)
                    Logger.LogError("We had to resort to timer wait (GCD)");

                return true;
            }

            if (_currentSpell.CastTime > 0)
            {
                // Wait for cast to end
                buffer = _currentSpell.PreventDoubleCast ? 0 : buffer;
                CombatDebug($"{_currentSpell.Name} is normal, wait until {buffer} left");
                while (Me.CastingTimeLeft > buffer)
                {
                    if (_currentSpell.IsResurrectionSpell && !_currentSpellTarget.IsDead)
                        Lua.RunMacroText("/stopcasting");

                    Thread.Sleep(50);
                }

                if (_currentSpell.PreventDoubleCast)
                    Thread.Sleep(100);

                _currentSpell.StartForcedCooldown();
            }

            return true;
        }

        // Manage spell target approach
        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            if (IsApproachingTarget)
            {
                ApproachSpellTarget();
                IsApproachingTarget = false;
            }
        }

        // Approach spell target
        private void ApproachSpellTarget()
        {
            Timer limit = new Timer(10000);
            if (_currentSpellTarget != null)
            {
                WoWUnit unit = _currentSpellTarget.WowUnit;
                Logger.Log($"Approaching {unit.Name} to cast {_currentSpell.Name}");
                MovementManager.Go(PathFinder.FindPath(unit.PositionWithoutType), false);
                Thread.Sleep(1000);
                while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && !limit.IsReady
                    && (!unit.IsDead || _currentSpell.OnDeadTarget)
                    && (unit.GetDistance > _currentSpell.MaxRange - 2 || TraceLine.TraceLineGo(_unitCache.Me.PositionWithoutType, unit.PositionWithoutType, CGWorldFrameHitFlags.HitTestSpellLoS)))
                {
                    Thread.Sleep(100);
                    //Logger.Log($"Approaching {_currentSpellTarget.Name} to cast {_currentSpell.Name} ({_currentSpellTarget.GetDistance}/{_currentSpell.MaxRange} {TraceLine.TraceLineGo(_unitCache.Me.PositionWithoutType, _currentSpellTarget.PositionWithoutType, CGWorldFrameHitFlags.HitTestSpellLoS)}");
                }
                MovementManager.StopMoveNewThread();
            }
            else if (_currentSpellLocation != null)
            {
                Logger.Log($"Approaching {_currentSpellLocation} to cast {_currentSpell.Name}");
                MovementManager.Go(PathFinder.FindPath(_currentSpellLocation), false);
                Thread.Sleep(1000);
                while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && !limit.IsReady
                    && (_currentSpellLocation.DistanceTo(ObjectManager.Me.Position) > _currentSpell.MaxRange - 2))
                    Thread.Sleep(100);
                MovementManager.StopMoveNewThread();
            }
        }

        // Stops using wand and waits for its CD to be over
        private void StopWandWaitGCD(AIOSpell wandSpell)
        {
            CombatDebug("Stopping Wand and waiting for GCD");
            wandSpell.Launch();
            Timer limit = new Timer(1500);

            while (!_currentSpell.IsSpellUsable && !limit.IsReady)
                Thread.Sleep(50);

            CombatDebug($"Waited for GCD after wand stop : {1500 - limit.TimeLeft()}");
        }

        private void CombatDebug(string s)
        {
            if (_combatDebugON)
                Logger.CombatDebug(s);
        }

        private void EventsLuaStringWithArgsHandler(string id, List<string> args)
        {
            if (args.Count >= 12 && autoDetectImmunities && args[11] == "IMMUNE")
                UnitImmunities.Add(_currentSpellTarget, args[9]);

            if (args.Count >= 12 && args[11] == "Target not in line of sight" && Fight.InFight)
            {
                Logger.Log("Forcing Approach");
                IsApproachingTarget = true;
            }

            if (id == "SPELLS_CHANGED")
            {
                UpdatePetSpells();
            }
        }
    }
}
