using System;
using System.Collections.Generic;
using System.Reflection;

namespace Assistant
{
    internal enum AOSAbility
    {
        Clear,
        ArmorIgnore,
        BleedAttack,
        ConcussionBlow,
        CrushingBlow,
        Disarm,
        Dismount,
        DoubleStrike,
        InfectiousStrike,
        MortalStrike,
        MovingShot,
        ParalyzingBlow,
        ShadowStrike,
        WhirlwindAttack,
        RidingSwipe,
        FrenziedWhirlwind,
        Block,
        DefenseMastery,
        NerveStrike,
        TalonStrike,
        Feint,
        DualWield,
        DoubleShot,
        ArmorPierce,
        Bladeweave,
        ForceArrow,
        LightningArrow,
        PsychicAttack,
        SerpentArrow,
        ForceOfNature,
        // SA Ability
        InfusedThrow,
        MysticArc,
        Invalid
    }

    internal class SpecialMoves
    {
        internal static List<int> Icon = new()
        {
            0,
            0x5200,
            0x5201,
            0x5202,
            0x5203,
            0x5204,
            0x5205,
            0x5206,
            0x5207,
            0x5208,
            0x5209,
            0x520A,
            0x520B,
            0x520C,
            0x520D,
            0x520E,
            0x520F,
            0x5210,
            0x5211,
            0x5212,
            0x5213,
            0x5214,
            0x5215,
            0x5216,
            0x5217,
            0x5218,
            0x5219,
            0x521A,
            0x521B,
            0x521C,
            0x521D,
            0x521E,
            0
        };
        private class AbilityInfo
        {
            private readonly AOSAbility m_Ability;
            private readonly int[][] m_Items;

            internal AbilityInfo(AOSAbility ab, params int[][] items)
            {
                m_Ability = ab;
                m_Items = items;
            }

            internal AOSAbility Ability { get { return m_Ability; } }

            internal bool HasItem(int item)
            {
                for (int a = 0; a < m_Items.Length; a++)
                {
                    for (int b = 0; b < m_Items[a].Length; b++)
                    {
                        if (m_Items[a][b] == item)
                            return true;
                    }
                }

                return false;
            }
        }

        private static DateTime m_LastToggle = DateTime.MinValue;

