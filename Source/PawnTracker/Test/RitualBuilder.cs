using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Test;

public class RitualBuilder
{
    private readonly List<Action> processors = [];
    private readonly Pawn organizer;
    
    public RitualBuilder(Pawn organizer)
    {
        this.organizer = organizer;
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
            var speech = organizer.abilities.GetAbility(AbilityDefOf.Speech, includeTemporary: true);
            var speechEffectComp = speech.EffectComps.OfType<CompAbilityEffect_StartRitual>().First();
            var dialog = (Dialog_BeginRitual)speechEffectComp.ConfirmationDialog((LocalTargetInfo)organizer, null);

            StartAndApplyOutcome(dialog, [organizer, ..spectators], speechEffectComp.Ritual);
        });

        return this;
    }

    public RitualBuilder ConversionRitual(Pawn convertee)
    {
        processors.Add(() =>
        {
            var forcedRoles = new Dictionary<string, Pawn>
            {
                ["moralist"] = organizer,
                ["convertee"] = convertee,
            };
            var (ritual, dialog) = CreateRitualDialogFromIdeogram(Extra.PreceptDefOf.Conversion, forcedRoles);

            StartAndApplyOutcome(dialog, [organizer, convertee], ritual);
        });

        return this;
    }

    public RitualBuilder Execution(Pawn prisoner, List<Pawn> spectators)
    {
        processors.Add(() =>
        {
            var forcedRoles = new Dictionary<string, Pawn>
            {
                ["executioner"] = organizer,
                ["prisoner"] = prisoner,
            };
            var (ritual, dialog) = CreateRitualDialogFromIdeogram(Extra.PreceptDefOf.Execution, forcedRoles);

            StartAndApplyOutcome(dialog, [organizer, ..spectators], ritual);
        });

        return this;
    }

    public void Execute()
    {
        processors.ForEach(processor => processor());
    }

    private (Precept_Ritual Ritual, Dialog_BeginRitual Dialog) CreateRitualDialogFromIdeogram(PreceptDef ritualDef, Dictionary<string, Pawn> forcedRoles)
    {
        var ritual = organizer.Ideo.GetPrecept(ritualDef) as Precept_Ritual
            ?? throw new InvalidOperationException($"Failed to find ritual precept {ritualDef.defName} on {organizer.Ideo}.");
        var ritualFocus = organizer.MapHeld?.listerThings.ThingsOfDef(ThingDefOf.Ideogram)
            .FirstOrDefault(thing => ritual.ShouldShowGizmo(thing))
            ?? throw new InvalidOperationException($"Failed to find an ideogram for {ritual.Label}.");
        ritual.ShowRitualBeginWindow(ritualFocus, forcedForRole: forcedRoles);
        var dialog = Find.WindowStack.WindowOfType<Dialog_BeginRitual>();

        return (ritual, dialog);
    }

    private void StartAndApplyOutcome(Dialog_BeginRitual dialog, IEnumerable<Pawn> attendees, Precept_Ritual ritual)
    {
        if (dialog == null)
            throw new InvalidOperationException("Failed to start ritual because no ritual dialog was created.");

        dialog.PostOpen(); // runs TryAssignSpectate()
        Accessor.Dialog_BeginRitual.Start(dialog);

        var lord = organizer.GetLord();
        var ritualLordJob = lord?.LordJob as LordJob_Ritual;
        var totalPresence = attendees.Distinct().ToDictionary(p => p, _ => 0);

        ritual.outcomeEffect.Apply(1f, totalPresence, ritualLordJob);
    }
}
