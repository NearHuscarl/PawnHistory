using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SiteAmbushRecorder : RecorderBase<SiteAmbushRecorder.Input>
{
    public record Input(IEnumerable<Pawn> Pawns, WorldObject WorldObject);

    public override void Register()
    {
        GameEventBus.Subscribe<ReceiveLetterEvent>(e =>
        {
            if (!e.Text.Resolve().MatchesTranslationTemplate("LetterAmbushInExistingMap", exactMatch: true))
                return;

            var pawns = e.Pawns.ToList();
            var worldObject = pawns.Select(p => p.MapHeld?.Parent).OfType<WorldObject>().FirstOrDefault();

            CreateRecord(new Input(pawns, worldObject));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (_, worldObject) = input;
        var pawns = input.Pawns.Where(ShouldRecord).ToList();
        var recordDef = HistoryRecordDefOf.SiteAmbush;
        
        foreach (var pawn in pawns)
        {
            var desc = recordDef.Description(pawn)
                .AddRule("Colony", Faction.OfPlayer.def.pawnsPlural)
                .AddRule("Faction", worldObject.Faction)
                .AddRule("HostileFaction", pawn.Faction)
                .AddRule("WorldObject", worldObject.ColoredLabel, addSubsymbols: true)
                .WithOthers(pawns)
                .AddConstant("locationType", worldObject.GetType().Name)
                .Resolve();

            AddRecord(recordDef, pawn, desc);
        }
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        scenario.Quest(DefLookup.QuestScript.OpportunitySite_DownedRefugee).Execute();
        var site = Find.WorldObjects.AllWorldObjects.OfType<Site>().First();
        var pawns = scenario.Pawn(3).Colonist().Execute();
        const string ambushSignal = "PH_Ambush";
        Expect.Assertions(1);

        scenario.Caravan(pawns)
            .VisitSite(site)
            .OnMapGenerated(e =>
            {
                // AmbushHidden or AmbushEdge is generated randomly via QuestNode_GetSitePartDefsByTagsAndFaction
                // there are better things to do in life than mocking this piece of shit of a quest system.
                var action = scenario.Thing(ThingDefOf.SignalAction_Ambush).Map(e.Map).CreateSingle<SignalAction_Ambush>();
                action.signalTag = ambushSignal;
                action.points = 200f;
                action.Notify_SignalReceived(new Signal(action.signalTag));
                var enemies = e.Map.mapPawns.AllPawnsSpawned.Where(p => p.Faction.HostileTo(Faction.OfPlayer));

                Expect.ThatAll(enemies)
                    .Eventually()
                    .ToHaveHistoryRecord("[PAWN] and [n] others from [Faction] ambushed the colonists at the downed refugee.", HistoryRecordDefOf.SiteAmbush);
            })
            .Execute();
    }
}
