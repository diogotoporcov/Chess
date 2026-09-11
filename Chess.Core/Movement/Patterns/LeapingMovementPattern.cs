using Chess.Core.Board;
using Chess.Core.Board.Topology;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Patterns;

public sealed class LeapingMovementPattern : IMovementPattern
{
    private readonly Displacement _displacement;

    public LeapingMovementPattern(
        Displacement displacement)
    {
        ArgumentNullException.ThrowIfNull(displacement);
        
        _displacement = displacement;
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves(
        BoardState boardState,
        Square from,
        Side movingSide)
    {
        ArgumentNullException.ThrowIfNull(boardState);

        var destinations = ResolveDestinations(
            boardState.Topology,
            from);

        foreach (var destination in destinations)
        {
            if (!boardState.TryGetPiece(
                    destination,
                    out var occupyingPiece))
            {
                yield return new Move(
                    from, 
                    destination);
                
                continue;
            }

            if (occupyingPiece.Side != movingSide)
            {
                yield return new Move(
                    from,
                    destination);
            }
        }
    }

    private IReadOnlySet<Square> ResolveDestinations(
        BoardTopology topology,
        Square from)
    {
        var components = _displacement.Components.ToArray();

        var remainingSteps = components
            .Select(component => component.Distance)
            .ToArray();
        
        var destinations = new HashSet<Square>();

        Resolve(
            topology,
            from,
            components,
            remainingSteps,
            remainingSteps.Sum(),
            destinations);
        
        return destinations;
    }

    private static void Resolve(
        BoardTopology topology,
        Square current,
        IReadOnlyList<DisplacementComponent> components,
        int[] remainingSteps,
        int remainingDistance,
        HashSet<Square> destinations)
    {
        if (remainingDistance == 0)
        {
            destinations.Add(current);
            return;
        }
        
        for (var index = 0; index < components.Count; index++)
        {
            if (remainingSteps[index] == 0)
            {
                continue;
            }
            
            var direction = components[index].Direction;

            if (!topology.TryGetNext(
                    current,
                    direction,
                    out var next))
            {
                continue;
            }

            remainingSteps[index]--;
            
            Resolve(
                topology,
                next,
                components, 
                remainingSteps, 
                remainingDistance - 1, 
                destinations);
            
            remainingSteps[index]++;
        }
    }
}