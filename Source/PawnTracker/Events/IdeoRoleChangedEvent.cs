using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Recorders;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record IdeoRoleChangedEvent(Pawn Pawn, string OldRoleLabel, string NewRoleLabel, List<Pawn> Spectators) : GameEventBase;

internal record IdeoRoleChangedState(Pawn Pawn, Precept_Role OldRole);

// TODO: handle the rest by following LetterLabelRoleLost when:
// - role is inactive (believer in ideo too low)
// - role is assigned to another pawn
[HarmonyPatch(typeof(RitualOutcomeEffectWorker_RoleChange), nameof(RitualOutcomeEffectWorker_RoleChange.Apply))]
internal static class RitualOutcomeEffectWorker_RoleChange_Apply_Patch
{
    private static void Prefix(LordJob_Ritual jobRitual, out IdeoRoleChangedState __state)
    {
        var pawn = jobRitual.PawnWithRole(RitualRoleId.RoleChanger);
        __state = new IdeoRoleChangedState(pawn, pawn.Ideo?.GetRole(pawn));
    }

    private static void Postfix(LordJob_Ritual jobRitual, IdeoRoleChangedState __state)
    {
        var pawn = __state.Pawn;
        var oldRole = __state.OldRole;
        var newRole = pawn.Ideo?.GetRole(pawn);
        var spectators = Accessor.RitualRoleAssignments.Spectators(jobRitual.assignments).ToList();

        GameEventBus.Publish(new IdeoRoleChangedEvent(pawn, oldRole?.LabelForPawn(pawn), newRole?.LabelForPawn(pawn), spectators));
    }
}
