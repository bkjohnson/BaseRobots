using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace BaseRobot
{
	public class JobGiver_RechargeEnergy : ThinkNode_JobGiver
	{
		//
		// Methods
		//
		public override float GetPriority (Pawn pawn)
		{
			Need_Rest rest = pawn.needs.rest;
			bool flag = rest == null;
			float result;
			if (flag) {
				result = 0;
			}
			else {
				float curLevel = rest.CurLevel;
				TimeAssignmentDef timeAssignmentDef = (pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything;
				bool flag2 = timeAssignmentDef == TimeAssignmentDefOf.Anything;
				if (flag2) {
					bool flag3 = !BaseRobot_Helper.IsInDistance (pawn.Position, (pawn as ArcBaseRobot).rechargeStation.Position, 50);
					bool flag4 = (curLevel < 0.5 && pawn is ArcBaseRobot) & flag3;
					if (flag4) {
						result = 8;
					}
					else {
						bool flag5 = curLevel < 0.3;
						if (flag5) {
							result = 8;
						}
						else {
							result = 0;
						}
					}
				}
				else {
					bool flag6 = timeAssignmentDef == TimeAssignmentDefOf.Work;
					if (flag6) {
						result = 0;
					}
					else {
						bool flag7 = timeAssignmentDef == TimeAssignmentDefOf.Joy;
						if (flag7) {
							bool flag8 = curLevel < 0.3;
							if (flag8) {
								result = 8;
							}
							else {
								result = 0;
							}
						}
						else {
							bool flag9 = timeAssignmentDef == TimeAssignmentDefOf.Sleep;
							if (flag9) {
								bool flag10 = curLevel < 0.75;
								if (flag10) {
									result = 8;
								}
								else {
									result = 0;
								}
							}
							else {
								result = 0;
							}
						}
					}
				}
			}
			return result;
		}

		protected override Job TryGiveJob (Pawn pawn)
		{
			ArcBaseRobot bot = pawn as ArcBaseRobot;
			Building_BaseRobotRechargeStation rechargeStation = BaseRobot_Helper.FindRechargeStationFor (bot);
			bool flag = rechargeStation == null;
			Job result;
			if (flag) {
				result = null;
			}
			else {
				bool flag2 = bot.rechargeStation != rechargeStation;
				if (flag2) {
					result = null;
				}
				else {
					Job job = new Job (DefDatabase<JobDef>.GetNamed ("AIRobot_GoRecharge", true), rechargeStation);
					result = job;
				}
			}
			return result;
		}
	}
}
