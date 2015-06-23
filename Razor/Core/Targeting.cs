using System;
using System.Collections.Generic;
using Assistant.Macros;

namespace Assistant
{
	internal class TargetInfo
	{
		internal byte Type;
		internal uint TargID;
		internal byte Flags;
		internal Serial Serial;
		internal int X, Y;
		internal int Z;
		internal ushort Gfx;
	}

	internal class Targeting
	{
		internal const uint LocalTargID = 0x7FFFFFFF; // uid for target sent from razor

		internal delegate void TargetResponseCallback(bool location, Serial serial, Point3D p, ushort gfxid);
		internal delegate void CancelTargetCallback();

		private static CancelTargetCallback m_OnCancel;
		private static TargetResponseCallback m_OnTarget;

		private static bool m_Intercept;
		private static bool m_HasTarget;
		private static bool m_ClientTarget;
		private static TargetInfo m_LastTarget;
		private static TargetInfo m_LastGroundTarg;
		private static TargetInfo m_LastBeneTarg;
		private static TargetInfo m_LastHarmTarg;

		private static bool m_AllowGround;
		private static uint m_CurrentID;
		private static byte m_CurFlags;

		private static uint m_PreviousID;
		private static bool m_PreviousGround;
		private static byte m_PrevFlags;

		private static Serial m_LastCombatant;

		private delegate bool QueueTarget();
		private static QueueTarget TargetSelfAction = new QueueTarget(DoTargetSelf);
		private static QueueTarget LastTargetAction = new QueueTarget(DoLastTarget);
		private static QueueTarget m_QueueTarget;


		private static uint m_SpellTargID = 0;
		internal static uint SpellTargetID { get { return m_SpellTargID; } set { m_SpellTargID = value; } }

		private static List<uint> m_FilterCancel = new List<uint>();

		internal static bool HasTarget { get { return m_HasTarget; } }

        internal static uint GetLastTarger { get { return m_LastTarget.Serial; } }

        public static void Initialize()
		{
			PacketHandler.RegisterClientToServerViewer(0x6C, new PacketViewerCallback(TargetResponse));
			PacketHandler.RegisterServerToClientViewer(0x6C, new PacketViewerCallback(NewTarget));
			PacketHandler.RegisterServerToClientViewer(0xAA, new PacketViewerCallback(CombatantChange));

	/*		HotKey.Add(HKCategory.Targets, LocString.LastTarget, new HotKeyCallback(LastTarget));
			HotKey.Add(HKCategory.Targets, LocString.TargetSelf, new HotKeyCallback(TargetSelf));
			HotKey.Add(HKCategory.Targets, LocString.ClearTargQueue, new HotKeyCallback(OnClearQueue));
			HotKey.Add(HKCategory.Targets, LocString.SetLT, new HotKeyCallback(TargetSetLastTarget));
			HotKey.Add(HKCategory.Targets, LocString.TargRandRed, new HotKeyCallback(TargetRandRed));
			HotKey.Add(HKCategory.Targets, LocString.TargRandNFriend, new HotKeyCallback(TargetRandNonFriendly));
			HotKey.Add(HKCategory.Targets, LocString.TargRandFriend, new HotKeyCallback(TargetRandFriendly));
			HotKey.Add(HKCategory.Targets, LocString.TargRandBlue, new HotKeyCallback(TargetRandInnocent));
			HotKey.Add(HKCategory.Targets, LocString.TargRandGrey, new HotKeyCallback(TargetRandGrey));
			HotKey.Add(HKCategory.Targets, LocString.TargRandEnemy, new HotKeyCallback(TargetRandEnemy));
			HotKey.Add(HKCategory.Targets, LocString.TargRandCriminal, new HotKeyCallback(TargetRandCriminal));

			HotKey.Add(HKCategory.Targets, LocString.TargRandEnemyHuman, new HotKeyCallback(TargetRandEnemyHumanoid));
			HotKey.Add(HKCategory.Targets, LocString.TargRandGreyHuman, new HotKeyCallback(TargetRandGreyHumanoid));
			HotKey.Add(HKCategory.Targets, LocString.TargRandInnocentHuman, new HotKeyCallback(TargetRandInnocentHumanoid));
			HotKey.Add(HKCategory.Targets, LocString.TargRandCriminalHuman, new HotKeyCallback(TargetRandCriminalHumanoid));

			HotKey.Add(HKCategory.Targets, LocString.AttackLastComb, new HotKeyCallback(AttackLastComb));
			HotKey.Add(HKCategory.Targets, LocString.AttackLastTarg, new HotKeyCallback(AttackLastTarg));
			HotKey.Add(HKCategory.Targets, LocString.CancelTarget, new HotKeyCallback(CancelTarget));

			HotKey.Add(HKCategory.Targets, LocString.NextTarget, new HotKeyCallback(NextTarget));
			HotKey.Add(HKCategory.Targets, LocString.NextTargetHumanoid, new HotKeyCallback(NextTargetHumanoid));

			HotKey.Add(HKCategory.Targets, LocString.TargCloseRed, new HotKeyCallback(TargetCloseRed));
			HotKey.Add(HKCategory.Targets, LocString.TargCloseNFriend, new HotKeyCallback(TargetCloseNonFriendly));
			HotKey.Add(HKCategory.Targets, LocString.TargCloseFriend, new HotKeyCallback(TargetCloseFriendly));
			HotKey.Add(HKCategory.Targets, LocString.TargCloseBlue, new HotKeyCallback(TargetCloseInnocent));
			HotKey.Add(HKCategory.Targets, LocString.TargCloseGrey, new HotKeyCallback(TargetCloseGrey));
			HotKey.Add(HKCategory.Targets, LocString.TargCloseEnemy, new HotKeyCallback(TargetCloseEnemy));
			HotKey.Add(HKCategory.Targets, LocString.TargCloseCriminal, new HotKeyCallback(TargetCloseCriminal));

			HotKey.Add(HKCategory.Targets, LocString.TargCloseEnemyHuman, new HotKeyCallback(TargetCloseEnemyHumanoid));
			HotKey.Add(HKCategory.Targets, LocString.TargCloseGreyHuman, new HotKeyCallback(TargetCloseGreyHumanoid));
			HotKey.Add(HKCategory.Targets, LocString.TargCloseInnocentHuman, new HotKeyCallback(TargetCloseInnocentHumanoid));
			HotKey.Add(HKCategory.Targets, LocString.TargCloseCriminalHuman, new HotKeyCallback(TargetCloseCriminalHumanoid));
            */
		}

