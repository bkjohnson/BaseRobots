using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BaseRobot
{
	public class JobGiver_LeaveLord : ThinkNode_JobGiver
	{
		//
		// Methods
		//
		public override float GetPriority (Pawn pawn)
		{
			bool flag = LordUtility.GetLord (pawn) != null;
			float result;
			if (flag) {
				result = 9;
			}
			else {
				result = 0;
			}
			return result;
		}

		protected override Job TryGiveJob (Pawn pawn)
		{
			Lord lord = LordUtility.GetLord (pawn);
			bool flag = lord != null;
			if (flag) {
				lord.Notify_PawnLost (pawn, PawnLostCondition.LeftVoluntarily);
			}
			return null;
		}
	}
}
