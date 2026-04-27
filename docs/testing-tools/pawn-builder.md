# PawnBuilder

Builds a pawn or reuses an existing one, then applies setup processors.

## Constructor

- `PawnBuilder(int count = 1)`: start a builder for one or more pawns.

## Common Setup

- `Position(IntVec3 position, int radius = 3)`: set the spawn position and search radius.
- `WithPawns(IEnumerable<Pawn> pawns)`: use existing pawns instead of generating new ones.
- `WithKind(PawnKindDef kind)`: pin the pawn kind.
- `WithFaction(Faction faction)`: pin the faction.
- `WithFriendlyFaction()`: pick a friendly non-player faction.
- `HumanLike(bool value = true)`: require humanlike or not.
- `Colonist(bool value = true)`: shorthand for humanlike player faction.
- `Enemy(bool value = true)`: shorthand for hostile humanlike pawns.
- `Animal(PawnKindDef def = null)`: pick an animal pawn kind.
- `AsPrisoner()`: make the pawn a prisoner of the player.
- `FactionLeader(Faction faction)`: generate or use a leader pawn.
- `WorldPawn(bool value = true)`: pass the pawn to the world pawn list.
- `Corpse(bool rotten = false)`: generate a dead pawn, optionally rotten.

## Processors

- `ThatMatches(Predicate<Pawn> filter)`: add a filter for pawn reuse.
- `DoOnce(Action<Pawn> processor)`: run a processor only once.
- `Do(Action<Pawn, int, List<Pawn>> processor)`: run a processor with index and full pawn list.
- `Do(Action<Pawn> processor)`: run a processor per pawn.
- `Do(Action<Pawn, int> processor)`: run a processor with pawn and index.

## Health And Skills

- `AddHediff(HediffDef def, BodyPartDef partDef = null, Action<Hediff> hediffCreated = null, int partIndex = 0)`: add a hediff.
- `StopMentalState()`: recover from the current mental state.
- `DiesOnNextHit()`: mark the pawn to die when damaged next.
- `Heal()`: remove injuries and refill needs.
- `FullHeal()`: remove injuries, refill needs, and restore missing parts.
- `GiveTrait(TraitDef traitDef, int degree = 0, Action<Trait> traitCreated = null)`: grant a trait.
- `ResetSkillLevel(SkillDef skillDef, int level)`: set a skill level and reset XP/passion.
- `Learn(SkillDef skillDef, float xp)`: add skill XP.
- `SetDoctor(bool isBadDoctor = false)`: configure medicine skill and supplies.
- `DoSurgery(Pawn patient, RecipeDef recipe, BodyPartDef partDef, bool instant = false, int partIndex = 0)`: queue or apply surgery.
- `GroupTogether(IntVec3? at = null, int radius = 2)`: spawn pawns in a cluster.
- `MakeHostile()`: set generated pawns hostile.

## Execution

- `CreateSingle(bool reusePawns = true)`: return one pawn.
- `Execute(bool reusePawns = true)`: return all generated or reused pawns.

## Extensions

- `ResetRecords()`: clear record totals.
- `ForceBirthday()`: force the pawn to have a birthday.
- `EquipWeapon(ThingDef weaponDef, Func<Pawn, int, bool> shouldEquip = null)`: equip a weapon.
- `Capture(Pawn captured)`: queue a capture job.
- `StartJob(JobDef jobDef, LocalTargetInfo? targetA = null, LocalTargetInfo? targetB = null)`: queue a job.
- `WeakenParts(HashSet<BodyPartDef> weakenParts, bool oneSide = false)`: damage specific body parts.
- `SetRelation(Pawn other, PawnRelationDef relationDef)`: add a direct relation.
- `SetRandomRelations(int relationsPerPawn)`: add random relations among generated pawns.
- `ForceAddictionTo(Thing drug)`: drive addiction to a drug.
- `SetRoyalTitle(RoyalTitleDef royalTitle)`: grant a royal title.
