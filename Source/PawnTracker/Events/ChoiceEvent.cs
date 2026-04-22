using RimWorld;

namespace PawnHistory.Source.PawnTracker.Events;

public record ChoiceEvent(Quest Quest) :  GameEventBase;