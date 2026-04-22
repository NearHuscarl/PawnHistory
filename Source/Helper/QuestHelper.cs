using RimWorld;
using Verse;

namespace PawnHistory.Source.Helper;

public static class QuestHelper
{
    public static Pawn GetAsker(Quest quest)
    {
        foreach (var part in quest.PartsListForReading)
        {
            switch (part)
            {
                case QuestPart_PawnsArrive pawnsArrive:
                {
                    var asker = pawnsArrive.pawns.FirstOrDefault(IsAskerCandidate);
                    if (asker != null)
                        return asker;
                    break;
                }
            }
        }

        return null;
    }
    
    private static bool IsAskerCandidate(Pawn pawn)
    {
        return pawn != null && pawn.RaceProps.Humanlike;
    }
}