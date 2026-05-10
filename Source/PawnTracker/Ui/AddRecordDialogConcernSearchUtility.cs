using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

internal static class AddRecordDialogConcernSearchUtility
{
    public static List<Thing> FindMatches(Map map, string query, IEnumerable<Thing> excludedConcerns, int maxResults = 32)
    {
        if (map == null || string.IsNullOrWhiteSpace(query))
            return [];

        var excluded = excludedConcerns?.ToHashSet() ?? [];
        var results = new HashSet<Thing>();

        foreach (var rootThing in map.listerThings.AllThings)
        {
            foreach (var match in ContentsFromThing(rootThing, map, query))
            {
                if (!excluded.Contains(match))
                    results.Add(match);
            }
        }

        return results
            .OrderBy(thing => thing.LabelNoParenthesis, StringComparer.InvariantCultureIgnoreCase)
            .Take(maxResults)
            .ToList();
    }

    private static IEnumerable<Thing> ContentsFromThing(Thing thing, Map map, string query)
    {
        if (thing == null)
            yield break;

        if (CanAddThing(thing, map, query))
            yield return thing;

        if (!thing.Faction.IsPlayerSafe() && thing is not Corpse)
            yield break;

        if (thing is Corpse { Bugged: false } corpse)
            thing = corpse.InnerPawn;

        if (thing is ISearchableContents { SearchableContents: { } searchableContents })
        {
            foreach (var item in searchableContents)
            {
                if (CanAddThing(item, map, query))
                    yield return item;
            }
        }

        if (thing is Pawn pawn && (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsAnimal || pawn.Corpse != null))
        {
            if (pawn.equipment != null)
            {
                foreach (var item in pawn.equipment.AllEquipmentListForReading)
                {
                    if (CanAddThing(item, map, query))
                        yield return item;
                }
            }

            if (pawn.apparel != null)
            {
                foreach (var item in pawn.apparel.WornApparel)
                {
                    if (CanAddThing(item, map, query))
                        yield return item;
                }
            }

            if (pawn.inventory != null)
            {
                foreach (var item in pawn.inventory.innerContainer)
                {
                    if (CanAddThing(item, map, query))
                        yield return item;
                }
            }
        }

        if (thing is ThingWithComps thingWithComps)
        {
            foreach (var comp in thingWithComps.AllComps)
            {
                if (comp is not ISearchableContents { SearchableContents: { } searchableContentsComp })
                    continue;

                foreach (var item in searchableContentsComp)
                {
                    if (CanAddThing(item, map, query))
                        yield return item;
                }
            }
        }
    }

    private static bool CanAddThing(Thing thing, Map map, string query)
    {
        if (thing?.def == null)
            return false;
        if (!thing.def.selectable || !thing.def.showInSearch || thing.Destroyed)
            return false;
        if (thing.MapHeld != map)
            return false;
        if (!DebugSettings.searchIgnoresRestrictions && thing.PositionHeld.Fogged(thing.MapHeld))
            return false;
        if (thing is Corpse { Bugged: not false })
            return false;
        if (thing is MinifiedThing { InnerThing: null })
            return false;
        if (!ThingIsVisibleToPlayer(thing))
            return false;

        return TextMatch(thing.LabelNoCount.StripTags(), query) || TextMatch(thing.def.label.StripTags(), query);
    }

    private static bool ThingIsVisibleToPlayer(Thing thing)
    {
        if (DebugSettings.searchIgnoresRestrictions)
            return true;

        if (thing is Pawn pawn && pawn.IsHiddenFromPlayer())
            return false;

        var parentHolder = thing.ParentHolder;
        if (parentHolder is Pawn_InventoryTracker inventoryTracker && inventoryTracker.pawn.IsHiddenFromPlayer())
            return false;
        if (parentHolder is Pawn_EquipmentTracker equipmentTracker && equipmentTracker.pawn.IsHiddenFromPlayer())
            return false;
        if (parentHolder is Pawn_ApparelTracker apparelTracker && apparelTracker.pawn.IsHiddenFromPlayer())
            return false;
        if (parentHolder is Pawn_CarryTracker carryTracker && carryTracker.pawn.IsHiddenFromPlayer())
            return false;

        return true;
    }

    private static bool TextMatch(string text, string query)
    {
        if (text.NullOrEmpty() || query.NullOrEmpty())
            return false;

        return text.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0;
    }
}
