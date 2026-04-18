using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record TechprintAppliedEvent(Pawn Pawn, ResearchProjectDef Project, float XpGained) : GameEventBase;

internal static class TechprintAppliedContext
{
    public static float XpBefore;

    public static float GetTotalXp(Pawn pawn)
    {
        var skill = pawn?.skills?.GetSkill(SkillDefOf.Intellectual);
        if (skill == null)
            return 0f;

        var total = skill.xpSinceLastLevel;
        for (var i = 0; i < skill.Level; i++)
            total += SkillRecord.XpRequiredToLevelUpFrom(i);

        return total;
    }
}

[HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ApplyTechprint))]
internal static class ResearchManager_ApplyTechprint_Patch
{
    private static void Prefix(Pawn applyingPawn)
    {
        TechprintAppliedContext.XpBefore = TechprintAppliedContext.GetTotalXp(applyingPawn);
    }

    private static void Postfix(ResearchProjectDef proj, Pawn applyingPawn)
    {
        var xpGained = TechprintAppliedContext.GetTotalXp(applyingPawn) - TechprintAppliedContext.XpBefore;

        GameEventBus.Publish(new TechprintAppliedEvent(applyingPawn, proj, xpGained));
    }
}