		private static void CombatantChange(PacketReader p, PacketHandlerEventArgs e)
		{
			Serial ser = p.ReadUInt32();
			if (ser.IsMobile && ser != World.Player.Serial && ser != Serial.Zero && ser != Serial.MinusOne)
				m_LastCombatant = ser;
		}

		private static void AttackLastComb()
		{
			if (m_LastCombatant.IsMobile)
				ClientCommunication.SendToServer(new AttackReq(m_LastCombatant));
		}

		private static void AttackLastTarg()
		{
			if (m_LastTarget != null && m_LastTarget.Serial.IsMobile)
				ClientCommunication.SendToServer(new AttackReq(m_LastTarget.Serial));
		}

		private static void OnClearQueue()
		{
			Targeting.ClearQueue();
			World.Player.OverheadMessage(LocString.TQCleared);
		}

		internal static void OneTimeTarget(TargetResponseCallback onTarget)
		{
			OneTimeTarget(false, onTarget, null);
		}

		internal static void OneTimeTarget(bool ground, TargetResponseCallback onTarget)
		{
			OneTimeTarget(ground, onTarget, null);
		}

		internal static void OneTimeTarget(TargetResponseCallback onTarget, CancelTargetCallback onCancel)
		{
			OneTimeTarget(false, onTarget, onCancel);
		}

		internal static void OneTimeTarget(bool ground, TargetResponseCallback onTarget, CancelTargetCallback onCancel)
		{
			if (m_Intercept && m_OnCancel != null)
			{
				m_OnCancel();
				CancelOneTimeTarget();
			}

			if (m_HasTarget && m_CurrentID != 0 && m_CurrentID != LocalTargID)
			{
				m_PreviousID = m_CurrentID;
				m_PreviousGround = m_AllowGround;
				m_PrevFlags = m_CurFlags;

				m_FilterCancel.Add(m_PreviousID);
			}

			m_Intercept = true;
			m_CurrentID = LocalTargID;
			m_OnTarget = onTarget;
			m_OnCancel = onCancel;

			m_ClientTarget = m_HasTarget = true;
			ClientCommunication.SendToClient(new Target(LocalTargID, ground));
			ClearQueue();
		}

		internal static void CancelOneTimeTarget()
		{
			m_ClientTarget = m_HasTarget = false;

			ClientCommunication.SendToClient(new CancelTarget(LocalTargID));
			EndIntercept();
		}

		private static bool m_LTWasSet;
		internal static void TargetSetLastTarget()
		{
			if (World.Player != null)
			{
				m_LTWasSet = false;
				OneTimeTarget(false, new TargetResponseCallback(OnSetLastTarget), new CancelTargetCallback(OnSLTCancel));
				World.Player.SendMessage(MsgLevel.Force, LocString.TargSetLT);
			}
		}

		private static void OnSLTCancel()
		{
			if (m_LastTarget != null)
				m_LTWasSet = true;
		}

		private static void OnSetLastTarget(bool location, Serial serial, Point3D p, ushort gfxid)
		{
			if (serial == World.Player.Serial)
			{
				OnSLTCancel();
				return;
			}

			m_LastBeneTarg = m_LastHarmTarg = m_LastGroundTarg = m_LastTarget = new TargetInfo();
			m_LastTarget.Flags = 0;
			m_LastTarget.Gfx = gfxid;
			m_LastTarget.Serial = serial;
			m_LastTarget.Type = (byte)(location ? 1 : 0);
			m_LastTarget.X = p.X;
			m_LastTarget.Y = p.Y;
			m_LastTarget.Z = p.Z;

			m_LTWasSet = true;

			World.Player.SendMessage(MsgLevel.Force, LocString.LTSet);
			if (serial.IsMobile)
			{
				LastTargetChanged();
				ClientCommunication.SendToClient(new ChangeCombatant(serial));
				m_LastCombatant = serial;
			}
		}

		private static Serial m_OldLT = Serial.Zero;

