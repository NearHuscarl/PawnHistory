using PawnHistory.Source.Helper;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Test;

public class PawnBuilder(int count = 1)
{
    private readonly List<Predicate<Pawn>> filters = [];
    private readonly List<Action<Pawn, int, List<Pawn>>> processors = [];
    private PawnKindDef kind = null;
    private List<Pawn> pawns = null;
    private Faction faction;
    private GuestStatus? guestStatus;
    private int count = count;
    private IntVec3? spawnPosition;
    private int spawnRadius = 4;
    private bool humanLike = true;
    private bool factionLeader = false;
    private bool isKilled = false;
    private bool isRotten = false;
    private bool isWorldPawn = false;

    public PawnBuilder Position(IntVec3 position, int radius = 3)
    {
        spawnPosition = position;
        spawnRadius = radius;
        return this;
    }

    public PawnBuilder WithPawns(IEnumerable<Pawn> pawns1)
    {
        this.pawns = pawns1.ToList();
        return this;
    }

    public PawnBuilder WithKind(PawnKindDef kind1)
    {
        this.kind = kind1;
        return this;
    }

    public PawnBuilder WithFaction(Faction faction1)
    {
        this.faction = faction1;
        return this;
    }

    public PawnBuilder HumanLike(bool value = true)
    {
        humanLike = value;
        return this;
    }

    public PawnBuilder Colonist(bool value = true)
    {
        if (value)
            return HumanLike().WithFaction(Faction.OfPlayer);

        return this;
    }

    public PawnBuilder Corpse(bool rotten = false)
    {
        isKilled = true;
        isRotten = rotten;

        return this;
    }

    public PawnBuilder FactionLeader(Faction faction1)
    {
        factionLeader = true;
        return HumanLike().WithFaction(faction1);
    }

    public PawnBuilder WorldPawn(bool value = true)
    {
        isWorldPawn = value;
        return this;
    }

    public PawnBuilder Enemy(bool value = true)
    {
        if (value)
            return HumanLike().WithFaction(Faction.OfHostile);

        return this;
    }

    public PawnBuilder ThatMatches(Predicate<Pawn> filter)
    {
        filters.Add(filter);
        return this;
    }

    public PawnBuilder Animal(PawnKindDef def = null)
    {
        return WithKind(def ?? DefDatabase<PawnKindDef>.AllDefs.Where(k => k.RaceProps?.Animal ?? false).RandomElement());
    }

    public PawnBuilder AsPrisoner()
    {
        guestStatus = GuestStatus.Prisoner;
        return WithFaction(Faction.OfHostile);
    }

    public PawnBuilder AsSlave()
    {
        guestStatus = GuestStatus.Slave;
        return WithFaction(Faction.OfHostile);
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
        processors.Add((p, _, _) => processor(p));
        return this;
    }

    public PawnBuilder Do(Action<Pawn, int> processor)
    {
        processors.Add((p, i, _) => processor(p, i));
        return this;
    }

    public PawnBuilder AddHediff(HediffDef def, BodyPartDef partDef = null, Action<Hediff> hediffCreated = null, int partIndex = 0)
    {
        return Do(pawn =>
        {
            var part = partDef != null ? pawn.GetBodyPart(partDef, partIndex) : null;
            var hediff = HediffMaker.MakeHediff(def, pawn, part);

            hediff.SetVisible();
            pawn.health.AddHediff(hediff, part);
            hediffCreated?.Invoke(hediff);
        });
    }

    public PawnBuilder StopMentalState()
    {
        return Do(pawn => pawn.mindState.mentalStateHandler.CurState?.RecoverFromState());
    }

    public PawnBuilder DiesOnNextHit()
    {
        return Do(pawn => TestManager.Scenario.DeathOnNextHitPawns.Add(pawn));
    }

