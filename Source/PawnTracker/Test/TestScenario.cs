using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TestScenario
{
    public bool NeverForceNormalSpeed = DebugViewSettings.neverForceNormalSpeed;

    public static CellRect LastRoomRect { get; internal set; }
    public static Dictionary<string, CellRect> TaggedRooms { get; internal set; } = [];
    public static HashSet<Pawn> ProcessedPawns { get; internal set; } = [];
    internal static readonly HashSet<Pawn> DeathOnNextHitPawns = [];

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
    public IncidentBuilder RaidFriendly() => Incident(IncidentDefOf.RaidFriendly).NonHostileFaction();
    public MapBuilder Map(IntVec3? pos = null) => new(pos);
    public ThingBuilder Thing(ThingDef thingDef, ThingDef stuffDef = null) => new(thingDef, stuffDef);
    public CaravanBuilder Caravan(List<Pawn> pawns) => new(pawns);
    public DropPodBuilder DropPod(List<Pawn> pawns) => new(pawns);
    public TradeSessionBuilder Trade(Pawn trader, Pawn negotiator) => new(trader, negotiator);

    public void OpenHistoryRecordTab(Pawn pawn)
    {
        CameraJumper.TryJumpAndSelect(pawn);

        var inspectWindow = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
        var historyTab = pawn.GetInspectTabs()?.FirstOrDefault(t => t is ITab_Pawn_History);

        if (historyTab != null)
            inspectWindow.OpenTabType = historyTab.GetType();
    }

    public IntVec3 OutsideOf(string taggedRoom)
    {
        var rect = TaggedRooms[taggedRoom];
        var side = Rand.RangeInclusive(0, 3);

        return side switch
        {
            // Left
            0 => new IntVec3(rect.minX - 1, 0, Rand.RangeInclusive(rect.minZ, rect.maxZ)),
            // Right
            1 => new IntVec3(rect.maxX + 1, 0, Rand.RangeInclusive(rect.minZ, rect.maxZ)),
            // Bottom
            2 => new IntVec3(Rand.RangeInclusive(rect.minX, rect.maxX), 0, rect.minZ - 1),
            // Top
            _ => new IntVec3(Rand.RangeInclusive(rect.minX, rect.maxX), 0, rect.maxZ + 1),
        };
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
        scenario.NeverForceNormalSpeed = DebugViewSettings.neverForceNormalSpeed;
        DebugViewSettings.neverForceNormalSpeed = true;
        Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
        return scenario;
    }

    public static TestScenario SlowDown(this TestScenario scenario)
    {
        DebugViewSettings.neverForceNormalSpeed = scenario.NeverForceNormalSpeed;
        Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
        return scenario;
    }

    public static TestScenario RunOnceOn<T>(this TestScenario scenario, Func<T, bool> runWhen, Action<T> listener) where T : GameEventBase
    {
        GameEventBus.Subscribe((Action<T>)Wrapper);

        return scenario;

        void Wrapper(T evt)
        {
            if (!runWhen(evt))
                return;

            try
            {
                listener(evt);
            }
            finally
            {
                GameEventBus.Unsubscribe((Action<T>)Wrapper);
            }
        }
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
