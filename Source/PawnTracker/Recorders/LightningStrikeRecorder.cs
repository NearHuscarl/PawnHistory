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
        GameEventListener.Subscribe<LightningStrikeEvent>(e =>
        {
            strikes.Add((e.Map, e.StrikeLoc, Find.TickManager.TicksGame, e.Radius));
        });
        GameEventListener.Subscribe<HediffPostAddEvent>(e =>
        {
            strikes.RemoveAll(s => Find.TickManager.TicksGame - s.tick > 10);

            var pawn = e.Pawn;
            var hediff = e.Hediff;
            var part = e.Part;
            var dinfo = e.Dinfo;

            if (!ShouldRecord(pawn))
                return;

            // handled by PermanentDamageRecorder
            if (hediff.IsPermanent() || hediff.def == HediffDefOf.MissingBodyPart || pawn.health.hediffSet.PartIsMissing(part))
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
        var eventDef = PawnEventDefOf.LightningStriked;
        var desc = eventDef.description.Formatted(
            pawn.NameShortColored.Named("PAWN"),
            pawn.Possessive().Named("POSSESSIVE"),
            part.Label.Colorize(hediff.LabelColor).Named("PART")
        ).Resolve();

        AddRecord(new HistoryRecord(eventDef, pawn, desc));
    }
}
