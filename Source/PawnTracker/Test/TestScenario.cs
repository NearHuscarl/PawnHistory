using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TestScenario
{
    public bool neverForceNormalSpeed = DebugViewSettings.neverForceNormalSpeed;

    public static CellRect LastRoomRect { get; internal set; }
    public static Dictionary<string, CellRect> TaggedRooms { get; internal set; } = [];
    public static HashSet<Pawn> ProcessedPawns { get; internal set; } = [];

    public static void ClearAll()
    {
        TaggedRooms.Clear();
        ProcessedPawns.Clear();
    }

    public PawnBuilder Pawn(int count = 1) => new(count);
    public PawnBuilder Pawn(IEnumerable<Pawn> pawns) => new PawnBuilder().WithPawns(pawns);
    public PawnBuilder Pawn(Pawn pawn) => Pawn([pawn]);
    public GatheringBuilder Incident(GatheringDef def) => new(def);
    public IncidentBuilder Incident(IncidentDef def) => new(def);
    public IncidentBuilder Incident(string defName) => new(DefDatabase<IncidentDef>.GetNamed(defName));
    public MapBuilder Thing(IntVec3? pos = null) => new(pos);

    public IncidentBuilder Siege()
    {
        var siegeStrategy = DefDatabase<RaidStrategyDef>.GetNamed("Siege");
        return Incident(IncidentDefOf.RaidEnemy)
            .RaidStrategy(siegeStrategy);
    }

    public IncidentBuilder RaidFriendly()
    {
        return Incident(IncidentDefOf.RaidFriendly)
            .Faction(Find.FactionManager.AllFactions.FirstOrDefault(f => f.PlayerRelationKind == FactionRelationKind.Neutral && !f.def.hidden));
    }

    internal void OpenHistoryRecordTab(Pawn pawn)
    {
        CameraJumper.TryJumpAndSelect(pawn);

        var inspectWindow = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
        var historyTab = pawn.GetInspectTabs()?.FirstOrDefault(t => t is ITab_Pawn_History);

        if (historyTab != null)
            inspectWindow.OpenTabType = historyTab.GetType();
    }
}


public static class TestScenarioExtensions
{
    public static TestScenario ForwardTime(this TestScenario scenario, float day)
    {
        Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + GenDate.DaysToTicks(day));
        return scenario;
    }

    public static TestScenario SpeedUp(this TestScenario scenario)
    {
        scenario.neverForceNormalSpeed = DebugViewSettings.neverForceNormalSpeed;
        DebugViewSettings.neverForceNormalSpeed = true;
        Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
        return scenario;
    }

    public static TestScenario SlowDown(this TestScenario scenario)
    {
        DebugViewSettings.neverForceNormalSpeed = scenario.neverForceNormalSpeed;
        Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
        return scenario;
    }

    public static TestScenario RunOnceOn<T>(this TestScenario scenario, Func<T, bool> runWhen, Action<T> listener) where T : GameEventBase
    {
        void wrapper(T evt)
        {
            if (!runWhen(evt))
                return;

            try
            {
                listener(evt);
            }
            finally
            {
                GameEventBus.Unsubscribe((Action<T>)wrapper);
            }
        }
        GameEventBus.Subscribe((Action<T>)wrapper);

        return scenario;
    }

    public static TestScenario RunUntil(this TestScenario scenario, Func<bool> stopCondition, Action action, Action onFinish = null, int interval = 1)
    {
        TickDelayManager.Interval(interval, (data) =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log.Error($"Error while executing action {action}, {ex}");
            }
            finally
            {
                if (stopCondition())
                {
                    data.Cancelled = true;
                    onFinish?.Invoke();
                }
            }
        });
        return scenario;
    }
}