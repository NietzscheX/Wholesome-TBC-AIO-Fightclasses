using System.Collections.Generic;
using WholesomeToolbox;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Helpers
{
    public class AIOSpell : Spell
    {
        public new string Name { get; }
        /// <summary>
        /// The spell name in the client's language (e.g., Chinese, German, etc.)
        /// Used for casting spells via macro commands
        /// </summary>
        public string LocalizedName { get; }
        /// <summary>
        /// The localized rank string from the client (e.g., "等级 1" for Chinese, "Rank 1" for English)
        /// </summary>
        public string LocalizedRankString { get; }
        public int Rank { get; }
        public int Cost { get; }
        public int PowerType { get; }
        public new float CastTime { get; }
        public new float MinRange { get; }
        public new float MaxRange { get; }
        public bool ForceLua { get; }
        public bool IsChannel { get; }
        public bool IsClickOnTerrain { get; }
        public bool IsResurrectionSpell { get; }
        public bool PreventDoubleCast { get; }
        public bool OnDeadTarget { get; }
        public uint SpellId { get; }
        public new bool IsSpellUsable
        {
            get
            {
                if (!ForceLua)
                    return base.IsSpellUsable;
                else
                    return KnownSpell && GetCurrentCooldown < 0;
            }
        }
        private int ForcedCooldown { get; set; }
        private Timer ForcedCooldownTimer { get; set; } = new Timer();

        private static List<AIOSpell> AllSpells = new List<AIOSpell>();

        public AIOSpell(string spellName, int rank = 0) : base(spellName)
        {
            Name = spellName;
            IsChannel = ChannelSpells.Contains(Name);
            PreventDoubleCast = SpellsToKeepFromDoubleCasting.Contains(Name);
            OnDeadTarget = OnDeadSpells.Contains(Name);
            IsResurrectionSpell = ResurrectionSpells.Contains(Name);
            //IsClickOnTerrain = ClickOnTerrainSpells.Contains(Name);

            // Get spell ID first using English name (WTSpell.GetId works with English names)
            SpellId = WTSpell.GetId(Name, rank);

            if (Name.Contains("(") || Name.Contains(")"))
                Name += "()";

            // Use SpellId to get localized spell info - this works regardless of client language
            // GetSpellInfo(spellId) returns the spell name in the client's language
            string infos = Lua.LuaDoString<string>($@"
                local name, rank, icon, cost, isFunnel, powerType, castTime, minRange, maxRange = GetSpellInfo({SpellId});
                if (name == nil) then return nil end
                local rankNum = 0;
                if (rank ~= nil and rank ~= '') then
                    -- Extract number from rank string (works for any language)
                    local num = string.match(rank, '%d+');
                    if (num ~= nil) then
                        rankNum = num;
                    end
                end
                return name..'$'..rankNum..'$'..(rank or '')..'$'..cost..'$'..powerType..'$'..castTime..'$'..minRange..'$'..maxRange;");
            string[] infosArray = infos.Split('$');

            if (infosArray.Length > 1)
            {
                LocalizedName = infosArray[0];
                Rank = ParseInt(infosArray[1]);
                LocalizedRankString = infosArray[2]; // The full localized rank string (e.g., "等级 1" or "Rank 1")
                Cost = ParseInt(infosArray[3]);
                PowerType = ParseInt(infosArray[4]);
                CastTime = ParseInt(infosArray[5]);
                MinRange = ParseInt(infosArray[6]);
                MaxRange = ParseInt(infosArray[7]);
            }
            else
            {
                // Fallback to English name if spell info not found
                LocalizedName = Name.Replace("()", "");
                LocalizedRankString = rank > 0 ? $"Rank {rank}" : "";
            }

            ForceLua = rank > 0 || Name.Contains("()");

            ForcedCooldown = ForcedCoolDowns.ContainsKey(Name) ? ForcedCoolDowns[Name] : 0;

            AllSpells.Add(this);
            //LogSpellInfos();
        }

        public new void Launch(bool stopMove, bool waitIsCast = true, bool ignoreIfCast = false, string unit = "target")
        {
            if (!ForceLua)
                base.Launch(stopMove, waitIsCast, ignoreIfCast, unit);
            else
            {
                if (stopMove)
                    MovementManager.StopMoveNewThread();

                // Use LocalizedRankString for casting (works for any client language)
                string rankString = !string.IsNullOrEmpty(LocalizedRankString) ? $"({LocalizedRankString})" : "";
                Logger.LogFight($"[Spell-LUA] Cast (on {unit}) {LocalizedName} {rankString}");
                Lua.RunMacroText($"/cast [target={unit}] {LocalizedName}{rankString}");
            }
        }

        public new void Launch()
        {
            if (!ForceLua)
                base.Launch();
            else
            {
                // Use LocalizedRankString for casting (works for any client language)
                string rankString = !string.IsNullOrEmpty(LocalizedRankString) ? $"({LocalizedRankString})" : "";
                Logger.LogFight($"[Spell] Cast (on target) {LocalizedName} {rankString}");
                Lua.RunMacroText($"/cast {LocalizedName}{rankString}");
            }
        }

        public static AIOSpell GetSpellByName(string name) => AllSpells.Find(s => s.Name == name);
        public void StartForcedCooldown()
        {
            if (ForcedCooldown > 0)
                ForcedCooldownTimer = new Timer(ForcedCooldown);
        }
        public bool IsForcedCooldownReady => ForcedCooldownTimer.IsReady;

        public float GetCurrentCooldown => WTCombat.GetSpellCooldown(Name);

        private int ParseInt(string stringToParse)
        {
            if (!int.TryParse(stringToParse, out int result))
                Logger.LogError($"Couldn't parse spell info {stringToParse}");
            return result;
        }

        public void LogSpellInfos()
        {
            Logger.Log($"**************************");
            Logger.Log($"Name : {Name}");
            Logger.Log($"LocalizedName : {LocalizedName}");
            Logger.Log($"SpellId : {SpellId}");
            Logger.Log($"Rank : {Rank}");
            Logger.Log($"LocalizedRankString : {LocalizedRankString}");
            Logger.Log($"Cost : {Cost}");
            Logger.Log($"PowerType : {PowerType}");
            Logger.Log($"CastTime : {CastTime}");
            Logger.Log($"MinRange : {MinRange}");
            Logger.Log($"MaxRange : {MaxRange}");
        }
        /*
        private List<string> ClickOnTerrainSpells = new List<string>()
        {
            "Mass Dispel",
            "Blizzard"
        };
        */
        private List<string> OnDeadSpells = new List<string>()
        {
            "Revive",
            "Rebirth",
            "Redemption",
            "Resurrection",
            "Ancestral Spirit"
        };

        private List<string> SpellsToKeepFromDoubleCasting = new List<string>()
        {
            "Healing Touch",
            "Regrowth",
            "Revive Pet",
            "Polymorph",
            "Hammer of Wrath",
            "Unstable Affliction",
            "Flash of Light",
            "Holy Light",
            "Redemption",
            "Lesser Heal",
            "Heal",
            "Greater Heal",
            "Holy Fire",
            "Flash Heal",
            "Vampiric Touch",
            "Resurrection",
            "Prayer of Healing",
            "Prayer of Mending",
            "Healing Wave",
            "Lesser Healing Wave",
            "Ghost Wolf",
            "Earth Shield",
            "Chain Heal",
            "Ancestral Spirit",
            "Immolate",
            "Corruption",
            "Summon Imp",
            "Summon Voidwalker",
            "Summon Felguard",
            "Create HealthStone",
            "Create Soulstone",
            "Seed of Corruption",
            //"Arcane Blast",
            //"Scorch",
        };

        private List<string> ChannelSpells = new List<string>()
        {
            "Arcane Missiles",
            "Evocation",
            "Mind Flay",
            "Drain Soul",
            "Drain Life",
            "Drain Mana",
            "Health Funnel",
            "Cannibalize",
            "Blizzard"
        };

        private List<string> ResurrectionSpells = new List<string>()
        {
            "Redemption",
            "Resurrection",
            "Ancestral Spirit"
        };

        private Dictionary<string, int> ForcedCoolDowns = new Dictionary<string, int>()
        {
            { "Redemption", 4000 },
            { "Resurrection", 4000 },
            { "Ancestral Spirit", 4000 },
            { "Call Pet", 5000 },
        };
    }
}
