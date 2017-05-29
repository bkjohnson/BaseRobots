using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BaseRobot
{
	public class ArcBaseRobot : Pawn
	{
		//
		// Fields
		//
		public Building_BaseRobotRechargeStation rechargeStation;

		public ThingDef_BaseRobot def2;

		private List<WorkGiver> workGiversEmergencyCache = null;

		private List<WorkGiver> workGiversNonEmergencyCache = null;

		//
		// Static Methods
		//
		public static Building_BaseRobotRechargeStation TryFindRechargeStation (ArcBaseRobot bot, Map map)
		{
			bool flag = map == null && bot.rechargeStation != null;
			if (flag) {
				map = bot.rechargeStation.Map;
			}
			bool flag2 = map == null;
			if (flag2) {
				map = Find.VisibleMap;
			}
			bool flag3 = map == null;
			Building_BaseRobotRechargeStation result;
			if (flag3) {
				result = null;
			}
			else {
				IEnumerable<Building_BaseRobotRechargeStation> enumerable = map.listerBuildings.AllBuildingsColonistOfClass<Building_BaseRobotRechargeStation> ();
				bool flag4 = enumerable == null;
				if (flag4) {
					result = null;
				}
				else {
					Building_BaseRobotRechargeStation Building_BaseRobotRechargeStation = (from t in enumerable
						where t.robot == bot
						select t).FirstOrDefault<Building_BaseRobotRechargeStation> ();
					result = Building_BaseRobotRechargeStation;
				}
			}
			return result;
		}

		//
		// Methods
		//
		public bool CanDoWorkType (WorkTypeDef workTypeDef)
		{
			bool flag = this.def2 == null;
			bool result;
			if (flag) {
				result = false;
			}
			else {
				bool flag2 = workTypeDef == null || workTypeDef.relevantSkills == null;
				int num;
				if (flag2) {
					num = 0;
				}
				else {
					num = workTypeDef.relevantSkills.Count;
				}
				foreach (SkillDef current in workTypeDef.relevantSkills) {
					foreach (ThingDef_BaseRobot.RobotSkills current2 in this.def2.robotSkills) {
						bool flag3 = current2.skillDef == current;
						if (flag3) {
							num--;
						}
					}
					bool flag4 = num == 0;
					if (flag4) {
						break;
					}
				}
				WorkTags workTags = this.def2.robotWorkTags & workTypeDef.workTags;
				bool flag5 = num == 0 && workTags > 0;
				result = flag5;
			}
			return result;
		}

		public override void Destroy (DestroyMode mode = 0)
		{
			IntVec3 intVec = (base.Position != IntVec3.Invalid) ? base.Position : base.PositionHeld;
			Map map = (base.Map != null) ? base.Map : base.MapHeld;
			Building_BaseRobotRechargeStation rechargestation = this.rechargeStation;
			ThingDef thingDef = null;
			bool flag = this != null && this.def2 != null && this.def2.destroyedDef != null;
			if (flag) {
				thingDef = this.def2.destroyedDef;
			}
			base.Destroy (0);
			bool flag2 = mode != null && thingDef != null;
			if (flag2) {
				BaseRobot_disabled BaseRobot_disabled = (BaseRobot_disabled)GenSpawn.Spawn (thingDef, intVec, map);
				BaseRobot_disabled.stackCount = 1;
				BaseRobot_disabled.rechargestation = rechargestation;
			}
		}

		private int GetPriority (WorkTypeDef workTypeDef)
		{
			bool flag = this.def2 == null;
			int result;
			if (flag) {
				result = 0;
			}
			else {
				foreach (ThingDef_BaseRobot.RobotWorkTypes current in this.def2.robotWorkTypes) {
					bool flag2 = current.workTypeDef == workTypeDef;
					if (flag2) {
						result = current.priority;
						return result;
					}
				}
				result = 0;
			}
			return result;
		}

		public List<WorkGiver> GetWorkGivers (bool emergency)
		{
			bool flag = emergency && this.workGiversEmergencyCache != null;
			List<WorkGiver> result;
			if (flag) {
				result = this.workGiversEmergencyCache;
			}
			else {
				bool flag2 = !emergency && this.workGiversNonEmergencyCache != null;
				if (flag2) {
					result = this.workGiversNonEmergencyCache;
				}
				else {
					List<WorkTypeDef> list = new List<WorkTypeDef> ();
					List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
					int num = 999;
					Predicate<WorkGiverDef> dummy = null;

					for (int i = 0; i < allDefsListForReading.Count; i++) {
						WorkTypeDef workTypeDef = allDefsListForReading [i];
						int priority = this.GetPriority (workTypeDef);
						bool flag3 = priority > 0;
						if (flag3) {
							bool flag4 = priority < num;
							if (flag4) {
								List<WorkGiverDef> arg_C8_0 = workTypeDef.workGiversByPriority;
								Predicate<WorkGiverDef> arg_C8_1;
								if ((arg_C8_1 = dummy) == null) {
									arg_C8_1 = (dummy= ((WorkGiverDef wg) => wg.emergency == emergency));
								}
								bool flag5 = GenCollection.Any<WorkGiverDef> (arg_C8_0, arg_C8_1);
								if (flag5) {
									num = priority;
								}
							}
							list.Add (workTypeDef);
						}
					}
					GenList.InsertionSort<WorkTypeDef> (list, delegate (WorkTypeDef a, WorkTypeDef b) {
						float value = (float)(a.naturalPriority + (4 - this.GetPriority (a)) * 100000);
						return ((float)(b.naturalPriority + (4 - this.GetPriority (b)) * 100000)).CompareTo (value);
					});
					List<WorkGiver> list2 = new List<WorkGiver> ();
					for (int j = 0; j < list.Count; j++) {
						WorkTypeDef workTypeDef2 = list [j];
						for (int k = 0; k < workTypeDef2.workGiversByPriority.Count; k++) {
							WorkGiver worker = workTypeDef2.workGiversByPriority [k].Worker;
							list2.Add (worker);
						}
					}
					bool emergency2 = emergency;
					if (emergency2) {
						this.workGiversEmergencyCache = list2;
					}
					else {
						this.workGiversNonEmergencyCache = list2;
					}
					result = list2;
				}
			}
			return result;
		}

		private void InitPawn_Setup ()
		{
			bool flag = Scribe.mode > 0;
			if (!flag) {
				this.equipment = new Pawn_EquipmentTracker (this);
				this.apparel = new Pawn_ApparelTracker (this);
				this.skills = new Pawn_SkillTracker (this);
				this.SetSkills ();
				this.story = new Pawn_StoryTracker (this);
				bool flag2 = this.gender == Gender.Male;
				if (flag2) {
					this.story.bodyType = RimWorld.BodyType.Male;
				}
				else {
					this.story.bodyType = RimWorld.BodyType.Female;
				}
				this.story.crownType = Verse.CrownType.Average;
				base.Drawer.renderer.graphics.ResolveApparelGraphics ();
			}
		}

		private void SetSkills ()
		{
			bool flag = this.def2 == null;
			if (!flag) {
				foreach (SkillRecord current in this.skills.skills) {
					foreach (ThingDef_BaseRobot.RobotSkills current2 in this.def2.robotSkills) {
						bool flag2 = current.def == current2.skillDef;
						if (flag2) {
							current.levelInt = current2.level;
							current.passion = current2.passion;
						}
					}
				}
			}
		}

		public override void SpawnSetup (Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup (map, respawningAfterLoad);
			this.def2 = (this.def as ThingDef_BaseRobot);
			bool flag = this.def2 == null;
			if (flag) {
				Log.Error ("BaseRobot -- def2 is null. Missing class definition in xml file?");
			}
			LongEventHandler.ExecuteWhenFinished (new Action (this.InitPawn_Setup));
		}

		public override void Tick ()
		{
			base.Tick ();
			bool flag = this.needs.food != null && this.needs.food.CurLevel < 1;
			if (flag) {
				this.needs.food.CurLevel = 1;
			}
			bool flag2 = this.def2 == null || !this.def2.allowLearning;
			if (flag2) {
				foreach (SkillRecord current in this.skills.skills) {
					bool flag3 = current.xpSinceLastLevel > 1;
					if (flag3) {
						current.xpSinceLastLevel = 1;
						current.xpSinceMidnight = 1;
					}
				}
			}
			bool flag4 = base.Spawned && (base.Dead || base.Downed || this.needs.rest.CurLevel <= 0.02);
			if (flag4) {
				this.Destroy ();
			}
			else {
				bool spawned = base.Spawned;
				if (spawned) {
					bool flag5 = this.rechargeStation == null;
					if (flag5) {
						this.rechargeStation = ArcBaseRobot.TryFindRechargeStation (this, base.Map);
					}
				}
			}
		}
	}
}