        private static readonly int[] HatchetID = new int[] { 0xF43, 0xF44 };
        private static readonly int[] LongSwordID = new int[] { 0xF60, 0xF61 };
        private static readonly int[] BroadswordID = new int[] { 0xF5E, 0xF5F };
        private static readonly int[] KatanaID = new int[] { 0x13FE, 0x13FF };
        private static readonly int[] BladedStaffID = new int[] { 0x26BD, 0x26C7 };
        private static readonly int[] HammerPickID = new int[] { 0x143C, 0x143D };
        private static readonly int[] WarAxeID = new int[] { 0x13AF, 0x13B0 };
        private static readonly int[] KryssID = new int[] { 0x1400, 0x1401 };
        private static readonly int[] SpearID = new int[] { 0xF62, 0xF63 };
        private static readonly int[] CompositeBowID = new int[] { 0x26C2, 0x26CC };
        private static readonly int[] CleaverID = new int[] { 0xEC2, 0xEC3 };
        private static readonly int[] LargeBattleAxeID = new int[] { 0x13FA, 0x13FB };
        private static readonly int[] BattleAxeID = new int[] { 0xF47, 0xF48 };
        private static readonly int[] ExecAxeID = new int[] { 0xF45, 0xF46 };
        private static readonly int[] CutlassID = new int[] { 0x1440, 0x1441 };
        private static readonly int[] ScytheID = new int[] { 0x26BA, 0x26C4 };
        private static readonly int[] WarMaceID = new int[] { 0x1406, 0x1407 };
        private static readonly int[] PitchforkID = new int[] { 0xE87, 0xE88 };
        private static readonly int[] WarForkID = new int[] { 0x1404, 0x1405 };
        private static readonly int[] HalberdID = new int[] { 0x143E, 0x143F };
        private static readonly int[] MaulID = new int[] { 0x143A, 0x143B };
        private static readonly int[] MaceID = new int[] { 0xF5C, 0x45D };
        private static readonly int[] GnarledStaffID = new int[] { 0x13F8, 0x13F9 };
        private static readonly int[] QuarterStaffID = new int[] { 0xE89, 0xE8A };
        private static readonly int[] LanceID = new int[] { 0x26C0, 0x26CA };
        private static readonly int[] CrossbowID = new int[] { 0xF4F, 0xF50 };
        private static readonly int[] VikingSwordID = new int[] { 0x13B9, 0x13BA };
        private static readonly int[] AxeID = new int[] { 0xF49, 0xF4A };
        private static readonly int[] ShepherdsCrookID = new int[] { 0xE81, 0xE82 };
        private static readonly int[] SmithsHammerID = new int[] { 0x13EC, 0x13E4 };
        private static readonly int[] WarHammerID = new int[] { 0x1438, 0x1439 };
        private static readonly int[] ScepterID = new int[] { 0x26BC, 0x26C6 };
        private static readonly int[] SledgeHammerID = new int[] { 0xFB4, 0xFB5 };
        private static readonly int[] ButcherKnifeID = new int[] { 0x13F6, 0x13F7 };
        private static readonly int[] PickaxeID = new int[] { 0xE85, 0xE86 };
        private static readonly int[] SkinningKnifeID = new int[] { 0xEC4, 0xEC5 };
        private static readonly int[] WandID = new int[] { 0xDF2, 0xDF3, 0xDF4, 0xDF5 };
        private static readonly int[] BardicheID = new int[] { 0xF4D, 0xF4E };
        private static readonly int[] ClubID = new int[] { 0x13B3, 0x13B4 };
        private static readonly int[] ScimitarID = new int[] { 0x13B5, 0x13B6 };
        private static readonly int[] HeavyCrossbowID = new int[] { 0x13FC, 0x13FD };
        private static readonly int[] TwoHandedAxeID = new int[] { 0x1442, 0x1443 };
        private static readonly int[] DoubleAxeID = new int[] { 0xF4B, 0xF4C };
        private static readonly int[] CrescentBladeID = new int[] { 0x26C1, 0x26CB };
        private static readonly int[] DoubleBladedStaffID = new int[] { 0x26BF, 0x26C9 };
        private static readonly int[] RepeatingCrossbowID = new int[] { 0x26C3, 0x26CD };
        private static readonly int[] DaggerID = new int[] { 0xF51, 0xF52 };
        private static readonly int[] PikeID = new int[] { 0x26BE, 0x26C8 };
        private static readonly int[] BoneHarvesterID = new int[] { 0x26BB, 0x26C5 };
        private static readonly int[] ShortSpearID = new int[] { 0x1402, 0x1403 };
        private static readonly int[] BowID = new int[] { 0x13B1, 0x13B2 };
        private static readonly int[] BlackStaffID = new int[] { 0xDF0, 0xDF1 };
        private static readonly int[] FistsID = new int[] { 0 };

