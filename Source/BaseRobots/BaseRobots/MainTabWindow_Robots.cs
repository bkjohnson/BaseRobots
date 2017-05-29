using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace BaseRobot
{
	public class MainTabWindow_Robots : MainTabWindow_PawnTable
	{
		//
		// Properties
		//
		protected override IEnumerable<Pawn> Pawns {
			get {
				return from p in Find.VisibleMap.mapPawns.PawnsInFaction (Faction.OfPlayer)
						where p.RaceProps.IsMechanoid
					orderby p.RaceProps.baseBodySize, p.def.label
					select p;
			}
		}

		protected override PawnTableDef PawnTableDef {
			get {
				return RobotPawnTableDefOf.Robots;
			}
		}

		//
		// Methods
		//
		public override void PostOpen ()
		{
			base.PostOpen ();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}
	}
}
