using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace PawnHistory.Source.Helper;

public class QuestPart_PawnHistory : QuestPart
{
    public Pawn Asker;
    public Pawn Joiner;
    public List<Pawn> Helpers = [];

    public QuestPart_PawnHistory() => EnsureInitialized();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref Asker, "asker");
        Scribe_References.Look(ref Joiner, "joiner");
        Scribe_Collections.Look(ref Helpers, "helpers", false, LookMode.Reference);
        
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;

        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        Helpers ??= [];
    }
}

[HarmonyPatch(typeof(QuestGen), "ClearQuestGenState")]
internal static class QuestGen_ClearQuestGenState_Patch
{
    private static void Prefix()
    {
        var quest = QuestGen.quest;
        var questPart = quest.GetFirstOrAddPart<QuestPart_PawnHistory>();
        
        questPart.Asker = QuestGen.slate.Get<Pawn>("asker");
        questPart.Joiner = QuestGen.slate.Get<Pawn>("joiner");
        questPart.Helpers = QuestGen.slate.Get<IEnumerable<Pawn>>("helpers")?.ToList() ?? [];
    }
}
