using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace BaseRobot
{
	public class PawnColumnWorker_Rename : PawnColumnWorker
	{
		//
		// Properties
		//
		protected override GameFont DefaultHeaderFont {
			get {
				return GameFont.Tiny;
			}
		}

		//
		// Methods
		//
		public override void DoCell (Rect rect, Pawn pawn, PawnTable table)
		{
			if (Widgets.ButtonText (rect, "Rename")) {
				Find.WindowStack.Add (new Dialog_ChangeLabel (pawn));
			}
		}

		public override int GetMinWidth (PawnTable table)
		{
			return Mathf.Max (base.GetMinWidth (table), 100);
		}

		public override int GetOptimalWidth (PawnTable table)
		{
			return Mathf.Clamp (170, this.GetMinWidth (table), this.GetMaxWidth (table));
		}
	}
}
