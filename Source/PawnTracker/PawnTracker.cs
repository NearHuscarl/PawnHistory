using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Recorders;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker;

// Features:
// - Display history record
//   - Colorized names and important information
//   - Tooltip
//   - Left Click to jump to related pawns
//   - Right Click to copy description to clipboard
//   - Some icons

// Icon References (AssetRipper)
// - Assets\Resources\textures\things\mote\thoughtsymbol
// - Assets\Resources\textures\things\mote\speechsymbols
// - Assets\Resources\textures\things\mote\battlesymbols
// - Assets\Resources\textures\ui
// - 2636329500
// - 3268401022
// - rimworld\assets-royalty\Assets\data\royalty\textures
// - rimworld\assets-biotech\Assets\data\biotech\textures
// - rimworld\assets-ideology\Assets\data\ideology\textures
// - rimworld\assets-anomaly\Assets\data\anomaly\textures
// - rimworld\assets-odyssey\Assets\data\odyssey\textures

// Events:
// - Skill level up/down
//  + Reason: Become a __ (lvl x) at doctor after doing x surgeries
//  ^ Reason: Become a __ (lvl x) at doctor after doing surgeries for x hours
//  - More detailed reason: Become a __ (lvl x) at social after selling [1st expensive item], [2nd expensive item] and x others
//  - Time record: time spent in bed, hunting...
// Crawling to safety
// Social gathering: Party, wedding, ritual...
// - Social fight
//  + Base
//  - Ideology
// - Panic Flee
// + Food poisoning
// + Birthday
// - Scarred body part
//  + Instant from injury damage
//  + Post heal
//  - Scarfication ritual
// - Recruited
//  + prisoner
//  ^ quest
// Catching fire: handle with permanent/missing hediffs.
// Craft a legendary item
// - Mental breaks
//  + Base game
//  - DLC breaks
// - Relationships (breakup, cheating, new love..)
// Raid type: seige with a different icon
// Ideology convert, belief reduced
// Quest: track giver, link to (visible) quest.
// Pawn.Notify_PassedToWorld() event?
// TaleRecorder.RecordTale()
// Letter.xml
// Search for <IncidentDef>
// - HistoryEventDefOf.cs
//  - .RecordEvent(
// - Guest transition: Prisoner/Slave/Guest
//  - "SetGuestStatus("
// - ^ Mod options
//  - Prune less important records
// - Docs
//  - DescriptionBuilder and rulepack
// - Test
//  - TestManager should collect all failing tests instead of stopping on first failure
//  - TestManager should re-run failed tests from previous run first

// Create a filter in WorldPawn window (All/Alive/Dead)
// - Add a column to see the history of dead AND destroyed pawn. Concerned dead&destoryed pawn will open a dedicated history window.

// Bug:
// - Test multiple maps to see if CasualtyRecorder use the correct battle log?
// -- kill record POV is wrong for the killer
// -- Exception when killing in dev mode, in KillOrDown handler
// -- In-memory information > Exposable
//  - BodyPartScarredContext.InstigatorLookup

[StaticConstructorOnStartup]
internal class PawnTracker
{
    static PawnTracker()
    {
        new HarmonyLib.Harmony("rimworld.mod.nearhuscarl.pawnhistory").PatchAllUncategorized(Assembly.GetExecutingAssembly());

        CompHistoryManager.AttachHistoryComp();
        HediffComp_History.InjectComp();
        CompCookTracker.InjectComp();
        RecorderManager.Initialize();
    }
}
