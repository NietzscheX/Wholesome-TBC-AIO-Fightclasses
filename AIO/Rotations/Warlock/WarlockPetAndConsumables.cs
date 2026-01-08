using System.Collections.Generic;
using WholesomeTBCAIO.Helpers;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public static class WarlockPetAndConsumables
    {
        #region Item ID Lists (language-independent)
        
        // Soulstone IDs
        public static readonly List<uint> SOULSTONE_IDS = new List<uint>
        {
            ItemIds.MinorSoulstone,
            ItemIds.LesserSoulstone,
            ItemIds.Soulstone,
            ItemIds.GreaterSoulstone,
            ItemIds.MajorSoulstone,
            ItemIds.MasterSoulstone
        };

        // Healthstone IDs
        public static readonly List<uint> HEALTHSTONE_IDS = new List<uint>
        {
            ItemIds.MinorHealthstone,
            ItemIds.LesserHealthstone,
            ItemIds.Healthstone,
            ItemIds.GreaterHealthstone,
            ItemIds.MajorHealthstone,
            ItemIds.MasterHealthstone
        };
        
        #endregion

        #region Legacy string lists (for backwards compatibility)
        
        // Soulstones list
        public static readonly List<string> SOULSTONES = new List<string>
        {
            "Minor Soulstone",
            "Lesser Soulstone",
            "Soulstone",
            "Major Soulstone",
            "Greater Soulstone",
            "Master Soulstone"
        };

        // Healthstones list
        public static readonly List<string> HEALTHSTONES = new List<string>
        {
            "Minor Healthstone",
            "Lesser Healthstone",
            "Healthstone",
            "Greater Healthstone",
            "Major Healthstone",
            "Master Healthstone"
        };
        
        #endregion

        // Checks if we have a Healthstone (language-independent)
        public static bool HaveHealthstone()
        {
            return ToolBox.HaveAnyItemById(HEALTHSTONE_IDS);
        }

        // Use Healthstone (language-independent)
        public static void UseHealthstone()
        {
            ToolBox.UseFirstMatchingItemById(HEALTHSTONE_IDS);
        }

        // Checks if we have a Soulstone (language-independent)
        public static bool HaveSoulstone()
        {
            return ToolBox.HaveAnyItemById(SOULSTONE_IDS);
        }

        // Use Soulstone (language-independent)
        public static void UseSoulstone()
        {
            ToolBox.UseFirstMatchingItemById(SOULSTONE_IDS);
        }

        // Get Soulstone cooldown (language-independent)
        public static int GetSoulstoneCooldown()
        {
            foreach (uint id in SOULSTONE_IDS)
            {
                if (ToolBox.HaveItemById(id))
                    return ToolBox.GetItemCooldownById(id);
            }
            return 0;
        }

        public static void Setup()
        {
            // Add Soul Shard to do not sell list by ID
            WTSettings.AddItemToDoNotSellListByID(ItemIds.SoulShard);
            
            // Add all soulstones and healthstones by ID
            foreach (uint id in SOULSTONE_IDS)
                WTSettings.AddItemToDoNotSellListByID((int)id);
            foreach (uint id in HEALTHSTONE_IDS)
                WTSettings.AddItemToDoNotSellListByID((int)id);
        }

        // Returns which pet the warlock has summoned (language-independent using icon paths)
        public static string MyWarlockPet()
        {
            return Lua.LuaDoString<string>
                ($"for i=1,10 do " +
                    "local name, _, icon = GetPetActionInfo(i); " +
                    "if icon then " +
                        "if string.find(string.lower(icon), 'firebolt') then " +
                        "return 'Imp' " +
                        "end " +
                        "if string.find(string.lower(icon), 'gathershadows') then " +
                        "return 'Voidwalker' " +
                        "end " +
                        "if string.find(string.lower(icon), 'painspike') or string.find(string.lower(icon), 'warrior_cleave') then " +
                        "return 'Felguard' " +
                        "end " +
                        "if string.find(string.lower(icon), 'spellshadowsummonvoidwalker') then " +
                        "return 'Voidwalker' " +
                        "end " +
                        "if string.find(string.lower(icon), 'spellshadowsuccubus') then " +
                        "return 'Succubus' " +
                        "end " +
                        "if string.find(string.lower(icon), 'felhunter') then " +
                        "return 'Felhunter' " +
                        "end " +
                    "end " +
                "end");
        }

        /// <summary>
        /// Count Soul Shards in bags (language-independent)
        /// </summary>
        public static int CountSoulShards()
        {
            return ToolBox.CountItemStacksById(ItemIds.SoulShard);
        }

        /// <summary>
        /// Delete one Soul Shard (language-independent)
        /// </summary>
        public static void DeleteOneSoulShard()
        {
            ToolBox.DeleteItemById(ItemIds.SoulShard);
        }
    }
}