using LudeonTK;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class CasketDropRecorder : RecorderBase<CasketDropEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<CasketDropEvent>(CreateRecord);
    }

    public override void CreateRecord(CasketDropEvent e)
    {
        var (pawn, casket, reason, opener) = e;

        if (!ShouldRecord(pawn))
            return;

        var isAwakened = !pawn.GetHistoryRecords().Any();
        var recordDef = isAwakened ? HistoryRecordDefOf.CasketAwakened : HistoryRecordDefOf.CasketDrop;
        var desc = recordDef.Description(pawn)
            .AddRule("Faction", pawn.Faction)
            .AddRule("Opener", opener)
            .AddRule("Casket", casket.def, addSubsymbols: true)
            .AddConstant("reason", reason)
            .AddConstant("isCorpse", pawn.Dead) // is removed from a corpse container (e.g. grave/sarcophagus)
            .AddConstant("hasOpener", opener != null)
            .AddConstant("isAwakened", isAwakened)
            .Resolve();
        AddRecord(recordDef, pawn, desc, [casket, opener]);
    }

    public void LogAllCasketClasses()
    {
        var baseType = typeof(Building_Casket);
        var types = GenTypes.AllSubclassesNonAbstract(baseType);

        DebugTables.MakeTablesDialog(types,
            new TableDataGetter<Type>("Class Name", t => t.Name),
            new TableDataGetter<Type>("Defs", t => DefDatabase<ThingDef>.AllDefs
                .Where(d => d.thingClass == t)
                .Select(d => d.defName)
                .ToList()
                .JoinToString()
            )
        );
    }

    [SkipTest]
    public void TestEjected(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6)
            .WithCasket(ThingDefOf.AncientCryptosleepCasket)
            .Execute();

        var casket = Find.CurrentMap.listerBuildings
            .AllBuildingsColonistOfClass<Building_CryptosleepCasket>()
            .FirstOrDefault();

        casket.EjectContents();
    }

    [SkipTest]
    public void TestEjectedBy(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6)
            .WithCasket(ThingDefOf.AncientCryptosleepCasket)
            .Execute();

        var casket = Find.CurrentMap.listerBuildings
            .AllBuildingsColonistOfClass<Building_CryptosleepCasket>()
            .FirstOrDefault();

        scenario.Pawn().Colonist().StartJob(JobDefOf.Open, casket).Execute();
    }

    [SkipTest]
    public void TestRemovedBy(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6)
            .WithCasket(ThingDefOf.Sarcophagus, ThingDefOf.Plasteel)
            .Execute();

        var casket = Find.CurrentMap.listerBuildings
            .AllBuildingsColonistOfClass<Building_Sarcophagus>()
            .FirstOrDefault();

        scenario.Pawn().Colonist().StartJob(JobDefOf.Open, casket).Execute();
    }

    [SkipTest]
    public void TestDestroyed(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6)
            .WithCasket(ThingDefOf.AncientCryptosleepCasket)
            .Execute();

        var casket = Find.CurrentMap.listerBuildings
            .AllBuildingsColonistOfClass<Building_CryptosleepCasket>()
            .FirstOrDefault();

        casket.Destroy(DestroyMode.KillFinalize);
    }
}