		private static void RemoveTextFlags(UOEntity m)
		{
			if (m != null)
			{
				bool oplchanged = false;

				oplchanged |= m.ObjPropList.Remove(Language.GetString(LocString.LastTarget));
				oplchanged |= m.ObjPropList.Remove(Language.GetString(LocString.HarmfulTarget));
				oplchanged |= m.ObjPropList.Remove(Language.GetString(LocString.BeneficialTarget));

				if (oplchanged)
					m.OPLChanged();
			}
		}

		private static void AddTextFlags(UOEntity m)
		{
			if (m != null)
			{
				bool oplchanged = false;

                if (RazorEnhanced.Settings.General.ReadBool("SmartLastTarget"))
				{
					if (m_LastHarmTarg != null && m_LastHarmTarg.Serial == m.Serial)
					{
						oplchanged = true;
						m.ObjPropList.Add(Language.GetString(LocString.HarmfulTarget));
					}

					if (m_LastBeneTarg != null && m_LastBeneTarg.Serial == m.Serial)
					{
						oplchanged = true;
						m.ObjPropList.Add(Language.GetString(LocString.BeneficialTarget));
					}
				}

				if (!oplchanged && m_LastTarget != null && m_LastTarget.Serial == m.Serial)
				{
					oplchanged = true;
					m.ObjPropList.Add(Language.GetString(LocString.LastTarget));
				}

				if (oplchanged)
					m.OPLChanged();
			}
		}

		private static void LastTargetChanged()
		{
			if (m_LastTarget != null)
			{
                bool lth = RazorEnhanced.Settings.General.ReadInt("LTHilight") != 0;

				if (m_OldLT.IsItem)
				{
					RemoveTextFlags(World.FindItem(m_OldLT));
				}
				else
				{
					Mobile m = World.FindMobile(m_OldLT);
					if (m != null)
					{
						if (lth)
							ClientCommunication.SendToClient(new MobileIncoming(m));

						RemoveTextFlags(m);
					}
				}

				if (m_LastTarget.Serial.IsItem)
				{
					AddTextFlags(World.FindItem(m_LastTarget.Serial));
				}
				else
				{
					Mobile m = World.FindMobile(m_LastTarget.Serial);
					if (m != null)
					{
						if (IsLastTarget(m) && lth)
							ClientCommunication.SendToClient(new MobileIncoming(m));

						CheckLastTargetRange(m);

						AddTextFlags(m);
					}
				}

				m_OldLT = m_LastTarget.Serial;
			}
		}

		internal static bool LTWasSet { get { return m_LTWasSet; } }

		internal static void TargetRandNonFriendly()
		{
			RandomTarget(3, 4, 5, 6);
		}

		internal static void TargetRandFriendly()
		{
			RandomTarget(0, 1, 2);
		}

		internal static void TargetRandEnemy()
		{
			RandomTarget(5);
		}

		internal static void TargetRandEnemyHumanoid()
		{
			RandomHumanoidTarget(5);
		}

		internal static void TargetRandRed()
		{
			RandomTarget(6);
		}

		internal static void TargetRandGrey()
		{
			RandomTarget(3, 4);
		}

		internal static void TargetRandGreyHumanoid()
		{
			RandomHumanoidTarget(3, 4);
		}

		internal static void TargetRandCriminal()
		{
			RandomTarget(4);
		}

		internal static void TargetRandCriminalHumanoid()
		{
			RandomHumanoidTarget(4);
		}

		internal static void TargetRandInnocent()
		{
			RandomTarget(1);
		}

		internal static void TargetRandInnocentHumanoid()
		{
			RandomHumanoidTarget(1);
		}

		internal static void TargetRandAnyone()
		{
			RandomTarget();
		}

		internal static void RandomTarget(params int[] noto)
		{
			if (!ClientCommunication.AllowBit(FeatureBit.RandomTargets))
				return;

			List<Mobile> list = new List<Mobile>();
			foreach (Mobile m in World.MobilesInRange(12))
			{
                if ((!RazorEnhanced.Friend.IsFriend(m.Serial) || (noto.Length > 0 && noto[0] == 0)) &&
					!m.Blessed && !m.IsGhost && m.Serial != World.Player.Serial &&
                    Utility.InRange(World.Player.Position, m.Position, RazorEnhanced.Settings.General.ReadInt("LTRange")))
				{
					foreach (int n in noto)
					{
						if (n == m.Notoriety)
						{
							list.Add(m);
							break;
						}
					}

					if (noto.Length == 0)
						list.Add(m);
				}
			}

			if (list.Count > 0)
				SetLastTargetTo(list[Utility.Random(list.Count)]);
			else
				World.Player.SendMessage(MsgLevel.Warning, LocString.TargNoOne);
		}

		internal static void RandomHumanoidTarget(params int[] noto)
		{
			if (!ClientCommunication.AllowBit(FeatureBit.RandomTargets))
				return;

			List<Mobile> list = new List<Mobile>();
			foreach (Mobile m in World.MobilesInRange(12))
			{
				if (m.Body != 0x0190 && m.Body != 0x0191 && m.Body != 0x025D && m.Body != 0x025E)
					continue;

                if ((!RazorEnhanced.Friend.IsFriend(m.Serial) || (noto.Length > 0 && noto[0] == 0)) &&
					!m.Blessed && !m.IsGhost && m.Serial != World.Player.Serial &&
                    Utility.InRange(World.Player.Position, m.Position, RazorEnhanced.Settings.General.ReadInt("LTRange")))
				{
					foreach (int n in noto)
					{
						if (n == m.Notoriety)
						{
							list.Add(m);
							break;
						}
					}

					if (noto.Length == 0)
						list.Add(m);
				}
			}

			if (list.Count > 0)
				SetLastTargetTo((Mobile)list[Utility.Random(list.Count)]);
			else
				World.Player.SendMessage(MsgLevel.Warning, LocString.TargNoOne);
		}


