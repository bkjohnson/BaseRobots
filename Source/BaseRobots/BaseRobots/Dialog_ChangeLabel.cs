using System;
using UnityEngine;
using Verse;

namespace BaseRobot
{
	// Modeled after Dialog_ChangeNameTriple
	public class Dialog_ChangeLabel : Window
	{
		//
		// Static Fields
		//
		private const int MaxNameLength = 16;

		//
		// Fields
		//
		private Pawn pawn;

		private string curName;

		//
		// Properties
		//
		private NameSingle CurPawnName {
			get {
				NameSingle name = this.pawn.Name as NameSingle;
				if (name != null) {
					return new NameSingle (this.curName);
				}
				throw new InvalidOperationException ();
			}
		}

		public override Vector2 InitialSize {
			get {
				return new Vector2 (500, 175);
			}
		}

		//
		// Constructors
		//
		public Dialog_ChangeLabel (Pawn pawn)
		{
			if (pawn.Name == null) {
				this.curName = pawn.Label;
				pawn.Name = new NameSingle (pawn.Label);
			}

			this.curName = pawn.Name.ToString();
			this.pawn = pawn;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.closeOnClickedOutside = true;
		}

		//
		// Methods
		//
		public override void DoWindowContents (Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label (new Rect (15, 15, 500, 50), this.curName.ToString ().Replace (" '' ", " "));
			Text.Font = GameFont.Small;
			string text = Widgets.TextField (new Rect (15, 50, inRect.width / 2 - 20, 35), this.curName);
			if (text.Length < 16) {
				this.curName = text;
			}
			if (Widgets.ButtonText (new Rect (inRect.width / 2 + 20, inRect.height - 35, inRect.width / 2 - 20, 35), "OK", true, false, true) 
				|| (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)) {
				if (this.curName.Length < 1) {
					this.curName = this.pawn.Name.ToString();
				}
				this.pawn.Name = this.CurPawnName;
				Find.WindowStack.TryRemove (this, true);
				Messages.Message ("RobotGainsName".Translate (new object[] {
					this.curName
				}), this.pawn, MessageSound.Benefit);
			}
		}
	}
}
