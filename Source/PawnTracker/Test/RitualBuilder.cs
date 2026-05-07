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
        
            dialog.PostOpen(); // runs TryAssignSpectate()
            Accessor.Dialog_BeginRitual.Start(dialog);
            var ritual = organizer.GetLord().LordJob as LordJob_Ritual;

            var totalPresence = spectators.ToDictionary(p => p, _ => 0);
            speechEffectComp.Ritual.outcomeEffect.Apply(1f, totalPresence, ritual);
        });

        return this;
    }

    public RitualBuilder ConversionRitual(Pawn convertee)
    {
        processors.Add(() =>
        {
            var ritualAbility = organizer.Ideo.GetRole(organizer).AbilitiesFor(organizer).First(a => a.def == Extra.AbilityDefOf.ConversionRitual);
            var ritualEffectComp = ritualAbility.EffectComps.OfType<CompAbilityEffect_StartConversion>().First();
            var dialog = (Dialog_BeginRitual)ritualEffectComp.ConfirmationDialog((LocalTargetInfo)convertee, null);

            dialog.PostOpen();
            Accessor.Dialog_BeginRitual.Start(dialog);
            var ritual = organizer.GetLord().LordJob as LordJob_Ritual;
            var totalPresence = new[] { organizer, convertee }.Distinct().ToDictionary(p => p, _ => 0);
            ritualEffectComp.Ritual.outcomeEffect.Apply(1f, totalPresence, ritual);
        });

        return this;
    }

    public void Execute()
    {
        processors.ForEach(processor => processor());
    }
}
