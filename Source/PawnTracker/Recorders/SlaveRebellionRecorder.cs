using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using PawnHistory.Source.Helper;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public enum SlaveRebellionType
{
    SingleRebellion,
    LocalRebellion,
    GrandRebellion,
}

public class SlaveRebellionRecorder : RecorderBase<SlaveRebellionEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<SlaveRebellionEvent>(CreateRecord);
    }

    public override void CreateRecord(SlaveRebellionEvent e)
    {
        if (e.Reason == SlaveEscapeReason.Rebellion)
            CreateEscapeRecord(e);
        else
            CreateJailBreakRecord(e);
    }

    private void CreateEscapeRecord(SlaveRebellionEvent e)
    {
        var recordDef = HistoryRecordDefOf.SlaveRebellion;
        var joiners = e.EscapingSlaves.Where(p => p != e.Initiator).ToList();
        var concerns = e.EscapingSlaves;
        var rebellionType = GetRebellionType(e);

        foreach (var pawn in joiners)
        {
            if (!ShouldRecord(pawn))
                continue;

            var desc = recordDef.Description(pawn)
                .WithOthers(joiners)
                .AddRule("Initiator", e.Initiator)
                .AddConstant("initiator", false)
                .AddConstant("escape", e.IsEscape)
                .AddConstant("rebellionType", rebellionType)
                .Resolve();

            AddRecord(recordDef, pawn, desc, concerns);
        }

        if (ShouldRecord(e.Initiator))
        {
            var desc = recordDef.Description(e.Initiator)
                .IncludePawnGrammar()
                .WithOthers(e.EscapingSlaves)
                .AddConstant("initiator", true)
                .AddConstant("escape", e.IsEscape)
                .AddConstant("rebellionType", rebellionType)
                .Resolve();

            AddRecord(recordDef, e.Initiator, desc, concerns);
        }
    }

    private void CreateJailBreakRecord(SlaveRebellionEvent e)
    {
        var recordDef = HistoryRecordDefOf.SlaveRebellion;
        var concerns = e.EscapingSlaves.Concat(e.Initiator).ToList();

        foreach (var pawn in e.EscapingSlaves)
        {
            if (!ShouldRecord(pawn))
                continue;

            var desc = recordDef.Description(pawn)
                .WithOthers(e.EscapingSlaves)
                .AddConstant("escape", e.IsEscape)
                .AddConstant("rebellionType", GetRebellionType(e))
                .AddRule("Reason", e.LogEntryText)
                .Resolve("jailbreaker");

            AddRecord(recordDef, pawn, desc, concerns);
        }
    }

    private static SlaveRebellionType GetRebellionType(SlaveRebellionEvent e)
    {
        if (e.EscapingSlaves.Count == 1)
            return SlaveRebellionType.SingleRebellion;
        if (e.EscapingSlaves.Count == e.EligibleSlaves.Count)
            return SlaveRebellionType.GrandRebellion;
        return SlaveRebellionType.LocalRebellion;
    }
    
    [RequiresIdeology]
    [SkipTest] // TODO: non-violent tests are indeterministic. Think of a good way to mock forceAggressive
    public void TestSingle(TestScenario scenario)
    {
        RunTest(scenario, SlaveRebellionType.SingleRebellion, false, "[PAWN] [Escape].");
    }
    
    [RequiresIdeology]
    public void TestSingleViolent(TestScenario scenario)
    {
        RunTest(scenario, SlaveRebellionType.SingleRebellion, true, "[PAWN] started a slave rebellion.");
    }
    
    [RequiresIdeology]
    [DebugMapSize(30)]
    [SkipTest]
    public void TestLocal(TestScenario scenario)
    {
        RunTest(scenario, SlaveRebellionType.LocalRebellion, false, "[PAWN] [Escape] with [Others].", "[PAWN] and [Others] joined a [EscapeNoun] led by [Initiator].");
    }
    
    [RequiresIdeology]
    [DebugMapSize(30)]
    public void TestLocalViolent(TestScenario scenario)
    {
        RunTest(scenario, SlaveRebellionType.LocalRebellion, true, "[PAWN] started a slave rebellion with [Others].", "[PAWN] and [Others] joined a slave rebellion led by [Initiator].");
    }
    
    [RequiresIdeology]
    [SkipTest] // TODO: non-violent tests are indeterministic. Think of a good way to mock forceAggressive
    public void TestGrand(TestScenario scenario)
    {
        RunTest(scenario, SlaveRebellionType.GrandRebellion, false, "[PAWN] started a grand slave escape with [Others].", "[PAWN] and [Others] joined a grand [EscapeNoun] led by [Initiator].");
    }
    
    [RequiresIdeology]
    public void TestGrandViolent(TestScenario scenario)
    {
        RunTest(scenario, SlaveRebellionType.GrandRebellion, true, "[PAWN] started a grand slave rebellion with [Others].", "[PAWN] and [Others] joined a grand slave rebellion led by [Initiator].");
    }

    private static void RunTest(TestScenario scenario, SlaveRebellionType rebellionType, bool forceViolent, string instigatorDesc, string joinerDesc = null)
    {
        scenario.SpeedUp();
        scenario.ForceSlaveRebellionType = rebellionType;
        scenario.ForceSlaveRebellionViolent = forceViolent;
        var topLeft = new IntVec3(0, 0, Find.CurrentMap.Size.z - 1);
        var bottomRight = new IntVec3(Find.CurrentMap.Size.x - 1, 0, 0);
                
        var slaveFarAway = scenario.Pawn()
            .Position(bottomRight)
            .AsSlave()
            .CreateSingle();
        var slaves = scenario.Pawn(3)
            .Position(topLeft)
            .AsSlave()
            .Execute();

        SlaveRebellionUtility.StartSlaveRebellion(slaves[0]);

        Expect.That(slaves[0])
            .Eventually(1000)
            .ToHaveHistoryRecord(HistoryRecordDefOf.SlaveRebellion, instigatorDesc);

        if (joinerDesc != null)
        {
            Expect.ThatAny(slaves.Skip(1))
                .Eventually(1000)
                .ToHaveHistoryRecord(new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.SlaveRebellion,
                    Description = joinerDesc,
                    ConcernAtLeast = [slaves[0]],
                });
        }
    }
    
    [RequiresIdeology]
    [SkipTest]
    public void TestJailbreakerSingle(TestScenario scenario)
    {
        RunTestJailbreaker(scenario, SlaveRebellionType.SingleRebellion, false, "[Reason] As a result, [PAWN] [Escape].");
    }
    
    [RequiresIdeology]
    public void TestJailbreakerSingleViolent(TestScenario scenario)
    {
        RunTestJailbreaker(scenario, SlaveRebellionType.SingleRebellion, true, "[Reason] As a result, [PAWN] started a slave rebellion.");
    }
    
    [RequiresIdeology]
    [DebugMapSize(30)]
    public void TestJailbreakerLocal(TestScenario scenario)
    {
        RunTestJailbreaker(scenario, SlaveRebellionType.LocalRebellion, false, "[Reason] As a result, [PAWN] and [Others] [Escape].");
    }
    
    [RequiresIdeology]
    [DebugMapSize(30)]
    public void TestJailbreakerLocalViolent(TestScenario scenario)
    {
        RunTestJailbreaker(scenario, SlaveRebellionType.LocalRebellion, true, "[Reason] As a result, [PAWN] and [Others] started a slave rebellion.");
    }
    
    [RequiresIdeology]
    [SkipTest]
    public void TestJailbreakerGrand(TestScenario scenario)
    {
        RunTestJailbreaker(scenario, SlaveRebellionType.GrandRebellion, false, "[Reason] As a result, [PAWN] and [Others] [Escape].");
    }
    
    [RequiresIdeology]
    public void TestJailbreakerGrandViolent(TestScenario scenario)
    {
        RunTestJailbreaker(scenario, SlaveRebellionType.GrandRebellion, true, "[Reason] As a result, [PAWN] and [Others] started a slave rebellion.");
    }

    private static void RunTestJailbreaker(TestScenario scenario, SlaveRebellionType rebellionType, bool forceViolent, string descriptionTemplate)
    {
        scenario.SpeedUp();
        scenario.ForceSlaveRebellionType = rebellionType;
        scenario.ForceSlaveRebellionViolent = forceViolent;
        var topLeft = new IntVec3(0, 0, Find.CurrentMap.Size.z - 1);
        var bottomRight = new IntVec3(Find.CurrentMap.Size.x - 1, 0, 0);
        
        var slaveFarAway = scenario.Pawn()
            .Position(bottomRight)
            .AsSlave()
            .CreateSingle();
        var slaves = scenario.Pawn(3)
            .Position(topLeft)
            .AsSlave()
            .Execute();
        var pawn = scenario.Pawn(slaves[0])
            .Do(p => p.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.Rebellion))
            .CreateSingle();

        Expect.ThatAny(slaves.Skip(1))
            .Eventually(1000)
            .ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.SlaveRebellion,
                Description = descriptionTemplate,
                ConcernAtLeast = [slaves[0]],
            });
    }
}
