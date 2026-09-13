using Chess.Core.Board;
using Chess.Core.Pieces;
using Chess.Core.Sides;

namespace Chess.Core.Games.Variants;

public sealed record InitialPiecePlacement
{
    public Square Square { get; }

    public Side Side { get; }

    public PieceDefinition Definition { get; }

    public InitialPiecePlacement(
        Square square,
        Side side,
        PieceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(side);
        ArgumentNullException.ThrowIfNull(definition);

        Square = square;
        Side = side;
        Definition = definition;
    }
}
