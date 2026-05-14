# PawnBuilder

Creates new pawns or reuses existing ones, then runs setup processors before returning them.

## Creation And Identity

- `scenario.Pawn(int count = 1)`: start a builder for generated pawns.
- `scenario.Pawn(IEnumerable<Pawn>)`: start from existing pawns.
- `scenario.Pawn(Pawn)`: start from one existing pawn.
- `Position(IntVec3 position, int radius = 3)`: pick the spawn or search position and radius.
- `WithPawns(IEnumerable<Pawn> pawns)`: reuse the given pawns instead of generating new ones.
- `WithKind(PawnKindDef kind)`: force a pawn kind for generation.
- `WithFaction(Faction faction)`: force the generation faction.
- `SetFaction(Faction faction)`: change the built pawn's faction after creation.
- `HumanLike(bool value = true)`: require or disable humanlike generation.
- `Colonist(bool value = true)`: shorthand for player-faction humanlikes.
- `Enemy(bool value = true)`: shorthand for hostile humanlikes.
- `Animal(PawnKindDef def = null)`: generate a random or specific animal kind.
- `AsPrisoner()`: mark generated pawns as player prisoners.
- `AsSlave()`: mark generated pawns as slaves.
- `FactionLeader(Faction faction)`: use or generate the faction leader.
- `WorldPawn(bool value = true)`: move the result into world-pawn handling.
- `Corpse(bool rotten = false)`: kill the pawn after creation and optionally rot the corpse.
- `ThatMatches(Predicate<Pawn> filter)`: add a filter for which reused pawns are acceptable.

## Processors

- `DoOnce(Action<Pawn> processor)`: run a processor only on the first built pawn.
- `Do(Action<Pawn> processor)`: run a simple processor on each pawn.
- `Do(Action<Pawn, int> processor)`: run a processor with the pawn index.
- `Do(Action<Pawn, int, List<Pawn>> processor)`: run a processor with pawn, index, and the whole result list.
- `GroupTogether(IntVec3? at = null, int radius = 2)`: cluster pawns around a point after spawning.

## Health And Skills

- `AddHediff(HediffDef def, BodyPartDef partDef = null, Action<Hediff> hediffCreated = null, int partIndex = 0)`: add a hediff to the pawn, optionally on a specific body part.
- `StopMentalState()`: end the pawn's current mental state.
- `DiesOnNextHit()`: mark the pawn so the next hit kills them.
- `Heal()`: clear injuries and refill needs.
- `FullHeal()`: clear injuries, refill needs, and restore missing parts.
- `GiveTrait(TraitDef traitDef, int degree = 0, Action<Trait> traitCreated = null)`: add a trait and optionally inspect it.
- `ResetSkillLevel(SkillDef skillDef, int level)`: set a skill to an exact level and reset its progress state.
- `Learn(SkillDef skillDef, float xp)`: add skill XP.
- `SetDoctor(bool isBadDoctor = false)`: configure the pawn as a good or bad doctor for surgery tests.
- `DoSurgery(Pawn patient, RecipeDef recipe, BodyPartDef partDef, bool instant = false, int partIndex = 0)`: queue or immediately perform surgery on a patient body part.

## Common Helpers

- `ResetRecords()`: clear record totals on the built pawn.
- `SetGender(Gender gender)`: force gender.
- `ForceBirthday(int ageOffset = 1)`: move the pawn onto a birthday and fire birthday logic.
- `SetAge(int age)`: set biological age in years.
- `EquipWeapon(ThingDef weaponDef, Func<Pawn, int, bool> shouldEquip = null)`: equip a weapon, optionally only on selected pawns.
- `Capture(Pawn captured)`: start a forced capture job on the target pawn.
- `StartJob(JobDef jobDef, LocalTargetInfo? targetA = null, LocalTargetInfo? targetB = null)`: start a forced job with optional targets.
- `WeakenParts(HashSet<BodyPartDef> weakenParts, bool oneSide = false)`: bruise the selected body-part defs to make them fragile.
- `Armed()`: equip the pawn for combat with strong armor and a weapon.
- `SetRelation(Pawn other, PawnRelationDef relationDef)`: create a direct relation to another pawn.
- `SetRandomRelations(int relationsPerPawn)`: assign random relations among the built pawns.
- `ForceAddictionTo(Thing drug)`: apply addiction pressure from the given drug.
- `ApplyAbility(AbilityDef abilityDef, LocalTargetInfo target, LocalTargetInfo? dest = null)`: apply an ability using its default effect comp.
- `ApplyAbility<T>(AbilityDef abilityDef, LocalTargetInfo target, LocalTargetInfo? dest = null)`: apply an ability through a specific effect comp type.

## DLC Helpers

- `SetGrowthTier(int tier)`: set child growth points to a specific growth tier threshold.
- `RemoveIdeo()`: clear the pawn's ideology.
- `SetIdeo(Ideo ideo = null, PreceptDef role = null, float? certainty = null)`: assign ideology and optionally role and certainty.
- `SetIdeoCertainty(float certainty)`: set certainty to an exact value.
- `SetRole(PreceptDef roleDef)`: assign an ideology role.
- `SetRoyalTitle(RoyalTitleDef royalTitle)`: assign an Empire royal title if needed.

## Execution

- `CreateSingle(bool reusePawns = true)`: build and return one pawn.
- `Execute(bool reusePawns = true)`: build and return the full pawn list.
