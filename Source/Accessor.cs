using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RimWorld.Planet;
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

    public class CompLongRangeMineralScanner
    {
        public static readonly AccessTools.FieldRef<RimWorld.CompLongRangeMineralScanner, ThingDef> TargetMineable =
            AccessTools.FieldRefAccess<RimWorld.CompLongRangeMineralScanner, ThingDef>("targetMineable");

        public static readonly Action<RimWorld.CompLongRangeMineralScanner, Pawn> DoFind =
            AccessTools.MethodDelegate<Action<RimWorld.CompLongRangeMineralScanner, Pawn>>(AccessTools.Method(typeof(RimWorld.CompLongRangeMineralScanner), "DoFind"));
    }

    public class CompDeepScanner
    {
        public static readonly Action<RimWorld.CompDeepScanner, Pawn> DoFind =
            AccessTools.MethodDelegate<Action<RimWorld.CompDeepScanner, Pawn>>(AccessTools.Method(typeof(RimWorld.CompDeepScanner), "DoFind"));
    }

    public class CompGoldenCube
    {
        public static readonly Action<RimWorld.CompGoldenCube, Pawn> OnInteracted =
            AccessTools.MethodDelegate<Action<RimWorld.CompGoldenCube, Pawn>>(AccessTools.Method(typeof(RimWorld.CompGoldenCube), "OnInteracted"));
    }

    public class DeepResourceGrid
    {
        public static readonly AccessTools.FieldRef<Verse.DeepResourceGrid, Map> Map =
            AccessTools.FieldRefAccess<Verse.DeepResourceGrid, Map>("map");
    }

    public class JobDriver
    {
        public static readonly AccessTools.FieldRef<Verse.AI.JobDriver, Pawn> Pawn =
            AccessTools.FieldRefAccess<Verse.AI.JobDriver, Pawn>("pawn");
    }

    public class JobDriver_Execute
    {
        public static readonly Func<RimWorld.JobDriver_Execute, Pawn> Victim =
            AccessTools.MethodDelegate<Func<RimWorld.JobDriver_Execute, Pawn>>(AccessTools.Method(typeof(RimWorld.JobDriver_Execute), "get_Victim"));
    }

    public class JobDriver_ReleasePrisoner
    {
        public static readonly Func<Verse.AI.JobDriver_ReleasePrisoner, Pawn> Prisoner =
            AccessTools.MethodDelegate<Func<Verse.AI.JobDriver_ReleasePrisoner, Pawn>>(AccessTools.Method(typeof(Verse.AI.JobDriver_ReleasePrisoner), "get_Prisoner"));
    }

    public class JobDriver_PredatorHunt
    {
        public static readonly AccessTools.FieldRef<RimWorld.JobDriver_PredatorHunt, bool> NotifiedPlayerAttacking =
            AccessTools.FieldRefAccess<RimWorld.JobDriver_PredatorHunt, bool>("notifiedPlayerAttacking");
    }

    public class GameComponent_OnetimeNotification
    {
        public static readonly AccessTools.FieldRef<Verse.GameComponent_OnetimeNotification, bool> SendAICoreRequestReminder =
            AccessTools.FieldRefAccess<Verse.GameComponent_OnetimeNotification, bool>("sendAICoreRequestReminder");
    }

    public class VisitorGiftForPlayerUtility
    {
        public static readonly Func<List<Pawn>, Faction, Pawn> GetGiftGiver =
            AccessTools.MethodDelegate<Func<List<Pawn>, Faction, Pawn>>(AccessTools.Method(typeof(RimWorld.VisitorGiftForPlayerUtility), "GetGiftGiver"));
    }

    public class ChoiceLetter_AcceptVisitors
    {
        public static readonly Func<RimWorld.ChoiceLetter_AcceptVisitors, Verse.DiaOption> Option_Accept =
            AccessTools.MethodDelegate<Func<RimWorld.ChoiceLetter_AcceptVisitors, Verse.DiaOption>>(AccessTools.Method(typeof(RimWorld.ChoiceLetter_AcceptVisitors), "get_Option_Accept"));
    }

    public class QuestPart_PawnJoinOffer
    {
        public static readonly Action<RimWorld.QuestPart_PawnJoinOffer> SendLetter =
            AccessTools.MethodDelegate<Action<RimWorld.QuestPart_PawnJoinOffer>>(AccessTools.Method(typeof(RimWorld.QuestPart_PawnJoinOffer), "SendLetter"));
    }
    
    public class QuestPart_DropPods
    {
        public static readonly AccessTools.FieldRef<RimWorld.QuestPart_DropPods, List<Thing>> TmpThingsToDrop = AccessTools.FieldRefAccess<RimWorld.QuestPart_DropPods, List<Thing>>("tmpThingsToDrop");
    }

    public class QuestGen
    {
        public static readonly Action<QuestScriptDef, Slate> InitializeQuestGen =
            AccessTools.MethodDelegate<Action<QuestScriptDef, Slate>>(AccessTools.Method(typeof(RimWorld.QuestGen.QuestGen), "InitializeQuestGen"));

        public static readonly Action ClearQuestGenState =
            AccessTools.MethodDelegate<Action>(AccessTools.Method(typeof(RimWorld.QuestGen.QuestGen), "ClearQuestGenState"));
    }

    public class Dialog_BeginRitual
    {
        public static readonly Action<RimWorld.Dialog_BeginRitual> Start =
            AccessTools.MethodDelegate<Action<RimWorld.Dialog_BeginRitual>>(AccessTools.Method(typeof(RimWorld.Dialog_BeginRitual), "Start"));
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

    public class CaravanArrivalAction_AttackSettlement
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.CaravanArrivalAction_AttackSettlement, Settlement> Settlement =
            AccessTools.FieldRefAccess<RimWorld.Planet.CaravanArrivalAction_AttackSettlement, Settlement>("settlement");
    }

    public class CaravanArrivalAction_Enter
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.CaravanArrivalAction_Enter, MapParent> MapParent =
            AccessTools.FieldRefAccess<RimWorld.Planet.CaravanArrivalAction_Enter, MapParent>("mapParent");
    }

    public class CaravanArrivalAction_VisitEscapeShip
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.CaravanArrivalAction_VisitEscapeShip, MapParent> Target =
            AccessTools.FieldRefAccess<RimWorld.Planet.CaravanArrivalAction_VisitEscapeShip, MapParent>("target");
    }

    public class CaravanArrivalAction_VisitPeaceTalks
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.CaravanArrivalAction_VisitPeaceTalks, PeaceTalks> PeaceTalks =
            AccessTools.FieldRefAccess<RimWorld.Planet.CaravanArrivalAction_VisitPeaceTalks, PeaceTalks>("peaceTalks");
    }

    public class CaravanArrivalAction_VisitSite
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.CaravanArrivalAction_VisitSite, Site> Site =
            AccessTools.FieldRefAccess<RimWorld.Planet.CaravanArrivalAction_VisitSite, Site>("site");
    }

    public class CaravanArrivalAction_VisitSettlement
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.CaravanArrivalAction_VisitSettlement, Settlement> Settlement =
            AccessTools.FieldRefAccess<RimWorld.Planet.CaravanArrivalAction_VisitSettlement, Settlement>("settlement");
    }

    public class TransportersArrivalAction_VisitSite
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.TransportersArrivalAction_VisitSite, Site> Site =
            AccessTools.FieldRefAccess<RimWorld.Planet.TransportersArrivalAction_VisitSite, Site>("site");
    }

    public class TransportersArrivalAction_AttackSettlement
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.TransportersArrivalAction_AttackSettlement, Settlement> Settlement =
            AccessTools.FieldRefAccess<RimWorld.Planet.TransportersArrivalAction_AttackSettlement, Settlement>("settlement");
    }

    public class TransportersArrivalAction_GiveGift
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.TransportersArrivalAction_GiveGift, Settlement> Settlement =
            AccessTools.FieldRefAccess<RimWorld.Planet.TransportersArrivalAction_GiveGift, Settlement>("settlement");
    }

    public class TransportersArrivalAction_GiveToCaravan
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.TransportersArrivalAction_GiveToCaravan, Caravan> Caravan =
            AccessTools.FieldRefAccess<RimWorld.Planet.TransportersArrivalAction_GiveToCaravan, Caravan>("caravan");
    }

    public class TransportersArrivalAction_LandInSpecificCell
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.TransportersArrivalAction_LandInSpecificCell, MapParent> MapParent =
            AccessTools.FieldRefAccess<RimWorld.Planet.TransportersArrivalAction_LandInSpecificCell, MapParent>("mapParent");
    }

    public class TransportersArrivalAction_Trade
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.TransportersArrivalAction_Trade, Settlement> Settlement =
            AccessTools.FieldRefAccess<RimWorld.Planet.TransportersArrivalAction_Trade, Settlement>("settlement");
    }

    public class TransportersArrivalAction_VisitSettlement
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.TransportersArrivalAction_VisitSettlement, Settlement> Settlement =
            AccessTools.FieldRefAccess<RimWorld.Planet.TransportersArrivalAction_VisitSettlement, Settlement>("settlement");
    }

    public class TransportersArrivalAction_VisitSpace
    {
        public static readonly AccessTools.FieldRef<RimWorld.Planet.TransportersArrivalAction_VisitSpace, MapParent> Parent =
            AccessTools.FieldRefAccess<RimWorld.Planet.TransportersArrivalAction_VisitSpace, MapParent>("parent");
    }

    public class TransportersArrivalAction_TransportShip
    {
        public static readonly AccessTools.FieldRef<RimWorld.TransportersArrivalAction_TransportShip, MapParent> MapParent =
            AccessTools.FieldRefAccess<RimWorld.TransportersArrivalAction_TransportShip, MapParent>("mapParent");
    }
}