        // SA e ML e KR Weapon
        private static readonly int[] DualShortAxesID = new int[] { 0x08FD, 0x4068 };
        private static readonly int[] GargishBattleAxeID = new int[] { 0x48B0, 0x48B1 };
        private static readonly int[] GargishAxeID = new int[] { 0x48B2, 0x48B3 };
        private static readonly int[] OrnateAxeID = new int[] { 0x2D28, 0x2D34 };
        private static readonly int[] BokutoID = new int[] { 0x27A8, 0x27F3 };
        private static readonly int[] DaishoID = new int[] { 0x27A9, 0x27F4 };
        private static readonly int[] DreadSwordID = new int[] { 0x090B, 0x4074 };
        private static readonly int[] ElvenMacheteID = new int[] { 0x2D29, 0x2D35 };
        private static readonly int[] GargishBardicheID = new int[] { 0x48B4, 0x48B5 };
        private static readonly int[] GargishBoneHarvesterID = new int[] { 0x48C6, 0x48C7 };
        private static readonly int[] GargishButcherKnifeID = new int[] { 0x48B6, 0x48B7 };
        private static readonly int[] GargishCleaverID = new int[] { 0x48AE, 0x48AF };
        private static readonly int[] GargishDaishoID = new int[] { 0x48D0, 0x48D1 };
        private static readonly int[] GargishKatanaID = new int[] { 0x48BA, 0x48BB };
        private static readonly int[] GargishScytheID = new int[] { 0x48C4, 0x48C5 };
        private static readonly int[] GargishTalwarID = new int[] { 0x0908, 0x4075 };
        private static readonly int[] GlassSwordID = new int[] { 0x090C, 0x4073 };
        private static readonly int[] NoDachiID = new int[] { 0x27A2, 0x27ED };
        private static readonly int[] RadiantScimitarID = new int[] { 0x2D27, 0x2D33 };
        private static readonly int[] RuneBladeID = new int[] { 0x2D26, 0x2D32 };
        private static readonly int[] WakizashiID = new int[] { 0x27A4, 0x27EF };
        private static readonly int[] DiamondMaceID = new int[] { 0x2D24, 0x2D30 };
        private static readonly int[] DiscMaceID = new int[] { 0x0903, 0x406E };
        private static readonly int[] GargishGnarledStaffID = new int[] { 0x48B8, 0x48B9 };
        private static readonly int[] GargishMaulID = new int[] { 0x48C2, 0x48C3 };
        private static readonly int[] GargishTessenID = new int[] { 0x48CC, 0x48CD };
        private static readonly int[] GargishWarHammerID = new int[] { 0x48C0, 0x48C1 };
        private static readonly int[] GlassStaffID = new int[] { 0x0905, 0x4070 };
        private static readonly int[] NunchakuID = new int[] { 0x27AE, 0x27F9 };
        private static readonly int[] SerpentstoneStaffID = new int[] { 0x0906, 0x406F };
        private static readonly int[] TessenID = new int[] { 0x27A3, 0x27EE };
        private static readonly int[] TetsuboID = new int[] { 0x27A6 };
        private static readonly int[] ElvenCompositeLongbowID = new int[] { 0x2D1E, 0x2D2A };
        private static readonly int[] MagicalShortbowID = new int[] { 0x2D1F, 0x2D2B };
        private static readonly int[] YumiID = new int[] { 0x27A5, 0x27F0 };
        private static readonly int[] AssassinSpikeID = new int[] { 0x2D21, 0x2D2D };
        private static readonly int[] BloodbladeID = new int[] { 0x08FE, 0x4072 };
        private static readonly int[] DualPointedSpearID = new int[] { 0x0904, 0x406D };
        private static readonly int[] ElvenSpellbladeID = new int[] { 0x2D20, 0x2D2C };
        private static readonly int[] GargishDaggerID = new int[] { 0x0902, 0x406A };
        private static readonly int[] GargishKryssID = new int[] { 0x48BC, 0x48BD };
        private static readonly int[] GargishLanceID = new int[] { 0x48CA, 0x48CB };
        private static readonly int[] GargishPikeID = new int[] { 0x48C8, 0x48C9 };
        private static readonly int[] GargishTekagiID = new int[] { 0x48CE, 0x48CF };
        private static readonly int[] GargishWarForkID = new int[] { 0x48BE, 0x48BF };
        private static readonly int[] KamaID = new int[] { 0x27AD, 0x27F8 };
        private static readonly int[] LajatangID = new int[] { 0x27A7, 0x27F2 };
        private static readonly int[] LeafbladeID = new int[] { 0x2D22, 0x2D2E };
        private static readonly int[] SaiID = new int[] { 0x27AF, 0x27FA };
        private static readonly int[] ShortbladeID = new int[] { 0x0907, 0x4076 };
        private static readonly int[] TekagiID = new int[] { 0x27AB, 0x27F6 };
        private static readonly int[] WarCleaverID = new int[] { 0x2D23, 0x2D2F };
        private static readonly int[] BoomerangID = new int[] { 0x4067, 0x08FF };
        private static readonly int[] CycloneID = new int[] { 0x406C, 0x0901 };
        private static readonly int[] SoulGlaiveID = new int[] { 0x406B, 0x090A };
        private static readonly int[] UOPSword = new int[] { 0x26CE };

        // Free server additions
        private static readonly int[] WhipMace = new int[] { 0xA289, 0xA291 };
        private static readonly int[] WhipFencing = new int[] { 0xA28A, 0xA292 };
        private static readonly int[] WhipSword = new int[] { 0xA28B, 0xA293 };

