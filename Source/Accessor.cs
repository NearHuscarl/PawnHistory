using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Verse;
// ReSharper disable InconsistentNaming

namespace PawnHistory.Source;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
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

        public static readonly AccessTools.FieldRef<Verse.PlayLogEntry_Interaction, List<RulePackDef>> ExtraSentencePacks =
            AccessTools.FieldRefAccess<Verse.PlayLogEntry_Interaction, List<RulePackDef>>("extraSentencePacks");
    }

    public class Pawn_HealthTracker
    {
        public static readonly AccessTools.FieldRef<Verse.Pawn_HealthTracker, Pawn> Pawn = AccessTools.FieldRefAccess<Verse.Pawn_HealthTracker, Pawn>("pawn");

        public static readonly Action<Verse.Pawn_HealthTracker, DamageInfo?, Verse.Hediff> MakeDowned =
            AccessTools.MethodDelegate<Action<Verse.Pawn_HealthTracker, DamageInfo?, Verse.Hediff>>(AccessTools.Method(typeof(Verse.Pawn_HealthTracker), "MakeDowned"));
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

    public class Pawn_MindState
    {
        public static readonly Action<Verse.AI.Pawn_MindState, Pawn, string, bool> StartManhunterBecauseOfPawnAction =
            AccessTools.MethodDelegate<Action<Verse.AI.Pawn_MindState, Pawn, string, bool>>(AccessTools.Method(typeof(Verse.AI.Pawn_MindState), "StartManhunterBecauseOfPawnAction"));
    }

    public class MentalStateHandler
    {
        public static readonly AccessTools.FieldRef<Verse.AI.MentalStateHandler, Pawn> Pawn = AccessTools.FieldRefAccess<Verse.AI.MentalStateHandler, Pawn>("pawn");
    }

    public class HediffGiver
    {
        public static readonly Action<Verse.HediffGiver, Pawn, Verse.Hediff> SendLetter =
            AccessTools.MethodDelegate<Action<Verse.HediffGiver, Pawn, Verse.Hediff>>(AccessTools.Method(typeof(Verse.HediffGiver), "SendLetter"));
    }

    public class LogLineDisplayableLog
    {
        public static readonly AccessTools.FieldRef<RimWorld.ITab_Pawn_Log_Utility.LogLineDisplayableLog, LogEntry> Log =
            AccessTools.FieldRefAccess<RimWorld.ITab_Pawn_Log_Utility.LogLineDisplayableLog, LogEntry>("log");
    }

    public class Building_Casket
    {
        public static readonly AccessTools.FieldRef<RimWorld.Building_Casket, ThingOwner> InnerContainer = AccessTools.FieldRefAccess<RimWorld.Building_Casket, ThingOwner>("innerContainer");
    }

    public class ScenPart_PlayerPawnsArriveMethod
    {
        public static readonly AccessTools.FieldRef<RimWorld.ScenPart_PlayerPawnsArriveMethod, PlayerPawnsArriveMethod> Method =
            AccessTools.FieldRefAccess<RimWorld.ScenPart_PlayerPawnsArriveMethod, PlayerPawnsArriveMethod>("method");
    }

    public class GenFilePaths
    {
        public static readonly Func<string, string> FolderUnderSaveData = AccessTools.MethodDelegate<Func<string, string>>(AccessTools.Method(typeof(Verse.GenFilePaths), "FolderUnderSaveData"));
    }
}
