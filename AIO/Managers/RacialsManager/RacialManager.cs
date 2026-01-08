using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.PartyManager;
using WholesomeTBCAIO.Managers.UnitCache;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static WholesomeTBCAIO.Helpers.Enums;

namespace WholesomeTBCAIO.Managers.RacialsManager
{
    internal class RacialManager : IRacialsManager
    {
        private AIOSpell Cannibalize = new AIOSpell("Cannibalize");
        private AIOSpell WillOfTheForsaken = new AIOSpell("Will of the Forsaken");
        private AIOSpell Berserking = new AIOSpell("Berserking");
        private AIOSpell EscapeArtist = new AIOSpell("Escape Artist");
        private AIOSpell ManaTap = new AIOSpell("Mana Tap");
        private AIOSpell ArcaneTorrent = new AIOSpell("Arcane Torrent");
        private AIOSpell Stoneform = new AIOSpell("Stoneform");
        private AIOSpell GiftOfTheNaaru = new AIOSpell("Gift of the Naaru");
        private AIOSpell WarStomp = new AIOSpell("War Stomp");
        private AIOSpell BloodFury = new AIOSpell("Blood Fury");
        private WoWLocalPlayer Me = ObjectManager.Me;
        private readonly BackgroundWorker _racialsThread = new BackgroundWorker();
        private readonly IPartyManager _partyManager;
        private readonly IUnitCache _unitCache;
        private bool _isRunning;
        private AuraHelper _auraHelper = new AuraHelper();

        public RacialManager(IPartyManager partyManager, IUnitCache unitCache)
        {
            _partyManager = partyManager;
            _unitCache = unitCache;
        }

        public void Initialize()
        {
            _isRunning = true;
            _racialsThread.DoWork += Pulse;
            _racialsThread.RunWorkerAsync();
        }

        public void Dispose()
        {
            _racialsThread.DoWork -= Pulse;
            _racialsThread.Dispose();
            _isRunning = false;
        }

        public void Pulse(object sender, DoWorkEventArgs args)
        {
            while (_isRunning)
            {
                try
                {
                    if (StatusChecker.BasicConditions())
                    {
                        if (StatusChecker.OutOfCombat(RotationRole.None))
                            RacialCannibalize();

                        if (StatusChecker.InCombat())
                        {
                            RacialManaTap();
                            RacialWillOfTheForsaken();
                            RacialEscapeArtist();
                            RacialBerserking();
                            RacialArcaneTorrent();
                            RacialStoneForm();
                            RacialGiftOfTheNaaru();
                            RacialWarStomp();
                            RacialBloodFury();
                        }
                    }
                }
                catch (Exception arg)
                {
                    Logger.LogError(string.Concat(arg));
                }
                Thread.Sleep(500);
            }
        }

        private void RacialBloodFury()
        {
            if (BloodFury.KnownSpell
                && _unitCache.GroupAndRaid.Count <= 1
                && BloodFury.IsSpellUsable
                && ObjectManager.Target.HealthPercent > 70)
            {
                BloodFury.Launch();
            }
        }

        private void RacialWarStomp()
        {
            if (WarStomp.KnownSpell
                && WarStomp.IsSpellUsable
                && !_auraHelper.PlayerHasAuraById(SpellIds.BearForm)
                && !_auraHelper.PlayerHasAuraById(SpellIds.CatForm)
                && !_auraHelper.PlayerHasAuraById(SpellIds.DireBearForm)
                && _unitCache.EnemiesAttackingMe.Count > 1
                && ObjectManager.Target.GetDistance < 8)
            {
                WarStomp.Launch();
                Usefuls.WaitIsCasting();
            }
        }

        private void RacialStoneForm()
        {
            if (Stoneform.KnownSpell
                && Stoneform.IsSpellUsable
                && (WTEffects.HasPoisonDebuff() || WTEffects.HasDiseaseDebuff() || HasBleedDebuff()))
            {
                Stoneform.Launch();
                Usefuls.WaitIsCasting();
            }
        }