        private const string TYPE = "ClassicUO.Game.GameActions";
        private static MethodInfo _usePrimaryMethod;
        private static MethodInfo _useSecondaryMethod;

        private static readonly AbilityInfo[] m_Primary = new AbilityInfo[]
        {
            new( AOSAbility.ArmorIgnore, HatchetID, LongSwordID, BladedStaffID, HammerPickID, WarAxeID, KryssID, SpearID, CompositeBowID, DiscMaceID, GargishKryssID, ShortbladeID, SoulGlaiveID ),
            new( AOSAbility.ArmorPierce, YumiID, WhipFencing ),
            new( AOSAbility.BleedAttack,  CleaverID, BattleAxeID, ExecAxeID, CutlassID, ScytheID, PitchforkID, WarForkID, GargishBattleAxeID, GargishCleaverID, GargishScytheID, GlassSwordID, BloodbladeID, GargishWarForkID, WhipSword ),
            new( AOSAbility.Block, NunchakuID ),
            new( AOSAbility.ConcussionBlow, MaceID, GnarledStaffID, CrossbowID, DiamondMaceID, GargishGnarledStaffID, WhipMace ),
            new( AOSAbility.CrushingBlow, VikingSwordID, AxeID, BroadswordID, ShepherdsCrookID, SmithsHammerID, WarMaceID, ScepterID, SledgeHammerID, GargishAxeID, DreadSwordID, NoDachiID, SerpentstoneStaffID ),
            new( AOSAbility.DefenseMastery, ElvenMacheteID, LajatangID ),
            new( AOSAbility.Disarm, FistsID, OrnateAxeID, RuneBladeID, WarCleaverID ),
            new( AOSAbility.Dismount, WandID, GargishLanceID, LanceID ),
            new( AOSAbility.DoubleStrike, PickaxeID, TwoHandedAxeID, DoubleAxeID, ScimitarID, MaulID, KatanaID, CrescentBladeID, QuarterStaffID, DoubleBladedStaffID, RepeatingCrossbowID, DualShortAxesID, GargishKatanaID, GargishMaulID, GlassStaffID, NunchakuID, DualPointedSpearID ),
            new( AOSAbility.DualWield, GargishTekagiID, SaiID, TekagiID ),
            new( AOSAbility.InfectiousStrike, ButcherKnifeID, GargishButcherKnifeID, AssassinSpikeID ),
            new( AOSAbility.LightningArrow, MagicalShortbowID ),
            new( AOSAbility.Feint, BokutoID, DaishoID, GargishDaishoID, GargishTessenID, TessenID, LeafbladeID ),
            new( AOSAbility.FrenziedWhirlwind, WakizashiID, TetsuboID ),
            new( AOSAbility.ForceArrow, ElvenCompositeLongbowID ),
            //new AbilityInfo( AOSAbility.MortalStrike ), // not primary for anything
            new( AOSAbility.MovingShot, HeavyCrossbowID, CycloneID ),
            new( AOSAbility.ParalyzingBlow, BardicheID, BoneHarvesterID, PikeID, BowID, GargishBardicheID, GargishBoneHarvesterID, GargishPikeID ),
            new( AOSAbility.PsychicAttack, ElvenSpellbladeID ),
            new( AOSAbility.ShadowStrike, SkinningKnifeID, DaggerID, ClubID, ShortSpearID, GargishDaggerID ),
            new( AOSAbility.WhirlwindAttack, LargeBattleAxeID, HalberdID, WarHammerID, BlackStaffID, GargishTalwarID, RadiantScimitarID, GargishWarHammerID, KamaID, UOPSword ),
            new( AOSAbility.MysticArc, BoomerangID)
        };

