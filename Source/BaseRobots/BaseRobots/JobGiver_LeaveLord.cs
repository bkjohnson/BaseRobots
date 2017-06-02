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
			return (LordUtility.GetLord (pawn) != null) ? 9 : 0;
		}

		protected override Job TryGiveJob (Pawn pawn)
		{
			Lord lord = LordUtility.GetLord (pawn);
			if (lord != null) {
				lord.Notify_PawnLost (pawn, PawnLostCondition.LeftVoluntarily);
			}
			return null;
		}
	}
}
