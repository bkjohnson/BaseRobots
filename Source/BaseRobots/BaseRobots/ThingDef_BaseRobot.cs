using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace BaseRobot
{
	public class ThingDef_BaseRobot : ThingDef
	{
		//
		// Fields
		//
		public List<ThingDef_BaseRobot.RobotSkills> robotSkills = new List<ThingDef_BaseRobot.RobotSkills> ();

		public ThingDef destroyedDef = null;

		public bool allowLearning = false;

		private WorkTags robotWorkTagsInt = 0;

		public List<ThingDef_BaseRobot.RobotWorkTypes> robotWorkTypes = new List<ThingDef_BaseRobot.RobotWorkTypes> ();

		//
		// Properties
		//
		public WorkTags robotWorkTags {
			get {
				bool flag = this.robotWorkTagsInt == null && this.robotWorkTypes.Count > 0;
				if (flag) {
					this.InitWorkTagsFromWorkTypes ();
				}
				return this.robotWorkTagsInt;
			}
			set {
				this.robotWorkTagsInt = value;
			}
		}

		//
		// Methods
		//
		private WorkTags InitWorkTagsFromWorkTypes ()
		{
			WorkTags workTags = 0;
			foreach (ThingDef_BaseRobot.RobotWorkTypes current in this.robotWorkTypes) {
				workTags |= current.workTypeDef.workTags;
			}
			return workTags;
		}

		//
		// Nested Types
		//
		public class RobotSkills
		{
			public SkillDef skillDef;

			public int level = 0;

			public Passion passion = 0;
		}

		public class RobotWorkTypes
		{
			public WorkTypeDef workTypeDef;

			public int priority = 1;
		}
	}
}