        private static readonly AbilityInfo[] m_Secondary = new AbilityInfo[]
        {

            new( AOSAbility.ArmorPierce, SaiID ),
            new( AOSAbility.ArmorIgnore,  BroadswordID, KatanaID, GargishKatanaID, LeafbladeID ),
            new( AOSAbility.Bladeweave, ElvenMacheteID, RadiantScimitarID, RuneBladeID, WarCleaverID ),
            new( AOSAbility.BleedAttack, LargeBattleAxeID, WarAxeID, ElvenSpellbladeID, BoomerangID ),
            new( AOSAbility.ConcussionBlow, LongSwordID, BattleAxeID, HalberdID, MaulID, QuarterStaffID, LanceID, GargishBattleAxeID, DreadSwordID, GargishMaulID, GargishLanceID),
            new( AOSAbility.CrushingBlow, WarHammerID, OrnateAxeID, DiamondMaceID, GargishWarHammerID, TetsuboID ),
            new( AOSAbility.DefenseMastery, KamaID ),
            new( AOSAbility.Disarm, ButcherKnifeID, PickaxeID, SkinningKnifeID, HatchetID, WandID, ShepherdsCrookID, MaceID, WarForkID, GargishButcherKnifeID, DiscMaceID, DualPointedSpearID, GargishWarForkID, UOPSword ),
            new( AOSAbility.Dismount, BardicheID, AxeID, BladedStaffID, ClubID, PitchforkID, HeavyCrossbowID, GargishAxeID, GargishBardicheID, GargishTalwarID, SerpentstoneStaffID ),
            new( AOSAbility.DualWield, TessenID ),
            new( AOSAbility.DoubleStrike, DaishoID, GargishDaishoID, WakizashiID),
            new( AOSAbility.DoubleShot, YumiID ),
            new( AOSAbility.FrenziedWhirlwind, LajatangID ),
            new( AOSAbility.InfectiousStrike, CleaverID, PikeID, KryssID, DaggerID, DoubleBladedStaffID, DualShortAxesID, GargishCleaverID, GargishDaggerID, GargishKryssID, GargishPikeID ),
            new( AOSAbility.MortalStrike, ExecAxeID, BoneHarvesterID, CrescentBladeID, HammerPickID, ScepterID, ShortSpearID, CrossbowID, BowID, GargishBoneHarvesterID, GlassSwordID, GlassStaffID, ShortbladeID, WarMaceID, SoulGlaiveID ),
            new( AOSAbility.MovingShot, CompositeBowID, RepeatingCrossbowID ),
            new( AOSAbility.ParalyzingBlow, VikingSwordID, ScimitarID, ScytheID, GnarledStaffID, BlackStaffID, SpearID, FistsID, GargishScytheID, GargishGnarledStaffID, BloodbladeID ),
            new( AOSAbility.PsychicAttack, MagicalShortbowID ),
            new( AOSAbility.RidingSwipe, NoDachiID ),
            new( AOSAbility.SerpentArrow, ElvenCompositeLongbowID ),
            new( AOSAbility.ShadowStrike, TwoHandedAxeID, CutlassID, SmithsHammerID, SledgeHammerID, AssassinSpikeID ),
            new( AOSAbility.TalonStrike, GargishTekagiID, TekagiID ),
            new( AOSAbility.WhirlwindAttack, DoubleAxeID, WhipMace, WhipSword, WhipFencing ),
            new( AOSAbility.NerveStrike, BokutoID ),
            new( AOSAbility.InfusedThrow, CycloneID )
        };

        internal static uint PrimaryGumpId
        {
            get
            {
                Item right = World.Player.GetItemOnLayer(Layer.RightHand);
                Item left = World.Player.GetItemOnLayer(Layer.LeftHand);

                AOSAbility a = AOSAbility.Invalid;
                if (right != null)
                    a = GetAbility(right.TypeID.Value, m_Primary);

                if (a == AOSAbility.Invalid && left != null)
                    a = GetAbility(left.TypeID.Value, m_Primary);

                if (a == AOSAbility.Invalid)
                    a = GetAbility(FistsID[0], m_Primary);

                if (a != AOSAbility.Invalid)
                {
                    return (uint)GetPrimaryIcon((int)a);
                }

                return 0x5204;
            }
        }

        internal static bool HasPrimary { get; set; }

