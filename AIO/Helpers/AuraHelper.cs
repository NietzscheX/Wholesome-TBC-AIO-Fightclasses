using System.Collections.Generic;
using System.Linq;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    /// <summary>
    /// Helper class for language-independent aura (buff/debuff) checking using Spell IDs
    /// </summary>
    public class AuraHelper
    {
        #region Drink Aura IDs (same as CachedWoWUnit)
        private static HashSet<uint> _drinkAuras = new HashSet<uint>()
        {
            430, 431, 432, 833, 1133, 1135, 1137, 2639, 10250, 18071, 18140, 
            18233, 22734, 23540, 23541, 23542, 23692, 23698, 24355, 24384, 
            24409, 24410, 24411, 24707, 25690, 25691, 25692, 25693, 25697, 
            25701, 25887, 25990, 26263, 27089, 29007, 29029, 29055, 33266, 
            33772, 34291, 41031, 42308, 42309, 42312, 43154, 43182, 43183, 
            44109, 44110, 44111, 44112, 44113, 44114, 44115, 44116, 44166, 
            45019, 45020, 46755, 49472, 52911, 53373, 56439, 57070, 57085, 
            57096, 57098, 57101, 57106, 57289, 57292, 57333, 57335, 57341, 
            57343, 57344, 57354, 57359, 57364, 57366, 57370, 58067, 58503, 
            58645, 58648, 61827, 61828, 61830, 64056, 64354, 65363, 65418, 
            65419, 65420, 65421, 65422, 69560, 69561, 72623
        };
        #endregion

        #region Food Aura IDs (same as CachedWoWUnit)
        private static HashSet<uint> _foodAuras = new HashSet<uint>()
        {
            433, 434, 435, 1127, 1129, 1131, 2639, 5004, 5005, 5006, 5007,
            7737, 9177, 10256, 10257, 18071, 18124, 18229, 18230, 18231,
            18232, 18233, 18234, 21149, 22731, 23540, 23541, 23542, 23692,
            24005, 24384, 24409, 24410, 24411, 24707, 24800, 24869, 25660,
            25690, 25691, 25692, 25693, 25697, 25700, 25886, 25990, 26030,
            26263, 27094, 28616, 29008, 29029, 29055, 29073, 32112, 33253,
            33255, 33258, 33260, 33262, 33264, 33266, 33269, 33725, 33772,
            35270, 35271, 40745, 40768, 41030, 41031, 42207, 42309, 42311,
            43180, 43763, 44166, 45548, 45618, 46683, 46812, 46898, 53283,
            56439, 57069, 57070, 57084, 57085, 57096, 57098, 57101, 57106,
            57110, 57138, 57285, 57287, 57289, 57292, 57324, 57326, 57328,
            57331, 57333, 57335, 57341, 57343, 57344, 57354, 57355, 57357, 
            57359, 57362, 57364, 57366, 57370, 57372, 57649, 58067, 58503, 
            58645, 58648, 58886, 59227, 61827, 61828, 61829, 61874, 62351, 
            64056, 64354, 64355, 65418, 65419, 65420, 65421, 65422, 71068, 
            71071, 71073, 71074
        };
        #endregion

        /// <summary>
        /// Check if player has an aura by spell ID (language-independent)
        /// </summary>
        public bool PlayerHasAuraById(uint spellId)
        {
            var auras = BuffManager.GetAuras(ObjectManager.Me.GetBaseAddress);
            return auras.Any(a => a.SpellId == spellId);
        }

        /// <summary>
        /// Check if player has a drink aura (language-independent)
        /// </summary>
        public bool PlayerHasDrinkAura()
        {
            var auras = BuffManager.GetAuras(ObjectManager.Me.GetBaseAddress);
            return auras.Any(a => _drinkAuras.Contains(a.SpellId));
        }

        /// <summary>
        /// Check if player has a food aura (language-independent)
        /// </summary>
        public bool PlayerHasFoodAura()
        {
            var auras = BuffManager.GetAuras(ObjectManager.Me.GetBaseAddress);
            return auras.Any(a => _foodAuras.Contains(a.SpellId));
        }

        /// <summary>
        /// Cancel a player buff by spell ID (language-independent)
        /// Uses GetSpellInfo to get the localized spell name
        /// </summary>
        public void CancelPlayerBuffById(uint spellId)
        {
            string localizedName = Lua.LuaDoString<string>($@"
                local name = GetSpellInfo({spellId});
                return name or '';
            ");
            if (!string.IsNullOrEmpty(localizedName))
            {
                Logger.Log($"Canceling buff: {localizedName} (ID: {spellId})");
                Lua.LuaDoString($@"CancelPlayerBuff(""{localizedName.EscapeLuaString()}"")");
            }
        }

        /// <summary>
        /// Check if a unit has an aura by spell ID (language-independent)
        /// </summary>
        public static bool UnitHasAuraById(WoWUnit unit, uint spellId)
        {
            if (unit == null || !unit.IsValid)
                return false;
            var auras = BuffManager.GetAuras(unit.GetBaseAddress);
            return auras.Any(a => a.SpellId == spellId);
        }

        /// <summary>
        /// Check if target has an aura by spell ID (language-independent)
        /// </summary>
        public static bool TargetHasAuraById(uint spellId)
        {
            if (!ObjectManager.Me.HasTarget || ObjectManager.Target == null)
                return false;
            return UnitHasAuraById(ObjectManager.Target, spellId);
        }

        /// <summary>
        /// Get the localized name of a spell by its ID
        /// </summary>
        public static string GetLocalizedSpellName(uint spellId)
        {
            return Lua.LuaDoString<string>($@"
                local name = GetSpellInfo({spellId});
                return name or '';
            ");
        }

        /// <summary>
        /// Get creature type ID instead of localized string (language-independent)
        /// Returns: 1=Beast, 2=Dragon, 3=Demon, 4=Elemental, 5=Giant, 6=Undead, 7=Humanoid, 
        /// 8=Critter, 9=Mechanical, 10=Not specified, 11=Totem, 12=NonCombatPet, 13=GasCloud
        /// </summary>
        public static int GetCreatureTypeId(string unit = "target")
        {
            // In TBC, we can use UnitCreatureType and compare against known localized values
            // But for true language-independence, we use a lookup table
            string creatureType = Lua.LuaDoString<string>($@"
                local ctype = UnitCreatureType(""{unit}"");
                return ctype or '';
            ");
            
            // Map English creature types to IDs (WoW API returns localized strings)
            // For full localization support, we need to check against all known translations
            return GetCreatureTypeIdFromString(creatureType);
        }

        /// <summary>
        /// Check if target is a specific creature type by ID
        /// </summary>
        public static bool IsCreatureType(int creatureTypeId, string unit = "target")
        {
            return GetCreatureTypeId(unit) == creatureTypeId;
        }

        /// <summary>
        /// Check if target is humanoid (language-independent)
        /// </summary>
        public static bool IsHumanoid(string unit = "target")
        {
            return IsCreatureType(CreatureTypes.Humanoid, unit);
        }

        /// <summary>
        /// Check if target is undead (language-independent)
        /// </summary>
        public static bool IsUndead(string unit = "target")
        {
            return IsCreatureType(CreatureTypes.Undead, unit);
        }

        /// <summary>
        /// Check if target is demon (language-independent)
        /// </summary>
        public static bool IsDemon(string unit = "target")
        {
            return IsCreatureType(CreatureTypes.Demon, unit);
        }

        /// <summary>
        /// Check if target is beast (language-independent)
        /// </summary>
        public static bool IsBeast(string unit = "target")
        {
            return IsCreatureType(CreatureTypes.Beast, unit);
        }

        /// <summary>
        /// Check if target is giant (language-independent)
        /// </summary>
        public static bool IsGiant(string unit = "target")
        {
            return IsCreatureType(CreatureTypes.Giant, unit);
        }

        /// <summary>
        /// Check if target is mechanical (language-independent)
        /// </summary>
        public static bool IsMechanical(string unit = "target")
        {
            return IsCreatureType(CreatureTypes.Mechanical, unit);
        }

        /// <summary>
        /// Check if target is elemental (language-independent)
        /// </summary>
        public static bool IsElemental(string unit = "target")
        {
            return IsCreatureType(CreatureTypes.Elemental, unit);
        }

        /// <summary>
        /// Maps creature type string (in any language) to type ID
        /// This includes known translations for English, Chinese, German, French, Spanish, Russian
        /// </summary>
        public static int GetCreatureTypeIdFromString(string creatureType)
        {
            if (string.IsNullOrEmpty(creatureType))
                return 0;

            creatureType = creatureType.ToLowerInvariant();

            // Beast - 野兽 (Chinese), Bestie (German), Bête (French), Bestia (Spanish), Животное (Russian)
            if (creatureType == "beast" || creatureType == "野兽" || creatureType == "bestie" || 
                creatureType == "bête" || creatureType == "bestia" || creatureType == "животное")
                return CreatureTypes.Beast;

            // Dragon - 龙 (Chinese), Drache (German), Dragon (French), Dragón (Spanish), Дракон (Russian)
            if (creatureType == "dragon" || creatureType == "龙" || creatureType == "drache" ||
                creatureType == "dragón" || creatureType == "дракон" || creatureType == "龍")
                return CreatureTypes.Dragon;

            // Demon - 恶魔 (Chinese), Dämon (German), Démon (French), Demonio (Spanish), Демон (Russian)
            if (creatureType == "demon" || creatureType == "恶魔" || creatureType == "dämon" ||
                creatureType == "démon" || creatureType == "demonio" || creatureType == "демон" || creatureType == "惡魔")
                return CreatureTypes.Demon;

            // Elemental - 元素 (Chinese), Elementar (German), Élémentaire (French), Elemental (Spanish), Элементаль (Russian)
            if (creatureType == "elemental" || creatureType == "元素" || creatureType == "elementar" ||
                creatureType == "élémentaire" || creatureType == "элементаль")
                return CreatureTypes.Elemental;

            // Giant - 巨人 (Chinese), Riese (German), Géant (French), Gigante (Spanish), Великан (Russian)
            if (creatureType == "giant" || creatureType == "巨人" || creatureType == "riese" ||
                creatureType == "géant" || creatureType == "gigante" || creatureType == "великан")
                return CreatureTypes.Giant;

            // Undead - 亡灵 (Chinese), Untoter (German), Mort-vivant (French), No-muerto (Spanish), Нежить (Russian)
            if (creatureType == "undead" || creatureType == "亡灵" || creatureType == "untoter" ||
                creatureType == "mort-vivant" || creatureType == "no-muerto" || creatureType == "нежить" || creatureType == "不死")
                return CreatureTypes.Undead;

            // Humanoid - 人型生物 (Chinese), Humanoid (German), Humanoïde (French), Humanoide (Spanish), Гуманоид (Russian)
            if (creatureType == "humanoid" || creatureType == "人型生物" || creatureType == "humanoïde" ||
                creatureType == "humanoide" || creatureType == "гуманоид" || creatureType == "人形")
                return CreatureTypes.Humanoid;

            // Critter - 小动物 (Chinese), Kleintier (German), Bestiole (French), Alimaña (Spanish), Зверёк (Russian)
            if (creatureType == "critter" || creatureType == "小动物" || creatureType == "kleintier" ||
                creatureType == "bestiole" || creatureType == "alimaña" || creatureType == "зверёк" || creatureType == "小生物")
                return CreatureTypes.Critter;

            // Mechanical - 机械 (Chinese), Mechanisch (German), Mécanique (French), Mecánico (Spanish), Механизм (Russian)
            if (creatureType == "mechanical" || creatureType == "机械" || creatureType == "mechanisch" ||
                creatureType == "mécanique" || creatureType == "mecánico" || creatureType == "механизм" || creatureType == "機械")
                return CreatureTypes.Mechanical;

            // Totem - 图腾 (Chinese), Totem (German/French/Spanish), Тотем (Russian)
            if (creatureType == "totem" || creatureType == "图腾" || creatureType == "тотем" || creatureType == "圖騰")
                return CreatureTypes.Totem;

            // Not specified
            if (creatureType == "not specified" || creatureType == "未指定" || creatureType == "nicht angegeben" ||
                creatureType == "non spécifié" || creatureType == "no especificado" || creatureType == "не указано")
                return CreatureTypes.NonSpecified;

            return 0; // Unknown
        }
    }
}
