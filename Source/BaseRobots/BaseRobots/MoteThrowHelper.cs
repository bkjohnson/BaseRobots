using System;
using UnityEngine;
using Verse;

namespace BaseRobot
{
	public static class MoteThrowHelper
	{
		//
		// Static Methods
		//
		public static Mote ThrowBatteryGreen (Vector3 loc, Map map, float scale)
		{
			return MoteThrowHelper.ThrowBatteryXYZ (DefDatabase<ThingDef>.GetNamed ("Mote_BatteryGreen", true), loc, map, scale);
		}

		public static Mote ThrowBatteryRed (Vector3 loc, Map map, float scale)
		{
			return MoteThrowHelper.ThrowBatteryXYZ (DefDatabase<ThingDef>.GetNamed ("Mote_BatteryRed", true), loc, map, scale);
		}

		public static Mote ThrowBatteryXYZ (ThingDef moteDef, Vector3 loc, Map map, float scale)
		{
			bool flag = !GenView.ShouldSpawnMotesAt (loc, map) || map.moteCounter.Saturated;
			Mote result;
			if (flag) {
				result = null;
			}
			else {
				MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing (moteDef, null);
				moteThrown.Scale = scale;
				moteThrown.rotationRate = (float)Rand.Range (-1, 1);
				moteThrown.exactPosition = loc;
				moteThrown.exactPosition += new Vector3 (0.35f, 0, 0.35f);
				moteThrown.exactPosition += new Vector3 (Rand.Value, 0, Rand.Value) * 0.1f;
				moteThrown.SetVelocity (Rand.Range (30, 60), Rand.Range (0.35f, 0.55f));
				GenSpawn.Spawn (moteThrown, IntVec3Utility.ToIntVec3 (loc), map);
				result = moteThrown;
			}
			return result;
		}

		public static Mote ThrowBatteryYellow (Vector3 loc, Map map, float scale)
		{
			return MoteThrowHelper.ThrowBatteryXYZ (DefDatabase<ThingDef>.GetNamed ("Mote_BatteryYellow", true), loc, map, scale);
		}

		public static Mote ThrowBatteryYellowYellow (Vector3 loc, Map map, float scale)
		{
			return MoteThrowHelper.ThrowBatteryXYZ (DefDatabase<ThingDef>.GetNamed ("Mote_BatteryYellowYellow", true), loc, map, scale);
		}
	}
}
