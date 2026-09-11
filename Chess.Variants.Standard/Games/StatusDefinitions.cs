using Chess.Core.Games.Status;

namespace Chess.Variants.Standard.Games;

public static class StatusDefinitions
{
    public static GameStatusId Active { get; } = new("chess:active");

    public static GameStatusId Check { get; } = new("chess:check");

    public static GameStatusId Checkmate { get; } = new("chess:checkmate");

    public static GameStatusId Stalemate { get; } = new("chess:stalemate");
}