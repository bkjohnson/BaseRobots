using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BaseRobot
{
	public class JobGiver_Work : ThinkNode_JobGiver
	{
		//
		// Methods
		//
		public override float GetPriority (Pawn pawn)
		{
			TimeAssignmentDef timeAssignmentDef = (pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything;
			bool flag = timeAssignmentDef == TimeAssignmentDefOf.Anything;
			float result;
			if (flag) {
				result = 5.5f;
			}
			else {
				bool flag2 = timeAssignmentDef == TimeAssignmentDefOf.Work;
				if (flag2) {
					result = 9;
				}
				else {
					bool flag3 = timeAssignmentDef == TimeAssignmentDefOf.Sleep;
					if (flag3) {
						result = 2;
					}
					else {
						bool flag4 = timeAssignmentDef == TimeAssignmentDefOf.Joy;
						if (!flag4) {
							throw new NotImplementedException ();
						}
						result = 2;
					}
				}
			}
			return result;
		}

		private Job GiverTryGiveJobPrioritized (Pawn pawn, WorkGiver giver, IntVec3 cell)
		{
			bool flag = !this.PawnCanUseWorkGiver (pawn, giver);
			Job result;
			if (flag) {
				result = null;
			}
			else {
				try {
					Job job = giver.NonScanJob (pawn);
					bool flag2 = job != null;
					if (flag2) {
						Job job2 = job;
						result = job2;
						return result;
					}
					WorkGiver_Scanner scanner = giver as WorkGiver_Scanner;
					bool flag3 = scanner != null;
					if (flag3) {
						bool scanThings = giver.def.scanThings;
						if (scanThings) {
							Predicate<Thing> predicate = (Thing t) => !ForbidUtility.IsForbidden (t, pawn) && scanner.HasJobOnThing (pawn, t, false);
							List<Thing> thingList = GridsUtility.GetThingList (cell, pawn.Map);
							for (int i = 0; i < thingList.Count; i++) {
								Thing thing = thingList [i];
								bool flag4 = scanner.PotentialWorkThingRequest.Accepts (thing) && predicate (thing);
								if (flag4) {
									pawn.mindState.lastGivenWorkType = giver.def.workType;
									Job job3 = scanner.JobOnThing (pawn, thing, false);
									result = job3;
									return result;
								}
							}
						}
						bool flag5 = giver.def.scanCells && !ForbidUtility.IsForbidden (cell, pawn) && scanner.HasJobOnCell (pawn, cell);
						if (flag5) {
							pawn.mindState.lastGivenWorkType = giver.def.workType;
							Job job4 = scanner.JobOnCell (pawn, cell);
							result = job4;
							return result;
						}
					}
				}
				catch (Exception ex) {
					Log.Error (string.Concat (new object[] {
						pawn,
						" threw exception in GiverTryGiveJobTargeted on WorkGiver ",
						giver.def.defName,
						": ",
						ex.ToString ()
					}));
				}
				result = null;
			}
			return result;
		}

		private bool PawnCanUseWorkGiver (Pawn pawn, WorkGiver giver)
		{
			return giver.MissingRequiredCapacity (pawn) == null && !giver.ShouldSkip (pawn);
		}

		protected override Job TryGiveJob (Pawn pawn)
		{
			ArcBaseRobot bot = pawn as ArcBaseRobot;
			bool flag = bot == null;
			Job result;
			if (flag) {
				result = null;
			}
			else {
				List<WorkGiver> workGivers = bot.GetWorkGivers (false);
				int num = -999;
				TargetInfo targetInfo = TargetInfo.Invalid;
				WorkGiver_Scanner workGiver_Scanner = null;
				for (int i = 0; i < workGivers.Count; i++) {
					WorkGiver workGiver = workGivers [i];
					bool flag2 = workGiver.def.priorityInType != num && targetInfo.IsValid;
					if (flag2) {
						break;
					}
					bool flag3 = this.PawnCanUseWorkGiver (pawn, workGiver);
					if (flag3) {
						try {
							Job job = workGiver.NonScanJob (pawn);
							bool flag4 = job != null;
							if (flag4) {
								result = job;
								return result;
							}
							WorkGiver_Scanner scanner = workGiver as WorkGiver_Scanner;
							bool flag5 = scanner != null;
							if (flag5) {
								bool scanThings = workGiver.def.scanThings;
								if (scanThings) {
									Predicate<Thing> predicate = (Thing t) => !ForbidUtility.IsForbidden (t, pawn) && scanner.HasJobOnThing (pawn, t, false);
									IEnumerable<Thing> enumerable = scanner.PotentialWorkThingsGlobal (pawn);
									bool prioritized = scanner.Prioritized;
									Thing thing;
									if (prioritized) {
										IEnumerable<Thing> enumerable2 = enumerable;
										bool flag6 = enumerable2 == null;
										if (flag6) {
											enumerable2 = pawn.Map.listerThings.ThingsMatching (scanner.PotentialWorkThingRequest);
										}
										Predicate<Thing> predicate2 = predicate;
										thing = GenClosest.ClosestThing_Global_Reachable (pawn.Position, pawn.Map, enumerable2, scanner.PathEndMode, TraverseParms.For (pawn, Danger.Deadly, 0, false), 9999, predicate2, (Thing x) => scanner.GetPriority (pawn, x));
									}
									else {
										Predicate<Thing> predicate3 = predicate;
										bool flag7 = enumerable != null;
										thing = GenClosest.ClosestThingReachable (pawn.Position, pawn.Map, scanner.PotentialWorkThingRequest, scanner.PathEndMode, TraverseParms.For (pawn, Danger.Deadly, 0, false), 9999, predicate3, enumerable, 0, scanner.LocalRegionsToScanFirst, flag7, RegionType.Set_Passable, false);
									}
									bool flag8 = thing != null;
									if (flag8) {
										targetInfo = thing;
										workGiver_Scanner = scanner;
									}
								}
								bool scanCells = workGiver.def.scanCells;
								if (scanCells) {
									IntVec3 position = pawn.Position;
									float num2 = 99999;
									float num3 = float.MinValue;
									bool prioritized2 = scanner.Prioritized;
									foreach (IntVec3 current in scanner.PotentialWorkCellsGlobal (pawn)) {
										bool flag9 = false;
										float num4 = (float)(current - position).LengthHorizontalSquared;
										bool flag10 = prioritized2;
										if (flag10) {
											bool flag11 = !ForbidUtility.IsForbidden (current, pawn) && scanner.HasJobOnCell (pawn, current);
											if (flag11) {
												float priority = scanner.GetPriority (pawn, current);
												bool flag12 = priority > num3 || (priority == num3 && num4 < num2);
												if (flag12) {
													flag9 = true;
													num3 = priority;
												}
											}
										}
										else {
											bool flag13 = num4 < num2 && !ForbidUtility.IsForbidden (current, pawn) && scanner.HasJobOnCell (pawn, current);
											if (flag13) {
												flag9 = true;
											}
										}
										bool flag14 = flag9;
										if (flag14) {
											targetInfo = new TargetInfo (current, pawn.Map, false);
											workGiver_Scanner = scanner;
											num2 = num4;
										}
									}
								}
							}
						}
						catch (Exception ex) {
							Log.Error (string.Concat (new object[] {
								pawn,
								" threw exception in WorkGiver ",
								workGiver.def.defName,
								": ",
								ex.ToString ()
							}));
						}
						finally {
						}
						bool isValid = targetInfo.IsValid;
						if (isValid) {
							pawn.mindState.lastGivenWorkType = workGiver.def.workType;
							bool hasThing = targetInfo.HasThing;
							Job job2;
							if (hasThing) {
								job2 = workGiver_Scanner.JobOnThing (pawn, targetInfo.Thing, false);
							}
							else {
								job2 = workGiver_Scanner.JobOnCell (pawn, targetInfo.Cell);
							}
							bool flag15 = job2 != null;
							if (flag15) {
								result = job2;
								return result;
							}
							Log.ErrorOnce (string.Concat (new object[] {
								workGiver_Scanner,
								" provided target ",
								targetInfo,
								" but yielded no actual job for pawn ",
								pawn,
								". The CanGiveJob and JobOnX methods may not be synchronized."
							}), 6112651);
						}
						num = workGiver.def.priorityInType;
					}
				}
				result = null;
			}
			return result;
		}
	}
}
