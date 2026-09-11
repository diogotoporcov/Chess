using Chess.Core.Board;
using Chess.Core.Board.Topology;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Patterns;

public sealed class LeapingMovementPattern :
    IMovementPattern
{
    private readonly Displacement _displacement;
    private readonly MovementTargetMode _targetMode;

    public LeapingMovementPattern(
        Displacement displacement,
        MovementTargetMode targetMode = MovementTargetMode.MoveOrCapture)
    {
        ArgumentNullException.ThrowIfNull(displacement);

        if (!Enum.IsDefined(targetMode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetMode),
                targetMode,
                "Unsupported movement target mode.");
        }

        _displacement = displacement;
        _targetMode = targetMode;
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves(
        MovementContext context,
        Square from,
        Side movingSide)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(movingSide);

        var boardState = context.BoardState;

        var destinations = ResolveDestinations(
            context,
            from,
            movingSide);

        foreach (var destination in destinations)
        {
            if (!boardState.TryGetPiece(
                    destination,
                    out var occupyingPiece))
            {
                if (_targetMode is
                    MovementTargetMode.MoveOrCapture or
                    MovementTargetMode.MoveOnly)
                {
                    yield return new Move(
                        from,
                        destination);
                }

                continue;
            }

            if (occupyingPiece.Side != movingSide &&
                _targetMode is
                    MovementTargetMode.MoveOrCapture or
                    MovementTargetMode.CaptureOnly)
            {
                yield return new Move(
                    from,
                    destination);
            }
        }
    }

    public IEnumerable<Square> GenerateAttackedSquares(
        MovementContext context,
        Square from,
        Side attackingSide)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(attackingSide);

        if (_targetMode == MovementTargetMode.MoveOnly)
        {
            yield break;
        }

        foreach (var destination in ResolveDestinations(
                     context,
                     from,
                     attackingSide))
        {
            yield return destination;
        }
    }

    private IReadOnlySet<Square> ResolveDestinations(
        MovementContext context,
        Square from,
        Side movingSide)
    {
        var topology = context.BoardState.Topology;

        var components = _displacement.Components.ToArray();

        var directions = components
            .Select(
                component =>
                    component.Direction.Resolve(
                        context,
                        movingSide))
            .ToArray();

        var remainingSteps = components
            .Select(component => component.Distance)
            .ToArray();

        var destinations = new HashSet<Square>();

        Resolve(
            topology,
            from,
            directions,
            remainingSteps,
            remainingSteps.Sum(),
            destinations);

        return destinations;
    }

    private static void Resolve(
        BoardTopology topology,
        Square current,
        IReadOnlyList<Direction> directions,
        int[] remainingSteps,
        int remainingDistance,
        HashSet<Square> destinations)
    {
        if (remainingDistance == 0)
        {
            destinations.Add(current);
            return;
        }

        for (var index = 0; index < directions.Count; index++)
        {
            if (remainingSteps[index] == 0)
            {
                continue;
            }

            if (!topology.TryGetNext(
                    current,
                    directions[index],
                    out var next))
            {
                continue;
            }

            remainingSteps[index]--;

            Resolve(
                topology,
                next,
                directions,
                remainingSteps,
                remainingDistance - 1,
                destinations);

            remainingSteps[index]++;
        }
    }
}