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

    public RitualBuilder Trial(Pawn convict, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var ritual = TrialRitualFor(convict);
            var forcedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Convict] = convict,
            };
            var dialog = (Dialog_BeginRitual)ritual.GetRitualBeginWindow(convict, null, null, null, forcedRoles, organizer);
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Leader] = organizer,
            };

            InitRitualDialog(dialog, assignedRoles, spectators);
            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, convict, ..spectators], ritual);
        });

        return this;
    }

    private Precept_Ritual TrialRitualFor(Pawn convict)
    {
        if (convict.InMentalState && organizer.Ideo.GetPrecept(Extra.PreceptDefOf.TrialMentalState) is Precept_Ritual mentalStateTrial)
            return mentalStateTrial;
        if (convict.IsPrisonerOfColony && organizer.Ideo.GetPrecept(Extra.PreceptDefOf.TrialPrisoner) is Precept_Ritual prisonerTrial)
            return prisonerTrial;

        return (Precept_Ritual)organizer.Ideo.GetPrecept(Extra.PreceptDefOf.Trial);
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

    public RitualBuilder TreeConnection(Thing gauranlenTree, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var ritual = (Precept_Ritual)organizer.Ideo.GetPrecept(Extra.PreceptDefOf.TreeConnection);
            var dialog = (Dialog_BeginRitual)ritual.GetRitualBeginWindow(gauranlenTree, null, null, null, null, organizer);
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Connector] = organizer,
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
        ReassignRoles(dialog, assignedRoles, spectators);
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

            ReassignRoles(dialog, assignedRoles, spectators);
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

    public RitualBuilder BlindingCeremony(Pawn target, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var ritual = organizer.Ideo.GetAllPreceptsOfType<Precept_Ritual>().First(p => p.def == Extra.PreceptDefOf.BlindingCeremony);
            var ritualFocus = organizer.MapHeld.listerThings.ThingsOfDef(ThingDefOf.Ideogram).First(thing => ritual.ShouldShowGizmo(thing));
            var dialog = (Dialog_BeginRitual)ritual.GetRitualBeginWindow(ritualFocus, null, null, organizer, null, organizer);
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Doer] = organizer,
                [RitualRoleId.Target] = target,
            };

            InitRitualDialog(dialog, assignedRoles, spectators);
            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, target, ..spectators], ritual);
        });

        return this;
    }

    public RitualBuilder ScarificationCeremony(Pawn target, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var ritual = organizer.Ideo.GetAllPreceptsOfType<Precept_Ritual>().First(p => p.def == Extra.PreceptDefOf.ScarificationCeremony);
            var ritualFocus = organizer.MapHeld.listerThings.ThingsOfDef(ThingDefOf.Ideogram).First(thing => ritual.ShouldShowGizmo(thing));
            var dialog = (Dialog_BeginRitual)ritual.GetRitualBeginWindow(ritualFocus, null, null, organizer, null, organizer);
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Doer] = organizer,
                [RitualRoleId.Target] = target,
            };

            InitRitualDialog(dialog, assignedRoles, spectators);
            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, target, ..spectators], ritual);
        });

        return this;
    }

    public RitualBuilder RoleChange(Precept_Role newRole, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.RoleChanger] = organizer,
            };
            var (ritual, dialog) = CreateRitualDialogFrom(ThingDefOf.Ideogram, PreceptDefOf.RoleChange, assignedRoles, spectators);

            Accessor.Dialog_BeginRitual.Assignments(dialog).SetRoleChangeSelection(newRole);
            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, ..spectators], ritual);
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

    public RitualBuilder GladiatorDuel(Pawn duelist1, Pawn duelist2, Pawn escort1, Pawn escort2, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var assignedRoles = new Dictionary<string, Pawn>
            {
                [RitualRoleId.Leader] = organizer,
                [RitualRoleId.Duelist1] = duelist1,
                [RitualRoleId.Duelist2] = duelist2,
                [RitualRoleId.Escort1] = escort1,
                [RitualRoleId.Escort2] = escort2,
            };
            var (ritual, dialog) = CreateRitualDialogFrom(ThingDefOf.RitualSpot, Extra.PreceptDefOf.GladiatorDuel, assignedRoles, spectators);

            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome([organizer, duelist1, duelist2, escort1, escort2, ..spectators], ritual);
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

    public RitualBuilder ChristmasTreeParty(List<Pawn> joiners)
    {
        processors.Add(() =>
        {
            List<Pawn> participants = [organizer, ..joiners];
            var (ritual, dialog) = CreateRitualDialogFrom(Extra.ThingDefOf.ChristmasTree, Extra.PreceptDefOf.DateRitualConsumable, [], participants);

            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome(participants, ritual);
        });

        return this;
    }

    public RitualBuilder BurnCircle(List<Pawn> joiners)
    {
        processors.Add(() =>
        {
            List<Pawn> participants = [organizer, ..joiners];
            var (ritual, dialog) = CreateRitualDialogFrom(Extra.ThingDefOf.Effigy, Extra.RitualPatternDefOf.BurnCircle, [], participants);

            Accessor.Dialog_BeginRitual.Start(dialog);
            ApplyOutcome(participants, ritual);
        });

        return this;
    }

    public RitualBuilder SmokeCircle(List<Pawn> joiners)
    {
        processors.Add(() =>
        {
            List<Pawn> participants = [organizer, ..joiners];
            var (ritual, dialog) = CreateRitualDialogFrom(Extra.ThingDefOf.Burnbong, Extra.RitualPatternDefOf.SmokeCircle, [], participants);

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

        ReassignRoles(dialog, assignedRoles, spectators);

        return (ritual, dialog);
    }

    private static void ReassignRoles(Dialog_BeginRitual dialog, Dictionary<string, Pawn> assignedRoles, IEnumerable<Pawn> spectators)
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

        // reassign spectators
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
