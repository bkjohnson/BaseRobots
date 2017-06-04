using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace BaseRobot
{
	// Modeled after Need_Rest
	public class Need_Battery : Need
	{
		//
		// Static Fields
		//
		private const float FullSleepHours = 10.5f;

		private const float BaseInvoluntarySleepMTBDays = 0.25f;

		public const float CanWakeThreshold = 0.2f;

		public const float DefaultNaturalWakeThreshold = 1;

		public const float DefaultFallAsleepMaxLevel = 0.75f;

		public const float ThreshVeryTired = 0.14f;

		public const float ThreshTired = 0.28f;

		private const float BaseRestFallPerTick = 1.583333E-05f;

		public const float BaseRestGainPerTick = 3.809524E-05f;

		//
		// Fields
		//
		private int ticksAtZero;

		private float lastRestEffectiveness = 1;

		private int lastRestTick = -999;

		//
		// Properties
		//
		public RestCategory CurCategory {
			get {
				if (this.CurLevel < 0.01) {
					return RestCategory.Exhausted;
				}
				if (this.CurLevel < 0.14) {
					return RestCategory.VeryTired;
				}
				if (this.CurLevel < 0.28) {
					return RestCategory.Tired;
				}
				return RestCategory.Rested;
			}
		}

		public override int GUIChangeArrow {
			get {
				if (this.Resting) {
					return 1;
				}
				return -1;
			}
		}

		private float RestFallFactor {
			get {
				float num = 1;
				List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++) {
					HediffStage curStage = hediffs [i].CurStage;
					if (curStage != null) {
						num *= curStage.restFallFactor;
					}
				}
				return num;
			}
		}

		public float RestFallPerTick {
			get {
				switch (this.CurCategory) {
				case RestCategory.Rested:
					return 1.583333E-05f * this.RestFallFactor;
				case RestCategory.Tired:
					return 1.583333E-05f * this.RestFallFactor * 0.7f;
				case RestCategory.VeryTired:
					return 1.583333E-05f * this.RestFallFactor * 0.3f;
				case RestCategory.Exhausted:
					return 1.583333E-05f * this.RestFallFactor * 0.6f;
				default:
					return 999;
				}
			}
		}

		private bool Resting {
			get {
				return Find.TickManager.TicksGame < this.lastRestTick + 2;
			}
		}

		public int TicksAtZero {
			get {
				return this.ticksAtZero;
			}
		}

		//
		// Constructors
		//
		public Need_Battery (Pawn pawn) : base (pawn)
		{
			this.threshPercents = new List<float> ();
			this.threshPercents.Add (0.28f);
			this.threshPercents.Add (0.14f);
		}

		//
		// Methods
		//
		public override void ExposeData ()
		{
			base.ExposeData ();
			Scribe_Values.Look<int> (ref this.ticksAtZero, "ticksAtZero", 0, false);
		}

		public override void NeedInterval ()
		{
			if (!base.IsFrozen) {
				if (this.Resting) {
					this.CurLevel += 0.005714286f * this.lastRestEffectiveness;
				}
				else {
					this.CurLevel -= this.RestFallPerTick * 150;
				}
			}
			if (this.CurLevel < 0.0001) {
				this.ticksAtZero += 150;
			}
			else {
				this.ticksAtZero = 0;
			}
			if (this.ticksAtZero > 1000 && this.pawn.Spawned) {
				float mtb;
				if (this.ticksAtZero < 15000) {
					mtb = 0.25f;
				}
				else {
					if (this.ticksAtZero < 30000) {
						mtb = 0.125f;
					}
					else {
						if (this.ticksAtZero < 45000) {
							mtb = 0.08333334f;
						}
						else {
							mtb = 0.0625f;
						}
					}
				}
				if (Rand.MTBEventOccurs (mtb, 60000, 150)) {
					this.pawn.jobs.StartJob (new Job (JobDefOf.LayDown, this.pawn.Position), JobCondition.InterruptForced, null, false, true, null, null);
					if (PawnUtility.ShouldSendNotificationAbout (this.pawn)) {
						Messages.Message ("MessageInvoluntarySleep".Translate (new object[] {
							this.pawn.LabelShort
						}), this.pawn, MessageSound.Negative);
					}
				}
			}
		}

		public override void SetInitialLevel ()
		{
			this.CurLevel = Rand.Range (0.9f, 1);
		}

		public void TickResting (float restEffectiveness)
		{
			this.lastRestTick = Find.TickManager.TicksGame;
			this.lastRestEffectiveness = restEffectiveness;
		}
	}
}
