using LudeonTK;

namespace PawnHistory.Source.DebugTools;

class NearDebugActionAttribute : DebugActionAttribute
{
    public NearDebugActionAttribute(DebugActionType actionType = DebugActionType.Action) : base(
            category: "Pawn History",
            name: null,
            requiresRoyalty: false,
            requiresIdeology: false,
            requiresBiotech: false,
            requiresAnomaly: false,
            requiresOdyssey: false,
            displayPriority: 0,
            hideInSubMenu: false
        )
    {
        this.actionType = actionType;
    }
}