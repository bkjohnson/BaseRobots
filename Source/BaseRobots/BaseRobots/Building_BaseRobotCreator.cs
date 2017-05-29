using RimWorld;
using System;
using Verse;

namespace BaseRobot
{
	public class Building_BaseRobotCreator : Building
	{
		//
		// Fields
		//
		private bool destroy = false;

		//
		// Static Methods
		//
		public static ArcBaseRobot CreateRobot (string pawnDefName, IntVec3 position, Map map)
		{
			return Building_BaseRobotCreator.CreateRobot (pawnDefName, position, map, Faction.OfPlayer);
		}

		public static ArcBaseRobot CreateRobot (string pawnDefName, IntVec3 position, Map map, Faction faction)
		{
			PawnKindDef named = DefDatabase<PawnKindDef>.GetNamed (pawnDefName, true);
			//PawnGenerationRequest pawnGenerationRequest = new PawnGenerationRequest (named, faction, 2, -1, true, true, false, false, false, false, 0, false, false, true, false, false, null, new float? (0), new float? (0), new Gender? (1), new float? (0), null);
			PawnGenerationRequest gen = new PawnGenerationRequest (named, faction, PawnGenerationContext.NonPlayer, -1, true, true, false, false, false, false, 0, false, false, true, false, false, null, new float? (0), new float? (0), Gender.None, 0, null);
			ArcBaseRobot newThing = (ArcBaseRobot)PawnGenerator.GeneratePawn (gen);
			return (ArcBaseRobot)Building_BaseRobotCreator.Spawn (newThing, position, map);
		}

		public static Thing Spawn (Thing newThing, IntVec3 loc, Map map)
		{
			return Building_BaseRobotCreator.Spawn (newThing, loc, map, Rot4.South);
		}

		public static Thing Spawn (Thing newThing, IntVec3 loc, Map map, Rot4 rot)
		{
			newThing = GenSpawn.Spawn (newThing, loc, map, rot, false);
			newThing.Position = loc;
			return newThing;
		}

		//
		// Methods
		//
		public override void Tick ()
		{
			bool flag = this.destroy;
			if (flag) {
				this.Destroy (0);
			}
			else {
				try {
					Building_BaseRobotCreator.CreateRobot ("BaseRobot_Hauler", base.Position, base.Map);
				}
				catch (Exception ex) {
					Log.Error ("Error while creating Robot." +
						ex.Message + "STACK: "
						+ ex.StackTrace);
				}
				this.destroy = true;
			}
		}
	}
}
