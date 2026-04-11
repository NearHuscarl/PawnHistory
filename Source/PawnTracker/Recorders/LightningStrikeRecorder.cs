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

    private static readonly List<(Map map, IntVec3 loc, int tick, float radius)> strikes = [];

    public override void Register()
    {
        GameEventBus.Subscribe<LightningStrikedEvent>(e =>
        {
            strikes.Add((e.Map, e.StrikeLoc, Find.TickManager.TicksGame, e.Radius));
        });
        GameEventBus.Subscribe<HediffAddedEvent>(e =>
        {
            strikes.RemoveAll(s => Find.TickManager.TicksGame - s.tick > 10);

            var (pawn, hediff, part, dinfo) = e;

            if (!ShouldRecord(pawn))
                return;

            // not a lightning strike
            if (dinfo?.Def != DamageDefOf.Flame)
                return;

            int hurtTick = Find.TickManager.TicksGame;

            foreach (var (map, loc, lightningTick, radius) in strikes)
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

    public override void CreateRecord(Input Input)
    {
        var (pawn, hediff, part) = Input;
        var recordDef = HistoryRecordDefOf.LightningStrike;
        var desc = recordDef.Description(pawn)
            .AddRule("POSSESSIVE", pawn.Possessive())
            .AddRule("PART", part.Label.Colorize(hediff.LabelColor))
            .Format();

        AddRecord(recordDef, pawn, desc);
    }

    public void Test(TestScenario scenario)
    {
        scenario.Pawn(10)
            .WithPosition(Find.CurrentMap.Center, 8)
            .ThatMatches(ShouldRecord)
            .Do(p => Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Find.CurrentMap, p.Position)))
            .Execute();

        Expect.AnyPawnOnMap().Eventually().ToHaveHistoryRecord("[PAWN] was struck by lightning, burning [POSSESSIVE] [PART].");
    }
}
