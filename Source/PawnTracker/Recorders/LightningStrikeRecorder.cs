using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class LightningStrikeRecorder : RecorderBase<LightningStrikeRecorder.Input>
{
    public record Input(Pawn Pawn, Hediff Hediff, BodyPartRecord Part);

    private const int LightningWindowTicks = 3;

    private static readonly List<(Map map, IntVec3 loc, int tick, float radius)> Strikes = [];

    public override void Register()
    {
        GameEventBus.Subscribe<LightningStrikedEvent>(e =>
        {
            Strikes.Add((e.Map, e.StrikeLoc, Find.TickManager.TicksGame, e.Radius));
        });
        GameEventBus.Subscribe<HediffAddedEvent>(e =>
        {
            Strikes.RemoveAll(s => Find.TickManager.TicksGame - s.tick > 10);

            var (pawn, hediff, part, dinfo) = e;

            if (!ShouldRecord(pawn))
                return;

            // not a lightning strike
            if (dinfo?.Def != DamageDefOf.Flame)
                return;

            var hurtTick = Find.TickManager.TicksGame;

            foreach (var (map, loc, lightningTick, radius) in Strikes)
            {
                if (map != pawn.Map)
                    continue;

                if (hurtTick - lightningTick > LightningWindowTicks)
                    continue;

                if (pawn.Position.DistanceTo(loc) <= radius)
                {
                    CreateRecord(new Input(pawn, hediff, part));
                    break;
                }
            }
        });
    }

    public override void CreateRecord(Input input)
    {
        var (pawn, hediff, part) = input;
        var recordDef = HistoryRecordDefOf.LightningStrike;
        var desc = recordDef.Description(pawn)
            .AddRule("POSSESSIVE", pawn.Possessive())
            .AddRule("PART", part.Label.Colorize(hediff.LabelColor))
            .Format();

        AddRecord(recordDef, pawn, desc);
    }

    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(3)
            .ThatMatches(ShouldRecord)
            .Do(p => Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Find.CurrentMap, p.Position)))
            .Execute();

        Expect.ThatAny(pawns).Eventually().ToHaveHistoryRecord(HistoryRecordDefOf.LightningStrike, "[PAWN] was struck by lightning, burning [POSSESSIVE] [PART].");
    }
}
