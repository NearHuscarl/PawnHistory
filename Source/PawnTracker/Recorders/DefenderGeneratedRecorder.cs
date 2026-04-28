using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class DefenderGeneratedRecorder : RecorderBase<DefenderGeneratedRecorder.Input>
{
    public enum OriginKind
    {
        Other,
        Settlement,
        Site,
    }

    public record Input(List<Pawn> Pawns, WorldObject WorldObject, Quest Quest, OriginKind OriginKind);

    public override void Register()
    {
        GameEventBus.Subscribe<DefenderGeneratedEvent>(e =>
        {
            var kind = e.WorldObject switch
            {
                Settlement => OriginKind.Settlement,
                Site => OriginKind.Site,
                _ => OriginKind.Other,
            };

            if (kind != OriginKind.Other)
                CreateRecord(new Input(e.Pawns, e.WorldObject, e.Quest, kind));
        });
    }

    public override void CreateRecord(Input input)
    {
        var pawns = input.Pawns;
        var recordDef = HistoryRecordDefOf.DefenderGenerated;

        foreach (var pawn in input.Pawns)
        {
            if (!ShouldRecord(pawn))
                continue;
            
            var desc = recordDef.Description(pawn)
                .WithOthers(pawns)
                .AddRule("Faction", input.WorldObject.Faction)
                .AddRule("WorldObject", input.WorldObject.ColoredLabel, addSubsymbols: true)
                .AddConstant("origin", input.OriginKind)
                .Resolve();

            AddRecord(recordDef, pawn, desc, quest: input.Quest);
        }
    }

    public void TestSite(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.OpportunitySite_BanditCamp).Execute();
        var site = QuestHelper.GetWorldObject<Site>(quest);
        var pawns = scenario.Pawn(3).Colonist().Execute();

        Expect.Assertions(1);

        scenario.Caravan(pawns)
            .VisitSite(site)
            .OnMapGenerated(e =>
            {
                var enemies = e.Map.mapPawns.AllPawnsSpawned.Where(pawn => pawn.HostileTo(Faction.OfPlayer)).ToList();

                Expect.ThatAll(enemies).ToHaveHistoryRecord(new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.DefenderGenerated,
                    Description = "[PAWN] and [n] others from [Faction] were stationed at the [WorldObject] as defenders.",
                    Quest = quest,
                });
            })
            .Execute();
    }

    public void TestSettlement(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.OpportunitySite_BanditCamp).Execute();
        var settlement = Find.WorldObjects.Settlements.FirstOrDefault(s => s.Faction.HostileTo(Faction.OfPlayer));
        var pawns = scenario.Pawn(3).Colonist().Execute();

        Expect.Assertions(2);

        scenario.Caravan(pawns)
            .Attack(settlement)
            .OnMapGenerated(e =>
            {
                var enemies = e.Map.mapPawns.AllPawnsSpawned.Where(pawn => pawn.HostileTo(Faction.OfPlayer)).ToList();

                Expect.ThatAll(enemies).ToHaveHistoryRecord(HistoryRecordDefOf.DefenderGenerated, "[PAWN] and [n] others from [Faction] were stationed at [WorldObject] as defenders.");
                Expect.ThatAll(enemies).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.DefenderGenerated,
                    Quest = quest,
                });
            })
            .Execute();
    }
}
