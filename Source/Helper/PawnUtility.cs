using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.Helper;

internal static class PawnUtility
{
    // GrammarResolverSimple.cs -> "nameDef"
    // TODO: test with shambler, shambler past colonist..
    public static string NameDef(this Pawn pawn) => pawn.Name != null
        ? Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, name: true).ApplyTag(TagType.Name).Resolve()
        : pawn.KindLabelDefinite();

    public static Pawn GetOperatingDoctor(this Pawn patient)
    {
        var comp = patient.CurrentBed()?.GetComp<CompAssignableToPawn_Bed>();
        var medicalBill = patient.BillStack.Bills.OfType<Bill_Medical>().FirstOrDefault();

        if (medicalBill == null) return null;

        return patient.Map.mapPawns.AllPawnsSpawned
            .FirstOrDefault(p => p.CurJob?.def == JobDefOf.DoBill && p.CurJob.bill == medicalBill);
    }

    public static void StartMentalBreakWithMadeupThought(this Pawn pawn, MentalBreakDef def)
    {
        var randomNegativeThought = DefDatabase<ThoughtDef>.AllDefs
            .Where(t => t.stages != null && t.stages.Any(s => s != null && s.baseMoodEffect < 0) && (!t.label.NullOrEmpty() || !t.stages.First().label.NullOrEmpty()))
            .RandomElementWithFallback();
        var reason = "MentalStateReason_Mood".Translate() + "\n\n" + "FinalStraw".Translate((NamedArgument)randomNegativeThought.LabelCap);

        if (!pawn.mindState.mentalBreaker.TryDoMentalBreak(reason, def))
            Log.Warning($"[PawnHistory] Failed to force mental break {def.defName} on {pawn.LabelShort}");
    }

    private static float GetDangerScore(Hediff h)
    {
        if (h.def.lethalSeverity <= 0f)
            return h.Severity / 1f / 3; // not lethal

        return h.Severity / h.def.lethalSeverity;
    }

    public static Hediff GetMostDangerousHediff(this Pawn pawn, BodyPartRecord part)
    {
        return pawn.health.hediffSet.hediffs.Where(h => h.Visible && h.Part == part && h.def.isBad).OrderByDescending(GetDangerScore).FirstOrDefault();
    }

    /// <summary>
    /// Copied from HealthCardUtility.DrawHediffListing()
    /// </summary>
    /// <param name="pawn"></param>
    /// <returns></returns>
    public static string GetBloodlossText(this Pawn pawn)
    {
        var bloodLoss = HealthUtility.TicksUntilDeathDueToBloodLoss(pawn);

        if (!ModsConfig.BiotechActive || pawn.genes == null || !pawn.genes.HasActiveGene(GeneDefOf.Deathless))
        {
            if (bloodLoss >= 60000)
                return "(" + "WontBleedOutSoon".Translate() + ")";
            return "(" + "TimeToDeath".Translate((NamedArgument)bloodLoss.ToStringTicksToPeriod()) + ")";
        }

        return "(" + "Deathless".Translate() + ")";
    }
}
