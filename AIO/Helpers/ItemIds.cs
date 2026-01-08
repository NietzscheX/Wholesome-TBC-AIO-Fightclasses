namespace WholesomeTBCAIO.Helpers
{
    /// <summary>
    /// Item IDs for language-independent item checking
    /// These IDs work regardless of client language (English, Chinese, German, etc.)
    /// </summary>
    public static class ItemIds
    {
        #region Warlock Items
        // Soul Shards
        public const uint SoulShard = 6265;

        // Healthstones
        public const uint MinorHealthstone = 5512;
        public const uint LesserHealthstone = 5511;
        public const uint Healthstone = 5509;
        public const uint GreaterHealthstone = 5510;
        public const uint MajorHealthstone = 9421;
        public const uint MasterHealthstone = 22103;

        // Soulstones
        public const uint MinorSoulstone = 5232;
        public const uint LesserSoulstone = 16892;
        public const uint Soulstone = 16893;
        public const uint GreaterSoulstone = 16895;
        public const uint MajorSoulstone = 16896;
        public const uint MasterSoulstone = 22116;
        #endregion

        #region Consumables - Potions
        // Health Potions
        public const uint MinorHealingPotion = 118;
        public const uint LesserHealingPotion = 858;
        public const uint HealingPotion = 929;
        public const uint GreaterHealingPotion = 1710;
        public const uint SuperiorHealingPotion = 3928;
        public const uint MajorHealingPotion = 13446;
        public const uint SuperHealingPotion = 22829;

        // Mana Potions
        public const uint MinorManaPotion = 2455;
        public const uint LesserManaPotion = 3385;
        public const uint ManaPotion = 3827;
        public const uint GreaterManaPotion = 6149;
        public const uint SuperiorManaPotion = 13443;
        public const uint MajorManaPotion = 13444;
        public const uint SuperManaPotion = 22832;
        #endregion

        #region Ammunition
        // Arrows
        public const uint RoughArrow = 2512;
        public const uint SharpArrow = 2515;
        public const uint RazorArrow = 3030;
        public const uint JaggedArrow = 11285;
        public const uint AccurateSlugs = 11284;
        
        // Bullets
        public const uint LightShot = 2516;
        public const uint HeavyShot = 2519;
        public const uint SolidShot = 3033;
        public const uint MithrilGyroShot = 10512;
        public const uint ThoriumHeadedArrow = 15997;
        public const uint ThoriumShells = 15998;
        #endregion

        #region Rogue Poisons
        // Instant Poison
        public const uint InstantPoison = 6947;
        public const uint InstantPoisonII = 6949;
        public const uint InstantPoisonIII = 6950;
        public const uint InstantPoisonIV = 8926;
        public const uint InstantPoisonV = 8927;
        public const uint InstantPoisonVI = 8928;
        public const uint InstantPoisonVII = 21927;
        public const uint InstantPoisonVIII = 43230; // Not TBC but listed
        public const uint InstantPoisonIX = 43231; // Not TBC but listed

        // Deadly Poison
        public const uint DeadlyPoison = 2892;
        public const uint DeadlyPoisonII = 2893;
        public const uint DeadlyPoisonIII = 8984;
        public const uint DeadlyPoisonIV = 8985;
        public const uint DeadlyPoisonV = 20844;
        public const uint DeadlyPoisonVI = 22053;
        public const uint DeadlyPoisonVII = 22054;
        public const uint DeadlyPoisonVIII = 43232; // Not TBC but listed
        public const uint DeadlyPoisonIX = 43233; // Not TBC but listed
        #endregion
    }

    /// <summary>
    /// Helper class for creature type checking
    /// Use these constants with the localized creature type comparison methods
    /// </summary>
    public static class CreatureTypes
    {
        // These are the English creature type names returned by UnitCreatureType in English clients
        // For localization, we need to compare against localized versions or use type IDs
        public const int Beast = 1;
        public const int Dragon = 2;
        public const int Demon = 3;
        public const int Elemental = 4;
        public const int Giant = 5;
        public const int Undead = 6;
        public const int Humanoid = 7;
        public const int Critter = 8;
        public const int Mechanical = 9;
        public const int NonSpecified = 10;
        public const int Totem = 11;
        public const int NonCombatPet = 12;
        public const int GasCloud = 13;
    }
}
