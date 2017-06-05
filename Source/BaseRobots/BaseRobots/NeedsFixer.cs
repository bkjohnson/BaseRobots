using System;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

namespace BaseRobot
{
	[StaticConstructorOnStartup]
	class Main
	{
		static Main()
		{
			var harmony = HarmonyInstance.Create("com.github.harmony.rimworld.baserobots");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	[HarmonyPatch(typeof(Pawn_NeedsTracker))]
	[HarmonyPatch("ShouldHaveNeed")]
	[HarmonyPatch(new Type[] { typeof(NeedDef) })]
	class Need_Battery_Patch
	{
		static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result)
		{
			Pawn pawn = (Pawn)AccessTools.Field (typeof(Pawn_NeedsTracker), "pawn").GetValue (__instance);

			if (nd.needClass == typeof(Need_Battery)) {
				if (pawn.def.thingClass == typeof(ArcBaseRobot)) {
					__result = true;
				} else {
					__result = false;
				}
			} else if (pawn.def.thingClass == typeof(ArcBaseRobot)) {
				__result = false;
			}
		}
	}
}