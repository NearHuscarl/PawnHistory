using PawnHistory.Source.PawnTracker.Recorders;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker;

// Features:
// - Display history record
//   - Colorize names and important information
//   - Tooltip
//   - Click to jump to related pawns
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
// Mental breaks, reason
// Skill level up/down
// Crawling to safety
// Craft a legendary item
// Permanent injury
// + From combat
// - From other curcumstances: scarification ritual, anomaly ritual, healing wound `.ispermanent = true`
// Raid type: seige with a different icon
// Ideology convert, belief reduced
// Pawn.Notify_PassedToWorld() event?
// TaleRecorder.RecordTale()
// Search for <IncidentDef>

// Create a filter in WorldPawn window (All/Alive/Dead)
// - Add a column to see the history of dead AND destroyed pawn. Concerned dead&destoryed pawn will open a dedicated history window.

// Bug:
// -- Human fist kill -> wrong message
// -- kill record POV is wrong for the killer

// TODO: handle another related pawn in BattleLogEntry_RangedImpact

[StaticConstructorOnStartup]
internal class PawnTracker
{
    static PawnTracker()
    {
        new HarmonyLib.Harmony("rimworld.mod.nearhuscarl.pawnhistory").PatchAllUncategorized(Assembly.GetExecutingAssembly());

        CompHistoryManager.AttachHistoryComp();
        RecorderManager.Initialize();
    }
}
