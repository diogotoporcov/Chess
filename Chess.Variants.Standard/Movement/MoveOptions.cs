using Chess.Core.Movement;

namespace Chess.Variants.Standard.Movement;

public static class MoveOptions
{
    public static MoveOptionId EnPassant { get; } = new("chess:en-passant");
}