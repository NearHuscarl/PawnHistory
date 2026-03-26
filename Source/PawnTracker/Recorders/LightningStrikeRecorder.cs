using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class LightningStrikeRecorder : RecorderBase
{
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

            var pawn = e.Pawn;
            var hediff = e.Hediff;
            var part = e.Part;
            var dinfo = e.Dinfo;

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
                    HandleLightningHitEvent(pawn, hediff, part);
                    break;
                }
            }
        });
    }

    private void HandleLightningHitEvent(Pawn pawn, Hediff hediff, BodyPartRecord part)
    {
        var recordDef = HistoryRecordDefOf.LightningStriked;
        var desc = recordDef.ResolveDescription(pawn)
            .AddRule("POSSESSIVE", pawn.Possessive())
            .AddRule("PART", part.Label.Colorize(hediff.LabelColor))
            .Resolve();

        AddRecord(recordDef, pawn, desc);
    }

    public override void Test(TestScenario scenario)
    {
        scenario.Pawn(10)
            .WithPosition(Find.CurrentMap.Center, 8)
            .ThatMatches(ShouldRecord)
            .Do(p => Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Find.CurrentMap, p.Position)))
            .Execute();
    }
}
