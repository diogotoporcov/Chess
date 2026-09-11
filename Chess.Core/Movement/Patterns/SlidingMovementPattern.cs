using Chess.Core.Board;
using Chess.Core.Pieces;

namespace Chess.Core.Movement.Patterns;

public sealed class SlidingMovementPattern : IMovementPattern
{
    public Direction Direction { get; }
    public int? MaxDistance { get; }

    public SlidingMovementPattern(
        Direction direction,
        int? maxDistance = null)
    {
        if (maxDistance is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDistance),
                "Maximum distance must be greater than zero.");
        }

        Direction = direction;
        MaxDistance = maxDistance;
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves(
        BoardState boardState,
        Square from,
        PieceColor movingColor)
    {
        ArgumentNullException.ThrowIfNull(boardState);

        var current = from;
        var distance = 0;

        while (
            (!MaxDistance.HasValue ||
             distance < MaxDistance.Value) &&
            boardState.Topology.TryGetNext(
                current,
                Direction,
                out var next))
        {
            distance++;

            if (!boardState.TryGetPiece(
                    next,
                    out var occupyingPiece))
            {
                yield return new Move(from, next);

                current = next;
                continue;
            }

            if (occupyingPiece.Color != movingColor)
            {
                yield return new Move(from, next);
            }

            yield break;
        }
    }
}