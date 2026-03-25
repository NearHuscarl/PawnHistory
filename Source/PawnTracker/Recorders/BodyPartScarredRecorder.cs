using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class BodyPartScarredRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<HediffAddedEvent>(e =>
        {
            var pawn = e.Pawn;
            var hediff = e.Hediff;
            var part = e.Part;
            var dinfo = e.Dinfo;

            if (!ShouldRecord(pawn))
                return;

            if (part == null)
                return;
            
            // scarred body part can be destroyed, which removes the scar after AddHediff(): PreAddHediff(Scar) > PreAddHediff(Missing) > PostAddHediff(Missing) > PostAddHediff(Scar)
            if (pawn.health.hediffSet.PartIsMissing(part))
                return;

            if (hediff.IsPermanent() && hediff.def != HediffDefOf.MissingBodyPart && dinfo.HasValue /* from combat rather than old wound */)
                HandleScarredPartEvent(pawn, hediff, part, dinfo);
        });
    }

    private void HandleScarredPartEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo)
    {
        var instigator = dinfo?.Instigator as Pawn;
        var weapon = dinfo?.Weapon?.race != null ? dinfo?.Tool?.label /* body part like fist/teeth */ : dinfo?.Weapon?.label;
        var recordDef = HistoryRecordDefOf.BodyPartScarred;
        var descBuilder = recordDef.Description("bodyPartScarred", pawn)
            .IncludePawnGrammar()
            .AddRule("PART", part.Label.Colorize(hediff.LabelColor))
            .AddRule("HEDIFF", hediff) // <permanentLabel>
            .AddRule("WEAPON", weapon)
            .AddConstantIf(weapon != null, "hasWeapon", "true");

        if (dinfo?.Instigator is Pawn)
        {
            descBuilder
                .AddRule("INSTIGATOR", instigator)
                .AddConstant("hasInstigator", "true");
        }

        AddRecord(recordDef, pawn, descBuilder.Resolve(), [instigator]);
    }
}
