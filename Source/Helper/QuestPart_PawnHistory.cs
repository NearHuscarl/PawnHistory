using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace PawnHistory.Source.Helper;

public class QuestPart_PawnHistory : QuestPart
{
    public Pawn Asker;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref Asker, "asker");
    }
}

[HarmonyPatch(typeof(QuestGen), "ClearQuestGenState")]
internal static class QuestGen_ClearQuestGenState_Patch
{
    private static void Prefix()
    {
        var quest = QuestGen.quest;
        var asker = QuestGen.slate.Get<Pawn>("asker");
        quest.GetFirstOrAddPart<QuestPart_PawnHistory>().Asker = asker;
    }
}
