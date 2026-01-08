namespace WholesomeTBCAIO.Helpers
{
    /// <summary>
    /// Spell IDs for language-independent buff/debuff checking
    /// These IDs work regardless of client language (English, Chinese, German, etc.)
    /// </summary>
    public static class SpellIds
    {
        #region Common Buffs
        // Food/Drink buffs are handled by the cached aura system with ID sets
        
        // Ghost Wolf
        public const uint GhostWolf = 2645;

        // Druid Forms
        public const uint BearForm = 5487;
        public const uint CatForm = 768;
        public const uint DireBearForm = 9634;
        public const uint TravelForm = 783;
        public const uint AquaticForm = 1066;
        public const uint FlightForm = 33943;
        public const uint SwiftFlightForm = 40120;
        public const uint MoonkinForm = 24858;
        public const uint TreeOfLife = 33891;
        
        // Priest
        public const uint SpiritOfRedemption = 27827;
        public const uint InnerFire = 588;
        public const uint InnerFireRank7 = 25431;
        public const uint PowerWordFortitude = 1243;
        public const uint PrayerOfFortitude = 21562;
        public const uint DivineSpiritRank1 = 14752;
        public const uint PrayerOfSpirit = 27681;
        public const uint ShadowProtection = 976;
        public const uint PrayerOfShadowProtection = 27683;
        public const uint Shadowform = 15473;
        public const uint WeakenedSoul = 6788;
        public const uint VampiricEmbrace = 15286;
        public const uint PowerWordShield = 17;
        public const uint Fear = 5782;
        
        // Shaman
        public const uint WaterShield = 33736;
        public const uint LightningShield = 324;
        public const uint EarthShield = 32594;
        public const uint Bloodlust = 2825;
        public const uint StrengthOfEarth = 8075;
        public const uint Stoneskin = 8071;
        public const uint TotemOfWrath = 30706;
        public const uint GraceOfAir = 8835;
        public const uint ManaSpring = 5675;
        public const uint NaturesSwiftness = 16188;
        
        // Mage
        public const uint ArcaneIntellect = 1459;
        public const uint ArcaneBrilliance = 27127;
        public const uint FrostArmor = 168;
        public const uint IceArmor = 7302;
        public const uint MageArmor = 6117;
        public const uint MoltenArmor = 30482;
        public const uint IceBarrier = 11426;
        public const uint ManaShield = 1463;
        public const uint FrostNova = 122;
        public const uint ArcaneBlast = 30451;
        public const uint Combustion = 29977;
        public const uint IcyVeins = 12472;
        public const uint ArcanePower = 12042;
        public const uint PresenceOfMind = 12043;
        public const uint ClearCasting = 12536;
        
        // Warlock
        public const uint DemonSkin = 687;
        public const uint DemonArmor = 706;
        public const uint FelArmor = 28176;
        public const uint SoulLink = 19028;
        public const uint ShadowTrance = 17941; // Nightfall proc
        public const uint UnendingBreath = 5697;
        public const uint SoulstoneResurrection = 20707;
        public const uint FireWard = 543; // Target buff check
        
        // Paladin
        public const uint DevotionAura = 465;
        public const uint RetributionAura = 7294;
        public const uint ConcentrationAura = 19746;
        public const uint ShadowResistanceAura = 19876;
        public const uint FrostResistanceAura = 19888;
        public const uint FireResistanceAura = 19891;
        public const uint CrusaderAura = 32223;
        public const uint SanctityAura = 20218;
        public const uint BlessingOfKings = 20217;
        public const uint BlessingOfMight = 19740;
        public const uint BlessingOfWisdom = 19742;
        public const uint BlessingOfSalvation = 1038;
        public const uint BlessingOfLight = 19977;
        public const uint BlessingOfSanctuary = 20911;
        public const uint GreaterBlessingOfKings = 25898;
        public const uint GreaterBlessingOfMight = 25782;
        public const uint GreaterBlessingOfWisdom = 25894;
        public const uint GreaterBlessingOfSalvation = 25895;
        public const uint GreaterBlessingOfLight = 25890;
        public const uint GreaterBlessingOfSanctuary = 25899;
        public const uint SealOfRighteousness = 21084;
        public const uint SealOfCommand = 20375;
        public const uint SealOfVengeance = 31801;
        public const uint SealOfCrusader = 21082;
        public const uint SealOfJustice = 20164;
        public const uint SealOfLight = 20165;
        public const uint SealOfWisdom = 20166;
        public const uint SealOfBlood = 31892;
        public const uint JudgementOfCrusader = 21183;
        public const uint JudgementOfLight = 20185;
        public const uint JudgementOfWisdom = 20186;
        public const uint RighteousFury = 25780;
        public const uint HolyShield = 20925;
        public const uint DivineShield = 642;
        public const uint Forbearance = 25771;
        public const uint AvengingWrath = 31884;
        
        // Warrior
        public const uint BattleShout = 6673;
        public const uint CommandingShout = 469;
        public const uint DemoralizingShout = 1160;
        public const uint ThunderClap = 6343;
        public const uint BattleStance = 2457;
        public const uint DefensiveStance = 71;
        public const uint BerserkerStance = 2458;
        public const uint SunderArmor = 7386;
        public const uint Rend = 772;
        public const uint ShieldBlock = 2565;
        public const uint ShieldWall = 871;
        public const uint LastStand = 12975;
        public const uint Retaliation = 20230;
        public const uint Bloodrage = 2687;
        public const uint BerserkerRage = 18499;
        public const uint Enrage = 12880;
        public const uint DeathWish = 12292;
        public const uint Rampage = 29801;
        public const uint Overpower = 7384;
        public const uint Revenge = 6572;
        public const uint Flurry = 12319;
        
        // Rogue
        public const uint Stealth = 1784;
        public const uint SliceAndDice = 5171;
        public const uint Evasion = 5277;
        public const uint BladeFlurry = 13877;
        public const uint AdrenalineRush = 13750;
        public const uint ColdBlood = 14177;
        public const uint Sprint = 2983;
        public const uint Vanish = 1856;
        
        // Hunter
        public const uint AspectOfTheHawk = 13165;
        public const uint AspectOfTheMonkey = 13163;
        public const uint AspectOfTheCheetah = 5118;
        public const uint AspectOfThePack = 13159;
        public const uint AspectOfTheWild = 20043;
        public const uint AspectOfTheBeast = 13161;
        public const uint AspectOfTheViper = 34074;
        public const uint RapidFire = 3045;
        public const uint Bestialwrath = 19574;
        public const uint QuickShots = 6150;
        public const uint HuntersMark = 1130;
        public const uint ConcussiveShot = 5116;
        
        // Druid  
        public const uint MarkOfTheWild = 1126;
        public const uint GiftOfTheWild = 21849;
        public const uint Thorns = 467;
        public const uint Omen = 16870;
        public const uint TigersFury = 5217;
        public const uint SavageRoar = 52610;
        public const uint Barkskin = 22812;
        public const uint Innervate = 29166;
        public const uint NaturesGrasp = 16689;
        public const uint Rejuvenation = 774;
        public const uint Regrowth = 8936;
        public const uint Lifebloom = 33763;
        public const uint SwiftmendCD = 18562;
        
        #endregion

        #region Debuffs on Target
        // Warlock
        public const uint Corruption = 172;
        public const uint CurseOfAgony = 980;
        public const uint CurseOfDoom = 603;
        public const uint CurseOfElements = 1490;
        public const uint CurseOfRecklessness = 704;
        public const uint CurseOfShadow = 17862;
        public const uint CurseOfTongues = 1714;
        public const uint CurseOfWeakness = 702;
        public const uint Immolate = 348;
        public const uint SiphonLife = 18265;
        public const uint UnstableAffliction = 30108;
        public const uint SeedOfCorruption = 27243;
        
        // Priest
        public const uint ShadowWordPain = 589;
        public const uint VampiricTouch = 34914;
        public const uint DevouringPlague = 2944;
        public const uint MindFlay = 15407;
        
        // Mage
        public const uint Polymorph = 118;
        public const uint Scorch = 2948;
        public const uint Frostbolt = 116;
        public const uint Fireball = 133;
        public const uint LivingBomb = 44457;
        
        // Druid
        public const uint Moonfire = 8921;
        public const uint InsectSwarm = 5570;
        public const uint FaerieFire = 770;
        public const uint FaerieFireFeral = 16857;
        public const uint Rake = 1822;
        public const uint Rip = 1079;
        public const uint Lacerate = 33745;
        public const uint MangleCat = 33876;
        public const uint MangleBear = 33878;
        
        // Hunter
        public const uint SerpentSting = 1978;
        public const uint ScorpidSting = 3043;
        public const uint ViperSting = 3034;
        
        // Shaman
        public const uint FlameShock = 8050;
        public const uint FrostShock = 8056;
        public const uint EarthShock = 8042;
        
        #endregion

        #region Racial Abilities
        public const uint Cannibalize = 20577;
        public const uint WillOfTheForsaken = 7744;
        public const uint Berserking = 26296;
        public const uint EscapeArtist = 20589;
        public const uint ManaTap = 28734;
        public const uint ArcaneTorrent = 28730;
        public const uint Stoneform = 20594;
        public const uint GiftOfTheNaaru = 28880;
        public const uint WarStomp = 20549;
        public const uint BloodFury = 33697;
        public const uint Bleed = 43246; // Generic bleed debuff type
        public const uint Charm = 1098;  // Generic charm effect
        public const uint Sleep = 700;   // Generic sleep effect
        #endregion

        #region Misc
        public const uint Attack = 6603;
        public const uint AutoShot = 75;
        public const uint Shoot = 5019;
        public const uint Throw = 2764;
        #endregion
    }
}
