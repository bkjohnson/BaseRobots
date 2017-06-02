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
			float result;
			if (rest == null) {
				result = 0;
			}
			else {
				float curLevel = rest.CurLevel;
				TimeAssignmentDef timeAssignmentDef = (pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything;

				if (timeAssignmentDef == TimeAssignmentDefOf.Anything) {

					if ((curLevel < 0.5 && pawn is ArcBaseRobot) & 
						!BaseRobot_Helper.IsInDistance (pawn.Position, (pawn as ArcBaseRobot).rechargeStation.Position, 50)) {
						result = 8;
					}
					else {
						result = (curLevel < 0.3) ? 8 : 0;
					}
				}
				else {
					if (timeAssignmentDef == TimeAssignmentDefOf.Work) {
						result = 0;
					}
					else {
						if (timeAssignmentDef == TimeAssignmentDefOf.Joy) {
							result = (curLevel < 0.3) ? 8 : 0;
						}
						else {
							if (timeAssignmentDef == TimeAssignmentDefOf.Sleep) {
								result = (curLevel < 0.75) ? 8 : 0;

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
			Job result;
			if (rechargeStation == null) {
				result = null;
			}
			else {
				if (bot.rechargeStation != rechargeStation) {
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