		internal static void TargetCloseNonFriendly()
		{
			ClosestTarget(3, 4, 5, 6);
		}

		internal static void TargetCloseFriendly()
		{
			ClosestTarget(0, 1, 2);
		}

		internal static void TargetCloseEnemy()
		{
			ClosestTarget(5);
		}

		internal static void TargetCloseEnemyHumanoid()
		{
			ClosestHumanoidTarget(5);
		}

		internal static void TargetCloseRed()
		{
			ClosestTarget(6);
		}

		internal static void TargetCloseGrey()
		{
			ClosestTarget(3, 4);
		}

		internal static void TargetCloseGreyHumanoid()
		{
			ClosestHumanoidTarget(3, 4);
		}

		internal static void TargetCloseCriminal()
		{
			ClosestTarget(4);
		}

		internal static void TargetCloseCriminalHumanoid()
		{
			ClosestHumanoidTarget(4);
		}

		internal static void TargetCloseInnocent()
		{
			ClosestTarget(1);
		}

		internal static void TargetCloseInnocentHumanoid()
		{
			ClosestHumanoidTarget(1);
		}

		internal static void TargetClosest()
		{
			ClosestTarget();
		}

		internal static void ClosestTarget(params int[] noto)
		{
			if (!ClientCommunication.AllowBit(FeatureBit.ClosestTargets))
				return;

			List<Mobile> list = new List<Mobile>();
			foreach (Mobile m in World.MobilesInRange(12))
			{
                if ((!RazorEnhanced.Friend.IsFriend(m.Serial) || (noto.Length > 0 && noto[0] == 0)) &&
					!m.Blessed && !m.IsGhost && m.Serial != World.Player.Serial &&
                    Utility.InRange(World.Player.Position, m.Position, RazorEnhanced.Settings.General.ReadInt("LTRange")))
				{
					foreach (int n in noto)
					{
						if (n == m.Notoriety)
						{
							list.Add(m);
							break;
						}
					}

					if (noto.Length == 0)
						list.Add(m);
				}
			}

			Mobile closest = null;
			double closestDist = double.MaxValue;

			foreach (Mobile m in list)
			{
				double dist = Utility.DistanceSqrt(m.Position, World.Player.Position);

				if (dist < closestDist || closest == null)
				{
					closestDist = dist;
					closest = m;
				}
			}

			if (closest != null)
				SetLastTargetTo(closest);
			else
				World.Player.SendMessage(MsgLevel.Warning, LocString.TargNoOne);
		}

		internal static void ClosestHumanoidTarget(params int[] noto)
		{
			if (!ClientCommunication.AllowBit(FeatureBit.ClosestTargets))
				return;

			List<Mobile> list = new List<Mobile>();
			foreach (Mobile m in World.MobilesInRange(12))
			{
				if (m.Body != 0x0190 && m.Body != 0x0191 && m.Body != 0x025D && m.Body != 0x025E)
					continue;

				if ((!RazorEnhanced.Friend.IsFriend(m.Serial) || (noto.Length > 0 && noto[0] == 0)) &&
					!m.Blessed && !m.IsGhost && m.Serial != World.Player.Serial &&
                    Utility.InRange(World.Player.Position, m.Position, RazorEnhanced.Settings.General.ReadInt("LTRange")))
				{
					foreach (int n in noto)
					{
						if (n == m.Notoriety)
						{
							list.Add(m);
							break;
						}
					}

					if (noto.Length == 0)
						list.Add(m);
				}
			}

			Mobile closest = null;
			double closestDist = double.MaxValue;

			foreach (Mobile m in list)
			{
				double dist = Utility.DistanceSqrt(m.Position, World.Player.Position);

				if (dist < closestDist || closest == null)
				{
					closestDist = dist;
					closest = m;
				}
			}

			if (closest != null)
				SetLastTargetTo(closest);
			else
				World.Player.SendMessage(MsgLevel.Warning, LocString.TargNoOne);
		}

		internal static void SetLastTargetTo(Mobile m)
		{
			SetLastTargetTo(m, 0);
		}

		internal static void SetLastTargetTo(Mobile m, byte flagType)
		{
			TargetInfo targ = new TargetInfo();
			m_LastGroundTarg = m_LastTarget = targ;

			if ((m_HasTarget && m_CurFlags == 1) || flagType == 1)
				m_LastHarmTarg = targ;
			else if ((m_HasTarget && m_CurFlags == 2) || flagType == 2)
				m_LastBeneTarg = targ;
			else if (flagType == 0)
				m_LastHarmTarg = m_LastBeneTarg = targ;

			targ.Type = 0;
			if (m_HasTarget)
				targ.Flags = m_CurFlags;
			else
				targ.Flags = flagType;

			targ.Gfx = m.Body;
			targ.Serial = m.Serial;
			targ.X = m.Position.X;
			targ.Y = m.Position.Y;
			targ.Z = m.Position.Z;

			ClientCommunication.SendToClient(new ChangeCombatant(m));
			m_LastCombatant = m.Serial;
			World.Player.SendMessage(MsgLevel.Force, LocString.NewTargSet);

			bool wasSmart = RazorEnhanced.Settings.General.ReadBool("SmartLastTarget");
			if (wasSmart)
                RazorEnhanced.Settings.General.WriteBool("SmartLastTarget", false);
			LastTarget();
			if (wasSmart)
                RazorEnhanced.Settings.General.WriteBool("SmartLastTarget", true);
			LastTargetChanged();
		}

