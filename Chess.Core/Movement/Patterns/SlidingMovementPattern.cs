using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Patterns;

public sealed class SlidingMovementPattern : IMovementPattern
{
    private readonly Direction _direction;
    private readonly int? _maxDistance;

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

        _direction = direction;
        _maxDistance = maxDistance;
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves(
        BoardState boardState,
        Square from,
        Side movingSide)
    {
        ArgumentNullException.ThrowIfNull(boardState);

        var current = from;
        var distance = 0;

        while (
            (!_maxDistance.HasValue ||
             distance < _maxDistance.Value) &&
            boardState.Topology.TryGetNext(
                current,
                _direction,
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

            if (occupyingPiece.Side != movingSide)
            {
                yield return new Move(from, next);
            }

            yield break;
        }
    }
}