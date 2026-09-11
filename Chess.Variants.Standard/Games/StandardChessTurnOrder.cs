using Chess.Core.Games;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Games;

public static class StandardChessTurnOrder
{
    public static TurnOrder Instance { get; } =
        new(
            StandardSides.White,
            StandardSides.Black);
}