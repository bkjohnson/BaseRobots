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

			float result;
			if (timeAssignmentDef == TimeAssignmentDefOf.Anything) {
				result = 5.5f;
			}
			else {
				if (timeAssignmentDef == TimeAssignmentDefOf.Work) {
					result = 9;
				}
				else {
					if (timeAssignmentDef == TimeAssignmentDefOf.Sleep) {
						result = 2;
					}
					else {
						if (timeAssignmentDef != TimeAssignmentDefOf.Joy) {
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
			Job result;
			if (!this.PawnCanUseWorkGiver (pawn, giver)) {
				result = null;
			}
			else {
				try {
					Job job = giver.NonScanJob (pawn);
					if (job != null) {
						Job job2 = job;
						result = job2;
						return result;
					}
					WorkGiver_Scanner scanner = giver as WorkGiver_Scanner;

					if (scanner != null) {

						if (giver.def.scanThings) {
							Predicate<Thing> predicate = (Thing t) => !ForbidUtility.IsForbidden (t, pawn) && scanner.HasJobOnThing (pawn, t, false);
							List<Thing> thingList = GridsUtility.GetThingList (cell, pawn.Map);
							for (int i = 0; i < thingList.Count; i++) {
								Thing thing = thingList [i];

								if (scanner.PotentialWorkThingRequest.Accepts (thing) && predicate (thing)) {
									pawn.mindState.lastGivenWorkType = giver.def.workType;
									Job job3 = scanner.JobOnThing (pawn, thing, false);
									result = job3;
									return result;
								}
							}
						}
						if (giver.def.scanCells && 
							!ForbidUtility.IsForbidden (cell, pawn) && 
							scanner.HasJobOnCell (pawn, cell)) {

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

			Job result;
			if (bot == null) {
				result = null;
			}
			else {
				List<WorkGiver> workGivers = bot.GetWorkGivers (false);
				int num = -999;
				TargetInfo targetInfo = TargetInfo.Invalid;
				WorkGiver_Scanner workGiver_Scanner = null;
				for (int i = 0; i < workGivers.Count; i++) {
					WorkGiver workGiver = workGivers [i];
					if (workGiver.def.priorityInType != num && 
						targetInfo.IsValid) {
						break;
					}
					if (this.PawnCanUseWorkGiver (pawn, workGiver)) {
						try {
							Job job = workGiver.NonScanJob (pawn);
							if (job != null) {
								result = job;
								return result;
							}
							WorkGiver_Scanner scanner = workGiver as WorkGiver_Scanner;

							if (scanner != null) {
								if (workGiver.def.scanThings) {
									Predicate<Thing> predicate = (Thing t) => !ForbidUtility.IsForbidden (t, pawn) && scanner.HasJobOnThing (pawn, t, false);
									IEnumerable<Thing> enumerable = scanner.PotentialWorkThingsGlobal (pawn);

									Thing thing;
									if (scanner.Prioritized) {
										IEnumerable<Thing> enumerable2 = enumerable;

										if (enumerable2 == null) {
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
									if (thing != null) {
										targetInfo = thing;
										workGiver_Scanner = scanner;
									}
								}

								if (workGiver.def.scanCells) {
									IntVec3 position = pawn.Position;
									float num2 = 99999;
									float num3 = float.MinValue;
									bool prioritized2 = scanner.Prioritized;
									foreach (IntVec3 current in scanner.PotentialWorkCellsGlobal (pawn)) {
										bool flag9 = false;
										float num4 = (float)(current - position).LengthHorizontalSquared;
										if (prioritized2) {
											if (!ForbidUtility.IsForbidden (current, pawn) && 
												scanner.HasJobOnCell (pawn, current)) {

												float priority = scanner.GetPriority (pawn, current);
												if (priority > num3 || 
													(priority == num3 && num4 < num2)) {
													flag9 = true;
													num3 = priority;
												}
											}
										}
										else {
											if (num4 < num2 && 
												!ForbidUtility.IsForbidden (current, pawn) && 
												scanner.HasJobOnCell (pawn, current)) {
												flag9 = true;
											}
										}
										if (flag9) {
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

						if (targetInfo.IsValid) {
							pawn.mindState.lastGivenWorkType = workGiver.def.workType;
							Job job2 = workGiver_Scanner.JobOnThing (pawn, targetInfo.Thing, false);

							if (job2 != null) {
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
