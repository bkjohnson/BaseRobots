using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BaseRobot
{
	[StaticConstructorOnStartup]
	public class Building_BaseRobotRechargeStation : Building
	{
		//
		// Static Fields
		//
		private static Texture2D UI_ButtonForceRecharge = ContentFinder<Texture2D>.Get ("UI/Commands/Robots/UI_ShutDown", true);

		private static Texture2D UI_ButtonForceActivateAll = ContentFinder<Texture2D>.Get ("UI/Commands/Robots/UI_StartAll", true);

		private static Texture2D UI_ButtonForceRechargeAll = ContentFinder<Texture2D>.Get ("UI/Commands/Robots/UI_ShutDownAll", true);

		private static Texture2D UI_ButtonStart = ContentFinder<Texture2D>.Get ("UI/Commands/Robots/UI_Start", true);

		//
		// Fields
		//
		private bool robotSpawnedOnce = false;

		public ArcBaseRobot robot;

		public List<ArcBaseRobot> container;

		public ThingDef_BaseRobot_Building_RechargeStation def2 = null;

		private bool robotIsDestroyed = false;

		private ThingDef thingDefSpawn;

		private int timerMoteThrow = 0;

		private bool updateGraphicForceNeeded = false;

		private Graphic graphicOld;

		public CompPowerTrader powerComp;

		private int checkDistanceAndEnergyTicks;

		private float rechargeEfficiency = 1;

		public bool SpawnRobotAfterRecharge = true;

		private string spawnThingDef = "";

		private string txtSendOwnerToRecharge = "AIRobot_SendOwnerToRecharge";

		private string lbSendOwnerToRecharge = "AIRobot_Label_SendOwnerToRecharge";

		private string txtSpawnOwner = "AIRobot_SpawnRobot";

		private string lbSpawnOwner = "AIRobot_Label_SpawnRobot";

		private string txtNoPower = "AIRobot_NoPower";

		private string lbRecallAllRobots = "AIRobot_Label_RecallAllRobots";

		public string SecondaryGraphicPath;

		public Graphic SecondaryGraphic;

		public Graphic PrimaryGraphic;

		private string txtActivateAllRobots = "AIRobot_ActivateAllRobots";

		private string lbActivateAllRobots = "AIRobot_Label_ActivateAllRobots";

		private string txtRecallAllRobots = "AIRobot_RecallAllRobots";

		//
		// Properties
		//
		public override Graphic Graphic {
			get {
				bool flag = this.PrimaryGraphic == null;
				Graphic result;
				if (flag) {
					this.GetGraphics ();
					bool flag2 = this.PrimaryGraphic == null;
					if (flag2) {
						result = base.Graphic;
						return result;
					}
				}
				bool flag3 = this.robot == null && !this.robotIsDestroyed;
				if (flag3) {
					result = this.PrimaryGraphic;
				}
				else {
					bool flag4 = this.SecondaryGraphic == null;
					if (flag4) {
						this.GetGraphics ();
						bool flag5 = this.SecondaryGraphic == null;
						if (flag5) {
							result = this.PrimaryGraphic;
						}
						else {
							result = this.SecondaryGraphic;
						}
					}
					else {
						result = this.SecondaryGraphic;
					}
				}
				return result;
			}
		}

		//
		// Methods
		//
		public void AddRobotToContainer (ArcBaseRobot bot)
		{
			bool flag = AttachmentUtility.HasAttachment (bot, ThingDefOf.Fire);
			if (flag) {
				AttachmentUtility.GetAttachment (bot, ThingDefOf.Fire).Destroy (0);
			}
			bot.stances.CancelBusyStanceHard ();
			bot.jobs.StopAll (false);
			bot.pather.StopDead ();
			bool drafted = bot.Drafted;
			if (drafted) {
				bot.drafter.Drafted = false;
			}
			bool flag2 = !this.container.Contains (bot);
			if (flag2) {
				this.container.Add (bot);
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++) {
				maps [i].designationManager.RemoveAllDesignationsOn (bot, false);
			}
			this.DespawnRobot (bot, false);
		}

		private void Button_CallAllBotsForShutdown ()
		{
			List<Building_BaseRobotRechargeStation> list = base.Map.listerThings.AllThings.OfType<Building_BaseRobotRechargeStation> ().ToList<Building_BaseRobotRechargeStation> ();
			for (int i = list.Count; i > 0; i--) {
				Building_BaseRobotRechargeStation Building_BaseRobotRechargeStation = list [i - 1];
				Building_BaseRobotRechargeStation.Notify_CallBotForShutdown ();
			}
		}

		private void Button_CallBotForShutdown ()
		{
			this.SpawnRobotAfterRecharge = false;
			bool flag = this.robot == null || this.robotIsDestroyed;
			if (!flag) {
				bool flag2 = !this.robot.Spawned;
				if (!flag2) {
					bool flag3 = this.robot.jobs == null;
					if (flag3) {
						Log.Error ("Robot has no job driver!");
					}
					JobGiver_RechargeEnergy x2_JobGiver_RechargeEnergy = new JobGiver_RechargeEnergy ();
					ThinkResult thinkResult = x2_JobGiver_RechargeEnergy.TryIssueJobPackage (this.robot, default(JobIssueParams));
					bool flag4 = thinkResult.Job == null || this.robot.CurJob.def == thinkResult.Job.def;
					if (!flag4) {
						this.robot.jobs.StopAll (false);
						bool flag5 = this.robot.drafter == null;
						if (flag5) {
							this.robot.drafter = new Pawn_DraftController (this.robot);
						}
						this.robot.drafter.Drafted = false;
						this.robot.jobs.StartJob (thinkResult.Job, 0, null, false, true, null, null);
					}
				}
			}
		}

		private void Button_ResetDestroyedRobot ()
		{
			bool flag = this.robot != null && !this.robot.Destroyed;
			if (flag) {
				this.robot.Destroy (0);
			}
			this.robot = null;
			this.robotIsDestroyed = false;
			this.Button_SpawnBot ();
		}

		private void Button_SpawnAllAvailableBots ()
		{
			List<Building_BaseRobotRechargeStation> list = base.Map.listerThings.AllThings.OfType<Building_BaseRobotRechargeStation> ().ToList<Building_BaseRobotRechargeStation> ();
			for (int i = list.Count; i > 0; i--) {
				Building_BaseRobotRechargeStation x2_Building_AIRobotRechargeStation = list [i - 1];
				x2_Building_AIRobotRechargeStation.Notify_SpawnBot ();
			}
		}

		private void Button_SpawnBot ()
		{
			bool flag = this.robot != null || this.robotIsDestroyed;
			if (!flag) {
				bool flag2 = GenText.NullOrEmpty (this.spawnThingDef);
				if (flag2) {
					Log.Error ("Robot Recharge Station: Wanted to spawn robot, but spawnThingDef is null or empty!");
				}
				else {
					bool flag3 = !this.IsRobotInContainer ();
					ArcBaseRobot bot;
					if (flag3) {
						bot = Building_BaseRobotCreator.CreateRobot (this.spawnThingDef, base.Position, base.Map, Faction.OfPlayer);
					}
					else {
						bot = this.container [0];
						this.container.Remove (bot);
						bot = (GenSpawn.Spawn (bot, base.Position, base.Map) as ArcBaseRobot);
					}
					this.robot = bot;
					this.robot.rechargeStation = this;
					this.robotSpawnedOnce = true;
					this.SpawnRobotAfterRecharge = true;
				}
			}
		}

		private void ClearContainer ()
		{
			bool flag = this.container != null && this.container.Count > 0;
			if (flag) {
				this.container.Clear ();
			}
			this.container = new List<ArcBaseRobot> ();
		}

		public void DespawnRobot (ArcBaseRobot bot, bool destroying = false)
		{
			bool flag = bot != null && !bot.Destroyed;
			if (flag) {
				if (destroying) {
					bot.Destroy (0);
				}
				else {
					bool spawned = bot.Spawned;
					if (spawned) {
						bot.DeSpawn ();
					}
				}
			}
			this.robot = null;
		}

		public override void Destroy (DestroyMode mode = 0)
		{
			this.DespawnRobot (this.robot, true);
			this.ClearContainer ();
			base.Destroy (mode);
		}

		public override void ExposeData ()
		{
			try {
				base.ExposeData ();
				Scribe_Values.Look<bool> (ref this.robotSpawnedOnce, "robotSpawned", false, false);
				Scribe_Values.Look<bool> (ref this.robotIsDestroyed, "robotDestroyed", false, false);
				Scribe_Values.Look<bool> (ref this.SpawnRobotAfterRecharge, "autospawn", true, false);
				try {
					Scribe_References.Look<ArcBaseRobot> (ref this.robot, "robot", true);
				}
				catch (Exception ex) {
					Log.Warning ("Building_BaseRobot_RechargeStation -- Error while loading 'robot': " + 
						ex.Message + "	"
						+ ex.StackTrace);
				}
				try {
					//Scribe_Collections.Look<BaseRobot> (ref this.container, "container", 2, null);
					Scribe_Collections.Look<ArcBaseRobot> (ref this.container, "container", LookMode.Deep, null);
				}
				catch (Exception ex2) {
					Log.Warning ("Building_BaseRobot_RechargeStation -- Error while loading 'container': " +
						ex2.Message + "	" +
						ex2.StackTrace);
				}
			}
			catch (Exception ex3) {
				Log.Error ("Building_BaseRobot_RechargeStation -- Unknown error while loading: " + 
					ex3.Message + "	" + 
					ex3.StackTrace);
			}

			bool flag = Scribe.mode == LoadSaveMode.PostLoadInit;
			if (flag) {
				this.updateGraphicForceNeeded = true;
				bool flag2 = this.container == null;
				if (flag2) {
					this.ClearContainer ();
				}
			}
		}
			
		public override IEnumerable<Gizmo> GetGizmos ()
		{
			int num = 31367676;

			/*foreach (Gizmo gizmo in this.GetGizmos) {
				yield return gizmo;
				gizmo = null;
			}/**/

			IEnumerator<Gizmo> enumerator = null;
			bool flag = this.robot == null && !this.robotIsDestroyed;
			if (flag) {
				Command_Action command_Action = new Command_Action ();
				command_Action.defaultLabel = Translator.Translate (this.lbSpawnOwner);
				command_Action.defaultDesc = Translator.Translate (this.txtSpawnOwner);
				command_Action.icon = Building_BaseRobotRechargeStation.UI_ButtonStart;
				command_Action.hotKey = KeyBindingDefOf.Misc4;
				command_Action.activateSound = SoundDef.Named ("Click");
				command_Action.action = new Action (this.Button_SpawnBot);
				command_Action.disabled = (this.powerComp != null && !this.powerComp.PowerOn);
				command_Action.disabledReason = Translator.Translate (this.txtNoPower);
				command_Action.groupKey = num + 1;
				yield return command_Action;
				command_Action = null;
			}
			bool flag2 = (this.robot != null || this.robotSpawnedOnce) && !this.robotIsDestroyed;
			if (flag2) {
				Command_Action command_Action2 = new Command_Action ();
				command_Action2.defaultLabel = Translator.Translate (this.lbSendOwnerToRecharge);
				command_Action2.defaultDesc = Translator.Translate (this.txtSendOwnerToRecharge);
				command_Action2.icon = Building_BaseRobotRechargeStation.UI_ButtonForceRecharge;
				command_Action2.hotKey = KeyBindingDefOf.Misc7;
				command_Action2.activateSound = SoundDef.Named ("Click");
				command_Action2.action = new Action (this.Button_CallBotForShutdown);
				command_Action2.disabled = (this.powerComp != null && !this.powerComp.PowerOn);
				command_Action2.disabledReason = Translator.Translate (this.txtNoPower);
				command_Action2.groupKey = num + 2;
				yield return command_Action2;
				command_Action2 = null;
			}
			Command_Action command_Action3 = new Command_Action ();
			command_Action3.defaultLabel = Translator.Translate (this.lbRecallAllRobots);
			command_Action3.defaultDesc = Translator.Translate (this.txtRecallAllRobots);
			command_Action3.icon = Building_BaseRobotRechargeStation.UI_ButtonForceRechargeAll;
			command_Action3.hotKey = KeyBindingDefOf.Misc8;
			command_Action3.activateSound = SoundDef.Named ("Click");
			command_Action3.action = new Action (this.Button_CallAllBotsForShutdown);
			command_Action3.disabled = (this.powerComp != null && !this.powerComp.PowerOn);
			command_Action3.disabledReason = Translator.Translate (this.txtNoPower);
			command_Action3.groupKey = num + 3;
			yield return command_Action3;
			command_Action3 = null;
			Command_Action command_Action4 = new Command_Action ();
			command_Action4.defaultLabel = Translator.Translate (this.lbActivateAllRobots);
			command_Action4.defaultDesc = Translator.Translate (this.txtActivateAllRobots);
			command_Action4.icon = Building_BaseRobotRechargeStation.UI_ButtonForceActivateAll;
			command_Action4.hotKey = KeyBindingDefOf.Misc10;
			command_Action4.activateSound = SoundDef.Named ("Click");
			command_Action4.action = new Action (this.Button_SpawnAllAvailableBots);
			command_Action4.disabled = (this.powerComp != null && !this.powerComp.PowerOn);
			command_Action4.disabledReason = Translator.Translate (this.txtNoPower);
			command_Action4.groupKey = num + 4;
			yield return command_Action4;
			command_Action4 = null;
			bool godMode = DebugSettings.godMode;
			if (godMode) {
				Command_Action command_Action5 = new Command_Action ();
				command_Action5.defaultLabel = "(DEBUG) Reset destroyed robot";
				command_Action5.defaultDesc = "";
				command_Action5.icon = BaseContent.BadTex;
				command_Action5.hotKey = null;
				command_Action5.activateSound = SoundDef.Named ("Click");
				command_Action5.action = new Action (this.Button_ResetDestroyedRobot);
				command_Action5.disabled = false;
				command_Action5.disabledReason = "";
				command_Action5.groupKey = num + 9;
				yield return command_Action5;
				command_Action5 = null;
			}
			yield break;
			yield break;
		}

		public void GetGraphics ()
		{
			bool flag = this.def2 == null;
			if (flag) {
				this.ReadXMLData ();
			}
			bool flag2 = this.PrimaryGraphic == null || this.PrimaryGraphic == BaseContent.BadGraphic;
			if (flag2) {
				this.PrimaryGraphic = base.Graphic;
			}
			bool flag3 = this.SecondaryGraphic == null || this.SecondaryGraphic == BaseContent.BadGraphic;
			if (flag3) {
				this.SecondaryGraphic = GraphicDatabase.Get<Graphic_Multi> (this.SecondaryGraphicPath, this.def2.graphic.Shader, this.def2.graphic.drawSize, this.def2.graphic.Color, this.def2.graphic.ColorTwo);
			}
			bool flag4 = Building_BaseRobotRechargeStation.UI_ButtonForceRecharge == null;
			if (flag4) {
				Building_BaseRobotRechargeStation.UI_ButtonForceRecharge = ContentFinder<Texture2D>.Get ("UI/Commands/Robots/UI_ShutDown", true);
			}
			bool flag5 = Building_BaseRobotRechargeStation.UI_ButtonStart == null;
			if (flag5) {
				Building_BaseRobotRechargeStation.UI_ButtonStart = ContentFinder<Texture2D>.Get ("UI/Commands/Robots/UI_Start", true);
			}
		}

		public override string GetInspectString ()
		{
			StringBuilder stringBuilder = new StringBuilder ();
			string inspectString = base.GetInspectString ();
			bool flag = GenText.NullOrEmpty (inspectString);
			if (flag) {
				stringBuilder.Append ("...");
			}
			else {
				stringBuilder.Append (base.GetInspectString ());
			}
			bool flag2 = this.robot != null && this.robot.Spawned;
			if (flag2) {
				stringBuilder.AppendLine ().Append (Translator.Translate ("AIRobot_RobotIs") + " " + this.robot.LabelShort);
			}
			else {
				bool flag3 = this.robotIsDestroyed;
				if (flag3) {
					stringBuilder.AppendLine ().Append (Translator.Translate ("AIRobot_RobotIsDestroyed"));
				}
			}
			return stringBuilder.ToString ();
		}

		private bool IsRobotInContainer ()
		{
			bool flag = this.container == null;
			bool result;
			if (flag) {
				this.ClearContainer ();
				result = false;
			}
			else {
				bool flag2 = GenText.NullOrEmpty (this.spawnThingDef);
				if (flag2) {
					result = false;
				}
				else {
					bool flag3 = this.thingDefSpawn == null;
					if (flag3) {
						this.thingDefSpawn = DefDatabase<ThingDef>.GetNamedSilentFail (this.spawnThingDef);
					}
					bool flag4 = this.thingDefSpawn == null || this.NumContained (this.container, this.thingDefSpawn) == 0;
					result = !flag4;
				}
			}
			return result;
		}

		public void Notify_CallBotForShutdown ()
		{
			this.Button_CallBotForShutdown ();
		}

		public void Notify_SpawnBot ()
		{
			this.Button_SpawnBot ();
		}

		public int NumContained (List<ArcBaseRobot> workList, ThingDef def)
		{
			int num = 0;
			for (int i = 0; i < workList.Count; i++) {
				bool flag = workList [i].def == def;
				if (flag) {
					num += workList [i].stackCount;
				}
			}
			return num;
		}

		public override void PostMake ()
		{
			base.PostMake ();
		}

		private void ReadXMLData ()
		{
			this.def2 = (this.def as ThingDef_BaseRobot_Building_RechargeStation);
			bool flag = this.def2 == null;
			if (!flag) {
				this.spawnThingDef = this.def2.spawnThingDef;
				this.SecondaryGraphicPath = this.def2.secondaryGraphicPath;
				this.rechargeEfficiency = this.def2.rechargeEfficiency;
			}
		}

		public void Setup_Part2 ()
		{
			this.GetGraphics ();
		}

		public override void SpawnSetup (Map map, bool respawningAfterLoad)
		{
			this.ReadXMLData ();
			base.SpawnSetup (map, respawningAfterLoad);
			this.powerComp = base.GetComp<CompPowerTrader> ();
			bool flag = this.container == null;
			if (flag) {
				this.ClearContainer ();
			}
			LongEventHandler.ExecuteWhenFinished (new Action (this.Setup_Part2));
		}

		public override void Tick ()
		{
			base.Tick ();
			this.UpdateGraphic ();
			bool flag = this.robot == null && this.IsRobotInContainer ();
			if (flag) {
				ArcBaseRobot bot = this.container [0];
				bool flag2 = bot == null;
				if (flag2) {
					this.container.Remove (this.container [0]);
				}
				else {
					bool flag3 = this.SpawnRobotAfterRecharge && bot.needs.rest.CurLevel >= 0.99;
					if (flag3) {
						this.Button_SpawnBot ();
					}
					else {
						bool flag4 = bot.needs.rest.CurLevel < 1;
						if (flag4) {
							Need_Rest expr_B8 = bot.needs.rest;
							expr_B8.CurLevel = expr_B8.CurLevel + 4E-05f * this.rechargeEfficiency;
							bool flag5 = bot.needs.rest.CurLevel > 1;
							if (flag5) {
								bot.needs.rest.CurLevel = 1;
							}
							this.TryThrowBatteryMote (bot);
						}
					}
				}
			}
			else {
				bool flag6 = this.robotIsDestroyed;
				if (!flag6) {
					bool flag7 = this.robot == null && (!this.robotSpawnedOnce || !this.IsRobotInContainer ());
					if (!flag7) {
						bool flag8 = this.robotSpawnedOnce && !this.IsRobotInContainer () && (this.robot == null || this.robot.Destroyed || this.robot.Dead);
						if (flag8) {
							bool flag9 = (this.robot.Destroyed || this.robot.Dead) && this.robot.Corpse != null;
							if (flag9) {
								this.robot.Corpse.Destroy (0);
							}
							this.robotIsDestroyed = true;
						}
						else {
							this.checkDistanceAndEnergyTicks--;
							bool flag10 = this.checkDistanceAndEnergyTicks > 0;
							if (!flag10) {
								this.checkDistanceAndEnergyTicks = 192;
								bool flag11 = this.robot.needs.rest.CurLevel < 0.4 && !this.robot.Drafted && !BaseRobot_Helper.IsInDistance (base.Position, this.robot.Position, 50);
								if (flag11) {
									this.Button_CallBotForShutdown ();
									this.SpawnRobotAfterRecharge = true;
								}
							}
						}
					}
				}
			}
		}

		private void TryThrowBatteryMote (ArcBaseRobot robot)
		{
			bool flag = robot == null;
			if (!flag) {
				this.timerMoteThrow--;
				bool flag2 = this.timerMoteThrow > 0;
				if (!flag2) {
					this.timerMoteThrow = 300;
					float curLevel = robot.needs.rest.CurLevel;
					bool flag3 = curLevel > 0.99;
					if (!flag3) {
						bool flag4 = curLevel > 0.9;
						if (flag4) {
							MoteThrowHelper.ThrowBatteryGreen (base.Position.ToVector3 (), base.Map, 0.8f);
						}
						else {
							bool flag5 = curLevel > 0.7;
							if (flag5) {
								MoteThrowHelper.ThrowBatteryYellowYellow (base.Position.ToVector3 (), base.Map, 0.8f);
							}
							else {
								bool flag6 = curLevel > 0.35;
								if (flag6) {
									MoteThrowHelper.ThrowBatteryYellow (base.Position.ToVector3 (), base.Map, 0.8f);
								}
								else {
									MoteThrowHelper.ThrowBatteryRed (base.Position.ToVector3 (), base.Map, 0.8f);
								}
							}
						}
					}
				}
			}
		}

		private void UpdateGraphic ()
		{
			bool flag = this.Graphic != this.graphicOld || this.updateGraphicForceNeeded;
			if (flag) {
				this.updateGraphicForceNeeded = false;
				this.graphicOld = this.Graphic;
				this.Notify_ColorChanged ();
				base.Map.mapDrawer.MapMeshDirty (base.Position, MapMeshFlag.Things, true, false);
			}
		}
	}
}
