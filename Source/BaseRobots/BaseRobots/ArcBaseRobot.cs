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
			if (map == null && bot.rechargeStation != null) {
				map = bot.rechargeStation.Map;
			}
			if (map == null) {
				map = Find.VisibleMap;
			}

			Building_BaseRobotRechargeStation result;
			if (map == null) {
				result = null;
			}
			else {
				IEnumerable<Building_BaseRobotRechargeStation> enumerable = map.listerBuildings.AllBuildingsColonistOfClass<Building_BaseRobotRechargeStation> ();
				if (enumerable == null) {
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
			bool result;
			if (this.def2 == null) {
				result = false;
			}
			else {
				int num;
				if (workTypeDef == null || workTypeDef.relevantSkills == null) {
					num = 0;
				}
				else {
					num = workTypeDef.relevantSkills.Count;
				}
				foreach (SkillDef current in workTypeDef.relevantSkills) {
					foreach (ThingDef_BaseRobot.RobotSkills current2 in this.def2.robotSkills) {
						if (current2.skillDef == current) {
							num--;
						}
					}
					if (num == 0) {
						break;
					}
				}
				WorkTags workTags = this.def2.robotWorkTags & workTypeDef.workTags;
				result = num == 0 && workTags > 0;
			}
			return result;
		}

		public override void Destroy (DestroyMode mode = 0)
		{
			IntVec3 intVec = (base.Position != IntVec3.Invalid) ? base.Position : base.PositionHeld;
			Map map = (base.Map != null) ? base.Map : base.MapHeld;
			Building_BaseRobotRechargeStation rechargestation = this.rechargeStation;
			ThingDef thingDef = null;

			if (this != null && this.def2 != null && this.def2.destroyedDef != null) {
				thingDef = this.def2.destroyedDef;
			}
			base.Destroy (0);

			if (mode != null && thingDef != null) {
				BaseRobot_disabled BaseRobot_disabled = (BaseRobot_disabled)GenSpawn.Spawn (thingDef, intVec, map);
				BaseRobot_disabled.stackCount = 1;
				BaseRobot_disabled.rechargestation = rechargestation;
			}
		}

		private int GetPriority (WorkTypeDef workTypeDef)
		{
			int result;

			if (this.def2 == null) {
				result = 0;
			}
			else {
				foreach (ThingDef_BaseRobot.RobotWorkTypes current in this.def2.robotWorkTypes) {
					if (current.workTypeDef == workTypeDef) {
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
			List<WorkGiver> result;
			if (emergency && this.workGiversEmergencyCache != null) {
				result = this.workGiversEmergencyCache;
			}
			else {
				if (!emergency && 
					this.workGiversNonEmergencyCache != null) {
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

						if (priority > 0) {
							if (priority < num) {
								List<WorkGiverDef> arg_C8_0 = workTypeDef.workGiversByPriority;
								Predicate<WorkGiverDef> arg_C8_1;
								if ((arg_C8_1 = dummy) == null) {
									arg_C8_1 = (dummy= ((WorkGiverDef wg) => wg.emergency == emergency));
								}
								if (GenCollection.Any<WorkGiverDef> (arg_C8_0, arg_C8_1)) {
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
					if (emergency) {
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
				this.story.bodyType = this.gender == Gender.Female ? RimWorld.BodyType.Male : RimWorld.BodyType.Female;
				this.story.crownType = Verse.CrownType.Average;
				base.Drawer.renderer.graphics.ResolveApparelGraphics ();
			}
		}

		private void SetSkills ()
		{
			if (this.def2 != null) {
				foreach (SkillRecord current in this.skills.skills) {
					foreach (ThingDef_BaseRobot.RobotSkills current2 in this.def2.robotSkills) {
						if (current.def == current2.skillDef) {
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

			if (this.def2 == null) {
				Log.Error ("BaseRobot -- def2 is null. Missing class definition in xml file?");
			}
			LongEventHandler.ExecuteWhenFinished (new Action (this.InitPawn_Setup));
		}

		public override void Tick ()
		{
			base.Tick ();

			// Not sure why this is here - robots don't need food
			/*if (this.needs.food != null && 
				this.needs.food.CurLevel < 1) {
				this.needs.food.CurLevel = 1;
			}/**/

			// Gain experience if learning is allowed
			if (this.def2 == null || 
				!this.def2.allowLearning) {
				foreach (SkillRecord current in this.skills.skills) {
					if (current.xpSinceLastLevel > 1) {
						current.xpSinceLastLevel = 1;
						current.xpSinceMidnight = 1;
					}
				}
			}
			if (base.Spawned && 
				(base.Dead || base.Downed)) {
				this.Destroy ();
			}
			else {
				if (base.Spawned) {
					if (this.rechargeStation == null) {
						this.rechargeStation = ArcBaseRobot.TryFindRechargeStation (this, base.Map);
					}
				}
			}
		}
	}
}