        /// <summary>
        /// Check if player has a bleed debuff (language-independent using debuff type)
        /// </summary>
        private bool HasBleedDebuff()
        {
            // Bleed is a debuff type, check using UnitDebuff with type check
            return Lua.LuaDoString<bool>(@"
                for i=1,40 do
                    local _, _, _, _, debuffType = UnitDebuff('player', i);
                    if debuffType == 'Bleed' then
                        return true;
                    end
                end
                return false;
            ");
        }

        private void RacialGiftOfTheNaaru()
        {
            if (GiftOfTheNaaru.KnownSpell
                && GiftOfTheNaaru.IsSpellUsable
                && _unitCache.EnemiesAttackingMe.Count > 1 && Me.HealthPercent < 50)
            {
                GiftOfTheNaaru.Launch();
                Usefuls.WaitIsCasting();
            }
        }

        private void RacialArcaneTorrent()
        {
            if (ArcaneTorrent.KnownSpell
                && ArcaneTorrent.IsSpellUsable
                && _auraHelper.PlayerHasAuraById(SpellIds.ManaTap)
                && (Me.ManaPercentage < 50 || (ObjectManager.Target.IsCast && ObjectManager.Target.GetDistance < 8)))
            {
                ArcaneTorrent.Launch();
            }
        }

        private void RacialBerserking()
        {
            if (Berserking.KnownSpell
                && Berserking.IsSpellUsable
                && ObjectManager.Target.HealthPercent > 70)
            {
                Berserking.Launch();
            }
        }

        private void RacialEscapeArtist()
        {
            if (EscapeArtist.KnownSpell
                && EscapeArtist.IsSpellUsable
                && (Me.Rooted || _auraHelper.PlayerHasAuraById(SpellIds.FrostNova)))
            {
                EscapeArtist.Launch();
                Usefuls.WaitIsCasting();
            }
        }

        private void RacialWillOfTheForsaken()
        {
            if (WillOfTheForsaken.KnownSpell
                && WillOfTheForsaken.IsSpellUsable
                && (HasFearDebuff() || HasCharmDebuff() || HasSleepDebuff()))
            {
                WillOfTheForsaken.Launch();
            }
        }

        /// <summary>
        /// Check if player has a fear debuff (language-independent using debuff type)
        /// </summary>
        private bool HasFearDebuff()
        {
            return Lua.LuaDoString<bool>(@"
                for i=1,40 do
                    local _, _, _, _, debuffType = UnitDebuff('player', i);
                    if debuffType == 'Fear' then
                        return true;
                    end
                end
                return false;
            ");
        }

        /// <summary>
        /// Check if player has a charm debuff (language-independent using debuff type)
        /// </summary>
        private bool HasCharmDebuff()
        {
            return Lua.LuaDoString<bool>(@"
                for i=1,40 do
                    local _, _, _, _, debuffType = UnitDebuff('player', i);
                    if debuffType == 'Charm' then
                        return true;
                    end
                end
                return false;
            ");
        }

        /// <summary>
        /// Check if player has a sleep debuff (language-independent using debuff type)
        /// </summary>
        private bool HasSleepDebuff()
        {
            return Lua.LuaDoString<bool>(@"
                for i=1,40 do
                    local _, _, _, _, debuffType = UnitDebuff('player', i);
                    if debuffType == 'Sleep' then
                        return true;
                    end
                end
                return false;
            ");
        }

        private void RacialManaTap()
        {
            if (ManaTap.IsDistanceGood
                && ManaTap.IsSpellUsable
                && ManaTap.KnownSpell
                && ObjectManager.Target.Mana > 0
                && ObjectManager.Target.ManaPercentage > 10)
            {
                ManaTap.Launch();
            }
        }

        private void RacialCannibalize()
        {
            // Cannibalize - use creature type check (language-independent)
            if (Cannibalize.KnownSpell
                && Cannibalize.IsSpellUsable
                && Me.HealthPercent < 50
                && !_auraHelper.PlayerHasDrinkAura()
                && !_auraHelper.PlayerHasFoodAura()
                && Me.IsAlive
                && ObjectManager.GetObjectWoWUnit().Where(u => 
                    u.GetDistance <= 8 
                    && u.IsDead 
                    && (AuraHelper.IsHumanoid("target") || AuraHelper.IsUndead("target"))).Count() > 0)
            {
                Cannibalize.Launch();
                Usefuls.WaitIsCasting();
            }
        }
    }
}
