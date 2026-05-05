namespace PawnHistory.Source.PawnTracker.Ui;

public abstract record HistoryTableCommand : Command;

public sealed record BeginEditRequested(HistoryRecord Record) : HistoryTableCommand;
public sealed record DeleteRecordRequested(HistoryRecord Record) : HistoryTableCommand;
public sealed record SaveEditedRecord : HistoryTableCommand;
public sealed record CancelEditedRecord : HistoryTableCommand;