        internal static uint SecondaryGumpId
        {
            get
            {
                Item right = World.Player.GetItemOnLayer(Layer.RightHand);
                Item left = World.Player.GetItemOnLayer(Layer.LeftHand);

                AOSAbility a = AOSAbility.Invalid;
                if (right != null)
                    a = GetAbility(right.TypeID.Value, m_Secondary);

                if (a == AOSAbility.Invalid && left != null)
                    a = GetAbility(left.TypeID.Value, m_Secondary);

                if (a == AOSAbility.Invalid)
                    a = GetAbility(FistsID[0], m_Secondary);

                if (a != AOSAbility.Invalid)
                {
                    return (uint)GetSecondaryIcon((int)a);
                }

                return 0x5206;
            }
        }

        internal static bool HasSecondary { get; set; }


        internal static void ToggleWarPeace()
        {
            Assistant.Client.Instance.SendToServer(new SetWarMode(!World.Player.Warmode));
        }

        internal static void OnStun(bool wait)
        {
            if (m_LastToggle + TimeSpan.FromSeconds(0.5) < DateTime.Now)
            {
                m_LastToggle = DateTime.Now;
                if (wait)
                    Assistant.Client.Instance.SendToServerWait(new StunRequest());
                else
                    Assistant.Client.Instance.SendToServer(new StunRequest());
            }
        }

        internal static void OnDisarm(bool wait)
        {
            if (m_LastToggle + TimeSpan.FromSeconds(0.5) < DateTime.Now)
            {
                m_LastToggle = DateTime.Now;
                if (wait)
                    Assistant.Client.Instance.SendToServerWait(new DisarmRequest());
                else
                    Assistant.Client.Instance.SendToServer(new DisarmRequest());
            }
        }

        internal static AOSAbility GetPrimaryAbility(int itemId)
        {
            return GetAbility(itemId, m_Primary);
        }
        internal static AOSAbility GetSecondaryAbility(int itemId)
        {
            return GetAbility(itemId, m_Secondary);
        }

        private static AOSAbility GetAbility(int item, AbilityInfo[] list)
        {
            for (int a = 0; a < list.Length; a++)
            {
                if (list[a].HasItem(item))
                    return list[a].Ability;
            }

            return AOSAbility.Invalid;
        }

        internal static void SetPrimaryAbility(bool wait)
        {
            if (HasPrimary)
                return;

            Item right = World.Player.GetItemOnLayer(Layer.RightHand);
            Item left = World.Player.GetItemOnLayer(Layer.LeftHand);

            AOSAbility a = AOSAbility.Invalid;
            if (right != null)
                a = GetAbility(right.TypeID.Value, m_Primary);

            if (a == AOSAbility.Invalid && left != null)
                a = GetAbility(left.TypeID.Value, m_Primary);

            if (a == AOSAbility.Invalid)
                a = GetAbility(FistsID[0], m_Primary);

            if (a != AOSAbility.Invalid)
            {
                World.Player.HasSpecial = true;
                HasPrimary = true;
                HasSecondary = false;
                RazorEnhanced.SpellGrid.UpdateSAHighLight((int)a);

                if (Client.IsOSI)
                {
                    if (wait)
                    {
                        Assistant.Client.Instance.SendToServerWait(new UseAbility(a));
                        Assistant.Client.Instance.SendToClientWait(ClearAbility.Instance);
                    }
                    else
                    {
                        Assistant.Client.Instance.SendToServer(new UseAbility(a));
                        Assistant.Client.Instance.SendToClient(ClearAbility.Instance);
                    }
                }
                else // CUO Client
                {
                    if (_usePrimaryMethod == null)
                    {
                        _usePrimaryMethod = ClassicUOClient.CUOAssembly?.GetType(TYPE)?.GetMethod("UsePrimaryAbility", BindingFlags.Public | BindingFlags.Static);
                    }
                    ClassicUOClient.CUOActionQueue.Enqueue(() => { _usePrimaryMethod.Invoke(null, null); });
                }
                World.Player.SendMessage(LocString.SettingAOSAb, a);
            }
        }

