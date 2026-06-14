using System.Linq;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class IdeoRoleChangedRecorder : RecorderBase<IdeoRoleChangedEvent>
{
    public override void Register()
    {
        if (!ModsConfig.IdeologyActive)
            return;

        GameEventBus.Subscribe<IdeoRoleChangedEvent>(CreateRecord);
    }

    public override void CreateRecord(IdeoRoleChangedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        if (e.OldRoleLabel == e.NewRoleLabel)
            return;

        var spectatorsAndPawn = e.Spectators.ToList();
        spectatorsAndPawn.Add(e.Pawn);

        var desc = HistoryRecordDefOf.IdeoRoleChanged.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("OldRole", e.OldRoleLabel?.Colorize(ColoredText.TipSectionTitleColor))
            .AddRule("NewRole", e.NewRoleLabel?.Colorize(ColoredText.TipSectionTitleColor), addSubsymbols: true)
            .AddConstant("hasOldRole", !e.OldRoleLabel.NullOrEmpty())
            .AddConstant("hasNewRole", !e.NewRoleLabel.NullOrEmpty())
            .WithOthers(spectatorsAndPawn)
            .Resolve();

        AddRecord(HistoryRecordDefOf.IdeoRoleChanged, e.Pawn, desc);
    }

    private static Ideo SetupRoleChange(TestScenario scenario)
    {
        scenario.SpeedUp();

        var ideo = scenario.Ideo().AddPrecept(PreceptDefOf.RoleChange).Execute();
        scenario.Map().BuildRoom(8, 8).AsShrine(ideo).Execute();

        return ideo;
    }

    [RequiresIdeology]
    public void TestRoleAdded(TestScenario scenario)
    {
        var ideo = SetupRoleChange(scenario);
        var roleChanger = scenario.Pawn()
            .Colonist()
            .SetIdeo(ideo)
            .CreateSingle();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(ideo)
            .Execute();
        var newRole = ideo.RolesListForReading.First(role => role.def == PreceptDefOf.IdeoRole_Moralist);

        scenario
            .Ritual(roleChanger)
            .Outcome(Extra.RitualOutcomeEffectDefOf.RoleChange.BestOutcome)
            .RoleChange(newRole, spectators)
            .Execute();

        Expect.That(roleChanger.Ideo.GetRole(roleChanger)).Same(newRole);
        Expect.That(roleChanger).ToHaveHistoryRecord(HistoryRecordDefOf.IdeoRoleChanged, "[PAWN] became [NewRole] after a successful role change in front of 2 others.");
    }

    [RequiresIdeology]
    public void TestRoleChanged(TestScenario scenario)
    {
        var ideo = SetupRoleChange(scenario);
        var roleChanger = scenario.Pawn()
            .Colonist()
            .SetIdeo(ideo, role: PreceptDefOf.IdeoRole_Leader)
            .CreateSingle();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(ideo)
            .Execute();
        var newRole = ideo.RolesListForReading.First(role => role.def == PreceptDefOf.IdeoRole_Moralist);

        scenario
            .Ritual(roleChanger)
            .Outcome(Extra.RitualOutcomeEffectDefOf.RoleChange.BestOutcome)
            .RoleChange(newRole, spectators)
            .Execute();

        Expect.That(roleChanger.Ideo.GetRole(roleChanger)).Same(newRole);
        Expect.That(roleChanger).ToHaveHistoryRecord(HistoryRecordDefOf.IdeoRoleChanged, "[PAWN] resigned from [OldRole] to become [NewRole] after a successful role change in front of 2 others.");
    }

    [RequiresIdeology]
    public void TestRoleRemoved(TestScenario scenario)
    {
        var ideo = SetupRoleChange(scenario);
        var roleChanger = scenario.Pawn()
            .Colonist()
            .SetIdeo(ideo, role: PreceptDefOf.IdeoRole_Leader)
            .CreateSingle();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(ideo)
            .Execute();

        scenario
            .Ritual(roleChanger)
            .Outcome(Extra.RitualOutcomeEffectDefOf.RoleChange.BestOutcome)
            .RoleChange(null, spectators)
            .Execute();

        Expect.That(roleChanger.Ideo.GetRole(roleChanger)).Null();
        Expect.That(roleChanger).ToHaveHistoryRecord(HistoryRecordDefOf.IdeoRoleChanged, "[PAWN] gave up [OldRole] after a successful role change in front of 2 others.");
    }
}
