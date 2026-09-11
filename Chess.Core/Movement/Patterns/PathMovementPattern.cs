using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Patterns;

public sealed class PathMovementPattern : IMovementPattern
{
    private readonly Direction[] _path;

    public PathMovementPattern(
        params Direction[] path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (path.Length == 0)
        {
            throw new ArgumentException(
                "Path must contain at least one direction.",
                nameof(path));
        }

        _path =
        [
            .. path
        ];
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves(
        BoardState boardState,
        Square from,
        Side movingSide)
    {
        ArgumentNullException.ThrowIfNull(boardState);

        var current = from;

        for (var index = 0; index < _path.Length; index++)
        {
            if (!boardState.Topology.TryGetNext(
                    current,
                    _path[index],
                    out var next))
            {
                yield break;
            }

            var isDestination = index == _path.Length - 1;

            if (!isDestination)
            {
                if (boardState.IsOccupied(next))
                {
                    yield break;
                }

                current = next;
                continue;
            }

            if (!boardState.TryGetPiece(
                    next,
                    out var occupyingPiece))
            {
                yield return new Move(
                    from,
                    next);

                yield break;
            }

            if (occupyingPiece.Side != movingSide)
            {
                yield return new Move(
                    from,
                    next);
            }
        }
    }
}