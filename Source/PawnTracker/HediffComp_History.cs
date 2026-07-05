using Verse;

namespace PawnHistory.Source.PawnTracker;

public class HediffComp_History : HediffComp
{
    public Thing instigator;

    public static void InjectComp()
    {
        foreach (var def in DefDatabase<HediffDef>.AllDefs)
        {
            if (def.HasComp(typeof(HediffComp_History)))
                continue;
            if (!def.HasComp(typeof(HediffComp_GetsPermanent)) && !def.HasComp(typeof(HediffComp_Infecter)))
                continue;

            def.comps ??= [];
            def.comps.Add(new HediffCompProperties { compClass = typeof(HediffComp_History) });
        }
    }

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        instigator = dinfo?.Instigator; // hediff.combatLogEntry.initiatorPawn is a weak reference and might be purged, so it's not reliable.
    }

    public override void CompExposeData()
    {
        Scribe_References.Look(ref instigator, "PH_instigator");
    }
}