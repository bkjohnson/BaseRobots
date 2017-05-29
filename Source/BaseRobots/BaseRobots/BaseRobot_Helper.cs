using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BaseRobot
{
	[StaticConstructorOnStartup]
	public class BaseRobot_Helper
	{
		//
		// Static Methods
		//
		public static Building_BaseRobotRechargeStation FindMedicalRechargeStationFor (ArcBaseRobot p)
		{
			return BaseRobot_Helper.FindRechargeStationFor (p, p, false, false, true);
		}

		public static Building_BaseRobotRechargeStation FindRechargeStationFor (ArcBaseRobot p)
		{
			return BaseRobot_Helper.FindRechargeStationFor (p, p, false, false, false);
		}

		public static Building_BaseRobotRechargeStation FindRechargeStationFor (ArcBaseRobot sleeper, ArcBaseRobot traveler, bool sleeperWillBePrisoner, bool checkSocialProperness, bool medicalBedNeeded = false)
		{
			Predicate<Thing> predicate = delegate (Thing t) {
				bool flag3 = !ReservationUtility.CanReserveAndReach (traveler, t, PathEndMode.OnCell, Danger.Some, 1, -1, null, false);
				bool result2;
				if (flag3) {
					result2 = false;
				}
				else {
					Building_BaseRobotRechargeStation rechargeStation = t as Building_BaseRobotRechargeStation;
					bool flag4 = rechargeStation == null;
					if (flag4) {
						result2 = false;
					}
					else {
						bool flag5 = rechargeStation.robot != null && rechargeStation.robot != sleeper;
						if (flag5) {
							result2 = false;
						}
						else {
							bool flag6 = ForbidUtility.IsForbidden (rechargeStation, traveler);
							if (flag6) {
								result2 = false;
							}
							else {
								bool flag7 = FireUtility.IsBurning (rechargeStation);
								result2 = !flag7;
							}
						}
					}
				}
				return result2;
			};
			bool flag = sleeper.rechargeStation != null && predicate (sleeper.rechargeStation);
			Building_BaseRobotRechargeStation result;
			if (flag) {
				Building_BaseRobotRechargeStation rechargeStation = sleeper.rechargeStation;
				bool flag2 = rechargeStation != null;
				if (flag2) {
					result = rechargeStation;
					return result;
				}
			}
			result = null;
			return result;
		}

		public static List<List<T>> GetAllCombos<T> (List<T> initialList, bool includeEmpty = false, bool includeInitList = true)
		{
			List<List<T>> list = new List<List<T>> ();
			int num;
			if (includeEmpty) {
				num = Convert.ToInt32 (Math.Pow (2, (double)initialList.Count<T> ()));
			}
			else {
				num = Convert.ToInt32 (Math.Pow (2, (double)initialList.Count<T> ())) - 1;
			}
			int num2;
			if (includeEmpty) {
				num2 = 0;
			}
			else {
				num2 = 1;
			}
			for (int i = num2; i < num; i++) {
				List<T> list2 = new List<T> ();
				for (int j = 0; j < initialList.Count<T> (); j++) {
					int num3 = 1 << j;
					bool flag = (i & num3) == num3;
					if (flag) {
						list2.Add (initialList [j]);
					}
				}
				list.Add (list2);
			}
			if (includeInitList) {
				list.Add (initialList);
			}
			return list;
		}

		public static double GetDistance (IntVec3 p1, IntVec3 p2)
		{
			int num = Math.Abs (p1.x - p2.x);
			int num2 = Math.Abs (p1.y - p2.y);
			int num3 = Math.Abs (p1.z - p2.z);

			return Math.Sqrt ((double)(num * num + num2 * num2 + num3 * num3));
		}

		public static bool IsInDistance (IntVec3 p1, IntVec3 p2, float distance)
		{
			int num = Math.Abs (p1.x - p2.x);
			int num2 = Math.Abs (p1.y - p2.y);
			int num3 = Math.Abs (p1.z - p2.z);

			return (float)(num * num + num2 * num2 + num3 * num3) <= distance * distance;
		}

		public static void ReApplyThingToListerThings (IntVec3 cell, Thing thing)
		{
			bool flag = cell == IntVec3.Invalid || thing == null || thing.Map == null || !thing.Spawned;
			if (!flag) {
				Map map = thing.Map;
				RegionGrid regionGrid = map.regionGrid;
				Region region = null;
				bool flag2 = GenGrid.InBounds (cell, map);
				if (flag2) {
					region = regionGrid.GetValidRegionAt (cell);
				}
				bool flag3 = region != null;
				if (flag3) {
					bool flag4 = !region.ListerThings.Contains (thing);
					if (flag4) {
						region.ListerThings.Add (thing);
					}
				}
			}
		}

		/*public static void RemoveCommUnit (BaseRobot pawn)
		{
			bool flag = pawn.workSettings.GetPriority (WorkTypeDefOf.Doctor) > 0 || pawn.workSettings.GetPriority (WorkTypeDefOf.Handling) > 0 || pawn.workSettings.GetPriority (WorkTypeDefOf.Warden) > 0;
			if (!flag) {
				PawnCapacityDef talking = PawnCapacityDefOf.Talking;
				bool flag2 = pawn.health.capacities.CapableOf (talking);
				if (flag2) {
					HediffSet hediffSet = pawn.health.hediffSet;
					IEnumerable<BodyPartRecord> notMissingParts = hediffSet.GetNotMissingParts (0, 0);
					IEnumerable<BodyPartRecord> arg_9D_0 = notMissingParts;
					Func<BodyPartRecord, bool> arg_9D_1;
					if ((arg_9D_1 = BaseRobot_Helper.Something.part) == null) {
						arg_9D_1 = (BaseRobot_Helper.Something.part = new Func<BodyPartRecord, bool> (BaseRobot_Helper.Something.blah.RemoveCommUnit(arg_9D_0));
					}
					BodyPartRecord bodyPartRecord = arg_9D_0.Where (arg_9D_1).FirstOrDefault<BodyPartRecord> ();
					bool flag3 = bodyPartRecord != null;
					if (flag3) {
						DamageInfo value = new DamageInfo (DamageDefOf.EMP, Mathf.RoundToInt (hediffSet.GetPartHealth (bodyPartRecord)), -1, null, bodyPartRecord, null, 0);
						Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff (HediffDefOf.MissingBodyPart, pawn, null);
						hediff_MissingPart.IsFresh  = false;
						hediff_MissingPart.lastInjury = null;
						pawn.health.AddHediff (hediff_MissingPart, bodyPartRecord, new DamageInfo? (value));
						pawn.health.Notify_HediffChanged (hediff_MissingPart);
						pawn.apparel.Notify_LostBodyPart ();
					}
				}
			}
		}/**/

		public static void UpdateBaseShieldingWhileRecharging (Pawn pawn, bool inRechargeStation, string shieldingDefName)
		{
			bool flag = !inRechargeStation;
			if (!flag) {
				foreach (Apparel current in pawn.apparel.WornApparel) {
					bool flag2 = current.def.defName == shieldingDefName && (double)current.HitPoints < (double)current.MaxHitPoints * 0.95;
					if (flag2) {
						Apparel expr_66 = current;
						expr_66.HitPoints = expr_66.HitPoints + 1;
					}
				}
				bool flag3 = pawn.apparel.WornApparelCount != 0;
				if (!flag3) {
					ThingDef named = DefDatabase<ThingDef>.GetNamed (shieldingDefName, true);
					Apparel apparel = (Apparel)ThingMaker.MakeThing (named, null);
					apparel.HitPoints = (int)((double)apparel.MaxHitPoints * 0.05);
					bool flag4 = ApparelUtility.HasPartsToWear (pawn, apparel.def);
					if (flag4) {
						pawn.apparel.Wear (apparel, false);
					}
				}
			}
		}

		//
		// Nested Types
		//
		[CompilerGenerated]
		[Serializable]
		private sealed class Something
		{
			public static readonly BaseRobot_Helper.Something blah = new BaseRobot_Helper.Something ();

			public static Func<BodyPartRecord, bool> part;

			internal bool RemoveCommUnit (BodyPartRecord p)
			{
				return p.def.defName == "AIRobot_CommUnit";
			}
		}
	}
}
