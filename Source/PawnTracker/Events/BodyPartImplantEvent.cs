using HarmonyLib;
using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Linq;
using System.Numerics;
using Verse;
using static Verse.DamageWorker;

namespace PawnHistory.Source.PawnTracker.Events;

internal class BodyPartImplantEvent(Pawn patient, Pawn doctor, Hediff hediff, Hediff replacedHediff) : GameEventBase
{
    public Pawn Patient { get; } = patient;
    public Pawn Doctor { get; } = doctor;
    public Hediff Hediff { get; } = hediff;
    public Hediff ReplacedHediff { get; } = replacedHediff;
}

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), [typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageResult)])]
internal class Pawn_HealthTracker_AddHediff_Patch_3
{
    // Prefix is required to get the replacedHediff before it is removed during the AddHediff call
    static void Prefix(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageResult result)
    {
        var pawn = Pawn_HealthTracker_AddHediff_Patch.PawnRef(__instance);

        if (hediff is Hediff_AddedPart)
        {
            var replacedHediff = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.Part == part && h.IsImplant());
            var doctor = pawn.GetOperatingDoctor();

            GameEventBus.Publish(new BodyPartImplantEvent(pawn, doctor, hediff, replacedHediff));
        }
    }
}
