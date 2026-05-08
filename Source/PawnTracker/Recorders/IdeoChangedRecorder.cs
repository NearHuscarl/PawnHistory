using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class IdeoChangedRecorder : RecorderBase<IdeoChangedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<IdeoChangedEvent>(CreateRecord);
    }

    public override void CreateRecord(IdeoChangedEvent e)
    {
        // handled by MentalBreakComp_IdeoChange
        if (e.Reason is IdeoChangeReason.MentalBreak)
            return;

        var recordDef = HistoryRecordDefOf.IdeoChanged;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Converter", e.Converter, addSubsymbols: true)
            .AddRule("OldIdeo", e.OldIdeo)
            .AddRule("NewIdeo", e.NewIdeo)
            .AddConstant("hasOldIdeo", e.OldIdeo != null)
            .AddConstant("hasNewIdeo", e.NewIdeo != null)
            .AddConstant("reason", e.Reason)
            .Resolve();

        if (ShouldRecord(e.Pawn))
            AddRecord(recordDef, e.Pawn, desc, [e.Converter]);

        if (ShouldRecord(e.Converter) && ShouldRecordConverter(e))
            AddRecord(recordDef, e.Converter, desc, [e.Pawn]);
    }

    private static bool ShouldRecordConverter(IdeoChangedEvent e) => e.Reason is IdeoChangeReason.ConvertAbility or IdeoChangeReason.SocialInteraction;

    [RequiresIdeology]
    public void TestAbility(TestScenario scenario)
    {
        var receiver = scenario.Pawn()
            .Colonist()
            .RemoveIdeo()
            .SetIdeoCertainty(0.1f)
            .CreateSingle();
        var converter = scenario.Pawn()
            .Colonist()
            .SetIdeo(role: PreceptDefOf.IdeoRole_Moralist)
            .ApplyAbility(Extra.AbilityDefOf.Convert, receiver)
            .CreateSingle();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.IdeoChanged,
            Description = "[Converter] converted [PAWN] to [His] own ideoligion. [PAWN] became convinced and adopted [NewIdeo].",
        };
        Expect.That(receiver).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [converter] }));
        Expect.That(converter).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [receiver] }));
    }

    [RequiresIdeology]
    public void TestSocialInteraction(TestScenario scenario)
    {
        var receiver = scenario.Pawn()
            .Colonist()
            .SetIdeo(Faction.OfHostile.ideos.PrimaryIdeo, certainty: 0f)
            .CreateSingle();
        var converter = scenario.Pawn()
            .Colonist()
            .SetIdeo()
            .CreateSingle();

        var extraSentencePacks = new List<RulePackDef>();
        InteractionDefOf.ConvertIdeoAttempt.Worker.Interacted(converter, receiver, extraSentencePacks, out _, out _, out _, out _);

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.IdeoChanged,
            Description = "[Converter] tried to convert [PAWN] to [His] own ideoligion. [PAWN] abandoned [OldIdeo] and adopted [NewIdeo].",
        };
        Expect.That(receiver).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [converter] }));
        Expect.That(converter).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [receiver] }));
    }

    [RequiresIdeology]
    public void TestConversionRitual(TestScenario scenario)
    {
        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(role: PreceptDefOf.IdeoRole_Moralist)
            .CreateSingle();
        var converted = scenario.Pawn()
            .Colonist()
            .SetIdeo(Faction.OfHostile.ideos.PrimaryIdeo)
            .CreateSingle();
        scenario.Map()
            .BuildRoom(8, 8)
            .AsShrine()
            .Execute();

        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.Conversion.BestOutcome)
            .ConversionRitual(converted)
            .Execute();

        Expect.That(converted).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.IdeoChanged,
            Description = "[PAWN] abandoned [OldIdeo] and adopted [NewIdeo] after a conversion ritual led by [Converter].",
            Concerns = [organizer],
        });
        Expect.That(organizer).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.IdeoChanged);
    }

    [RequiresIdeology]
    [RequiresRoyalty]
    public Action TestSpeechRitual(TestScenario scenario)
    {
        var organizer = scenario.Pawn()
            .FullHeal()
            .Colonist()
            .SetIdeo(role: PreceptDefOf.IdeoRole_Leader)
            .SetRoyalTitle(Extra.RoyalTitleDefOf.Praetor)
            .CreateSingle();
        var converted = scenario.Pawn()
            .Colonist()
            .SetIdeo(Faction.OfHostile.ideos.PrimaryIdeo)
            .CreateSingle();

        scenario.Map()
            .BuildRoom(10, 10, floorDef: TerrainDefOf.MetalTile)
            .AsThroneRoom(organizer)
            .Execute();

        var previousChance = Accessor.RitualOutcomeEffectWorker_Speech.ConversionChanceFromInspirationalSpeech;
        Accessor.RitualOutcomeEffectWorker_Speech.ConversionChanceFromInspirationalSpeech = 1f;

        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.AttendedSpeech.BestOutcome)
            .ThroneSpeech([converted])
            .Execute();

        Expect.That(converted).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.IdeoChanged,
            Description = "[PAWN] abandoned [OldIdeo] and adopted [NewIdeo] hearing [Converter]'s speech.",
            Concerns = [organizer],
        });
        Expect.That(organizer).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.IdeoChanged);

        return () => Accessor.RitualOutcomeEffectWorker_Speech.ConversionChanceFromInspirationalSpeech = previousChance;
    }

    [RequiresIdeology]
    public void TestUnknown(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .SetIdeo()
            .CreateSingle();

        pawn.ideo.SetIdeo(Faction.OfHostile.ideos.PrimaryIdeo);

        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.IdeoChanged,
            Description = "[PAWN] abandoned [OldIdeo] and adopted [NewIdeo]",
        });
    }

    [RequiresIdeology]
    public void TestMentalBreak(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .StopMentalState()
            .SetIdeo(certainty: 0.1f)
            .CreateSingle();

        pawn.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.IdeoChange);

        Expect.That(pawn).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.IdeoChanged);
        Expect.That(pawn).ToHaveHistoryRecordOf(HistoryRecordDefOf.MentalBreak);
    }
}
