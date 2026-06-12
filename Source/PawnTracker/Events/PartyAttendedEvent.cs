using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Events;

public enum PartyType
{
    Party,
    Concert,
}

public record PartyAttendedEvent(Pawn Organizer, List<Pawn> Partygoers, PartyType Type) : GameEventBase;

[HarmonyPatch(typeof(LordJob_Joinable_Party), nameof(LordJob_Joinable_Party.CreateGraph))]
public static class LordJob_Joinable_Party_CreateGraph_Patch
{
    public static void Postfix(LordJob_Joinable_Party __instance, StateGraph __result)
    {
        var timeoutTransition = __result.transitions.FirstOrDefault(IsSuccessfulPartyTransition);

        if (timeoutTransition == null)
        {
            L.Warning($"Cannot find timeout transition for {nameof(PartyAttendedEvent)}");
            return;
        }

        timeoutTransition.AddPreAction(new TransitionAction_Custom(() =>
        {
            var organizer = __instance.Organizer;
            var partyGoers = __instance.lord.ownedPawns.ToList();
            var type = __instance is LordJob_Joinable_Concert ? PartyType.Concert : PartyType.Party;

            GameEventBus.Publish(new PartyAttendedEvent(organizer, partyGoers, type));
        }));
    }

    private static bool IsSuccessfulPartyTransition(Transition transition)
    {
        return transition.target is LordToil_End
            && transition.sources.Any(source => source is LordToil_Party)
            && transition.triggers.Any(trigger => trigger is Trigger_TicksPassed);
    }
}
