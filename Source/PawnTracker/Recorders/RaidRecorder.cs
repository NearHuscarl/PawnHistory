using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;
using static RimWorld.PsychicRitualRoleDef;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class RaidRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<RaidEvent>(e =>
        {
            var pawns = e.Pawns.Where(ShouldRecord).ToList();

            if (e.IsFriendly)
                HandleRaidFriendlyStartedEvent(pawns, e.Faction);
            else
                HandleRaidEnemyStartedEvent(pawns, e.Faction, e.RaidStrategy, e.RaidArrivalMode);
        });
    }

    private void HandleRaidFriendlyStartedEvent(List<Pawn> pawns, Faction faction)
    {
        var eventDef = HistoryRecordDefOf.RaidFriendly;
        var hostileFaction = pawns[0].MapHeld.lordManager.lords
            .FirstOrDefault(l => l.faction != null && l.faction.HostileTo(faction))
            ?.faction;

        foreach (var pawn in pawns)
        {
            var rules = new List<Rule>();
            var constants = new Dictionary<string, string>();
            var desc = eventDef.ResolveDescription("raidFriendly", pawn)
                .WithFaction(faction)
                .WithOthers(pawns)
                .AddConstantIf(hostileFaction != null, "hostileFaction", "true") // not manhunter/insect
                .AddRule("HOSTILEFACTION", hostileFaction)
                .Resolve();

            AddRecord(new HistoryRecord(eventDef, pawn, desc));
        }
    }

    enum RaidProperty
    {
        None,
        Siege,
        Breacher,
        Sapper,
        CenterDrop,
    }

    private void HandleRaidEnemyStartedEvent(List<Pawn> pawns, Faction faction, RaidStrategyDef raidStrategy, PawnsArrivalModeDef raidArrivalMode)
    {
        var raidProperty = RaidProperty.None;

        if (raidArrivalMode.defName == "CenterDrop")
            raidProperty = RaidProperty.CenterDrop;
        else if (raidStrategy.defName.StartsWith("ImmediateAttackBreaching"))
            raidProperty = RaidProperty.Breacher;
        else if (raidStrategy.defName.StartsWith("ImmediateAttackSappers"))
            raidProperty = RaidProperty.Sapper;
        else if (raidStrategy.defName.StartsWith("Siege"))
            raidProperty = RaidProperty.Siege;

        var eventDef = HistoryRecordDefOf.Raid;

        foreach (var pawn in pawns)
        {
            var desc = eventDef.ResolveDescription("raid", pawn)
                .WithFaction(faction)
                .WithOthers(pawns)
                .AddConstant("raidProperty", raidProperty)
                .Resolve();
            AddRecord(new HistoryRecord(eventDef, pawn, desc));
        }
    }
}