		private static void EndIntercept()
		{
			m_Intercept = false;
			m_OnTarget = null;
			m_OnCancel = null;
		}

		internal static void TargetSelf()
		{
			TargetSelf(false);
		}

		internal static void TargetSelf(bool forceQ)
		{
			if (World.Player == null)
				return;

			//if ( Macros.MacroManager.AcceptActions )
			//	MacroManager.Action( new TargetSelfAction() );

			if (m_HasTarget)
			{
				if (!DoTargetSelf())
					ResendTarget();
			}
            else if (forceQ || RazorEnhanced.Settings.General.ReadBool("QueueTargets"))
			{
				if (!forceQ)
					World.Player.OverheadMessage(LocString.QueuedTS);
				m_QueueTarget = TargetSelfAction;
			}
		}

		internal static bool DoTargetSelf()
		{
			if (World.Player == null)
				return false;

			if (CheckHealPoisonTarg(m_CurrentID, World.Player.Serial))
				return false;

			CancelClientTarget();
			m_HasTarget = false;

			if (m_Intercept)
			{
				TargetInfo targ = new TargetInfo();
				targ.Serial = World.Player.Serial;
				targ.Gfx = World.Player.Body;
				targ.Type = 0;
				targ.X = World.Player.Position.X;
				targ.Y = World.Player.Position.Y;
				targ.Z = World.Player.Position.Z;
				targ.TargID = LocalTargID;
				targ.Flags = 0;

				OneTimeResponse(targ);
			}
			else
			{
				ClientCommunication.SendToServer(new TargetResponse(m_CurrentID, World.Player));
			}

			return true;
		}

		internal static void LastTarget()
		{
			LastTarget(false);
		}

		internal static void LastTarget(bool forceQ)
		{
			//if ( Macros.MacroManager.AcceptActions )
			//	MacroManager.Action( new LastTargetAction() );

			if (m_HasTarget)
			{
				if (!DoLastTarget())
					ResendTarget();
			}
            else if (forceQ || RazorEnhanced.Settings.General.ReadBool("QueueTargets"))
			{
				if (!forceQ)
					World.Player.OverheadMessage(LocString.QueuedLT);
				m_QueueTarget = LastTargetAction;
			}
		}

		internal static bool DoLastTarget()
		{
			TargetInfo targ;
            if (RazorEnhanced.Settings.General.ReadBool("SmartLastTarget"))
			{
				if (m_AllowGround && m_LastGroundTarg != null)
					targ = m_LastGroundTarg;
				else if (m_CurFlags == 1)
					targ = m_LastHarmTarg;
				else if (m_CurFlags == 2)
					targ = m_LastBeneTarg;
				else
					targ = m_LastTarget;

				if (targ == null)
					targ = m_LastTarget;
			}
			else
			{
				if (m_AllowGround && m_LastGroundTarg != null)
					targ = m_LastGroundTarg;
				else
					targ = m_LastTarget;
			}

			if (targ == null)
				return false;

			Point3D pos = Point3D.Zero;
			if (targ.Serial.IsMobile)
			{
				Mobile m = World.FindMobile(targ.Serial);
				if (m != null)
				{
					pos = m.Position;

					targ.X = pos.X;
					targ.Y = pos.Y;
					targ.Z = pos.Z;
				}
				else
				{
					pos = Point3D.Zero;
				}
			}
			else if (targ.Serial.IsItem)
			{
				Item i = World.FindItem(targ.Serial);
				if (i != null)
				{
					pos = i.GetWorldPosition();

					targ.X = i.Position.X;
					targ.Y = i.Position.Y;
					targ.Z = i.Position.Z;
				}
				else
				{
					pos = Point3D.Zero;
					targ.X = targ.Y = targ.Z = 0;
				}
			}
			else
			{
				if (!m_AllowGround && (targ.Serial == Serial.Zero || targ.Serial >= 0x80000000))
				{
					World.Player.SendMessage(MsgLevel.Warning, LocString.LTGround);
					return false;
				}
				else
				{
					pos = new Point3D(targ.X, targ.Y, targ.Z);
				}
			}

            if (RazorEnhanced.Settings.General.ReadBool("RangeCheckLT") && (pos == Point3D.Zero || !Utility.InRange(World.Player.Position, pos, RazorEnhanced.Settings.General.ReadInt("LTRange"))))
			{
                if (RazorEnhanced.Settings.General.ReadBool("QueueTargets"))
					m_QueueTarget = LastTargetAction;
				World.Player.SendMessage(MsgLevel.Warning, LocString.LTOutOfRange);
				return false;
			}

			if (CheckHealPoisonTarg(m_CurrentID, targ.Serial))
				return false;

			CancelClientTarget();
			m_HasTarget = false;

			targ.TargID = m_CurrentID;

			if (m_Intercept)
				OneTimeResponse(targ);
			else
				ClientCommunication.SendToServer(new TargetResponse(targ));
			return true;
		}

