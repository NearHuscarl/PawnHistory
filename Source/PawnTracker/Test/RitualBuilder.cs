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

    public void Execute()
    {
        processors.ForEach(processor => processor());
    }

    public RitualBuilder Outcome(RitualOutcomePossibility outcome)
    {
        TestManager.Scenario.ForcedRitualOutcome = outcome;
        return this;
    }

    public RitualBuilder ThroneSpeech(List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var (ritual, dialog) = CreateRitualDialogFrom(AbilityDefOf.Speech);
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Speaker] = organizer,
            };
            InitRitualDialog(dialog, assignedRoles, spectators);
            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, ..spectators], ritual);
        });

        return this;
    }

    public RitualBuilder LeaderSpeech(List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var (ritual, dialog) = CreateRitualDialogFrom(Extra.AbilityDefOf.LeaderSpeech);
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Speaker] = organizer,
            };
            InitRitualDialog(dialog, assignedRoles, spectators);
            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, ..spectators], ritual);
        });

        return this;
    }

    private (Precept_Ritual Ritual, Dialog_BeginRitual Dialog) CreateRitualDialogFrom(AbilityDef abilityDef)
    {
        var speech = organizer.abilities.GetAbility(abilityDef, includeTemporary: true);
        var speechEffectComp = speech.EffectComps.OfType<CompAbilityEffect_StartRitual>().First();
        var dialog = (Dialog_BeginRitual)speechEffectComp.ConfirmationDialog((LocalTargetInfo)organizer, null);

        return (speechEffectComp.Ritual, dialog);
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

    public RitualBuilder Funeral(Pawn deceased, List<Pawn> spectators, bool noCorpse = false)
    {
        processors.Add(() =>
        {
            var ritual = (Precept_Ritual)organizer.Ideo.GetPrecept(noCorpse ? PreceptDefOf.FuneralNoCorpse : PreceptDefOf.Funeral);
            var obligation = ritual.activeObligations.First(o => o.targetA.Thing == deceased);
            var graves = organizer.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Grave).OfType<Building_Grave>();
            var grave = noCorpse ? graves.First(g => g.Corpse == null) : graves.First(g => g.Corpse?.InnerPawn == deceased);
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Moralist] = organizer,
            };
            var dialog = (Dialog_BeginRitual)ritual.GetRitualBeginWindow(grave, obligation, null, null, null, organizer);
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
            
            foreach (var letter in Find.LetterStack.LettersListForReading.OfType<ChoiceLetter_BabyBirth>().ToList())
                Find.LetterStack.RemoveLetter(letter);
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
            var (ritual, dialog) = CreateRitualDialogFrom(ThingDefOf.Ideogram, Extra.PreceptDefOf.Conversion, assignedRoles, spectators);

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
            var (ritual, dialog) = CreateRitualDialogFrom(ThingDefOf.Ideogram, Extra.PreceptDefOf.Execution, assignedRoles, spectators);

            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, ..spectators], ritual);
        });

        return this;
    }

    public RitualBuilder Festival(List<Pawn> joiners)
    {
        processors.Add(() =>
        {
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Leader] = organizer,
            };
            var (ritual, dialog) = CreateRitualDialogFrom(ThingDefOf.PartySpot, Extra.PreceptDefOf.Festival, assignedRoles, joiners);

            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, ..joiners], ritual);
        });

        return this;
    }

    public RitualBuilder DrumParty(List<Pawn> joiners)
    {
        processors.Add(() =>
        {
            List<Pawn> participants = [organizer, ..joiners];
            var (ritual, dialog) = CreateRitualDialogFrom(ThingDefOf.RitualSpot, Extra.PreceptDefOf.Classic_DrumParty, [] /* <roles Inherit="False"/> */, participants);

            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome(participants, ritual);
        });

        return this;
    }

    public RitualBuilder DanceParty(List<Pawn> joiners)
    {
        processors.Add(() =>
        {
            List<Pawn> participants = [organizer, ..joiners];
            var (ritual, dialog) = CreateRitualDialogFrom(ThingDefOf.RitualSpot, Extra.PreceptDefOf.Classic_DanceParty, [], participants);

            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome(participants, ritual);
        });

        return this;
    }

    public RitualBuilder SkyLanternFestival(List<Pawn> joiners)
    {
        processors.Add(() =>
        {
            List<Pawn> participants = [organizer, ..joiners];
            var (ritual, dialog) = CreateRitualDialogFrom(ThingDefOf.RitualSpot, Extra.RitualPatternDefOf.CelebrationSkyLanterns, [], participants);

            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome(participants, ritual);
        });

        return this;
    }

    private (Precept_Ritual Ritual, Dialog_BeginRitual Dialog) CreateRitualDialogFrom(ThingDef thingDef, PreceptDef ritualDef, Dictionary<string, Pawn> assignedRoles, List<Pawn> spectators)
    {
        var ritual = organizer.Ideo.GetAllPreceptsOfType<Precept_Ritual>().First(p => p.def == ritualDef);
        return CreateRitualDialogFrom(thingDef, ritual, assignedRoles, spectators);
    }

    private (Precept_Ritual Ritual, Dialog_BeginRitual Dialog) CreateRitualDialogFrom(ThingDef thingDef, RitualPatternDef ritualPatternDef, Dictionary<string, Pawn> assignedRoles, List<Pawn> spectators)
    {
        var ritual = organizer.Ideo.GetAllPreceptsOfType<Precept_Ritual>().First(p => p.sourcePattern == ritualPatternDef);
        return CreateRitualDialogFrom(thingDef, ritual, assignedRoles, spectators);
    }

    private (Precept_Ritual Ritual, Dialog_BeginRitual Dialog) CreateRitualDialogFrom(ThingDef thingDef, Precept_Ritual ritual, Dictionary<string, Pawn> assignedRoles, List<Pawn> spectators)
    {
        var ritualFocus = organizer.MapHeld.listerThings.ThingsOfDef(thingDef).First(thing => ritual.ShouldShowGizmo(thing));
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
