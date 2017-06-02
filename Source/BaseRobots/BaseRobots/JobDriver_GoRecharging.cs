using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BaseRobot
{
	public class JobDriver_GoRecharging : JobDriver
	{
		//
		// Methods
		//
		private Toil DespawnIntoContainer ()
		{
			Toil toil = new Toil ();
			toil.initAction = delegate {
				ArcBaseRobot bot = toil.actor as ArcBaseRobot;
				if (bot != null && 
					bot.rechargeStation != null) {

					bot.rechargeStation.AddRobotToContainer (bot);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}

		public Toil GotoThing (IntVec3 cell, Map map, PathEndMode PathEndMode)
		{
			Toil toil = new Toil ();
			LocalTargetInfo target = new LocalTargetInfo (cell);
			toil.initAction = delegate {
				Pawn actor = toil.actor;
				actor.pather.StartPath (target, PathEndMode);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		protected override IEnumerable<Toil> MakeNewToils ()
		{
			yield return ToilFailConditions.FailOnDespawnedOrNull<Toil> (this.GotoThing (this.TargetA.Cell, this.Map, PathEndMode.OnCell), TargetIndex.A);
			yield return this.DespawnIntoContainer ();
			yield break;
		}
	}
}
