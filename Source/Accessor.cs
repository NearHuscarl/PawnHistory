using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source;

public static class Accessor
{
    public static class BattleLogEntry_StateTransition
    {
        public static readonly AccessTools.FieldRef<Verse.BattleLogEntry_StateTransition, Pawn> SubjectPawn =
            AccessTools.FieldRefAccess<Verse.BattleLogEntry_StateTransition, Pawn>("subjectPawn");

        public static readonly AccessTools.FieldRef<Verse.BattleLogEntry_StateTransition, Pawn> Initiator =
            AccessTools.FieldRefAccess<Verse.BattleLogEntry_StateTransition, Pawn>("initiator");

        public static readonly AccessTools.FieldRef<Verse.BattleLogEntry_StateTransition, HediffDef> CulpritHediff =
            AccessTools.FieldRefAccess<Verse.BattleLogEntry_StateTransition, HediffDef>("culpritHediffDef");

        public static readonly AccessTools.FieldRef<Verse.BattleLogEntry_StateTransition, BodyPartRecord> CulpritHediffTargetPart =
            AccessTools.FieldRefAccess<Verse.BattleLogEntry_StateTransition, BodyPartRecord>("culpritHediffTargetPart");
    }

    public class BattleLogEntry_RangedImpact
    {
        public static readonly AccessTools.FieldRef<Verse.BattleLogEntry_RangedImpact, Pawn> OriginalTargetPawn =
            AccessTools.FieldRefAccess<Verse.BattleLogEntry_RangedImpact, Pawn>("originalTargetPawn");
    }

    public static class PlayLogEntry_Interaction
    {
        public static readonly AccessTools.FieldRef<Verse.PlayLogEntry_Interaction, Pawn> Initiator =
            AccessTools.FieldRefAccess<Verse.PlayLogEntry_Interaction, Pawn>("initiator");

        public static readonly AccessTools.FieldRef<Verse.PlayLogEntry_Interaction, Pawn> Recipient =
            AccessTools.FieldRefAccess<Verse.PlayLogEntry_Interaction, Pawn>("recipient");

        public static readonly AccessTools.FieldRef<Verse.PlayLogEntry_Interaction, InteractionDef> InteractionDef =
            AccessTools.FieldRefAccess<Verse.PlayLogEntry_Interaction, InteractionDef>("intDef");
    }

    public class Pawn_JobTracker
    {
        public static readonly AccessTools.FieldRef<Verse.AI.Pawn_JobTracker, Pawn> Pawn = AccessTools.FieldRefAccess<Verse.AI.Pawn_JobTracker, Pawn>("pawn");
    }

    public class Pawn_AgeTracker
    {
        public static readonly List<HediffDef> tmpHediffsGained = AccessTools.StaticFieldRefAccess<Verse.Pawn_AgeTracker, List<HediffDef>>("tmpHediffsGained");
    }

    public class Pawn_GuestTracker
    {
        public static readonly AccessTools.FieldRef<RimWorld.Pawn_GuestTracker, Pawn> Pawn = AccessTools.FieldRefAccess<RimWorld.Pawn_GuestTracker, Pawn>("pawn");
    }
}
