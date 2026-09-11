using Chess.Core.Board;
using Chess.Core.Movement;
using Chess.Core.Sides;

namespace Chess.Core.Pieces;

public sealed class Piece
{
    public Side Side { get; }
    public PieceDefinition Definition { get; }

    public Piece(
        Side side,
        PieceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(side);
        ArgumentNullException.ThrowIfNull(definition);

        Side = side;
        Definition = definition;
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves(
        MovementContext context,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(context);

        var boardState = context.BoardState;

        if (!boardState.TryGetPiece(
                from,
                out var occupyingPiece) ||
            !ReferenceEquals(
                occupyingPiece,
                this))
        {
            throw new InvalidOperationException(
                "This piece is not placed on the specified square.");
        }

        var generatedMoves = new HashSet<Move>();

        foreach (var pattern in Definition.MovementPatterns)
        {
            foreach (var move in
                     pattern.GeneratePseudoLegalMoves(
                         context,
                         from,
                         Side))
            {
                if (generatedMoves.Add(move))
                {
                    yield return move;
                }
            }
        }
    }
}