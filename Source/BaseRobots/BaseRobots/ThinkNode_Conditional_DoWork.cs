using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace BaseRobot
{
	public class ThinkNode_Conditional_DoWork : ThinkNode_Conditional
	{
		//
		// Fields
		//
		private WorkTypeDef workType;

		//
		// Methods
		//
		public override ThinkNode DeepCopy (bool resolve = true)
		{
			ThinkNode_Conditional_DoWork thinkNode = (ThinkNode_Conditional_DoWork)base.DeepCopy (resolve);
			thinkNode.workType = this.workType;
			return thinkNode;
		}

		public override float GetPriority (Pawn pawn)
		{
			return this.priority;
		}

		protected override bool Satisfied (Pawn pawn)
		{
			bool flag = this.workType == null;
			bool result;
			if (flag) {
				result = false;
			}
			else {
				ArcBaseRobot bot = pawn as ArcBaseRobot;
				bool flag2 = bot == null;
				if (flag2) {
					result = false;
				}
				else {
					ThingDef_BaseRobot thingDef_bot = bot.def as ThingDef_BaseRobot;
					bool flag3 = thingDef_bot == null;
					result = (!flag3 && bot.CanDoWorkType (this.workType));
				}
			}
			return result;
		}
	}
}
