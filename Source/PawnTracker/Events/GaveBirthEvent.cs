using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

/// <summary>
/// </summary>
/// <param name="Baby"></param>
/// <param name="Carrier">Is null if IsVatBirth=true</param>
/// <param name="GeneticMother">Different to Carrier if it's surrogacy</param>
/// <param name="Father"></param>
/// <param name="OutcomeLabel"></param>
/// <param name="IsVatBirth"></param>
/// <param name="IsSurrogacy"></param>
/// <param name="IsInbred"></param>
public record GaveBirthEvent(
    Pawn Baby,
    Pawn Carrier,
    Pawn GeneticMother,
    Pawn Father,
    string OutcomeLabel,
    bool IsVatBirth,
    bool IsSurrogacy,
    bool IsInbred) : GameEventBase;

[HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.ApplyBirthOutcome))]
internal static class PregnancyUtility_ApplyBirthOutcome_Patch
{
    private static void Postfix(Thing __result, RitualOutcomePossibility outcome, Pawn geneticMother, Thing birtherThing, Pawn father)
    {
        var baby = ResolveBaby(__result);
        var isVatBirth = birtherThing is Building_GrowthVat;
        var carrier = birtherThing as Pawn;
        var isSurrogacy = carrier != null && carrier != geneticMother;

        GameEventBus.Publish(new GaveBirthEvent(
            baby,
            carrier,
            geneticMother,
            father,
            outcome.label,
            isVatBirth,
            isSurrogacy,
            baby.genes?.HasActiveGene(GeneDefOf.Inbred) == true));
    }

    private static Pawn ResolveBaby(Thing thing)
    {
        return thing switch
        {
            Pawn pawn => pawn,
            Corpse corpse => corpse.InnerPawn,
            _ => null,
        };
    }
}