		internal static void ClearQueue()
		{
			m_QueueTarget = null;
		}

		private static TimerCallbackState m_OneTimeRespCallback = new TimerCallbackState(OneTimeResponse);

		private static void OneTimeResponse(object state)
		{
			TargetInfo info = state as TargetInfo;

			if (info != null)
			{
				if ((info.X == 0xFFFF && info.X == 0xFFFF) && (info.Serial == 0 || info.Serial >= 0x80000000))
				{
					if (m_OnCancel != null)
						m_OnCancel();
				}
				else
				{
					if (Macros.MacroManager.AcceptActions)
						MacroManager.Action(new AbsoluteTargetAction(info));

					if (m_OnTarget != null)
						m_OnTarget(info.Type == 1 ? true : false, info.Serial, new Point3D(info.X, info.Y, info.Z), info.Gfx);
				}
			}

			EndIntercept();
		}

		private static void CancelTarget()
		{
			OnClearQueue();
			CancelClientTarget();

			if (m_HasTarget)
			{
				ClientCommunication.SendToServer(new TargetCancelResponse(m_CurrentID));
				m_HasTarget = false;
			}
		}

		private static void CancelClientTarget()
		{
			if (m_ClientTarget)
			{
				m_FilterCancel.Add((uint)m_CurrentID);
				ClientCommunication.SendToClient(new CancelTarget(m_CurrentID));
				m_ClientTarget = false;
			}
		}

		internal static void Target(TargetInfo info)
		{
			if (m_Intercept)
			{
				OneTimeResponse(info);
			}
			else if (m_HasTarget)
			{
				info.TargID = m_CurrentID;
				m_LastGroundTarg = m_LastTarget = info;
				ClientCommunication.SendToServer(new TargetResponse(info));
			}

			CancelClientTarget();
			m_HasTarget = false;
		}

		internal static void Target(Point3D pt)
		{
			TargetInfo info = new TargetInfo();
			info.Type = 1;
			info.Flags = 0;
			info.Serial = 0;
			info.X = pt.X;
			info.Y = pt.Y;
			info.Z = pt.Z;
			info.Gfx = 0;

			Target(info);
		}

		internal static void Target(Point3D pt, int gfx)
		{
			TargetInfo info = new TargetInfo();
			info.Type = 1;
			info.Flags = 0;
			info.Serial = 0;
			info.X = pt.X;
			info.Y = pt.Y;
			info.Z = pt.Z;
			info.Gfx = (ushort)(gfx & 0x3FFF);

			Target(info);
		}

		internal static void Target(Serial s)
		{
			TargetInfo info = new TargetInfo();
			info.Type = 0;
			info.Flags = 0;
			info.Serial = s;

			if (s.IsItem)
			{
				Item item = World.FindItem(s);
				if (item != null)
				{
					info.X = item.Position.X;
					info.Y = item.Position.Y;
					info.Z = item.Position.Z;
					info.Gfx = item.ItemID;
				}
			}
			else if (s.IsMobile)
			{
				Mobile m = World.FindMobile(s);
				if (m != null)
				{
					info.X = m.Position.X;
					info.Y = m.Position.Y;
					info.Z = m.Position.Z;
					info.Gfx = m.Body;
				}
			}

			Target(info);
		}

		internal static void Target(object o)
		{
			if (o is Item)
			{
				Item item = (Item)o;
				TargetInfo info = new TargetInfo();
				info.Type = 0;
				info.Flags = 0;
				info.Serial = item.Serial;
				info.X = item.Position.X;
				info.Y = item.Position.Y;
				info.Z = item.Position.Z;
				info.Gfx = item.ItemID;
				Target(info);
			}
			else if (o is Mobile)
			{
				Mobile m = (Mobile)o;
				TargetInfo info = new TargetInfo();
				info.Type = 0;
				info.Flags = 0;
				info.Serial = m.Serial;
				info.X = m.Position.X;
				info.Y = m.Position.Y;
				info.Z = m.Position.Z;
				info.Gfx = m.Body;
				Target(info);
			}
			else if (o is Serial)
			{
				Target((Serial)o);
			}
			else if (o is TargetInfo)
			{
				Target((TargetInfo)o);
			}
		}

		internal static void CheckTextFlags(Mobile m)
		{
            if (RazorEnhanced.Settings.General.ReadBool("SmartLastTarget"))
			{
				bool harm = m_LastHarmTarg != null && m_LastHarmTarg.Serial == m.Serial;
				bool bene = m_LastBeneTarg != null && m_LastBeneTarg.Serial == m.Serial;
				if (harm)
					m.OverheadMessage(0x90, String.Format("[{0}]", Language.GetString(LocString.HarmfulTarget)));
				if (bene)
					m.OverheadMessage(0x3F, String.Format("[{0}]", Language.GetString(LocString.BeneficialTarget)));
			}

			if (m_LastTarget != null && m_LastTarget.Serial == m.Serial)
				m.OverheadMessage(0x3B2, String.Format("[{0}]", Language.GetString(LocString.LastTarget)));
		}

		internal static bool IsLastTarget(Mobile m)
		{
			if (m != null)
			{
                if (RazorEnhanced.Settings.General.ReadBool("SmartLastTarget"))
				{
					if (m_LastHarmTarg != null && m_LastHarmTarg.Serial == m.Serial)
						return true;
				}
				else
				{
					if (m_LastTarget != null && m_LastTarget.Serial == m.Serial)
						return true;
				}
			}

			return false;
		}

