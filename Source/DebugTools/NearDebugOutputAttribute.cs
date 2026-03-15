using LudeonTK;

namespace PawnHistory.Source.DebugTools;


class NearDebugOutputAttribute : DebugOutputAttribute
{
    public NearDebugOutputAttribute() : base(category: "Pawn History", true) { }
}