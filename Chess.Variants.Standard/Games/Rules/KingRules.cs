using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Pieces;

namespace Chess.Variants.Standard.Games.Rules;

internal static class KingRules
{
    public static bool IsKing(
        Piece piece)
    {
        ArgumentNullException.ThrowIfNull(piece);

        return piece.Definition.Id == PieceDefinitions.King.Id;
    }

    public static Square FindKingSquare(
        GameState gameState,
        Side side)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(side);

        var kingSquares = gameState
            .BoardState
            .GetPiecePositions(side)
            .Where(position => IsKing(position.Piece))
            .Select(position => position.Square)
            .Take(2)
            .ToArray();

        if (kingSquares.Length != 1)
        {
            throw new InvalidOperationException(
                $"Side '{side}' must have exactly one king.");
        }

        return kingSquares[0];
    }
}
