using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TestScenario
{
    public static TestScenario Empty => field ??= new TestScenario();
    public bool NeverForceNormalSpeed = DebugViewSettings.neverForceNormalSpeed;

    public CellRect LastRoomRect { get; internal set; }
    public Dictionary<string, CellRect> TaggedRooms { get; } = [];
    public HashSet<Pawn> ProcessedPawns { get; } = [];
    
    internal readonly HashSet<Pawn> DeathOnNextHitPawns = [];
    public bool AlwaysHaveCancerOnBirthday = false;
    public RitualOutcomePossibility ForcedRitualOutcome;
    public Pawn ForceRewardPawnInQuest;
    public bool ForceInjuryScar = false;
    public bool ForcePostHealScar = false;
    public readonly int ForcedDebugMapSize = 25;

    public PawnBuilder Pawn(int count = 1) => new(count);
    public PawnBuilder Pawn(IEnumerable<Pawn> pawns) => new PawnBuilder().WithPawns(pawns);
    public PawnBuilder Pawn(Pawn pawn) => Pawn([pawn]);
    public GatheringBuilder Incident(GatheringDef def) => new(def);
    public IncidentBuilder Incident(IncidentDef def) => new(def);
    public IncidentBuilder Incident(IncidentDef def, IIncidentTarget target) => new(def, target);
    public IncidentBuilder RaidFriendly() => Incident(IncidentDefOf.RaidFriendly).NonHostileFaction();
    public MapBuilder Map(IntVec3? pos = null) => new(pos);
    public ThingBuilder Thing(ThingDef thingDef, ThingDef stuffDef = null) => new(thingDef, stuffDef);
    public QuestBuilder Quest(Quest quest) => new QuestBuilder().WithQuest(quest);
    public QuestBuilder Quest(QuestScriptDef quest, float points = 500f) => new(quest, points);
    public CaravanBuilder Caravan(List<Pawn> pawns) => new(pawns);
    public DropPodBuilder DropPod(List<Thing> things) => new(things);
    public DropPodBuilder DropPod(List<Pawn> pawns) => new(pawns.Cast<Thing>().ToList());
    public TradeSessionBuilder Trade(Pawn negotiator) => new(negotiator);
    public RitualBuilder Ritual(Pawn organizer) => new(organizer);
    public LetterAction<T> Letter<T>() where T : ChoiceLetter => new();

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
    extension(TestScenario scenario)
    {
        public TestScenario ForwardTime(float day)
        {
            Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + GenDate.DaysToTicks(day));
            return scenario;
        }
        
        public TestScenario ForwardTicks(int ticks)
        {
            Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + ticks);
            return scenario;
        }

        public TestScenario SpeedUp()
        {
            scenario.NeverForceNormalSpeed = DebugViewSettings.neverForceNormalSpeed;
            DebugViewSettings.neverForceNormalSpeed = true;
            Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
            return scenario;
        }

        public TestScenario SlowDown()
        {
            DebugViewSettings.neverForceNormalSpeed = scenario.NeverForceNormalSpeed;
            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            return scenario;
        }

        public TestScenario RunUntil(Func<bool> stopCondition, Action action, Action onFinish = null, int interval = 1)
        {
            TickDelayManager.Interval(interval, TestManager.Timeout, data =>
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

        public TestScenario WaitUntil(Func<bool> condition, Action thenDo, int interval = 1)
        {
            TickDelayManager.Interval(interval, TestManager.Timeout, data =>
            {
                if (!condition())
                    return;

                data.Cancelled = true;
                thenDo?.Invoke();
            });
            return scenario;
        }
    }
}
