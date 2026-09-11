using Chess.Core.Sides;

namespace Chess.Core.Games;

public static class StandardChessTurnOrder
{
    public static TurnOrder Instance { get; } =
        new(
            StandardSides.White,
            StandardSides.Black);
}