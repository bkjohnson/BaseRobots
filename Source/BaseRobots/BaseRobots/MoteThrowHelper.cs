using System;
using UnityEngine;
using Verse;

namespace BaseRobot
{
	public static class MoteThrowHelper
	{

		private const float THIRTY_FIVE = 0.35f;
		private const float FIFTY_FIVE = 0.55f;
		private const float POINT_TEN = 0.1f;
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
			Mote result;
			if (!GenView.ShouldSpawnMotesAt (loc, map) || 
				map.moteCounter.Saturated) {
				result = null;
			}
			else {
				MoteThrown batteryMote = (MoteThrown)ThingMaker.MakeThing (moteDef, null);
				batteryMote.Scale = scale;
				batteryMote.rotationRate = (float)Rand.Range (-1, 1);
				batteryMote.exactPosition = loc;
				batteryMote.exactPosition += new Vector3 (THIRTY_FIVE, 0, THIRTY_FIVE);
				batteryMote.exactPosition += new Vector3 (Rand.Value, 0, Rand.Value) * POINT_TEN;
				batteryMote.SetVelocity (Rand.Range (30, 60), Rand.Range (THIRTY_FIVE, FIFTY_FIVE));
				GenSpawn.Spawn (batteryMote, IntVec3Utility.ToIntVec3 (loc), map);
				result = batteryMote;
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
