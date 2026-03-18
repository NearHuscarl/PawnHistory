using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace PawnHistory.Source.PawnTracker.Test;

public class PawnBuilder(int count = 1)
{
    private PawnKindDef kind = null;
    private Faction faction;
    private GuestStatus guestStatus;
    private readonly List<Predicate<Pawn>> filters = [];
    private readonly List<Action<Pawn, int, List<Pawn>>> processors = [];
    private int count = count;
    private IntVec3 spawnPosition = Find.CameraDriver.MapPosition;
    private int spawnRadius = 4;

    public PawnBuilder WithPosition(IntVec3 position, int radius = 3)
    {
        spawnPosition = position;
        spawnRadius = radius;
        return this;
    }

    public PawnBuilder WithKind(PawnKindDef kind)
    {
        this.kind = kind;
        return this;
    }

    public PawnBuilder WithFaction(Faction faction)
    {
        this.faction = faction;
        return this;
    }

    public PawnBuilder HumanLike(bool value = true)
    {
        if (value)
            filters.Add(p => p.RaceProps.Humanlike);

        return this;
    }

    public PawnBuilder ThatMatches(Predicate<Pawn> filter)
    {
        filters.Add(filter);
        return this;
    }

    public PawnBuilder Animal()
    {
        var animalKinds = DefDatabase<PawnKindDef>.AllDefs.Where(k => k.RaceProps?.Animal ?? false);
        kind = animalKinds.RandomElement();

        return WithKind(animalKinds.RandomElement());
    }

    public PawnBuilder Do(Action<Pawn, int, List<Pawn>> processor)
    {
        processors.Add(processor);
        return this;
    }

    public PawnBuilder Do(Action<Pawn> processor)
    {
        processors.Add((p, _, __) => processor(p));
        return this;
    }

    public PawnBuilder Do(Action<Pawn, int> processor)
    {
        processors.Add((p, i, __) => processor(p, i));
        return this;
    }

    public PawnBuilder Count(int count)
    {
        this.count = count;
        return this;
    }

    public Pawn CreateSingle()
    {
        return Create().FirstOrDefault();
    }

    public List<Pawn> Create(bool reusePawns = true)
    {
        var allFilters = filters.Concat([
            p => !p.Downed,
            p => kind == null || p.kindDef == kind,
            p => p.Faction == faction,
            p => p.GuestStatus == guestStatus
        ]).ToList();
        var pawns = reusePawns ? Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(p => allFilters.All(f => f(p))).Take(count).ToList() : [];
        var existingCount = pawns.Count;

        for (var i = 0; i < count - existingCount; i++)
        {
            var generatedKind = kind ?? PawnKindDefOf.Colonist;
            var pawn = PawnGenerator.GeneratePawn(generatedKind, FactionUtility.DefaultFactionFrom(faction?.def ?? generatedKind.defaultFactionDef), new PlanetTile?(Find.CurrentMap.Tile));
            var spawnPos = CellFinder.RandomClosewalkCellNear(spawnPosition, Find.CurrentMap, spawnRadius);

            GenSpawn.Spawn(pawn, spawnPos, Find.CurrentMap);

            if (pawn.Faction != Faction.OfPlayer)
                pawn.guest?.SetGuestStatus(Faction.OfPlayer, guestStatus);
            pawns.Add(pawn);
        }

        for (var i = 0; i < pawns.Count; i++)
        {
            foreach (var processor in processors)
            {
                processor(pawns[i], i, pawns);
            }
        }

        return pawns;
    }

    public PawnBuilder AsPrisoner()
    {
        guestStatus = GuestStatus.Prisoner;
        return WithFaction(Faction.OfPirates);
    }

    /// <summary>
    /// Removes all injuries, restores missing body parts, and refills all needs (Food, Rest, etc.).
    /// </summary>
    public PawnBuilder FullHeal()
    {
        return Do(pawn =>
        {
            pawn.health.RestorePart(pawn.RaceProps.body.corePart);

            var badHediffs = pawn.health.hediffSet.hediffs
                .Where(h => h is Hediff_Injury || h.def.isBad)
                .ToList();

            foreach (var hediff in badHediffs)
                pawn.health.RemoveHediff(hediff);

            // Fully satisfy all needs (Food, Joy, Rest) so they don't collapse mid-test
            if (pawn.needs != null)
            {
                foreach (var need in pawn.needs.AllNeeds)
                    need.CurLevel = need.MaxLevel;
            }
        });
    }


    /// <summary>
    /// Teleports all pawns in this group to a specific location packed closely together.
    /// </summary>
    public PawnBuilder GroupTogether(IntVec3? at = null, int radius = 2)
    {
        return Do(pawn =>
        {
            var map = Find.CurrentMap;
            var center = at ?? Find.CameraDriver.MapPosition;

            // Find a spot for each pawn in a circle/cluster around the center
            var spot = CellFinder.RandomClosewalkCellNear(center, map, 2);

            if (pawn.Spawned)
                pawn.DeSpawn();

            GenSpawn.Spawn(pawn, spot, map);
        });
    }

    /// <summary>
    /// Forces the pawns into a Berserk rage. They will attack the nearest target (pawn or animal).
    /// </summary>
    public PawnBuilder MakeBerserk()
    {
        processors.Add((pawn, index, all) => pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, true, false, false));
        return this;
    }

    public PawnBuilder MakeHostile()
    {
        var hostileFactions = Find.FactionManager.AllFactions
            .Where(f => f.HostileTo(Faction.OfPlayer) && !f.def.isPlayer && f.def.humanlikeFaction)
            .ToList();

        if (hostileFactions.Count == 0)
            return this;

        return Do((pawn, index) =>
        {
            pawn.SetFaction(hostileFactions[index % hostileFactions.Count]);
            pawn.jobs.StopAll();
            pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
        });
    }

    public PawnBuilder WithRandomRelations(int relationsPerPawn)
    {
        var possibleRelations = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where(def => def != null && def.defName != "Bond").ToList();

        return Do((pawn, _, pawns) =>
        {
            var pawnsToCreateRelation = pawns
                .Where(p => p != null && p.relations != null && p.RaceProps?.Humanlike == true)
                .ToList();

            if (pawnsToCreateRelation.Count < 2)
                return;

            for (var i = 0; i < relationsPerPawn; i++)
            {
                var other = pawnsToCreateRelation.Where(p => p != pawn).RandomElementWithFallback(null);
                var relation = possibleRelations.RandomElement();

                if (pawn.relations.DirectRelationExists(relation, other))
                    continue;
                pawn.relations.AddDirectRelation(relation, other);
            }
        });
    }
}