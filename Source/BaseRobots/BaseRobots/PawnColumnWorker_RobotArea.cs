using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace BaseRobot
{
	public class PawnColumnWorker_RobotArea : PawnColumnWorker_AllowedArea
	{
		//
		// Methods
		//
		public override void DoCell (Rect rect, Pawn pawn, PawnTable table)
		{
			// Allow robots to be assigned to any area
			AllowedAreaMode mode = AllowedAreaMode.Any;
			AreaAllowedGUI.DoAllowedAreaSelectors (rect, pawn, mode);
		}
	}
}
