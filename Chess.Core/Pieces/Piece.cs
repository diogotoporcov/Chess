using Chess.Core.Board;
using Chess.Core.Movement;
using Chess.Core.Movement.Patterns;

namespace Chess.Core.Pieces;

public sealed class Piece
{
    private readonly IMovementPattern[] _movementPatterns;

    public PieceColor Color { get; }

    public Piece(
        PieceColor color,
        params IMovementPattern[] movementPatterns)
    {
        ArgumentNullException.ThrowIfNull(movementPatterns);

        if (movementPatterns.Any(
                pattern => pattern is null))
        {
            throw new ArgumentException(
                "Movement patterns cannot contain null values.",
                nameof(movementPatterns));
        }

        Color = color;

        _movementPatterns =
        [
            .. movementPatterns
        ];
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves(
        BoardState boardState,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(boardState);

        if (!boardState.TryGetPiece(
                from,
                out var occupyingPiece) ||
            !ReferenceEquals(occupyingPiece, this))
        {
            throw new InvalidOperationException(
                "This piece is not placed on the specified square.");
        }

        var generatedMoves = new HashSet<Move>();

        foreach (var pattern in _movementPatterns)
        {
            foreach (var move in
                     pattern.GeneratePseudoLegalMoves(
                         boardState,
                         from,
                         Color))
            {
                if (generatedMoves.Add(move))
                {
                    yield return move;
                }
            }
        }
    }
}