    public PawnBuilder Heal()
    {
        return Do(pawn =>
        {
            var badHediffs = pawn.health.hediffSet.hediffs.Where(h => h is Hediff_Injury || h.def.isBad).ToList();

            foreach (var hediff in badHediffs)
                pawn.health.RemoveHediff(hediff);

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

    public PawnBuilder GiveTrait(TraitDef traitDef, int degree = 0, Action<Trait> traitCreated = null)
    {
        return Do(p =>
        {
            if (!p.story?.traits.HasTrait(traitDef) ?? false)
            {
                var newTrait = new Trait(traitDef, degree, forced: true);
                p.story?.traits?.GainTrait(newTrait);
                traitCreated?.Invoke(newTrait);
            }
        });
    }

    public PawnBuilder ResetSkillLevel(SkillDef skillDef, int level)
    {
        return Do(pawn =>
        {
            var skill = pawn.skills.GetSkill(skillDef);
            skill.Level = level;
            skill.xpSinceLastLevel = 0f;
            skill.passion = Passion.None;
        });
    }

    public PawnBuilder ResetSkillLevel(int level, int xpSinceLastLevel = 0)
    {
        return Do(pawn =>
        {
            foreach (var skill in pawn.skills.skills)
            {
                skill.Level = level;
                skill.xpSinceLastLevel = xpSinceLastLevel;
                skill.passion = Passion.None;
            }
        });
    }

    public PawnBuilder Learn(SkillDef skillDef, float xp)
    {
        return Do(pawn =>
        {
            var skill = pawn.skills.GetSkill(skillDef);
            skill.xpSinceMidnight = 0f;
            skill.Learn(xp);
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
                var archotechArm = Extra.HediffDefOf.ArchotechArm;

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
                pawn.health.AddHediff(Extra.HediffDefOf.SmokeleafHigh, torso);
            }

            pawn.inventory.innerContainer.TryAdd(ThingMaker.MakeThing(ThingDefOf.MedicineUltratech), 4);
            pawn.workSettings.SetPriority(WorkTypeDefOf.Doctor, 1);
        });
    }

    public PawnBuilder DoSurgery(Pawn patient, RecipeDef recipe, BodyPartDef partDef = null, bool instant = false, int partIndex = 0)
    {
        return DoOnce(doctor =>
        {
            var part = recipe.targetsBodyPart ? patient.GetBodyPart(partDef, partIndex) : null;
            var bill = new Bill_Medical(recipe, []);
            patient.BillStack.AddBill(bill);
            bill.Part = part;

            if (instant)
            {
                recipe.Worker.ApplyOnPawn(patient, part, doctor, [], bill);
                return;
            }

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

    public Pawn CreateSingle(bool reusePawns = true)
    {
        return Execute(reusePawns).FirstOrDefault();
    }

    private List<Pawn> SourcePawns(bool reusePawns)
    {
        var allFilters = filters.Concat([
            p => !p.Downed,
            p => isKilled == p.Dead,
            p => kind == null || p.kindDef == kind,
            p => faction == null || p.Faction == faction,
            p => guestStatus == null || p.GuestStatus == guestStatus,
            p => p.RaceProps.Humanlike == humanLike,
            p => !TestManager.Scenario.ProcessedPawns.Contains(p),
            p => p.IsFactionLeader(faction) == factionLeader,
        ]).ToList();
        var sourcedPawns = reusePawns ? Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(p => allFilters.All(f => f(p))).Take(count).ToList() : [];
        var existingCount = sourcedPawns.Count;
        var generateCount = count - existingCount;

        foreach (var pawn in sourcedPawns)
        {
            if (spawnPosition.HasValue)
                pawn.Position = spawnPosition.Value;
            if (faction != null && faction != pawn.Faction)
                pawn.SetFaction(faction ?? pawn.Faction);
        }

        for (var i = 0; i < generateCount; i++)
        {
            var generatedKind = kind ?? PawnKindDefOf.Colonist;
            var pawn = factionLeader
                ? faction?.leader
                : PawnGenerator.GeneratePawn(generatedKind, FactionUtility.DefaultFactionFrom(faction?.def ?? generatedKind.defaultFactionDef), new PlanetTile?(Find.CurrentMap.Tile));
            var spawnPos = CellFinder.RandomClosewalkCellNear(spawnPosition ?? Find.CameraDriver.MapPosition, Find.CurrentMap, spawnRadius);

            GenSpawn.Spawn(pawn, spawnPos, Find.CurrentMap);

            if (guestStatus.HasValue)
                pawn.guest?.SetGuestStatus(Faction.OfPlayer, guestStatus.Value);

            sourcedPawns.Add(pawn);
        }

        foreach (var pawn in sourcedPawns)
        {
            if (isKilled && !pawn.Dead)
                HealthUtility.DamageUntilDead(pawn);
            if (isRotten && pawn.Dead && pawn.Corpse.TryGetComp<CompRottable>(out var rot))
            {
                var corpse = pawn.Corpse;
                rot.RotImmediately();
                corpse.Map.gasGrid.AddGas(corpse.Position, GasType.RotStink, 1000); // don't know where this shit comes from
            }

            if (isWorldPawn)
            {
                pawn.DeSpawn();
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
            }
        }

        return sourcedPawns;
    }

    public List<Pawn> Execute(bool reusePawns = true)
    {
        var res = pawns ?? SourcePawns(reusePawns);

        for (var i = 0; i < res.Count; i++)
        {
            MakePawnCapable(res[i]);
            foreach (var processor in processors)
            {
                processor(res[i], i, res);
            }
            TestManager.Scenario.ProcessedPawns.Add(res[i]);
        }

        return res;
    }

    private static readonly BackstoryDef Childhood = Extra.BackstoryDefOf.MusicalKid86;
    private static readonly BackstoryDef Adulthood = Extra.BackstoryDefOf.NavyScientist52;
    private void MakePawnCapable(Pawn pawn)
    {
        if (!pawn.RaceProps.Humanlike)
            return;

        // 1. Remove work-disabling backstories
        // Setting these to null removes the primary source of "Incapable of"
        pawn.story.Childhood = Childhood;
        pawn.story.Adulthood = Adulthood;
        pawn.Notify_DisabledWorkTypesChanged();

        // Make sure no work is disabled
        pawn.workSettings?.EnableAndInitialize();

        // 2. Remove work-disabling traits (e.g., Lazy, Brawler, etc.)
        pawn.story.traits.allTraits.Clear();

        // 3. Optional: Clear DLC restrictions if testing with Royalty/Ideology
        if (ModsConfig.RoyaltyActive) pawn.royalty?.AllTitlesForReading.Clear();

        // 4. IMPORTANT: Refresh the pawn's internal work-tag cache
        pawn.Notify_DisabledWorkTypesChanged();
    }

    public PawnBuilder SetFaction(Faction faction2)
    {
        return Do(p => p.SetFaction(faction2));
    }

    public PawnBuilder TakeDamage(float amount, BodyPartDef bodyPart = null)
    {
        return Do(p => p.TakeDamage(new DamageInfo(
            DamageDefOf.Cut,
            amount,
            hitPart: p.RaceProps.body.AllParts.FirstOrDefault(p2 => bodyPart == null || p2.def == bodyPart))
        ));
    }
}

internal static class PawnBuilderExtension
{
    extension(PawnBuilder builder)
    {
        public PawnBuilder ResetRecords()
        {
            return builder.Do(p =>
            {
                var recordDefs = DefDatabase<RecordDef>.AllDefsListForReading.ToList();

                foreach (var recordDef in recordDefs)
                {
                    if (recordDef.type == RecordType.Time)
                        continue;
                    p.records.AddTo(recordDef, -p.records.GetValue(recordDef));
                }
            });
        }
        
        public PawnBuilder SetGender(Gender gender) => builder.Do(p => p.gender = gender);

        public PawnBuilder ForceBirthday(int ageOffset = 1)
        {
            return builder.Do(p =>
            {
                p.ageTracker.AgeBiologicalTicks = (p.ageTracker.AgeBiologicalYears + ageOffset) * 3_600_000 + 1;
                p.ageTracker.DebugForceBirthdayBiological();
            });
        }

        public PawnBuilder SetAge(int age)
        {
            return builder.Do(p =>
            {
                p.ageTracker.AgeBiologicalTicks = age * GenDate.TicksPerYear;
            });
        }

        public PawnBuilder EquipWeapon(ThingDef weaponDef, Func<Pawn, int, bool> shouldEquip = null)
        {
            return builder.Do((pawn, i) =>
            {
                if (pawn.Dead || (shouldEquip?.Invoke(pawn, i) ?? false)) return;

                var weapon = ThingMaker.MakeThing(weaponDef);

                pawn.equipment ??= new Pawn_EquipmentTracker(pawn);
                pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
                pawn.equipment.AddEquipment((ThingWithComps)weapon);
            });
        }

        public PawnBuilder Capture(Pawn captured)
        {
            return builder.DoOnce(captor =>
            {
                CaptureUtility.TryGetBed(captor, captured, out var bed);
                var job = JobMaker.MakeJob(JobDefOf.Capture, captured, bed);
                job.count = 1;
                job.playerForced = true;
                captor.jobs.StartJob(job, JobCondition.InterruptForced);
            });
        }

        public PawnBuilder StartJob(JobDef jobDef, LocalTargetInfo? targetA = null, LocalTargetInfo? targetB = null)
        {
            return builder.DoOnce(pawn =>
            {
                var job = JobMaker.MakeJob(jobDef, targetA ?? null, targetB ?? null);
                job.count = 1;
                job.playerForced = true;
                pawn.jobs.StartJob(job, JobCondition.InterruptForced);
            });
        }

        public PawnBuilder WeakenParts(HashSet<BodyPartDef> weakenParts, bool oneSide = false)
        {
            var bruise = Extra.HediffDefOf.Bruise;
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
                    injury?.Severity = damage;
                    pawn.health.forceDowned = true;
                    pawn.health.AddHediff(injury);
                }
            });
        }

        public PawnBuilder Armed()
        {
            return builder.Do(pawn =>
            {
                var armor1 = new ThingBuilder(Extra.ThingDefOf.Apparel_PowerArmor).Quality(QualityCategory.Legendary).CreateSingle<Apparel>(false);
                var armor2 = new ThingBuilder(Extra.ThingDefOf.Apparel_PowerArmorHelmet).Quality(QualityCategory.Legendary).CreateSingle<Apparel>(false);

                pawn.apparel.Wear(armor1);
                pawn.apparel.Wear(armor2);
            });
        }

        public PawnBuilder SetRelation(Pawn other, PawnRelationDef relationDef)
        {
            return builder.Do(pawn => pawn.relations.AddDirectRelation(relationDef, other));
        }

        public PawnBuilder SetRandomRelations(int relationsPerPawn)
        {
            var possibleRelations = DefDatabase<PawnRelationDef>.AllDefsListForReading.Where(def => def is { implied: false } && def.defName != "Bond").ToList();

            return builder.Do((pawn, _, pawns) =>
            {
                var pawnsToCreateRelation = pawns
                    .Where(p => p is { relations: not null, RaceProps.Humanlike: true })
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

        public PawnBuilder ForceAddictionTo(Thing drug)
        {
            return builder.Do(pawn =>
            {
                if (!drug.TryGetComp<CompDrug>(out var comp))
                    return;

                for (var i = 0; i < 300; i++)
                {
                    comp.PrePostIngested(pawn);
                    if (pawn.health.hediffSet.HasHediff(comp.Props.chemical.addictionHediff))
                        break;
                }
            });
        }

        public PawnBuilder ApplyAbility(AbilityDef abilityDef, LocalTargetInfo target, LocalTargetInfo? dest = null)
        {
            return builder.ApplyAbility<CompAbilityEffect>(abilityDef, target, dest);
        }

        public PawnBuilder ApplyAbility<T>(AbilityDef abilityDef, LocalTargetInfo target, LocalTargetInfo? dest = null) where T : CompAbilityEffect
        {
            return builder.Do(pawn =>
            {
                var ability = pawn.abilities.GetAbility(abilityDef, true);
                var convertEffect = ability.EffectComps.OfType<T>().First();
                convertEffect.Apply(target, dest ?? LocalTargetInfo.Invalid);
            });
        }
    }
}
