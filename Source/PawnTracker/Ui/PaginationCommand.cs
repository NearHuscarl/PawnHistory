namespace PawnHistory.Source.PawnTracker.Ui;

public abstract record PaginationCommand;

public sealed record FirstPageClicked : PaginationCommand;
public sealed record PreviousPageClicked : PaginationCommand;
public sealed record NextPageClicked : PaginationCommand;
public sealed record LastPageClicked : PaginationCommand;
public sealed record PageInputSubmitted : PaginationCommand;