        internal static void SetSecondaryAbility(bool wait)
        {
            if (HasSecondary)
                return;
            Item right = World.Player.GetItemOnLayer(Layer.RightHand);
            Item left = World.Player.GetItemOnLayer(Layer.LeftHand);

            AOSAbility a = AOSAbility.Invalid;
            if (right != null)
                a = GetAbility(right.TypeID.Value, m_Secondary);

            if (a == AOSAbility.Invalid && left != null)
                a = GetAbility(left.TypeID.Value, m_Secondary);

            if (a == AOSAbility.Invalid)
                a = GetAbility(FistsID[0], m_Secondary);

            if (a != AOSAbility.Invalid)
            {
                World.Player.HasSpecial = true;
                HasPrimary = false;
                HasSecondary = true;
                RazorEnhanced.SpellGrid.UpdateSAHighLight((int)a);


                if (Client.IsOSI)
                {
                    if (wait)
                    {
                        Assistant.Client.Instance.SendToServerWait(new UseAbility(a));
                        Assistant.Client.Instance.SendToClientWait(ClearAbility.Instance);
                    }
                    else
                    {
                        Assistant.Client.Instance.SendToServer(new UseAbility(a));
                        Assistant.Client.Instance.SendToClient(ClearAbility.Instance);
                    }
                }
                else // CUO Client
                {
                    if (_useSecondaryMethod == null)
                    {
                        _useSecondaryMethod = ClassicUOClient.CUOAssembly?.GetType(TYPE)?.GetMethod("UseSecondaryAbility", BindingFlags.Public | BindingFlags.Static);
                    }

                    ClassicUOClient.CUOActionQueue.Enqueue(() => { _useSecondaryMethod.Invoke(null, null); });
                }
                World.Player.SendMessage(LocString.SettingAOSAb, a);
            }
        }

        internal static void ClientSentSpecial(int ability)
        {
            if (ability == 0)
            {
                World.Player.HasSpecial = false;
                HasSecondary = false;
                HasPrimary = false;
                return;
            }
            Item right = World.Player.GetItemOnLayer(Layer.RightHand);
            Item left = World.Player.GetItemOnLayer(Layer.LeftHand);
            int leftPrimary = GetPrimaryAbility(left);
            int rightPrimary = GetPrimaryAbility(right);
            int leftSecondary = GetSecondaryAbility(left);
            int rightSecondary = GetSecondaryAbility(right);
            if (ability == leftPrimary || ability == rightPrimary)
            {
                World.Player.HasSpecial = true;
                HasPrimary = true;
                HasSecondary = false;
            }
            if (ability == leftSecondary || ability == rightSecondary)
            {
                World.Player.HasSpecial = true;
                HasPrimary = false;
                HasSecondary = true;
            }
        }
        internal static int GetPrimaryAbility(Assistant.Item wep)
        {
            if (wep != null)
                return (int)GetAbility(wep.TypeID.Value, m_Primary);
            else
                return 0;
        }

        internal static int GetSecondaryAbility(Assistant.Item wep)
        {
            if (wep != null)
                return (int)GetAbility(wep.TypeID.Value, m_Secondary);
            else
                return 0;
        }
        internal static int GetPrimaryIcon(int id)
        {
            if (id == 0 || id > 31)
                return 0x5204;
            else
                return Icon[id];
        }

        internal static int GetSecondaryIcon(int id)
        {
            if (id == 0 || id > 31)
                return 0x520A;
            else
                return Icon[id];
        }

        internal static void ClearAbilities(bool wait)
        {
            // Seems OSI does not send an execute special on weapon clear, but free servers do
            // I'd prefer to wait till server tells me its clear, but for OSI
            // I'll have to force it clear
            //if (Client.Instance.ServerEncrypted && World.Player.HasSpecial) // just a guess it is OSI                                                 
            //{                
            //    World.Player.HasSpecial = HasPrimary = HasSecondary = false;
            //    System.Threading.Thread doAction = new System.Threading.Thread(() => RazorEnhanced.SpellGrid.UpdateSAHighLight(0));
            //   doAction.Start();
            //}

            // clear these when server sends back clear packet
            if (wait)
            {
                Assistant.Client.Instance.SendToServerWait(new UseAbility(AOSAbility.Clear));
                //  Assistant.Client.Instance.SendToClientWait(ClearAbility.Instance);
            }
            else
            {
                Assistant.Client.Instance.SendToServer(new UseAbility(AOSAbility.Clear));
                //  Assistant.Client.Instance.SendToClient(ClearAbility.Instance);
            }
            World.Player.SendMessage(LocString.AOSAbCleared);
        }
    }
}
