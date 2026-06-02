using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Recorders;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Test;

public class RitualBuilder(Pawn organizer)
{
    private readonly List<Action> processors = [];

    public RitualBuilder Outcome(RitualOutcomePossibility outcome)
    {
        TestManager.Scenario.ForcedRitualOutcome = outcome;
        return this;
    }

    public RitualBuilder ThroneSpeech(List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var speech = organizer.abilities.GetAbility(AbilityDefOf.Speech, includeTemporary: true);
            var speechEffectComp = speech.EffectComps.OfType<CompAbilityEffect_StartRitual>().First();
            var dialog = (Dialog_BeginRitual)speechEffectComp.ConfirmationDialog((LocalTargetInfo)organizer, null);
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Speaker] = organizer,
            };
            InitRitualDialog(dialog, assignedRoles, spectators);
            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, ..spectators], speechEffectComp.Ritual);
        });

        return this;
    }

    public RitualBuilder AnimaTreeLinking(Thing animaTree, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var ritual = (Precept_Ritual)organizer.Ideo.GetPrecept(PreceptDefOf.AnimaTreeLinking);
            var dialog = (Dialog_BeginRitual)ritual.GetRitualBeginWindow(animaTree, null, null, null, null, organizer);
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Organizer] = organizer,
            };
            InitRitualDialog(dialog, assignedRoles, spectators);
            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, ..spectators], ritual);
        });

        return this;
    }
    
    private static void InitRitualDialog(Dialog_BeginRitual dialog, Dictionary<string, Pawn> assignedRoles, List<Pawn> spectators)
    {
        dialog.PostOpen(); // runs TryAssignSpectate()
        ReassignRoles(dialog, assignedRoles);
        ReassignSpectators(dialog, spectators);
    }

    public RitualBuilder ChildBirth(Pawn carrier, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var ritual = (Precept_Ritual)carrier.Ideo.GetPrecept(PreceptDefOf.ChildBirth);
            var birthBed = carrier.CurrentBed() ?? RestUtility.FindPatientBedFor(carrier);
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Mother] = carrier,
                [RitualRoleId.Doctor] = organizer,
            };

            ritual.ShowRitualBeginWindow(birthBed, null, carrier);
            var dialog = Find.WindowStack.WindowOfType<Dialog_BeginRitual>();

            ReassignRoles(dialog, assignedRoles);
            ReassignSpectators(dialog, spectators);
            Accessor.Dialog_BeginRitual.Start(dialog);
            
            carrier.health.RemoveHediff(carrier.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PregnancyLabor)); // add PregnancyLaborPushing in PreRemoved 
            carrier.health.RemoveHediff(carrier.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PregnancyLaborPushing)); // run ApplyOutcome(1f) in PreRemoved
        });

        return this;
    }

    public RitualBuilder ConversionRitual(Pawn convertee, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Moralist] = organizer,
                [RitualRoleId.Convertee] = convertee,
            };
            var (ritual, dialog) = CreateRitualDialogFromIdeogram(Extra.PreceptDefOf.Conversion, assignedRoles, spectators);

            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, convertee, ..spectators], ritual);
        });

        return this;
    }

    public RitualBuilder Execution(Pawn prisoner, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Executioner] = organizer,
                [RitualRoleId.Prisoner] = prisoner,
            };
            var (ritual, dialog) = CreateRitualDialogFromIdeogram(Extra.PreceptDefOf.Execution, assignedRoles, spectators);

            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, ..spectators], ritual);
        });

        return this;
    }

    public void Execute()
    {
        processors.ForEach(processor => processor());
    }

    private (Precept_Ritual Ritual, Dialog_BeginRitual Dialog) CreateRitualDialogFromIdeogram(PreceptDef ritualDef, Dictionary<string, Pawn> assignedRoles, List<Pawn> spectators)
    {
        var ritual = organizer.Ideo.GetAllPreceptsOfType<Precept_Ritual>().First(p => p.def == ritualDef);
        var ritualFocus = organizer.MapHeld?.listerThings.ThingsOfDef(ThingDefOf.Ideogram).FirstOrDefault(thing => ritual.ShouldShowGizmo(thing))
                          ?? throw new InvalidOperationException($"Failed to find an ideogram for {ritual.Label}.");
        ritual.ShowRitualBeginWindow(ritualFocus);
        var dialog = Find.WindowStack.WindowOfType<Dialog_BeginRitual>();

        ReassignRoles(dialog, assignedRoles);
        ReassignSpectators(dialog, spectators);

        return (ritual, dialog);
    }

    private static void ReassignRoles(Dialog_BeginRitual dialog, Dictionary<string, Pawn> assignedRoles)
    {
        var assignments = Accessor.Dialog_BeginRitual.Assignments(dialog);

        // reset the assigned roles happened in dialog.PostOpen() > FillPawns(). Roles can only be reassigned again after it was unassigned first. 
        foreach (var roleId in assignedRoles.Keys)
        {
            foreach (var p in assignments.AssignedPawns(roleId).ToList())
                assignments.TryUnassignAnyRole(p);
        }

        // Use our roles instead. Note that ritual.ShowRitualBeginWindow(.., forcedForRole: assignedRoles); does not work in manual test. 
        foreach (var (roleId, pawn) in assignedRoles)
        {
            var role = assignments.GetRole(roleId);

            if (!assignments.TryAssign(pawn, role, out _, default))
                throw new InvalidOperationException($"Failed to assign {pawn} to ritual role '{role}'.");
        }
    }

    private static void ReassignSpectators(Dialog_BeginRitual dialog, IEnumerable<Pawn> spectators)
    {
        var assignments = Accessor.Dialog_BeginRitual.Assignments(dialog);

        foreach (var pawn in assignments.SpectatorsForReading.ToList())
            assignments.RemoveParticipant(pawn);

        foreach (var spectator in spectators.Distinct())
        {
            if (!assignments.TryAssignSpectate(spectator))
                throw new InvalidOperationException($"Failed to assign {spectator} as a ritual spectator.");
        }
    }

    private void ApplyOutcome(IEnumerable<Pawn> attendees, Precept_Ritual ritual)
    {
        var lord = organizer.GetLord();
        var ritualLordJob = lord?.LordJob as LordJob_Ritual;
        var totalPresence = attendees.Distinct().ToDictionary(p => p, _ => 0);

        ritual.outcomeEffect.Apply(1f, totalPresence, ritualLordJob);
    }
}