		private static int m_NextTargIdx = 0;


		internal static void NextTarget()
		{
			List<Mobile> list = World.MobilesInRange(12);
			TargetInfo targ = new TargetInfo();
			Mobile m = null, old = World.FindMobile(m_LastTarget == null ? Serial.Zero : m_LastTarget.Serial);

			if (list.Count <= 0)
			{
				World.Player.SendMessage(MsgLevel.Warning, LocString.TargNoOne);
				return;
			}

			for (int i = 0; i < 3; i++)
			{
				m_NextTargIdx++;

				if (m_NextTargIdx >= list.Count)
					m_NextTargIdx = 0;

				m = (Mobile)list[m_NextTargIdx];

				if (m != null && m != World.Player && m != old)
					break;
				else
					m = null;
			}

			if (m == null)
				m = old;

			if (m == null)
			{
				World.Player.SendMessage(MsgLevel.Warning, LocString.TargNoOne);
				return;
			}

			m_LastGroundTarg = m_LastTarget = targ;

			m_LastHarmTarg = m_LastBeneTarg = targ;

			if (m_HasTarget)
				targ.Flags = m_CurFlags;
			else
				targ.Type = 0;

			targ.Gfx = m.Body;
			targ.Serial = m.Serial;
			targ.X = m.Position.X;
			targ.Y = m.Position.Y;
			targ.Z = m.Position.Z;

			ClientCommunication.SendToClient(new ChangeCombatant(m));
			m_LastCombatant = m.Serial;
			World.Player.SendMessage(MsgLevel.Force, LocString.NewTargSet);

			/*if ( m_HasTarget )
			{
				DoLastTarget();
				ClearQueue();
			}*/
		}

		private static int m_NextTargHumanoidIdx = 0;
		internal static void NextTargetHumanoid()
		{
			List<Mobile> mobiles = World.MobilesInRange(12);
			List<Mobile> list = new List<Mobile>();

			foreach (Mobile mob in mobiles)
			{
				if (mob.Body == 0x0190 || mob.Body == 0x0191 || mob.Body == 0x025D || mob.Body == 0x025E)
					list.Add(mob);
			}

			if (list.Count <= 0)
			{
				World.Player.SendMessage(MsgLevel.Warning, LocString.TargNoOne);
				return;
			}

			TargetInfo targ = new TargetInfo();
			Mobile m = null, old = World.FindMobile(m_LastTarget == null ? Serial.Zero : m_LastTarget.Serial);

			for (int i = 0; i < 3; i++)
			{
				m_NextTargHumanoidIdx++;

				if (m_NextTargHumanoidIdx >= list.Count)
					m_NextTargHumanoidIdx = 0;

				m = (Mobile)list[m_NextTargHumanoidIdx];

				if (m != null && m != World.Player && m != old)
					break;
				else
					m = null;
			}

			if (m == null)
				m = old;

			if (m == null)
			{
				World.Player.SendMessage(MsgLevel.Warning, LocString.TargNoOne);
				return;
			}

			m_LastGroundTarg = m_LastTarget = targ;

			m_LastHarmTarg = m_LastBeneTarg = targ;

			if (m_HasTarget)
				targ.Flags = m_CurFlags;
			else
				targ.Type = 0;

			targ.Gfx = m.Body;
			targ.Serial = m.Serial;
			targ.X = m.Position.X;
			targ.Y = m.Position.Y;
			targ.Z = m.Position.Z;

			ClientCommunication.SendToClient(new ChangeCombatant(m));
			m_LastCombatant = m.Serial;
			World.Player.SendMessage(MsgLevel.Force, LocString.NewTargSet);

			/*if ( m_HasTarget )
			{
				DoLastTarget();
				ClearQueue();
			}*/
		}

		internal static void CheckLastTargetRange(Mobile m)
		{
			if (World.Player == null)
				return;

			if (m_HasTarget && m != null && m_LastTarget != null && m.Serial == m_LastTarget.Serial && m_QueueTarget == LastTargetAction)
			{
                if (RazorEnhanced.Settings.General.ReadBool("RangeCheckLT"))
				{
                    if (Utility.InRange(World.Player.Position, m.Position, RazorEnhanced.Settings.General.ReadInt("LTRange")))
					{
						if (m_QueueTarget())
							ClearQueue();
					}
				}
			}
		}

		private static bool CheckHealPoisonTarg(uint targID, Serial ser)
		{
			if (World.Player == null)
				return false;

			if (targID == m_SpellTargID && ser.IsMobile && (World.Player.LastSpell == Spell.ToID(1, 4) || World.Player.LastSpell == Spell.ToID(4, 5)) && Config.GetBool("BlockHealPoison") && ClientCommunication.AllowBit(FeatureBit.BlockHealPoisoned))
			{
				Mobile m = World.FindMobile(ser);

				if (m != null && m.Poisoned)
				{
					World.Player.SendMessage(MsgLevel.Warning, LocString.HealPoisonBlocked);
					return true;
				}
			}

			return false;
		}

