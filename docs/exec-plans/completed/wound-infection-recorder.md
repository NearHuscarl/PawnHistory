# Wound Infection Recorder

## Summary

Wound infections now get their own history record instead of going through the generic discovered-hediff path. This matters because a wound infection reads best when it points back to the wound that later became infected, including the original combat log text when RimWorld still has body-part-specific text available on the source wound.

## Scope

The recorder handles `WoundInfection` and `ScariaInfection` created by `HediffComp_Infecter`. It does not replace generic disease, drug overdose, or health-complication recorders, and `HediffDiscoveredRecorder` already excludes wound and scaria infections so duplicate health entries are avoided.

## Design

The source wound receives the shared `HediffComp_History` whenever its hediff def has `HediffComp_Infecter`. The comp stores only the original damage instigator as a persisted `Thing` reference so concerns survive until the delayed infection appears. It deliberately does not store `DamageInfo`: the description prefers the wound's existing `combatLogText`, matching the casualty recorder's preference for RimWorld's own combat prose, but only when that text names the same body part as the source wound. The same comp is also used by scar recording, so a wound with both infection and permanent-scar behavior has only one `PH_instigator` save field.

The infection event is emitted from a Harmony postfix around `HediffComp_Infecter.CheckMakeInfection`. The postfix checks RimWorld's own `AlreadyMadeInfectionValue` sentinel through the infecter's private timer state, then finds the newly-created `WoundInfection` or `ScariaInfection` on the source part. The event carries only the infection and its source wound; the pawn and body part are derived at record time.

## Rules

When the source wound has combat log text that contains the source wound's full body-part label, the record keeps that text and appends a delayed infection sentence. When the combat log text is unavailable or names another damaged part from the same attack, the record falls back to the visible source wound label, such as the cut or bite in the affected body part. Scaria infections with a remembered animal source use a more specific fallback that says the infection developed after being bitten or scratched by that animal; the wound verb text is selected in the rulepack from the wound def name. Instigator concerns come from the stored source-wound tracker and are filtered by the normal history-record concern handling.

## Coverage

Three tests cover the recorder's behavioral branches: combat-log narration from real combat, non-combat fallback narration from a real lightning burn with no instigator concern, and scaria infection creation. The scaria test adds vanilla `Scaria` to an attacker and has that pawn deal real bite damage, reusing `Hediff_Scaria.Notify_PawnDamagedThing` to mark the victim wound as `fromScaria`. The combat-log test follows the same broad integration style as `CasualtyRecorder`: it spawns a friendly raid and an enemy raid, lets RimWorld combat run on the normal tick loop, and watches for the first wound that already has real `combatLogText`, a matching body-part label in that text, and `HediffComp_Infecter`. The test does not write `combatLogText` directly, call melee verbs directly, create a custom arena, or force attack jobs. Once a matching combat-logged infectable wound exists, `ForceInfection` owns the whole deterministic infection shortcut: it raises source-wound severity to vanilla's max infection-chance point, sets infection chance to `1`, moves the private infection timer to `1`, and ticks the wound once. This covers all infectable injury hediff defs because they all share the same `HediffComp_Infecter` infection creation path.

## Verification

Built with `MSBuild PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`.
