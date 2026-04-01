using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace PawnHistory.Source.PawnTracker.Test;

public class PawnBuilder(int count = 1)
{
    private PawnKindDef kind = null;
    private List<Pawn> pawns = null;
    private Faction faction;
    private GuestStatus? guestStatus;
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

    public PawnBuilder WithPawns(IEnumerable<Pawn> pawns)
    {
        this.pawns = pawns.ToList();
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

    public PawnBuilder Colonist(bool value = true)
    {
        if (value)
            return HumanLike().WithFaction(Faction.OfPlayer);

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

    public PawnBuilder AsPrisoner()
    {
        guestStatus = GuestStatus.Prisoner;
        return WithFaction(Faction.OfPirates);
    }

    public PawnBuilder DoOnce(Action<Pawn> processor)
    {
        var done = false;
        processors.Add((p, _, _) =>
        {
            if (done) return;
            processor(p);
            done = true;
        });
        return this;
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

    public Pawn CreateSingle(bool reusePawns = true)
    {
        return Execute(reusePawns).FirstOrDefault();
    }

    private List<Pawn> SourcePawns(bool reusePawns)
    {
        var allFilters = filters.Concat([
            p => !p.Downed,
            p => kind == null || p.kindDef == kind,
            p => faction == null || p.Faction == faction,
            p => p.GuestStatus == null || p.GuestStatus == guestStatus,
            p => !TestScenario.ProcessedPawns.Contains(p),
        ]).ToList();
        var pawns = reusePawns ? Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(p => allFilters.All(f => f(p))).Take(count).ToList() : [];
        var existingCount = pawns.Count;

        for (var i = 0; i < count - existingCount; i++)
        {
            var generatedKind = kind ?? PawnKindDefOf.Colonist;
            var pawn = PawnGenerator.GeneratePawn(generatedKind, FactionUtility.DefaultFactionFrom(faction?.def ?? generatedKind.defaultFactionDef), new PlanetTile?(Find.CurrentMap.Tile));
            var spawnPos = CellFinder.RandomClosewalkCellNear(spawnPosition, Find.CurrentMap, spawnRadius);

            GenSpawn.Spawn(pawn, spawnPos, Find.CurrentMap);

            if (guestStatus.HasValue)
                pawn.guest?.SetGuestStatus(Faction.OfPlayer, guestStatus.Value);
            pawns.Add(pawn);
        }

        return pawns;
    }

    public List<Pawn> Execute(bool reusePawns = true)
    {
        var res = pawns ?? SourcePawns(reusePawns);

        for (var i = 0; i < res.Count; i++)
        {
            foreach (var processor in processors)
            {
                processor(res[i], i, res);
            }
            TestScenario.ProcessedPawns.Add(res[i]);
        }

        return res;
    }

    public PawnBuilder AddHediff(HediffDef def, BodyPartDef partDef = null, Action<Hediff> hediffCreated = null, int partIndex = 0)
    {
        return Do(pawn =>
        {
            var parts = partDef != null ? pawn.RaceProps.body.GetPartsWithDef(partDef).ToList() : null;
            var part = parts[partIndex];
            var hediff = HediffMaker.MakeHediff(def, pawn, part);

            hediff.SetVisible();
            pawn.health.AddHediff(hediff, part);
            hediffCreated?.Invoke(hediff);
        });
    }

    public PawnBuilder AddHediff(string defName, BodyPartDef partDef)
    {
        var hediffDef = DefDatabase<HediffDef>.GetNamed(defName);
        return AddHediff(hediffDef, partDef);
    }

    public PawnBuilder AddHediff(string defName, string partDefName)
    {
        var hediffDef = DefDatabase<HediffDef>.GetNamed(defName);
        var partDef = DefDatabase<BodyPartDef>.GetNamed(partDefName);
        return AddHediff(hediffDef, partDef);
    }

    public PawnBuilder TendInjuries(float quality = 1f)
    {
        return Do(pawn =>
        {
            if (pawn.Dead) return;

            var injuries = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(h => h.TendableNow())
                .ToList();

            foreach (var injury in injuries)
            {
                injury.Tended(quality, quality);
            }
        });
    }

    public PawnBuilder Heal()
    {
        return Do(pawn =>
        {
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
    /// Removes all injuries, restores missing body parts, and refills all needs (Food, Rest, etc.).
    /// </summary>
    public PawnBuilder FullHeal()
    {
        return Heal().Do(pawn =>
        {
            var missingHediffs = pawn.health.hediffSet.GetMissingPartsCommonAncestors();

            foreach (var hediff in missingHediffs)
            {
                // Skip if this part already has an added part (prosthetic, archotech, etc.)
                var hasAddedPart = pawn.health.hediffSet.hediffs
                    .Any(h => h is Hediff_AddedPart && h.Part == hediff.Part);

                if (!hasAddedPart)
                    pawn.health.RestorePart(hediff.Part);
            }
        });
    }

    public PawnBuilder SetDoctor(bool isBadDoctor = false)
    {
        return Do(pawn =>
        {
            pawn.skills.GetSkill(SkillDefOf.Medicine).Level = isBadDoctor ? 0 : 20;

            if (!isBadDoctor)
            {
                var arms = pawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Arm).ToList();
                var archotechArm = DefDatabase<HediffDef>.GetNamed("ArchotechArm");

                foreach (var arm in arms)
                    pawn.health.AddHediff(archotechArm, arm);
            }
            else
            {
                var arms = pawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Arm).ToList();
                var eyes = pawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Eye).ToList();
                var torso = pawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Torso).FirstOrDefault();

                foreach (var part in arms.Concat(eyes))
                    pawn.health.AddHediff(HediffDefOf.MissingBodyPart, part);
                pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed("SmokeleafHigh"), torso);
            }

            pawn.inventory.innerContainer.TryAdd(ThingMaker.MakeThing(ThingDefOf.MedicineUltratech), 4);
            pawn.workSettings.SetPriority(WorkTypeDefOf.Doctor, 1);
        });
    }

    public PawnBuilder DoSurgery(Pawn patient, RecipeDef recipe, BodyPartDef partDef, bool instant = false, int partIndex = 0)
    {
        return DoOnce(doctor =>
        {
            var parts = patient.RaceProps.body.GetPartsWithDef(partDef).ToList();
            var part = parts[partIndex];
            var bill = new Bill_Medical(recipe, []);
            patient.BillStack.AddBill(bill);
            bill.Part = part;

            if (instant)
            {
                recipe.Worker.ApplyOnPawn(patient, part, doctor, [], bill);
                return;
            }

            // Surgery is slooow
            Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;

            var bed = RestUtility.FindPatientBedFor(patient);
            patient.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.LayDown, bed), JobTag.SatisfyingNeeds);

            var job = JobMaker.MakeJob(JobDefOf.DoBill, patient, bed);
            job.bill = patient.BillStack.Bills.First();
            doctor.jobs.TryTakeOrderedJob(job, JobTag.MiscWork);
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
            var spot = CellFinder.RandomClosewalkCellNear(center, map, radius);

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
}

static class PawnBuilderExtension
{
    public static PawnBuilder EquipWeapon(this PawnBuilder builder, string weaponDefName, Func<Pawn, int, bool> ShouldEquip = null)
    {
        return builder.EquipWeapon(DefDatabase<ThingDef>.GetNamed(weaponDefName), ShouldEquip);
    }
    public static PawnBuilder EquipWeapon(this PawnBuilder builder, ThingDef weaponDef, Func<Pawn, int, bool> ShouldEquip = null)
    {
        return builder.Do((pawn, i) =>
        {
            if (pawn.Dead || (ShouldEquip?.Invoke(pawn, i) ?? false)) return;

            var weapon = ThingMaker.MakeThing(weaponDef);

            pawn.equipment ??= new Pawn_EquipmentTracker(pawn);
            pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
            pawn.equipment.AddEquipment((ThingWithComps)weapon);
        });
    }

    public static PawnBuilder Capture(this PawnBuilder builder, Pawn captured)
    {
        return builder.DoOnce(captor =>
        {
            CaptureUtility.TryGetBed(captor, captured, out Thing bed);
            var job = JobMaker.MakeJob(JobDefOf.Capture, captured, bed);
            job.count = 1;
            job.playerForced = true;
            captor.jobs.StartJob(job, JobCondition.InterruptForced);
        });
    }

    public static PawnBuilder StartJob(this PawnBuilder builder, JobDef jobDef, LocalTargetInfo? targetA = null, LocalTargetInfo? targetB = null)
    {
        return builder.DoOnce(pawn =>
        {
            var job = JobMaker.MakeJob(jobDef, targetA ?? null, targetB ?? null);
            job.count = 1;
            job.playerForced = true;
            pawn.jobs.StartJob(job, JobCondition.InterruptForced);
        });
    }

    public static PawnBuilder StripNaked(this PawnBuilder builder)
    {
        return builder.Do(pawn =>
        {
            if (pawn?.apparel == null)
                return;

            var worn = pawn.apparel.WornApparel.ToList();

            foreach (var apparel in worn)
            {
                pawn.apparel.Remove(apparel);
                if (pawn.Spawned)
                    GenPlace.TryPlaceThing(apparel, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            }
        });
    }

    public static PawnBuilder WeakenParts(this PawnBuilder builder, HashSet<BodyPartDef> weakenParts, bool oneSide = false)
    {
        var bruise = DefDatabase<HediffDef>.GetNamed("Bruise");
        return builder.Do(pawn =>
        {
            var hediffSet = pawn.health.hediffSet;

            foreach (var part in pawn.RaceProps.body.AllParts)
            {
                if (!weakenParts.Contains(part.def) || (oneSide && part.Label.Contains("right")))
                    continue;

                var currentHp = hediffSet.GetPartHealth(part);
                if (currentHp <= 1)
                    continue;

                var damage = currentHp - 1;
                var injury = HediffMaker.MakeHediff(bruise, pawn, part) as Hediff_Injury;
                injury.Severity = damage;
                pawn.health.AddHediff(injury);
            }
        });
    }

    public static PawnBuilder SetRandomRelations(this PawnBuilder builder, int relationsPerPawn)
    {
        var possibleRelations = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where(def => def != null && !def.implied && def.defName != "Bond").ToList();

        return builder.Do((pawn, _, pawns) =>
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

    public static PawnBuilder StartMentalBreak(this PawnBuilder builder, MentalBreakDef def)
    {
        var randomNegativeThought = DefDatabase<ThoughtDef>.AllDefs
            .Where(t => t.stages != null && t.stages.Any(s => s != null && s.baseMoodEffect < 0) && (!t.label.NullOrEmpty() || !t.stages.First().label.NullOrEmpty()))
            .RandomElementWithFallback();
        var reason = "MentalStateReason_Mood".Translate() + "\n\n" + "FinalStraw".Translate((NamedArgument)randomNegativeThought.LabelCap);

        return builder.Do((pawn, _, pawns) =>
        {
            if (!pawn.mindState.mentalBreaker.TryDoMentalBreak(reason, def))
                Log.Warning($"[PawnHistory] Failed to force mental break {def.defName} on {pawn.LabelShort}");
        });
    }
}