		private static void TargetResponse(PacketReader p, PacketHandlerEventArgs args)
		{
			TargetInfo info = new TargetInfo();
			info.Type = p.ReadByte();
			info.TargID = p.ReadUInt32();
			info.Flags = p.ReadByte();
			info.Serial = p.ReadUInt32();
			info.X = p.ReadUInt16();
			info.Y = p.ReadUInt16();
			info.Z = p.ReadInt16();
			info.Gfx = p.ReadUInt16();

			m_ClientTarget = false;

			// check for cancel
			if (info.X == 0xFFFF && info.X == 0xFFFF && (info.Serial <= 0 || info.Serial >= 0x80000000))
			{
				m_HasTarget = false;

				if (m_Intercept)
				{
					args.Block = true;
					Timer.DelayedCallbackState(TimeSpan.Zero, m_OneTimeRespCallback, info).Start();
					EndIntercept();

					if (m_PreviousID != 0)
					{
						m_CurrentID = m_PreviousID;
						m_AllowGround = m_PreviousGround;
						m_CurFlags = m_PrevFlags;

						m_PreviousID = 0;

						ResendTarget();
					}
				}
				else if (m_FilterCancel.Contains((uint)info.TargID) || info.TargID == LocalTargID)
				{
					args.Block = true;
				}

				m_FilterCancel.Clear();
				return;
			}

			ClearQueue();

			if (m_Intercept)
			{
				if (info.TargID == LocalTargID)
				{
					Timer.DelayedCallbackState(TimeSpan.Zero, m_OneTimeRespCallback, info).Start();

					m_HasTarget = false;
					args.Block = true;

					if (m_PreviousID != 0)
					{
						m_CurrentID = m_PreviousID;
						m_AllowGround = m_PreviousGround;
						m_CurFlags = m_PrevFlags;

						m_PreviousID = 0;

						ResendTarget();
					}

					m_FilterCancel.Clear();

					return;
				}
				else
				{
					EndIntercept();
				}
			}

			m_HasTarget = false;

			if (CheckHealPoisonTarg(m_CurrentID, info.Serial))
			{
				ResendTarget();
				args.Block = true;
			}

			if (info.Serial != World.Player.Serial)
			{
				if (info.Serial.IsValid)
				{
					// only let lasttarget be a non-ground target

					m_LastTarget = info;
					if (info.Flags == 1)
						m_LastHarmTarg = info;
					else if (info.Flags == 2)
						m_LastBeneTarg = info;

					LastTargetChanged();
				}

				m_LastGroundTarg = info; // ground target is the true last target

				if (Macros.MacroManager.AcceptActions)
					MacroManager.Action(new AbsoluteTargetAction(info));
			}
			else
			{
				if (Macros.MacroManager.AcceptActions)
				{
					//KeyData hk = HotKey.Get((int)LocString.TargetSelf);
					//if (hk != null)
					//	MacroManager.Action(new HotKeyAction(hk));
					//else
						MacroManager.Action(new AbsoluteTargetAction(info));
				}
			}

			m_FilterCancel.Clear();
		}

		private static void NewTarget(PacketReader p, PacketHandlerEventArgs args)
		{
			bool prevAllowGround = m_AllowGround;
			uint prevID = m_CurrentID;
			byte prevFlags = m_CurFlags;
			bool prevClientTarget = m_ClientTarget;

			m_AllowGround = p.ReadBoolean(); // allow ground
			m_CurrentID = p.ReadUInt32(); // target uid
			m_CurFlags = p.ReadByte(); // flags
			// the rest of the packet is 0s

			// check for a server cancel command
			if (!m_AllowGround && m_CurrentID == 0 && m_CurFlags == 3)
			{
				m_HasTarget = false;
				m_ClientTarget = false;
				if (m_Intercept)
				{
					EndIntercept();
					World.Player.SendMessage(MsgLevel.Error, LocString.OTTCancel);
				}
				return;
			}

			if (Spell.LastCastTime + TimeSpan.FromSeconds(3.0) > DateTime.Now && Spell.LastCastTime + TimeSpan.FromSeconds(0.5) <= DateTime.Now && m_SpellTargID == 0)
				m_SpellTargID = m_CurrentID;

			m_HasTarget = true;
			m_ClientTarget = false;

			if (m_QueueTarget == null && Macros.MacroManager.AcceptActions && MacroManager.Action(new WaitForTargetAction()))
			{
				args.Block = true;
			}
			else if (m_QueueTarget != null && m_QueueTarget())
			{
				ClearQueue();
				args.Block = true;
			}

			if (args.Block)
			{
				if (prevClientTarget)
				{
					m_AllowGround = prevAllowGround;
					m_CurrentID = prevID;
					m_CurFlags = prevFlags;

					m_ClientTarget = true;

					if (!m_Intercept)
						CancelClientTarget();
				}
			}
			else
			{
				m_ClientTarget = true;

				if (m_Intercept)
				{
					if (m_OnCancel != null)
						m_OnCancel();
					EndIntercept();
					World.Player.SendMessage(MsgLevel.Error, LocString.OTTCancel);

					m_FilterCancel.Add((uint)prevID);
				}
			}
		}

		internal static void ResendTarget()
		{
			if (!m_ClientTarget || !m_HasTarget)
			{
				CancelClientTarget();
				m_ClientTarget = m_HasTarget = true;
				ClientCommunication.SendToClient(new Target(m_CurrentID, m_AllowGround, m_CurFlags));
			}
		}
	}
}
